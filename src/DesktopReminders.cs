using System;
using System.Linq;
using System.Windows.Forms;
internal sealed partial class MainForm {
 NotifyIcon remindersIcon;bool budgetReminderOnly;DateTime lastScheduleCheck=DateTime.MinValue,lastScheduleDay=DateTime.Today;
 void InitializeReminders(){if(remindersIcon!=null)return;remindersIcon=new NotifyIcon{Icon=Icon,Text="Mon espace · Rappels",Visible=true};var menu=new ContextMenuStrip();menu.Items.Add("Ouvrir Mon espace",null,delegate{RestoreWorkspace();});menu.Items.Add("Afficher le widget",null,delegate{ShowDesktopWidget();});menu.Items.Add("Capture rapide",null,delegate{ShowQuickCapture();});menu.Items.Add("Quitter",null,delegate{RequestQuit();});remindersIcon.ContextMenuStrip=menu;remindersIcon.DoubleClick+=delegate{RestoreWorkspace();};remindersIcon.BalloonTipClicked+=delegate{RestoreWorkspace();if(focusBalloon){focusBalloon=false;modern.OpenFocusPage();}else if(budgetReminderOnly)modern.OpenBudget();else modern.OpenReminders();};}
 void RestoreWorkspace(){ShowInTaskbar=true;if(WindowState==FormWindowState.Minimized)WindowState=FormWindowState.Normal;Show();Activate();SetForegroundWindow(Handle);}
 void DisposeReminders(){if(remindersIcon!=null){remindersIcon.Visible=false;remindersIcon.Dispose();remindersIcon=null;}}
 void CheckSchedule(){
  if(!rendering&&Program.ActivationSignal!=null&&Program.ActivationSignal.WaitOne(0))RestoreWorkspace();
  if(rendering||closing||remindersIcon==null||DateTime.UtcNow-lastScheduleCheck<TimeSpan.FromSeconds(15))return;
  lastScheduleCheck=DateTime.UtcNow;modern.CheckAutomaticBackup();modern.RefreshNoticeBadge();if(lastScheduleDay!=DateTime.Today&&Enabled){lastScheduleDay=DateTime.Today;modern.Refresh();}
  if(!store.Value.settings.Notifications||store.Value.settings.PrivacyMode)return;
  var due=Schedule.Due(store.Value,DateTime.Now);var budgetDue=BudgetExtras.Due(store.Value.budget,DateTime.Now);int count=due.Count+budgetDue.Count;if(count==0)return;
  var labels=due.Select(r=>r.Title).Concat(budgetDue.Select(r=>r.Occurrence.Rule.title)).ToList();
  string title=count==1?labels[0]:count+" rappels à consulter";
  string text=count==1?(due.Count==1?due[0].Text:"Budget · "+budgetDue[0].Occurrence.Date.ToString("dd/MM")+" · "+Finance.Money(budgetDue[0].Occurrence.Rule.cents)):String.Join("\n",labels.Take(3))+(count>3?"\n… et "+(count-3)+" autre(s)":"");
  try{remindersIcon.ShowBalloonTip(8000,title.Length>63?title.Substring(0,60)+"…":title,text.Length>250?text.Substring(0,247)+"…":text,ToolTipIcon.Info);focusBalloon=false;budgetReminderOnly=due.Count==0;foreach(var reminder in due)reminder.Mark();BudgetExtras.Mark(store.Value.budget,budgetDue);Save();}
  catch(Exception ex){notice.Text="Rappel Windows indisponible : "+ex.Message;}
 }
}
internal sealed partial class ModernWorkspace {internal void OpenReminders(){if(owner.Enabled)Go("Rappels");}}
