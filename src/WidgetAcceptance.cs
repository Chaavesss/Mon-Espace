using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
internal static class WidgetAcceptance {
 static void Check(bool condition,string text){if(!condition)throw new Exception(text);}
 internal static IEnumerable<T> Controls<T>(DependencyObject node)where T:DependencyObject{var match=node as T;if(match!=null)yield return match;foreach(object child in LogicalTreeHelper.GetChildren(node)){var dep=child as DependencyObject;if(dep!=null)foreach(var item in Controls<T>(dep))yield return item;}}
 internal static void Core(string folder){
  var data=new Data();data.tasks.AddRange(new[]{new WorkTask{id="done",title="Terminé",status="done",due="2020-01-01"},new WorkTask{id="later",title="Plus tard",status="todo",due="2028-04-01"},new WorkTask{id="early",title="Urgent",status="todo",due="2028-03-01",priority="Haute"},new WorkTask{id="none",title="Sans date",status="todo",due=""},new WorkTask{id="doing",title="En cours",status="doing",due="2028-03-01",priority="Haute"}});
  var tasks=WidgetModel.Tasks(data);Check(tasks.Count==3&&tasks[0].id=="doing"&&tasks[1].id=="early"&&tasks[2].id=="later","Ordre ou limite des tâches widget");
  var screens=new[]{new Rect(0,0,1920,1040),new Rect(-1280,0,1280,984)};
  var point=WidgetModel.Clamp(new Point(9000,9000),new Size(340,560),screens);Check(point.X==1580&&point.Y==480,"Écran débranché");
  point=WidgetModel.Clamp(new Point(-1000,800),new Size(340,560),screens);Check(point.X== -1000&&point.Y==424,"Écran secondaire négatif");
  var settings=new Settings();settings.Widget=null;WidgetModel.Validate(settings);Check(settings.Widget!=null&&!settings.Widget.openOnStartup,"Migration widget");settings.Widget.left=Double.NaN;settings.Widget.positionSaved=true;WidgetModel.Validate(settings);Check(!settings.Widget.positionSaved,"Position non finie");
  var store=new Store(Path.Combine(folder,"widget-core"));store.Value.settings.Widget=new DesktopWidgetOptions{openOnStartup=true,topmost=false,positionSaved=true,left=-1000,top=100,width=420,height=620};store.Save();var loaded=new Store(store.Root);Check(loaded.Value.settings.Widget.width==420&&loaded.Value.settings.Widget.height==620&&loaded.Value.settings.Widget.left== -1000&&loaded.Value.settings.Widget.openOnStartup&&!loaded.Value.settings.Widget.topmost,"Persistance widget");
  string archive=Path.Combine(folder,"widget.mespace");FullBackup.Export(store,archive);var target=new Store(Path.Combine(folder,"widget-restored"));using(var plan=FullBackup.Prepare(target,archive))FullBackup.Commit(target,plan);Check(new Store(target.Root).Value.settings.Widget.left== -1000,"Sauvegarde complète widget");
 }
 internal static void Ui(DesktopWidget widget){
  var content=(DependencyObject)widget.Content;Check(widget.Owner==null&&!widget.ShowInTaskbar,"Widget lié à la fenêtre principale");
  var buttons=Controls<Button>(content).Select(b=>Convert.ToString(b.Content)).ToList();Check(buttons.Contains("+ Capture rapide")&&buttons.Contains("Ouvrir l'espace")&&buttons.Contains("Précédent")&&buttons.Contains("Suivant"),"Commandes widget");
  var startup=Controls<CheckBox>(content).First(c=>Convert.ToString(c.Content)=="Ouvrir avec Mon espace");startup.IsChecked=true;startup.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
  var done=Controls<CheckBox>(content).First(c=>Convert.ToString(c.ToolTip)=="Terminer cette tâche");done.IsChecked=true;done.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
 }
}
internal sealed partial class MainForm {
 internal void VerifyDesktopWidget(string folder){
  rendering=true;store.Value.tasks.AddRange(new[]{new WorkTask{id="widget-1",title="Préparer les documents du projet",project="Travail",priority="Haute",status="doing",due=DateTime.Today.ToString("yyyy-MM-dd"),notes=""},new WorkTask{id="widget-2",title="Vérifier les prochaines échéances",project="Personnel",priority="Normale",status="todo",due=DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),notes=""},new WorkTask{id="widget-3",title="Faire le point sur mes projets",project="Projets",priority="Normale",status="todo",due="",notes=""}});
  store.Save();bool visible=Visible;
  var widget=new DesktopWidget(this,store);
  try{
   WidgetAcceptance.Ui(widget);if(Visible!=visible||new Store(store.Root).Value.tasks.First(t=>t.id=="widget-1").status!="done"||!new Store(store.Root).Value.settings.Widget.openOnStartup)throw new Exception("Action widget ou visibilité principale");
   widget.TestPlayback(new DesktopMusicState{Available=true,Playing=true,CanToggle=true,CanNext=true,CanPrevious=true,Album="Pochette de démonstration",Artwork=ArtworkAcceptance.Demo(),Title="Exemple de morceau",Artist="Spotify · aperçu de test"});if(!WidgetAcceptance.Controls<AlbumArtwork>((DependencyObject)widget.Content).Single().HasImage)throw new Exception("Pochette widget absente");widget.Render(Path.Combine(folder,"Widget.png"));
   widget.TestPlayback(new DesktopMusicState());if(WidgetAcceptance.Controls<AlbumArtwork>((DependencyObject)widget.Content).Single().HasImage)throw new Exception("Ancienne pochette widget conservée");if(WidgetAcceptance.Controls<Button>((DependencyObject)widget.Content).Any(b=>Convert.ToString(b.Content)=="Lecture"&&b.IsEnabled))throw new Exception("Lecture active sans Spotify");
   store.Value.settings.Appearance.card="#F1EEE8";store.Value.settings.Appearance.color1="#F1EEE8";widget.RefreshView();widget.Render(Path.Combine(folder,"Widget-clair.png"));
  }finally{widget.Shutdown();}
  if(uiTicker!=null)uiTicker.Dispose();modern.CancelMusic();Stop();if(speech!=null)speech.Dispose();player.Close();
 }
}
internal static partial class Program {
 internal static int WidgetTest(string folder){Root=Path.GetFullPath(folder);Directory.CreateDirectory(Root);System.Windows.Forms.Application.EnableVisualStyles();try{WidgetAcceptance.Core(Root);using(var form=new MainForm(false))form.VerifyDesktopWidget(Root);File.WriteAllText(Path.Combine(Root,"WIDGET_PASS.txt"),"PASS: task ordering/limit, screen clamping and negative coordinates, migration and invalid position repair, preference persistence and full backup, independent unowned WPF window, actual task checkbox completion and startup checkbox, main window visibility unchanged, album artwork display and clearing, Spotify controls with/without session, dark/light rendered previews. Native dragging, monitor DPI, window stacking and real Spotify playback require target PC verification.");return 0;}catch(Exception ex){File.WriteAllText(Path.Combine(Root,"WIDGET_FAIL.txt"),ex.ToString());return 1;}}
}
