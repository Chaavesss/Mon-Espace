using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
internal static class FocusAcceptance {
 static void Check(bool value,string text){if(!value)throw new Exception(text);}
 internal static void Core(string folder){
  var store=new Store(Path.Combine(folder,"focus-core"));var now=new DateTime(2028,1,20,10,0,0,DateTimeKind.Utc);
  store.Value.tasks.Add(new WorkTask{id="focus-task",title="Travail",project="Travail",status="todo",priority="Normale",due="2028-01-31",repeat="monthly",repeatDay=31,notes="Notes conservées"});
  store.Value.modes.Add(new LaunchMode{id="focus-mode",name="Travail"});store.Save();
  FocusClock.Start(store,"focus-task","focus-mode",5,now);Check(store.Value.tasks[0].status=="doing"&&FocusClock.Remaining(store.Value.focus,now.AddSeconds(100))==200,"Départ et décompte");
  string id=store.Value.focus.id;bool rejected=false;try{FocusClock.Start(store,"","",1,now);}catch(InvalidOperationException){rejected=true;}Check(rejected&&store.Value.focus.id==id,"Deux sessions simultanées");
  FocusClock.Pause(store,now.AddSeconds(100));Check(store.Value.focus.state=="paused"&&FocusClock.Remaining(store.Value.focus,now.AddHours(3))==200,"Pause");
  store=new Store(store.Root);Check(store.Value.focus.state=="paused"&&store.Value.focus.remainingSeconds==200,"Rechargement en pause");
  var resume=now.AddHours(3);FocusClock.Resume(store,resume);Check(FocusClock.Display(store.Value.focus,resume)=="03:20"&&!FocusClock.Tick(store,resume.AddSeconds(199)),"Reprise");
  Check(FocusClock.Tick(store,resume.AddSeconds(200))&&!FocusClock.Tick(store,resume.AddSeconds(201))&&store.Value.tasks[0].status=="doing","Fin unique sans terminer la tâche");
  FocusClock.MarkNotified(store);Check(new Store(store.Root).Value.focus.notified,"Notification persistée");
  TaskBoard.Move(store,"focus-task","done");TaskBoard.Move(store,"focus-task","done");Check(store.Value.tasks.Count==2&&store.Value.tasks.Last().due=="2028-02-29"&&store.Value.tasks[0].notes=="Notes conservées","Validation récurrente sans doublon");
  FocusClock.Start(store,"","",2,now);FocusClock.Cancel(store,now.AddSeconds(10));Check(store.Value.focus.state=="cancelled"&&store.Value.focus.remainingSeconds==110,"Arrêt session");
  foreach(int minutes in new[]{0,241}){rejected=false;try{FocusClock.Start(store,"","",minutes,now);}catch(InvalidDataException){rejected=true;}Check(rejected,"Durée invalide");}
  rejected=false;try{FocusClock.Start(store,"missing","",25,now);}catch(InvalidDataException){rejected=true;}Check(rejected,"Tâche introuvable");rejected=false;try{FocusClock.Start(store,"","missing",25,now);}catch(InvalidDataException){rejected=true;}Check(rejected,"Mode introuvable");
  FocusClock.Start(store,"","",1,now);store=new Store(store.Root);Check(FocusClock.Remaining(store.Value.focus,now.AddSeconds(30))==30&&FocusClock.Tick(store,now.AddMinutes(2)),"Reprise après fermeture ou veille");
  FocusClock.Start(store,"","",45,now);string exported=Path.Combine(folder,"focus-export.json");store.Export(exported);store.Value.focus=new FocusSessionData();store.Import(exported);Check(store.Value.focus.state=="running"&&store.Value.focus.durationSeconds==2700,"Import session");
  string archive=Path.Combine(folder,"focus.mespace");FullBackup.Export(store,archive);var target=new Store(Path.Combine(folder,"focus-restored"));using(var plan=FullBackup.Prepare(target,archive))FullBackup.Commit(target,plan);Check(new Store(target.Root).Value.focus.endUtcTicks==store.Value.focus.endUtcTicks,"Restauration session");
  FocusClock.Cancel(store,now);string taskId=store.Value.tasks.Last().id;string state=Path.Combine(store.Root,"state.json");File.Delete(state);Directory.CreateDirectory(state);rejected=false;try{FocusClock.Start(store,taskId,"",25,now);}catch(IOException){rejected=true;}catch(UnauthorizedAccessException){rejected=true;}Directory.Delete(state);Check(rejected&&store.Value.focus.state=="cancelled"&&store.Value.tasks.Last().status=="todo","Retour après échec sauvegarde");store.Save();
  var legacy=new Data();legacy.focus=null;Store.Validate(legacy);Check(legacy.focus.state=="idle","Migration concentration");
  var bad=new Data();bad.focus.state="broken";rejected=false;try{Store.Validate(bad);}catch(InvalidDataException){rejected=true;}Check(rejected,"Session invalide importée");
 }
}
internal sealed partial class ModernWorkspace {
 DependencyObject FocusUiRoot(){return (DependencyObject)((ScrollViewer)content.Content).Content;}
 void ClickFocus(string label){var button=LogicalControls<Button>(FocusUiRoot()).FirstOrDefault(b=>Convert.ToString(b.Content)==label);if(button==null)throw new Exception("Bouton concentration absent : "+label);button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));}
 internal void VerifyFocusUi(string folder){
  Go("Concentration");var task=LogicalControls<ComboBox>(FocusUiRoot()).First();task.SelectedIndex=1;var duration=LogicalControls<TextBox>(FocusUiRoot()).First();duration.Text="0";ClickFocus("Démarrer la concentration");if(store.Value.focus.state!="idle")throw new Exception("Durée nulle UI");
  duration.Text="1";ClickFocus("Démarrer la concentration");if(store.Value.focus.state!="running"||store.Value.tasks[0].status!="doing")throw new Exception("Démarrage UI");
  Preview(Path.Combine(folder,"Concentration.png"));ClickFocus("Pause");if(store.Value.focus.state!="paused")throw new Exception("Pause UI");Preview(Path.Combine(folder,"Concentration-pause.png"));
  ClickFocus("Reprendre");if(store.Value.focus.state!="running")throw new Exception("Reprise UI");var widget=new DesktopWidget(owner,store);try{widget.Render(Path.Combine(folder,"Widget-concentration.png"));}finally{widget.Shutdown();}
  FocusClock.Tick(store,new DateTime(store.Value.focus.endUtcTicks,DateTimeKind.Utc).AddSeconds(1));FocusChanged();
  if(store.Value.tasks[0].status=="done")throw new Exception("Tâche terminée automatiquement");Preview(Path.Combine(folder,"Concentration-fin.png"));ClickFocus("Marquer la tâche terminée");if(new Store(store.Root).Value.tasks[0].status!="done")throw new Exception("Validation tâche UI");
  modeSession=new ModeLaunchSession(new LaunchMode{id="focus-destination",name="Mode de test",pageKind="project",pageId="Travail"});modeKeepPage=true;modeTimer=new System.Windows.Threading.DispatcherTimer();FinishMode();if(selected!="Concentration")throw new Exception("Le mode quitte la page concentration");
 }
}
internal sealed partial class MainForm {
 internal void VerifyFocusUi(string folder){
  rendering=true;store.Value.tasks.Add(new WorkTask{id="focus-ui",title="Préparer les documents de travail",project="Travail",status="todo",priority="Haute",due=DateTime.Today.ToString("yyyy-MM-dd"),notes=""});store.Save();modern.VerifyFocusUi(folder);
  if(uiTicker!=null)uiTicker.Dispose();modern.CancelMusic();modern.CancelModeLaunch();Stop();if(speech!=null)speech.Dispose();player.Close();
 }
}
internal static partial class Program {
 internal static int FocusTest(string folder){Root=Path.GetFullPath(folder);Directory.CreateDirectory(Root);System.Windows.Forms.Application.EnableVisualStyles();try{FocusAcceptance.Core(Root);using(var form=new MainForm(false))form.VerifyFocusUi(Root);File.WriteAllText(Path.Combine(Root,"FOCUS_PASS.txt"),"PASS: countdown, start and task doing state, single active session, pause/resume and persistence, finish once, explicit task completion and recurrence deduplication, cancellation, invalid duration/task/mode, restore after close/sleep, JSON and complete backup, save-failure rollback and migration; actual WPF start/invalid input/pause/resume/complete controls and rendered page/widget previews. No external application launch or real Windows notification performed.");return 0;}catch(Exception ex){File.WriteAllText(Path.Combine(Root,"FOCUS_FAIL.txt"),ex.ToString());return 1;}}
}
