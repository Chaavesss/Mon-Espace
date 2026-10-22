using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
internal sealed partial class ModernWorkspace {
 void VerifyProductivity(string folder){
  var t=new WorkTask{id="notification-ui",title="Préparer les documents",project="Personnel",priority="Normale",status="todo",due=DateTime.Today.ToString("yyyy-MM-dd")};store.Value.tasks.Add(t);store.Save();noticeAll=false;Go("Notifications");PreviewSized(Path.Combine(folder,"Notifications.png"),1200,800);
  var current=NotificationCenter.Current(store.Value,DateTime.Now).Single(n=>n.target==t.id);var card=LogicalControls<Border>((DependencyObject)((ScrollViewer)content.Content).Content).Single(b=>Convert.ToString(b.Tag)=="card:"+t.title);var button=LogicalControls<Button>(card).First(b=>Convert.ToString(b.Content)=="Marquer comme lu");button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
  if(!new Store(store.Root).Value.noticeStates.Single(s=>s.key==current.key).read)throw new Exception("Lecture notification UI");
  NotificationCenter.Set(store.Value,current.key,false,null);store.Save();Refresh();
  card=LogicalControls<Border>((DependencyObject)((ScrollViewer)content.Content).Content).Single(b=>Convert.ToString(b.Tag)=="card:"+t.title);var snooze=LogicalControls<Button>(card).Single(b=>Convert.ToString(b.Content)=="Reporter ⌄");((MenuItem)snooze.ContextMenu.Items[1]).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));if(NotificationCenter.Unread(new Store(store.Root).Value,current,DateTime.UtcNow))throw new Exception("Report notification UI");
  var prerequisite=new WorkTask{id="dependency-ui",title="Valider le dossier",project="Personnel",priority="Normale",status="todo"};store.Value.tasks.Add(prerequisite);t.dependsOn.Add(prerequisite.id);store.Save();Go("Mes tâches");if(!LogicalControls<TextBlock>((DependencyObject)((ScrollViewer)content.Content).Content).Any(x=>x.Text.Contains("En attente · Valider le dossier")))throw new Exception("Badge prérequis absent");PreviewSized(Path.Combine(folder,"Taches-prerequis.png"),1200,800);
  store.Value.settings.PrivacyMode=true;Refresh();if(noticeButton.Visibility!=Visibility.Collapsed)throw new Exception("Cloche visible en mode discret");store.Value.settings.PrivacyMode=false;Refresh();updateFound=new UpdateManifest{version="9.0.0.0",downloadUrl="https://updates.example/MonEspace.zip",notes="Version de démonstration"};Go("Mises à jour");if(!LogicalControls<Button>((DependencyObject)((ScrollViewer)content.Content).Content).Any(b=>Convert.ToString(b.Content)=="Mettre à jour et relancer"))throw new Exception("Bouton mise à jour absent");PreviewSized(Path.Combine(folder,"Mise-a-jour.png"),1200,800);updateFound=null;store.Value.tasks.Remove(t);store.Value.tasks.Remove(prerequisite);store.Save();File.WriteAllText(Path.Combine(folder,"PRODUCTIVITY_UI_PASS.txt"),"PASS: real notification page render, mark read via button persisted, snooze state, dependency badge. Actual update/relaunch requires target PC verification.");
 }
}
