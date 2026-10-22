using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
public sealed class DayDashboardOptions {
 public string date="",activeModeId="";public bool chosen;public int version;
 public List<string> priorityIds=new List<string>();
 public List<ResumeEntry> recent=new List<ResumeEntry>();
 public List<HomeBlock> homeBeforeRoutine;
}
public sealed class ResumeEntry {public string kind="",id="",visited="";}
internal static class DayDashboard {
 internal static List<HomeBlock> CloneLayout(IEnumerable<HomeBlock> blocks){return blocks==null?null:blocks.Select(b=>new HomeBlock{id=b.id,column=b.column,size=b.size,visible=b.visible,height=b.height}).ToList();}
 internal static void Validate(Data data){
  if(data.settings.Day==null)data.settings.Day=new DayDashboardOptions();var day=data.settings.Day;if(day.version<1){var resume=data.settings.HomeBlocks.FirstOrDefault(b=>b.id=="resume");if(resume!=null){resume.visible=true;data.settings.HomeBlocks.Remove(resume);data.settings.HomeBlocks.Insert(Math.Min(2,data.settings.HomeBlocks.Count),resume);}day.version=1;}
  if(day.date!=DateTime.Today.ToString("yyyy-MM-dd")){day.date=DateTime.Today.ToString("yyyy-MM-dd");day.chosen=false;day.priorityIds=new List<string>();}
  day.priorityIds=(day.priorityIds??new List<string>()).Where(id=>id!=null&&data.tasks.Any(t=>t!=null&&t.id==id)).Distinct().Take(3).ToList();
  day.recent=(day.recent??new List<ResumeEntry>()).Where(r=>r!=null&&new[]{"note","project","document"}.Contains(r.kind)&&!String.IsNullOrWhiteSpace(r.id)&&r.id.Length<=4096).GroupBy(r=>r.kind+"\n"+r.id,StringComparer.OrdinalIgnoreCase).Select(g=>g.First()).Take(8).ToList();
  foreach(var mode in data.modes){mode.homePreset=mode.homePreset??"";mode.homeProject=mode.homeProject??"";if(!new[]{"","Travail","Dofus","Perso","Personnalisé"}.Contains(mode.homePreset)||mode.homeProject.Length>80)throw new InvalidDataException("Accueil du mode invalide.");if(mode.homeLayout!=null)NormalizeLayout(mode.homeLayout);}
  if(day.homeBeforeRoutine!=null)NormalizeLayout(day.homeBeforeRoutine);
  if(!String.IsNullOrEmpty(day.activeModeId)&&!data.modes.Any(m=>m.id==day.activeModeId)){if(day.homeBeforeRoutine!=null)data.settings.HomeBlocks=CloneLayout(day.homeBeforeRoutine);day.activeModeId="";day.homeBeforeRoutine=null;}
 }
 internal static void NormalizeLayout(List<HomeBlock> blocks){if(blocks.Count>30)throw new InvalidDataException("Disposition d’accueil trop grande.");var s=new Settings{HomeBlocks=blocks};HomeLayout.Validate(s);blocks.Clear();blocks.AddRange(s.HomeBlocks);}
 internal static string Scope(Data data){var mode=data.modes.FirstOrDefault(m=>m.id==data.settings.Day.activeModeId);return mode==null?"":mode.homeProject;}
 internal static IEnumerable<WorkTask> Tasks(Data data){string scope=Scope(data);return data.tasks.Where(t=>String.IsNullOrEmpty(scope)||t.project==scope);}
 internal static List<WorkTask> Priorities(Data data,DateTime today){
  var d=data.settings.Day;string key=today.ToString("yyyy-MM-dd");
  if(d.date==key&&d.chosen)return d.priorityIds.Select(id=>Tasks(data).FirstOrDefault(t=>t.id==id)).Where(t=>t!=null).ToList();
  return Tasks(data).Where(t=>t.status!="done"&&TaskDependencies.Blocking(data,t).Count==0).OrderBy(t=>!String.IsNullOrEmpty(t.due)&&String.CompareOrdinal(t.due,key)<0?0:t.due==key?1:t.priority=="Haute"?2:t.status=="doing"?3:4).ThenBy(t=>String.IsNullOrEmpty(t.due)?"9999":t.due).ThenBy(t=>t.title).Take(3).ToList();
 }
 internal static void Choose(Data data,IEnumerable<string> ids,DateTime today){var chosen=ids.Distinct().ToList();if(chosen.Count>3||chosen.Any(id=>!Tasks(data).Any(t=>t.id==id)))throw new InvalidDataException("Choisissez jusqu’à trois tâches existantes.");var d=data.settings.Day;d.date=today.ToString("yyyy-MM-dd");d.chosen=true;d.priorityIds=chosen;}
 internal static bool Exists(Data data,ResumeEntry entry){if(entry==null)return false;if(entry.kind=="note")return data.pages.Any(n=>n.id==entry.id&&!n.trashed);if(entry.kind=="project")return data.settings.Projects.Contains(entry.id)||data.projectData.Any(p=>p.name==entry.id)||data.tasks.Any(t=>t.project==entry.id)||data.pages.Any(n=>n.project==entry.id&&!n.trashed);if(entry.kind=="document"){try{return Path.IsPathRooted(entry.id)&&File.Exists(entry.id);}catch{return false;}}return false;}
 internal static string Title(Data data,ResumeEntry entry){if(entry.kind=="note"){var note=data.pages.FirstOrDefault(n=>n.id==entry.id);return note==null?"Note indisponible":note.title;}return entry.kind=="document"?Path.GetFileName(entry.id):entry.id;}
 internal static void Record(Data data,string kind,string id,DateTime now){
  if(data.settings.PrivacyMode)return;var entry=new ResumeEntry{kind=kind,id=id,visited=now.ToString("o")};if(!Exists(data,entry))return;var recent=data.settings.Day.recent;recent.RemoveAll(r=>r.kind==kind&&String.Equals(r.id,id,StringComparison.OrdinalIgnoreCase));recent.Insert(0,entry);if(recent.Count>8)recent.RemoveRange(8,recent.Count-8);
 }
 internal static List<HomeBlock> Preset(string name){
  string[] visible=name=="Dofus"?new[]{"resume","modes","links","projects","notes","today"}:name=="Perso"?new[]{"priorities","today","resume","finance","inbox","habits","links"}:new[]{"priorities","today","resume","projects","inbox","links"};
  var all=HomeLayout.Defaults();var result=new List<HomeBlock>();for(int i=0;i<visible.Length;i++){var b=all.First(x=>x.id==visible[i]);b.visible=true;b.column=i%2;result.Add(b);}foreach(var b in all.Where(b=>!visible.Contains(b.id))){b.visible=false;result.Add(b);}return result;
 }
 internal static void Activate(Data data,LaunchMode mode){if(String.IsNullOrEmpty(mode.homePreset))return;var day=data.settings.Day;if(String.IsNullOrEmpty(day.activeModeId))day.homeBeforeRoutine=CloneLayout(data.settings.HomeBlocks);data.settings.HomeBlocks=CloneLayout(mode.homeLayout??Preset(mode.homePreset));day.activeModeId=mode.id;}
 internal static void RememberLayout(Data data){var mode=data.modes.FirstOrDefault(m=>m.id==data.settings.Day.activeModeId);if(mode!=null)mode.homeLayout=CloneLayout(data.settings.HomeBlocks);}
 internal static void ExitRoutine(Data data){var d=data.settings.Day;if(d.homeBeforeRoutine!=null)data.settings.HomeBlocks=CloneLayout(d.homeBeforeRoutine);d.activeModeId="";d.homeBeforeRoutine=null;}
 internal static void Move(Data data,string source,string target,int column){var list=data.settings.HomeBlocks;var from=list.FirstOrDefault(b=>b.id==source);var to=list.FirstOrDefault(b=>b.id==target);if(from==null||to==null)return;if(from!=to){list.Remove(from);list.Insert(list.IndexOf(to),from);}from.column=Math.Max(0,Math.Min(2,column));RememberLayout(data);}
}
