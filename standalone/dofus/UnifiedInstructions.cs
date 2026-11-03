using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
internal sealed class UnifiedQuestInstructions {
 internal readonly List<QuestInstruction> Items=new List<QuestInstruction>();
 internal readonly Dictionary<string,List<QuestInstruction>> References=new Dictionary<string,List<QuestInstruction>>();
 internal bool ConflictingOrder,UncertainOrder;
 internal static string TextKey(string value){return Regex.Replace((value??"").Trim(),@"\s+"," ");}
 internal static UnifiedQuestInstructions Build(List<QuestInstruction> matches,string questIdentity=""){
 var result=new UnifiedQuestInstructions();var nodes=new Dictionary<string,QuestInstruction>();var rank=new Dictionary<string,int>();var edges=new Dictionary<string,HashSet<string>>();var degree=new Dictionary<string,int>();
 foreach(var source in matches.GroupBy(x=>x.GuideSlug)){
 string previous=null;var occurrences=new Dictionary<string,int>();
 foreach(var entry in source.OrderBy(x=>x.Order)){
 string identity=TextKey(entry.Details)+"/"+(entry.Coordinates??"");int occurrence;occurrences.TryGetValue(identity,out occurrence);occurrences[identity]=occurrence+1;string key="u/"+WorkspaceNext.Hash(Encoding.UTF8.GetBytes(questIdentity+"/"+identity+"/"+occurrence));
 if(!nodes.ContainsKey(key)){nodes[key]=new QuestInstruction{Key=key,Details=entry.Details,Coordinates=entry.Coordinates,Map=entry.Map,Url=entry.Url,Credit=entry.Credit,GuideSlug=entry.GuideSlug,GuideTitle=entry.GuideTitle,QuestName=entry.QuestName,GameQuestId=entry.GameQuestId,StepId=entry.StepId,Order=entry.Order};rank[key]=rank.Count;edges[key]=new HashSet<string>();degree[key]=0;result.References[key]=new List<QuestInstruction>();}
 result.References[key].Add(entry);
 if(previous!=null&&previous!=key&&edges[previous].Add(key))degree[key]++;previous=key;
 }
 }
 var ready=new SortedSet<int>(rank.Where(p=>degree[p.Key]==0).Select(p=>p.Value));var keys=rank.OrderBy(p=>p.Value).Select(p=>p.Key).ToArray();
 while(ready.Count>0){if(ready.Count>1)result.UncertainOrder=true;int first=ready.Min;ready.Remove(first);string key=keys[first];result.Items.Add(nodes[key]);foreach(var next in edges[key])if(--degree[next]==0)ready.Add(rank[next]);}
 if(result.Items.Count<nodes.Count){result.ConflictingOrder=true;var added=new HashSet<string>(result.Items.Select(x=>x.Key));foreach(string key in keys)if(added.Add(key))result.Items.Add(nodes[key]);}
 return result;
 }
 internal int Position(Data data,TougliStep step,string cursorKey){string saved;if(data.instructions.cursors.TryGetValue(cursorKey,out saved))return Math.Max(0,Items.FindIndex(x=>x.Key==saved));
 string preferred;data.instructions.sources.TryGetValue(WorkspaceNext.Hash(Encoding.UTF8.GetBytes(step.QuestId+"/"+QuestInstructionIndex.NameKey(step.Title))),out preferred);
 foreach(var source in References.Values.SelectMany(x=>x).GroupBy(x=>x.GuideSlug).OrderByDescending(g=>g.Key==preferred)){string old;if(!data.instructions.cursors.TryGetValue(QuestInstructionIndex.CursorKey(data,step,source.Key),out old))continue;int position=Items.FindIndex(x=>References[x.Key].Any(r=>r.Key==old));if(position>=0)return position;}return 0;
 }
}
