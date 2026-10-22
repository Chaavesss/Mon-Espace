using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Web.Script.Serialization;

internal static class QuickCaptureData {
 internal static bool Add(Store store,bool note,string title,string project,string details,Func<bool> save){
  if(String.IsNullOrWhiteSpace(title))return false;
  if(note){var item=new NotePage{id=Guid.NewGuid().ToString(),title=title.Trim(),project=project,body=details??"",updated=DateTime.Now.ToString("o")};store.Value.pages.Add(item);try{if(save())return true;}catch{store.Value.pages.Remove(item);throw;}store.Value.pages.Remove(item);}
  else{var item=new WorkTask{id=Guid.NewGuid().ToString(),title=title.Trim(),project=project,status="todo",priority="Normale",due="",notes=details??""};store.Value.tasks.Add(item);try{if(save())return true;}catch{store.Value.tasks.Remove(item);throw;}store.Value.tasks.Remove(item);}
  return false;
 }
}
internal sealed partial class ModernWorkspace {
 internal void Capture(){
  StackPanel body,actions;var w=Dialog("Capture rapide",out body,out actions,590);
  w.Width=550;w.MinWidth=550;w.WindowStartupLocation=WindowStartupLocation.CenterScreen;w.ShowInTaskbar=false;w.Topmost=true;
  body.Children.Add(T("Une idée, une chose à faire. Notez-la et reprenez votre activité.",12,Muted,new Thickness(0,0,0,16)));
  var kind=Combo(new[]{"Tâche","Note","À trier"},"À trier");Field(body,"À ajouter",kind);
  var title=new TextBox{MaxLength=180};Field(body,"Titre",title);
  var projects=owner.AllProjects();var project=Combo(projects,projects.Contains(projectFilter)?projectFilter:projects[0]);Field(body,"Projet",project);project.IsEnabled=kind.SelectedIndex!=2;kind.SelectionChanged+=delegate{project.IsEnabled=kind.SelectedIndex!=2;};
  var details=new TextBox{AcceptsReturn=true,TextWrapping=TextWrapping.Wrap,Height=86,MaxLength=10000,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};Field(body,"Détails (facultatif)",details);
  var hint=T("Ctrl + Entrée : enregistrer • Échap : annuler",11,Muted);body.Children.Add(hint);
  bool accepted=false;
  Action save=delegate{
   if(accepted)return;
   if(String.IsNullOrWhiteSpace(title.Text)){hint.Text="Ajoutez un titre pour enregistrer.";title.Focus();return;}
   if(!(kind.SelectedIndex==2?PersonalWorkflows.CaptureInbox(store,title.Text,details.Text,owner.Save):QuickCaptureData.Add(store,kind.SelectedIndex==1,title.Text,Convert.ToString(project.SelectedItem),details.Text,owner.Save))){hint.Text="L'enregistrement a échoué. Votre saisie reste disponible.";return;}
   accepted=true;if(dialogProbe==null&&previewPrefix==null)w.DialogResult=true;
  };
  w.PreviewKeyDown+=delegate(object sender,KeyEventArgs e){if(e.Key==Key.Enter&&(Keyboard.Modifiers&ModifierKeys.Control)!=0){e.Handled=true;save();}};
  actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));actions.Children.Add(Btn("Ajouter",save,true));
  w.Loaded+=delegate{w.Activate();title.Focus();Keyboard.Focus(title);};
  ShowWindow(w);Refresh();
 }
 internal void VerifyQuickCapture(string folder){
  var json=new JavaScriptSerializer();string before=json.Serialize(store.Value);bool tested=false;
  dialogProbe=delegate(Window w){
   if(w.Width!=550||!w.Topmost||w.ShowInTaskbar||w.WindowStartupLocation!=WindowStartupLocation.CenterScreen)throw new Exception("Fenêtre de capture");
   ClickWizard(w,"Ajouter");if(json.Serialize(store.Value)!=before)throw new Exception("Titre vide accepté");
   var boxes=LogicalControls<TextBox>(w).ToList();boxes[0].Text="  Idée rapide  ";boxes[1].Text="Détails conservés";
   LogicalControls<ComboBox>(w).First().SelectedIndex=1;
   ClickWizard(w,"Ajouter");ClickWizard(w,"Ajouter");
   if(store.Value.pages.Count!=1||store.Value.pages[0].title!="Idée rapide"||store.Value.pages[0].body!="Détails conservés")throw new Exception("Note ou double clic");
   tested=true;return true;
  };
  try{Capture();}finally{dialogProbe=null;}
  if(!tested||new Store(store.Root).Value.pages.Count!=1)throw new Exception("Persistance note");
  if(!QuickCaptureData.Add(store,false,"Tâche rapide","Personnel","À retrouver dans le tableau",delegate{store.Save();return true;}))throw new Exception("Ajout tâche");
  var loaded=new Store(store.Root);if(loaded.Value.tasks.Count!=1||loaded.Value.tasks[0].status!="todo")throw new Exception("Persistance tâche");
  int count=store.Value.tasks.Count;
  if(QuickCaptureData.Add(store,false,"Échec","Personnel","",delegate{return false;})||store.Value.tasks.Count!=count)throw new Exception("Annulation échec");
  bool thrown=false;try{QuickCaptureData.Add(store,true,"Échec","Personnel","",delegate{throw new IOException("Échec simulé");});}catch(IOException){thrown=true;}if(!thrown||store.Value.pages.Count!=1)throw new Exception("Annulation exception");
  before=json.Serialize(store.Value);dialogProbe=delegate{return false;};try{Capture();}finally{dialogProbe=null;}if(json.Serialize(store.Value)!=before)throw new Exception("Annulation modifie les données");
  previewPrefix=Path.Combine(folder,"Capture");try{Capture();}finally{previewPrefix=null;}
 }
}
internal sealed partial class MainForm {
 internal void VerifyQuickCapture(string folder){
  rendering=true;bool visibleBefore=Visible;var stateBefore=WindowState;modern.VerifyQuickCapture(folder);
  if(Visible!=visibleBefore||WindowState!=stateBefore)throw new Exception("La capture a restauré la fenêtre principale");
  if(uiTicker!=null)uiTicker.Dispose();Stop();if(speech!=null)speech.Dispose();player.Close();
 }
}
internal static partial class Program {
 internal static int CaptureTest(string folder){Root=Path.GetFullPath(folder);Directory.CreateDirectory(Root);System.Windows.Forms.Application.EnableVisualStyles();try{using(var form=new MainForm(false))form.VerifyQuickCapture(Root);File.WriteAllText(Path.Combine(Root,"CAPTURE_PASS.txt"),"PASS: actual WPF input and Add button, blank title, note creation, duplicate submit protection, note/task persistence, rollback on failed save and exception, cancellation, compact centered window configuration, main window visibility unchanged, rendered preview. Global hotkey and desktop focus require real PC verification.");return 0;}catch(Exception ex){File.WriteAllText(Path.Combine(Root,"CAPTURE_FAIL.txt"),ex.ToString());return 1;}}
}
