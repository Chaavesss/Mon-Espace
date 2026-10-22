using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

internal sealed partial class ModernWorkspace {
 void TemplatesPage(StackPanel root){
  var tools=new WrapPanel{Margin=new Thickness(0,0,0,18)};tools.Children.Add(Btn("Tous les projets",delegate{Go("Mes projets");},false,true));tools.Children.Add(Btn("+ Nouveau modèle",delegate{EditTemplate(new ProjectTemplate(),true);},true));root.Children.Add(tools);
  root.Children.Add(T("Une base réutilisable. Choisissez un modèle, nommez votre projet et ajustez les tâches à créer.",12,Muted,new Thickness(0,0,0,22)));
  foreach(var group in new[]{new{Title="Pour commencer",Items=ProjectTemplates.Defaults()},new{Title="Mes modèles",Items=store.Value.projectTemplates.ToList()}}){
   root.Children.Add(T(group.Title,21,Ink,new Thickness(0,0,0,14)));
   if(group.Items.Count==0)root.Children.Add(T("Créez un modèle, ou enregistrez un projet existant comme modèle.",12,Muted,new Thickness(0,0,0,18)));
   foreach(var template in group.Items){var item=template;var body=new StackPanel();body.Children.Add(T(item.description,12,Muted,new Thickness(0,0,0,10)));body.Children.Add(T(item.tasks.Count+" tâches · "+item.notes.Count+" notes",11,Muted,new Thickness(0,0,0,12)));var row=new WrapPanel();row.Children.Add(Btn("Utiliser ce modèle",delegate{CreateFromTemplate(item);},true));row.Children.Add(Btn(item.id.StartsWith("builtin-")?"Personnaliser":"Modifier",delegate{EditTemplate(ProjectTemplates.Copy(item),item.id.StartsWith("builtin-"));},false,true));if(!item.id.StartsWith("builtin-"))row.Children.Add(Btn("Supprimer",delegate{if(MessageBox.Show("Supprimer le modèle « "+item.name+" » ? Les projets déjà créés seront conservés.","Supprimer le modèle",MessageBoxButton.YesNo)==MessageBoxResult.Yes)try{ProjectTemplates.Delete(store,item.id);Refresh();}catch(Exception ex){MessageBox.Show(ex.Message);}},false,true));body.Children.Add(row);var card=Card(item.name,body);card.Margin=new Thickness(0,0,0,15);root.Children.Add(card);}
  }
 }
 void FinishTemplateDialog(Window w){if(dialogProbe==null&&previewPrefix==null)w.DialogResult=true;}
 void CreateFromTemplate(ProjectTemplate template){
  StackPanel body,actions;var w=Dialog("Créer depuis un modèle",out body,out actions,720);
  var name=new TextBox{Text=template.name,MaxLength=80};Field(body,"Nom du nouveau projet",name);
  body.Children.Add(T(template.description,12,Muted,new Thickness(0,0,0,12)));body.Children.Add(T("Tâches à créer",14,Ink,new Thickness(0,0,0,8)));
  var checks=new List<CheckBox>();foreach(var task in template.tasks){var check=new CheckBox{Content=task.title,IsChecked=true};checks.Add(check);body.Children.Add(check);}
  var notes=new CheckBox{Content="Inclure les "+template.notes.Count+" notes du modèle",IsChecked=template.notes.Count>0,Visibility=template.notes.Count>0?Visibility.Visible:Visibility.Collapsed};body.Children.Add(notes);
  body.Children.Add(T("Les tâches démarrent « À faire », sans échéance, rappel ni répétition. Vous pourrez les ajuster dans le projet.",11,Muted,new Thickness(0,12,0,8)));
  var error=T("",12,Accent);body.Children.Add(error);bool created=false;string result="";
  actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));actions.Children.Add(Btn("Créer le projet",delegate{if(created)return;try{result=ProjectTemplates.Create(store,template,name.Text,Enumerable.Range(0,checks.Count).Where(i=>checks[i].IsChecked==true),notes.IsChecked==true);created=true;FinishTemplateDialog(w);}catch(Exception ex){error.Text=ex.Message;}},true));
  ShowWindow(w);if(created)OpenProject(result);else Refresh();
 }
 void EditTemplate(ProjectTemplate draft,bool newTemplate){
  if(newTemplate)draft.id=Guid.NewGuid().ToString();
  StackPanel body,actions;var w=Dialog(newTemplate?"Nouveau modèle de projet":"Modifier le modèle de projet",out body,out actions,760);
  var name=new TextBox{Text=draft.name,MaxLength=80};Field(body,"Nom du modèle",name);
  var description=new TextBox{Text=draft.description,AcceptsReturn=true,Height=75,MaxLength=10000,TextWrapping=TextWrapping.Wrap};Field(body,"Description du projet",description);
  var tasks=new TextBox{Text=String.Join(Environment.NewLine,draft.tasks.Select(t=>t.title)),AcceptsReturn=true,Height=200,MaxLength=250000,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,TextWrapping=TextWrapping.NoWrap};Field(body,"Tâches : une par ligne",tasks);
  var notes=new CheckBox{Content="Conserver les "+draft.notes.Count+" notes du modèle",IsChecked=draft.notes.Count>0,Visibility=draft.notes.Count>0?Visibility.Visible:Visibility.Collapsed};body.Children.Add(notes);
  body.Children.Add(T("Les statuts, dates, rappels, répétitions, liens et fichiers du projet source ne sont pas repris. Les détails et priorités des tâches conservées sont gardés.",11,Muted,new Thickness(0,12,0,8)));
  var error=T("",12,Accent);body.Children.Add(error);bool saved=false;
  actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));actions.Children.Add(Btn("Enregistrer le modèle",delegate{if(saved)return;try{
   var copy=ProjectTemplates.Copy(draft);copy.name=name.Text.Trim();copy.description=description.Text;
   var lines=tasks.Text.Replace("\r","").Split('\n').Select(t=>t.Trim()).Where(t=>t.Length>0).ToList();var remaining=draft.tasks.ToList();
   copy.tasks=lines.Select(line=>{var found=remaining.FirstOrDefault(t=>t.title==line);if(found!=null){remaining.Remove(found);return ProjectTemplates.Copy(found);}return new TemplateTask{title=line};}).ToList();
   if(notes.IsChecked!=true)copy.notes.Clear();ProjectTemplates.Save(store,copy);saved=true;FinishTemplateDialog(w);
  }catch(Exception ex){error.Text=ex.Message;}},true));
  ShowWindow(w);Refresh();
 }
 void SaveProjectTemplate(string project){EditTemplate(ProjectTemplates.FromProject(store.Value,project,true),true);}
 internal void VerifyTemplatesUi(string folder){
  bool tested=false;dialogProbe=delegate(Window w){
   LogicalControls<TextBox>(w).First().Text="Projet de test";var checks=LogicalControls<CheckBox>(w).Where(c=>c.Visibility==Visibility.Visible).ToList();checks[1].IsChecked=false;
   ClickWizard(w,"Créer le projet");ClickWizard(w,"Créer le projet");tested=true;return true;
  };try{CreateFromTemplate(ProjectTemplates.Defaults()[0]);}finally{dialogProbe=null;}
  if(!tested||store.Value.tasks.Count!=4||store.Value.tasks.Any(t=>t.project!="Projet de test"))throw new Exception("Sélection de tâches ou double création");
  var before=ProjectTemplates.Copy(store.Value);dialogProbe=delegate{return false;};try{EditTemplate(ProjectTemplates.FromProject(store.Value,"Projet de test",true),true);}finally{dialogProbe=null;}if(store.Value.projectTemplates.Count!=before.projectTemplates.Count)throw new Exception("Annulation modèle");
  dialogProbe=delegate(Window w){var fields=LogicalControls<TextBox>(w).ToList();fields[0].Text="Mon chantier type";fields[2].Text="Préparer\nVérifier";ClickWizard(w,"Enregistrer le modèle");return true;};try{SaveProjectTemplate("Projet de test");}finally{dialogProbe=null;}
  if(new Store(store.Root).Value.projectTemplates.Single().tasks.Count!=2)throw new Exception("Édition modèle");
  Go("Modèles de projets");Preview(Path.Combine(folder,"Modeles.png"));
 }
}
internal sealed partial class MainForm {
 internal void VerifyTemplatesUi(string folder){rendering=true;modern.VerifyTemplatesUi(folder);if(uiTicker!=null)uiTicker.Dispose();Stop();if(speech!=null)speech.Dispose();player.Close();}
}
internal static partial class Program {
 internal static int TemplatesTest(string folder){Root=Path.GetFullPath(folder);Directory.CreateDirectory(Root);System.Windows.Forms.Application.EnableVisualStyles();try{ProjectTemplates.Test(Root);using(var form=new MainForm(false))form.VerifyTemplatesUi(Root);File.WriteAllText(Path.Combine(Root,"TEMPLATES_PASS.txt"),"PASS: model persistence, independent task IDs, reset statuses/dates/recurrence, source project capture, trash exclusion, note cloning, duplicate name rejection, JSON import/export, full backup restore, failed-save rollback, deletion preserving projects, legacy migration; WPF task selection, double submit protection, cancellation, template editing and rendered page.");return 0;}catch(Exception ex){File.WriteAllText(Path.Combine(Root,"TEMPLATES_FAIL.txt"),ex.ToString());return 1;}}
}
