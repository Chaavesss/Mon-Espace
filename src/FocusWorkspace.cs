using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
internal sealed class FocusChoice {public string Id="",Label="";public override string ToString(){return Label;}}
internal sealed partial class ModernWorkspace {
 TextBlock focusClockLabel,focusTaskLabel,focusError;
 void FocusPage(StackPanel root){
  var f=store.Value.focus;var top=new WrapPanel{Margin=new Thickness(0,0,0,18)};top.Children.Add(Btn("Afficher le widget",delegate{owner.ShowDesktopWidget();},false,true));top.Children.Add(Btn("Mes tâches",delegate{Go("Mes tâches");},false,true));root.Children.Add(top);
  focusError=T("",12,Accent);root.Children.Add(focusError);
  if(FocusClock.Active(f)||f.state=="finished"){
   var body=new StackPanel();body.Children.Add(T(f.state=="paused"?"SESSION EN PAUSE":f.state=="finished"?"SESSION TERMINÉE":"CONCENTRATION EN COURS",10,Accent));
   focusTaskLabel=T(f.taskTitle,22,Ink,new Thickness(0,13,0,8));body.Children.Add(focusTaskLabel);focusClockLabel=T("",54,Ink,new Thickness(0,5,0,18));focusClockLabel.FontFamily=new System.Windows.Media.FontFamily("Georgia");body.Children.Add(focusClockLabel);
   var actions=new WrapPanel();if(FocusClock.Active(f)){actions.Children.Add(Btn(f.state=="paused"?"Reprendre":"Pause",delegate{FocusAction(delegate{owner.ToggleFocusSession();});},true));actions.Children.Add(Btn("Arrêter la session",delegate{FocusAction(delegate{owner.CancelFocusSession();});},false,true));}
   else{
    var task=store.Value.tasks.FirstOrDefault(t=>t.id==f.taskId);if(task!=null&&task.status!="done")actions.Children.Add(Btn("Marquer la tâche terminée",delegate{FocusAction(delegate{TaskBoard.Move(store,f.taskId,"done");Refresh();});},true));
    body.Children.Add(T(task!=null&&task.status=="done"?"Cette tâche est terminée.":"La session est finie. La tâche reste en cours jusqu'à votre validation.",12,Muted,new Thickness(0,0,0,14)));
   }body.Children.Add(actions);root.Children.Add(Card("Une chose à la fois",body));UpdateFocusClock();
   if(FocusClock.Active(f)){root.Children.Add(T("Le minuteur continue si la fenêtre principale est réduite. Les outils déjà ouverts restent disponibles pendant une pause.",12,Muted,new Thickness(0,18,0,0)));if(modeSession!=null&&modeKeepPage)root.Children.Add(Btn("Arrêter les ouvertures du mode",delegate{CancelModeLaunch();Refresh();},false,true));return;}
  }
  var setup=new StackPanel();var tasks=new List<FocusChoice>{new FocusChoice{Label="Session libre, sans tâche"}};tasks.AddRange(store.Value.tasks.Where(t=>t.status!="done").OrderBy(t=>String.IsNullOrEmpty(t.due)?"9999":t.due).Select(t=>new FocusChoice{Id=t.id,Label=t.title+" · "+t.project}));
  var taskCombo=new ComboBox{ItemsSource=tasks,SelectedItem=tasks.FirstOrDefault(t=>t.Id==f.taskId)??tasks[0]};Field(setup,"Sur quoi souhaitez-vous avancer ?",taskCombo);
  var duration=Combo(new[]{"25 minutes","45 minutes","60 minutes","90 minutes"},"25 minutes");Field(setup,"Durée",duration);var custom=new TextBox{MaxLength=3};Field(setup,"Ou une durée personnalisée, de 1 à 240 minutes (facultatif)",custom);
  var modes=new List<FocusChoice>{new FocusChoice{Label="Ne pas lancer d'outils"}};modes.AddRange(store.Value.modes.Select(m=>new FocusChoice{Id=m.id,Label=m.name}));var modeCombo=new ComboBox{ItemsSource=modes,SelectedIndex=0};Field(setup,"Mode de lancement facultatif",modeCombo);
  setup.Children.Add(T("La tâche choisie passe en cours. À la fin, vous déciderez si elle est terminée. Le mode choisi ouvre ses outils une seule fois, au démarrage de cette session.",12,Muted,new Thickness(0,0,0,16)));
  var start=Btn("Démarrer la concentration",delegate{int minutes;if(String.IsNullOrWhiteSpace(custom.Text))minutes=new[]{25,45,60,90}[duration.SelectedIndex];else if(!Int32.TryParse(custom.Text,out minutes)){focusError.Text="Indiquez une durée entière de 1 à 240 minutes.";return;}
   var selectedTask=(FocusChoice)taskCombo.SelectedItem;var selectedMode=(FocusChoice)modeCombo.SelectedItem;
   FocusAction(delegate{if(modeSession!=null)throw new InvalidOperationException("Attendez la fin des ouvertures du mode actuel.");FocusClock.Start(store,selectedTask.Id,selectedMode.Id,minutes,DateTime.UtcNow);if(!String.IsNullOrEmpty(selectedMode.Id))RunMode(store.Value.modes.First(m=>m.id==selectedMode.Id),true);else Refresh();});
  },true);setup.Children.Add(start);var card=Card(f.state=="finished"?"Préparer une nouvelle session":"Votre prochaine session",setup);card.Margin=new Thickness(0,18,0,0);root.Children.Add(card);
 }
 void FocusAction(Action action){try{action();}catch(Exception ex){if(focusError!=null)focusError.Text=ex.Message;}}
 internal void UpdateFocusClock(){if(selected=="Concentration"&&focusClockLabel!=null)focusClockLabel.Text=store.Value.focus.state=="finished"?"C'est fait.":FocusClock.Display(store.Value.focus,DateTime.UtcNow);}
 internal void FocusChanged(){if(selected=="Concentration")Refresh();else UpdateFocusClock();}
 internal void OpenFocusPage(){Go("Concentration");}
}
internal sealed partial class MainForm {
 string focusNotifiedId="";bool focusBalloon;
 internal void ToggleFocusSession(){if(store.Value.focus.state=="running")FocusClock.Pause(store,DateTime.UtcNow);else if(store.Value.focus.state=="paused")FocusClock.Resume(store,DateTime.UtcNow);modern.FocusChanged();}
 internal void CancelFocusSession(){FocusClock.Cancel(store,DateTime.UtcNow);modern.CancelFocusOwnedMode();modern.FocusChanged();}
 internal void OpenFocusFromWidget(){RestoreWorkspace();modern.OpenFocusPage();}
 void CheckFocus(){
  if(rendering||closing)return;try{
   if(FocusClock.Tick(store,DateTime.UtcNow))modern.FocusChanged();else modern.UpdateFocusClock();
   var f=store.Value.focus;if(f.state!="finished"||f.notified||focusNotifiedId==f.id)return;focusNotifiedId=f.id;
   if(store.Value.settings.Notifications&&!store.Value.settings.PrivacyMode&&remindersIcon!=null){try{remindersIcon.ShowBalloonTip(8000,"Session de concentration terminée","Votre session est finie. Retrouvez votre tâche dans Mon espace.",System.Windows.Forms.ToolTipIcon.Info);focusBalloon=true;}catch{notice.Text="Session de concentration terminée.";}
   }FocusClock.MarkNotified(store);
  }catch(Exception ex){notice.Text="Session de concentration non enregistrée : "+ex.Message;}
 }
}
internal sealed partial class ModernWorkspace {internal void CancelFocusOwnedMode(){if(modeSession!=null&&modeKeepPage)CancelModeLaunch();}}
