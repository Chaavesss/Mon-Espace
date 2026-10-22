using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;
public sealed class InboxItem {public string id="",title="",body="",created="",resultKind="",resultId="";public bool processed;}
public sealed class Habit {public string id="",name="",created="";public bool archived;public List<int> days=new List<int>{0,1,2,3,4,5,6};public List<string> checks=new List<string>();}
public sealed class FocusRecord {public string id="",taskId="",title="",project="";public long startedUtcTicks,endedUtcTicks;public int seconds;public bool completed;}
internal sealed class PersonalWeek {internal int Tasks,Projects,HabitChecks;internal long FocusSeconds;internal List<WorkTask> Completed=new List<WorkTask>();internal List<FocusRecord> Sessions=new List<FocusRecord>();}
internal static class PersonalWorkflows {
 internal static void Change(Store store,Action action){var j=new JavaScriptSerializer{MaxJsonLength=8*1024*1024};var before=j.Deserialize<Data>(j.Serialize(store.Value));try{action();store.Save();}catch{store.Value=before;throw;}}
 internal static void Validate(Data d){
  if(d.inbox==null)d.inbox=new List<InboxItem>();if(d.habits==null)d.habits=new List<Habit>();if(d.focusHistory==null)d.focusHistory=new List<FocusRecord>();
  if(d.inbox.Count>5000||d.habits.Count>100||d.focusHistory.Count>2000)throw new InvalidDataException("Trop d'idées, d'habitudes ou de sessions.");
  var ids=new HashSet<string>();foreach(var item in d.inbox)if(item==null||String.IsNullOrWhiteSpace(item.id)||item.id.Length>100||!ids.Add(item.id)||String.IsNullOrWhiteSpace(item.title)||item.title.Length>200||(item.body??"").Length>10000||!Finance.DateOk(item.created)||!new[]{"","task","note","project"}.Contains(item.resultKind)||(item.resultId??"").Length>100)throw new InvalidDataException("Élément de boîte de réception invalide.");
  ids.Clear();foreach(var h in d.habits){if(h==null||String.IsNullOrWhiteSpace(h.id)||h.id.Length>100||!ids.Add(h.id)||String.IsNullOrWhiteSpace(h.name)||h.name.Length>120||!Finance.DateOk(h.created)||h.days==null||h.days.Count==0||h.days.Any(day=>day<0||day>6)||h.days.Distinct().Count()!=h.days.Count||h.checks==null||h.checks.Count>10000||h.checks.Distinct().Count()!=h.checks.Count||h.checks.Any(date=>!Finance.DateOk(date)||String.CompareOrdinal(date,h.created)<0))throw new InvalidDataException("Habitude invalide.");}
  ids.Clear();foreach(var record in d.focusHistory)if(record==null||String.IsNullOrWhiteSpace(record.id)||record.id.Length>100||!ids.Add(record.id)||(record.taskId??"").Length>100||(record.title??"").Length>500||(record.project??"").Length>80||record.startedUtcTicks<=0||record.endedUtcTicks<record.startedUtcTicks||record.endedUtcTicks>DateTime.MaxValue.Ticks||record.seconds<0||record.seconds>14400)throw new InvalidDataException("Historique de concentration invalide.");
  foreach(var task in d.tasks){if(task==null)continue;task.completedOn=task.completedOn??"";if(task.completedOn!=""&&!Finance.DateOk(task.completedOn))throw new InvalidDataException("Date de fin de tâche invalide.");}
 }
 internal static bool CaptureInbox(Store store,string title,string body,Func<bool> save){
  if(String.IsNullOrWhiteSpace(title))return false;var item=new InboxItem{id=Guid.NewGuid().ToString(),title=title.Trim(),body=body??"",created=DateTime.Today.ToString("yyyy-MM-dd")};store.Value.inbox.Add(item);
  var block=store.Value.settings.HomeBlocks.FirstOrDefault(b=>b.id=="inbox");bool visible=block!=null&&block.visible;if(block!=null)block.visible=true;
  try{if(save())return true;}catch{store.Value.inbox.Remove(item);if(block!=null)block.visible=visible;throw;}store.Value.inbox.Remove(item);if(block!=null)block.visible=visible;return false;
 }
 internal static void ConvertInbox(Store store,string id,string kind,string name,string project){
  Change(store,delegate{
   var item=store.Value.inbox.FirstOrDefault(i=>i.id==id);if(item==null||item.processed)throw new InvalidOperationException("Cette idée a déjà été classée ou supprimée.");
   name=(name??"").Trim();if(String.IsNullOrWhiteSpace(name))throw new InvalidDataException("Indiquez un titre.");
   string resultId=Guid.NewGuid().ToString();
   if(kind=="task"){if(String.IsNullOrWhiteSpace(project))throw new InvalidDataException("Choisissez un projet.");store.Value.tasks.Add(new WorkTask{id=resultId,title=name,project=project,priority="Normale",status="todo",due="",notes=item.body});}
   else if(kind=="note")store.Value.pages.Add(new NotePage{id=resultId,title=name,project=project??"",body=item.body,updated=DateTime.Now.ToString("o")});
   else if(kind=="project"){if(name.Length>80||store.Value.settings.Projects.Concat(store.Value.projectData.Select(p=>p.name)).Concat(store.Value.tasks.Select(t=>t.project)).Concat(store.Value.pages.Select(n=>n.project)).Any(p=>String.Equals(p,name,StringComparison.OrdinalIgnoreCase)))throw new InvalidDataException("Choisissez un nouveau nom de projet (80 caractères maximum).");store.Value.settings.Projects.Add(name);store.Value.projectData.Add(new ProjectSpace{name=name,description=item.body});resultId=name;}
   else throw new InvalidDataException("Classement inconnu.");
   item.processed=true;item.resultKind=kind;item.resultId=resultId;
  });
 }
 internal static int Weekday(DateTime day){return ((int)day.DayOfWeek+6)%7;}
 internal static bool Scheduled(Habit habit,DateTime day){return day.Date>=Finance.Day(habit.created)&&habit.days.Contains(Weekday(day));}
 internal static void ToggleHabit(Store store,string id,DateTime day,DateTime today){
  var habit=store.Value.habits.FirstOrDefault(h=>h.id==id);if(habit==null||habit.archived||day.Date>today.Date||!Scheduled(habit,day))throw new InvalidDataException("Ce jour ne peut pas être coché.");
  Change(store,delegate{string date=day.ToString("yyyy-MM-dd");if(!habit.checks.Remove(date))habit.checks.Add(date);});
 }
 internal static int Streak(Habit habit,DateTime today){
  int streak=0;var day=today.Date;if(Scheduled(habit,day)&&!habit.checks.Contains(day.ToString("yyyy-MM-dd")))day=day.AddDays(-1);
  for(int i=0;i<10000&&day>=Finance.Day(habit.created);i++,day=day.AddDays(-1)){if(!Scheduled(habit,day))continue;if(!habit.checks.Contains(day.ToString("yyyy-MM-dd")))break;streak++;}return streak;
 }
 internal static DateTime Monday(DateTime date){return date.Date.AddDays(-Weekday(date));}
 internal static void RecordFocus(Data data,DateTime utc,bool completed){
  var f=data.focus;if(data.focusHistory.Any(r=>r.id==f.id))return;
  var task=data.tasks.FirstOrDefault(t=>t.id==f.taskId);data.focusHistory.Add(new FocusRecord{id=f.id,taskId=f.taskId,title=f.taskTitle,project=task==null?"":task.project,startedUtcTicks=f.startedUtcTicks,endedUtcTicks=Math.Max(f.startedUtcTicks,utc.ToUniversalTime().Ticks),seconds=f.activeSeconds,completed=completed});
  if(data.focusHistory.Count>2000)data.focusHistory.RemoveAt(0);
 }
 internal static PersonalWeek Week(Data data,DateTime date){
  var start=Monday(date);var end=start.AddDays(7);var result=new PersonalWeek();
  result.Completed=data.tasks.Where(t=>t.status=="done"&&!String.IsNullOrEmpty(t.completedOn)&&Finance.Day(t.completedOn)>=start&&Finance.Day(t.completedOn)<end).ToList();result.Tasks=result.Completed.Count;
  result.Sessions=data.focusHistory.Where(r=>new DateTime(r.endedUtcTicks,DateTimeKind.Utc).ToLocalTime().Date>=start&&new DateTime(r.endedUtcTicks,DateTimeKind.Utc).ToLocalTime().Date<end).ToList();result.FocusSeconds=result.Sessions.Sum(r=>(long)r.seconds);
  result.Projects=result.Completed.Select(t=>t.project).Concat(result.Sessions.Select(r=>r.project)).Where(p=>!String.IsNullOrWhiteSpace(p)).Distinct().Count();result.HabitChecks=data.habits.Sum(h=>h.checks.Count(s=>Finance.Day(s)>=start&&Finance.Day(s)<end));return result;
 }
}
