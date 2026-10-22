using System;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
public sealed class FocusSessionData {
 public string id="",state="idle",taskId="",taskTitle="",modeId="";
 public int activeSeconds;public int durationSeconds=1500,remainingSeconds=1500;public long endUtcTicks,startedUtcTicks,endedUtcTicks;public bool notified;
}
internal static class FocusClock {
 internal static bool Active(FocusSessionData focus){return focus.state=="running"||focus.state=="paused";}
 internal static void Validate(Data data){
  if(data.focus==null)data.focus=new FocusSessionData();var f=data.focus;
  if(!new[]{"idle","running","paused","finished","cancelled"}.Contains(f.state)||f.durationSeconds<60||f.durationSeconds>14400||f.activeSeconds<0||f.activeSeconds>f.durationSeconds||f.remainingSeconds<0||f.remainingSeconds>f.durationSeconds||(f.id??"").Length>100||(f.taskId??"").Length>100||(f.taskTitle??"").Length>500||(f.modeId??"").Length>100||f.endUtcTicks<0||f.endUtcTicks>DateTime.MaxValue.Ticks||f.startedUtcTicks<0||f.startedUtcTicks>DateTime.MaxValue.Ticks||f.endedUtcTicks<0||f.endedUtcTicks>DateTime.MaxValue.Ticks||(f.state!="idle"&&String.IsNullOrWhiteSpace(f.id))||(f.state=="running"&&f.endUtcTicks==0))throw new InvalidDataException("Session de concentration invalide.");
 }
 internal static int Remaining(FocusSessionData f,DateTime utc){return f.state=="running"?(int)Math.Min(f.durationSeconds,Math.Max(0,Math.Ceiling((f.endUtcTicks-utc.ToUniversalTime().Ticks)/(double)TimeSpan.TicksPerSecond))):f.state=="paused"?f.remainingSeconds:0;}
 internal static string Display(FocusSessionData f,DateTime utc){int seconds=Remaining(f,utc);return (seconds/60).ToString("00")+":"+(seconds%60).ToString("00");}
 static void Change(Store store,Action change){var json=new JavaScriptSerializer{MaxJsonLength=8*1024*1024};var before=json.Deserialize<Data>(json.Serialize(store.Value));try{change();store.Save();}catch{store.Value=before;throw;}}
 internal static void Start(Store store,string taskId,string modeId,int minutes,DateTime utc){
  if(Active(store.Value.focus))throw new InvalidOperationException("Une session est déjà en cours.");
  if(minutes<1||minutes>240)throw new InvalidDataException("Choisissez une durée de 1 à 240 minutes.");
  var task=String.IsNullOrEmpty(taskId)?null:store.Value.tasks.FirstOrDefault(t=>t.id==taskId&&t.status!="done");if(!String.IsNullOrEmpty(taskId)&&task==null)throw new InvalidDataException("La tâche n'est plus disponible.");
  if(!String.IsNullOrEmpty(modeId)&&!store.Value.modes.Any(m=>m.id==modeId))throw new InvalidDataException("Le mode de lancement n'est plus disponible.");
  if(task!=null)TaskDependencies.Ensure(store.Value,task,"doing");var now=utc.ToUniversalTime();Change(store,delegate{store.Value.focus=new FocusSessionData{id=Guid.NewGuid().ToString(),state="running",taskId=taskId??"",taskTitle=task==null?"Session libre":task.title,modeId=modeId??"",durationSeconds=minutes*60,remainingSeconds=minutes*60,startedUtcTicks=now.Ticks,endUtcTicks=now.AddMinutes(minutes).Ticks};if(task!=null)task.status="doing";});
 }
 internal static bool Tick(Store store,DateTime utc){
  if(store.Value.focus.state!="running"||Remaining(store.Value.focus,utc)>0)return false;
  Change(store,delegate{store.Value.focus.activeSeconds=Math.Min(store.Value.focus.durationSeconds,store.Value.focus.activeSeconds+store.Value.focus.remainingSeconds);store.Value.focus.state="finished";store.Value.focus.remainingSeconds=0;store.Value.focus.endedUtcTicks=utc.ToUniversalTime().Ticks;PersonalWorkflows.RecordFocus(store.Value,utc,true);});return true;
 }
 internal static void Pause(Store store,DateTime utc){if(Tick(store,utc))return;if(store.Value.focus.state!="running")return;int remaining=Remaining(store.Value.focus,utc);Change(store,delegate{store.Value.focus.activeSeconds+=Math.Max(0,store.Value.focus.remainingSeconds-remaining);store.Value.focus.state="paused";store.Value.focus.remainingSeconds=remaining;store.Value.focus.endUtcTicks=0;});}
 internal static void Resume(Store store,DateTime utc){if(store.Value.focus.state!="paused")return;Change(store,delegate{store.Value.focus.state="running";store.Value.focus.endUtcTicks=utc.ToUniversalTime().AddSeconds(store.Value.focus.remainingSeconds).Ticks;});}
 internal static void Cancel(Store store,DateTime utc){if(!Active(store.Value.focus))return;Change(store,delegate{int remaining=Remaining(store.Value.focus,utc);store.Value.focus.activeSeconds+=Math.Max(0,store.Value.focus.remainingSeconds-remaining);store.Value.focus.remainingSeconds=remaining;store.Value.focus.state="cancelled";store.Value.focus.endedUtcTicks=utc.ToUniversalTime().Ticks;PersonalWorkflows.RecordFocus(store.Value,utc,false);});}
 internal static void MarkNotified(Store store){if(store.Value.focus.notified)return;Change(store,delegate{store.Value.focus.notified=true;});}
}
