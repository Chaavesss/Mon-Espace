using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
internal sealed class DayAgendaRow {internal DateTime At;internal string Title,Source,Time;internal Action Open;}
internal sealed partial class ModernWorkspace {
 bool homeEditing;
 void ChangeDay(Action change){try{PersonalWorkflows.Change(store,change);Refresh();}catch(Exception ex){MessageBox.Show(ex.Message,"Mon espace");}}
 void RecordResume(string kind,string id){if(store.Value.settings.PrivacyMode)return;var recent=store.Value.settings.Day.recent;if(recent.Count>0&&recent[0].kind==kind&&recent[0].id==id)return;var before=recent.Select(r=>new ResumeEntry{kind=r.kind,id=r.id,visited=r.visited}).ToList();DayDashboard.Record(store.Value,kind,id,DateTime.Now);if(!owner.Save())store.Value.settings.Day.recent=before;}
 void RecordTabResume(){if(selected=="Mes projets"&&!String.IsNullOrEmpty(openProject))RecordResume("project",openProject);else if(selected=="Documents locaux"&&!String.IsNullOrEmpty(openDocument))RecordResume("document",openDocument);}
 void DayHomeBar(StackPanel root){EnhancedHomeTools(root);
  var row=new WrapPanel{Margin=new Thickness(0,0,0,18)};row.Children.Add(Btn("Choisir mes 3 priorités",ChooseDayPriorities,false,true));if(store.Value.settings.Day.recent.Any(r=>DayDashboard.Exists(store.Value,r)))row.Children.Add(Btn("Reprendre le dernier",ResumeLast,false,true));row.Children.Add(Btn("Mes modes",delegate{Go("Mes modes");},false,true));row.Children.Add(Btn("Boîte de réception · "+store.Value.inbox.Count(i=>!i.processed),delegate{Go("Boîte de réception");},false,true));row.Children.Add(Btn(homeEditing?"Terminer l’organisation":"Déplacer les blocs",delegate{homeEditing=!homeEditing;Refresh();},homeEditing));root.Children.Add(row);
  var mode=store.Value.modes.FirstOrDefault(m=>m.id==store.Value.settings.Day.activeModeId);
  if(mode!=null){var active=new DockPanel{Margin=new Thickness(0,0,0,18)};var leave=Btn("Revenir à mon accueil",delegate{ChangeDay(delegate{DayDashboard.ExitRoutine(store.Value);});},false,true);DockPanel.SetDock(leave,Dock.Right);active.Children.Add(leave);active.Children.Add(T("Mode · "+mode.name+(String.IsNullOrEmpty(mode.homeProject)?"":"  /  "+mode.homeProject),13,AccentText,new Thickness(0,9,8,0)));var modeCard=Box(active,new Thickness(12));modeCard.Margin=new Thickness(0,0,0,16);root.Children.Add(modeCard);}
  if(homeEditing)root.Children.Add(T("Glissez la poignée d’un bloc pour le déplacer. Déposez-le à gauche ou à droite ; maintenez Ctrl pour toute la largeur. Le coin inférieur droit règle sa hauteur. Les options du bloc permettent aussi de choisir sa largeur et de retrouver sa taille automatique.",12,Muted,new Thickness(0,0,0,18)));
 }
 StackPanel DayPrioritiesHome(){
  var body=new StackPanel();var priorities=DayDashboard.Priorities(store.Value,DateTime.Today);bool chosen=store.Value.settings.Day.chosen;
  body.Children.Add(T(chosen?priorities.Count(t=>t.status=="done")+" / "+priorities.Count+" terminées aujourd’hui":"Trois suggestions · à confirmer selon votre journée",11,Muted,new Thickness(0,0,0,12)));
  if(priorities.Count>0)body.Children.Add(TaskList(priorities,3));else body.Children.Add(T(chosen?"Aucune priorité choisie pour aujourd’hui.":"Aucune tâche à proposer pour le moment.",12,Muted));
  return body;
 }
 void ChooseDayPriorities(){
  StackPanel body,actions;var w=Dialog("Mes trois priorités du jour",out body,out actions,680);body.Children.Add(T("Choisissez jusqu’à trois tâches. Votre sélection reste la même aujourd’hui, même après avoir terminé une tâche ; elle sera renouvelée demain.",12,Muted,new Thickness(0,0,0,16)));
  var chosen=DayDashboard.Priorities(store.Value,DateTime.Today).Select(t=>t.id).ToList();var message=T("",11,Accent);body.Children.Add(message);var search=new TextBox{MaxLength=200};Field(body,"Rechercher une tâche ou un projet",search);var summary=T("",11,Muted,new Thickness(0,0,0,10));body.Children.Add(summary);var list=new StackPanel();body.Children.Add(list);
  Action draw=delegate{list.Children.Clear();var tasks=DayDashboard.Tasks(store.Value).Where(t=>t.status!="done"||chosen.Contains(t.id)).Where(t=>String.IsNullOrWhiteSpace(search.Text)||GlobalSearch.Normalize(t.title+" "+t.project).Contains(GlobalSearch.Normalize(search.Text))).OrderBy(t=>t.status=="done").ThenBy(t=>t.project).ThenBy(t=>t.title).ToList();summary.Text=chosen.Count+" / 3 choisies · "+tasks.Count+" résultat(s)"+(tasks.Count>100?" · affinez la recherche pour voir les autres":"");
   foreach(var task in tasks.Take(100)){var item=task;var check=new CheckBox{Content=task.title+"  ·  "+task.project+(task.status=="done"?"  ✓":""),IsChecked=chosen.Contains(task.id),Margin=new Thickness(0,7,0,7),Tag=task.id};check.Click+=delegate{if(check.IsChecked==true&&!chosen.Contains(item.id)){if(chosen.Count==3){check.IsChecked=false;message.Text="Trois priorités maximum. Retirez-en une avant d’en choisir une autre.";}else{chosen.Add(item.id);message.Text="";}}else if(check.IsChecked!=true)chosen.Remove(item.id);summary.Text=chosen.Count+" / 3 choisies";};list.Children.Add(check);}if(tasks.Count==0)list.Children.Add(T("Aucune tâche ne correspond. Vous pouvez en créer dans Mes tâches.",12,Muted));
  };search.TextChanged+=delegate{draw();};draw();
  actions.Children.Add(Btn("Suggestions automatiques",delegate{try{PersonalWorkflows.Change(store,delegate{store.Value.settings.Day.chosen=false;store.Value.settings.Day.priorityIds.Clear();});FinishFinanceDialog(w);}catch(Exception ex){message.Text=ex.Message;}},false,true));actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));actions.Children.Add(Btn("Enregistrer",delegate{try{PersonalWorkflows.Change(store,delegate{DayDashboard.Choose(store.Value,chosen,DateTime.Today);});FinishFinanceDialog(w);}catch(Exception ex){message.Text=ex.Message;}},true));ShowWindow(w);Refresh();
 }
 StackPanel ResumeHome(bool compact){
  var body=new StackPanel();var items=store.Value.settings.Day.recent.Where(r=>DayDashboard.Exists(store.Value,r)).Take(compact?1:3).ToList();
  if(items.Count==0)body.Children.Add(T("Ouvrez un projet, une note ou un document : vous pourrez le retrouver ici au prochain lancement.",12,Muted));
  foreach(var entry in items){var item=entry;var button=Btn(DayDashboard.Title(store.Value,item),delegate{OpenResume(item);},false,true);button.HorizontalContentAlignment=HorizontalAlignment.Left;body.Children.Add(button);DateTime visited;string when=DateTime.TryParse(item.visited,null,DateTimeStyles.RoundtripKind,out visited)?visited.ToLocalTime().ToString("dd/MM · HH:mm"):"";body.Children.Add(T((item.kind=="note"?"Note":item.kind=="project"?"Projet":"Document")+"  ·  "+when,10,Muted,new Thickness(0,0,0,12)));}
  if(items.Count>0)body.Children.Add(Btn("Reprendre le dernier",ResumeLast,true));if(store.Value.settings.Day.recent.Count>0)body.Children.Add(Btn("Effacer cet historique",delegate{if(MessageBox.Show("Effacer seulement la liste des éléments récents ? Vos contenus restent conservés.","Historique",MessageBoxButton.YesNo)==MessageBoxResult.Yes)ChangeDay(delegate{store.Value.settings.Day.recent.Clear();});},false,true));return body;
 }
 void ResumeLast(){var entry=store.Value.settings.Day.recent.FirstOrDefault(r=>DayDashboard.Exists(store.Value,r));if(entry==null){MessageBox.Show("Aucun contenu récent disponible sur ce PC.");return;}OpenResume(entry);}
 void OpenResume(ResumeEntry entry){if(!DayDashboard.Exists(store.Value,entry)){MessageBox.Show("Ce contenu n’est plus disponible. Vos autres éléments récents sont conservés.");Refresh();return;}if(entry.kind=="project")OpenProject(entry.id);else if(entry.kind=="note")ReadNote(store.Value.pages.First(n=>n.id==entry.id));else OpenDocument(entry.id);}
 CalendarEvent NextDayAppointment(DateTime now){
  var next=store.Value.events.Where(e=>Schedule.At(e.date,e.time)>=now).OrderBy(e=>e.date).ThenBy(e=>e.time).FirstOrDefault();DateTime best=next==null?DateTime.MaxValue:Schedule.At(next.date,next.time);
  if(CalendarSession()!=null&&calendarLoadedId==store.Value.settings.GoogleCalendarId)foreach(var item in calendarEvents){try{var start=GoogleCalendarClient.LocalStart(item);if(start<now||start>=best)continue;best=start;next=new CalendarEvent{title=item.summary,date=start.ToString("yyyy-MM-dd"),time=start.ToString("HH:mm")};}catch{}}return next;
 }
 int GoogleTodayCount(){if(CalendarSession()==null||calendarLoadedId!=store.Value.settings.GoogleCalendarId)return 0;int count=0;foreach(var item in calendarEvents)try{var start=GoogleCalendarClient.LocalStart(item);var end=GoogleCalendarClient.LocalStart(new GoogleCalendarEvent{start=item.end});if(start<DateTime.Today.AddDays(1)&&end>DateTime.Today)count++;}catch{}return count;}
 StackPanel DayTimeline(bool compact){
  var body=new StackPanel();var day=DateTime.Today;string key=day.ToString("yyyy-MM-dd");var active=DayDashboard.Tasks(store.Value).Where(t=>t.status!="done").ToList();var overdue=active.Where(t=>!String.IsNullOrEmpty(t.due)&&String.CompareOrdinal(t.due,key)<0).OrderBy(t=>t.due).ToList();
  if(overdue.Count>0){body.Children.Add(T(overdue.Count+" tâche(s) en retard",13,AccentText,new Thickness(0,0,0,8)));body.Children.Add(TaskList(overdue,compact?1:2));}
  var rows=new List<DayAgendaRow>();foreach(var appointment in store.Value.events.Where(e=>e.date==key)){var item=appointment;rows.Add(new DayAgendaRow{At=Schedule.At(item.date,item.time),Time=item.time,Title=item.title,Source="Agenda local",Open=delegate{EditAppointment(item,day);}});}
  var session=CalendarSession();bool loaded=session!=null&&calendarLoadedId==store.Value.settings.GoogleCalendarId&&day>=calendarFrom&&day<calendarUntil;
  if(loaded)foreach(var appointment in calendarEvents){try{var start=GoogleCalendarClient.LocalStart(appointment);var end=GoogleCalendarClient.LocalStart(new GoogleCalendarEvent{start=appointment.end});if(start>=day.AddDays(1)||end<=day)continue;var item=appointment;rows.Add(new DayAgendaRow{At=start<day?day:start,Time=!String.IsNullOrEmpty(item.start.date)?"Journée":start.ToString("HH:mm"),Title=item.summary,Source="Google Agenda",Open=delegate{EditGoogleEvent(item);}});}catch{}}
  foreach(var row in rows.OrderBy(r=>r.At).Take(compact?2:4)){var item=row;var button=Btn(row.Time+"  ·  "+row.Title,item.Open,false,true);button.HorizontalContentAlignment=HorizontalAlignment.Left;body.Children.Add(button);body.Children.Add(T(row.Source,10,Muted,new Thickness(0,0,0,10)));}
  if(rows.Count==0)body.Children.Add(T(loaded||session==null?"Aucun rendez-vous chargé pour aujourd’hui.":"Les rendez-vous Google ne sont pas encore chargés.",12,Muted,new Thickness(0,0,0,12)));
  body.Children.Add(T(active.Count(t=>t.due==key)+" tâche(s) à échéance aujourd’hui",11,Muted,new Thickness(0,7,0,12)));
  if(session!=null){body.Children.Add(T(loaded?"Google · dernière tentative à "+calendarAttempt.ToLocalTime().ToString("HH:mm"):"Google · actualisation nécessaire",10,Muted));var sync=Btn(googleBusy?"Actualisation…":"Actualiser Google",delegate{agendaDate=day;SyncGoogleCalendar();},false,true);sync.IsEnabled=!googleBusy;body.Children.Add(sync);if(!owner.IsPreview&&!googleBusy&&DateTime.UtcNow-calendarAttempt>TimeSpan.FromMinutes(2))Dispatcher.BeginInvoke((Action)delegate{agendaDate=day;SyncGoogleCalendar();});}
  body.Children.Add(Btn("+ Rendez-vous",delegate{EditAppointment(null,day);},false,true));return body;
 }
}
