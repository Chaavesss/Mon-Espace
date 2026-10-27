using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
public sealed class CompletedQuest {public string character="",gameId="",name="";}
internal static class QuestProgress {
 internal static void Validate(Data d){if(d.completedQuests==null)d.completedQuests=new List<CompletedQuest>();if(d.completedQuests.Count>60000||d.completedQuests.Any(q=>q==null||String.IsNullOrEmpty(q.character)||q.character.Length>100||(q.gameId??"").Length>12||String.IsNullOrEmpty(q.name)||q.name.Length>500))throw new InvalidDataException("Progression commune des quêtes invalide.");}
 internal static bool Same(CompletedQuest a,string id,string name){return !String.IsNullOrEmpty(a.gameId)&&!String.IsNullOrEmpty(id)?a.gameId==id:a.name==QuestInstructionIndex.NameKey(name);}
 internal static bool Done(Data d,string character,string id,string name){return QuestCaptureImport.Done(d,character,id,name)||d.completedQuests.Any(q=>q.character==character&&Same(q,id,name));}
 static void Remember(Data d,string character,string id,string name){if(!d.completedQuests.Any(q=>q.character==character&&Same(q,id,name)))d.completedQuests.Add(new CompletedQuest{character=character,gameId=id??"",name=QuestInstructionIndex.NameKey(name)});}
 internal static void ImportExisting(Store store){foreach(var p in store.Value.dofus.progress.ToList()){var c=store.Value.dofus.characters.FirstOrDefault(x=>x.id==p.characterId);if(c==null||GanymedeReader.IsGuide(p.guide))continue;try{var doc=Read(store,c,p.guide);if(doc==null)continue;foreach(var step in doc.Steps.Where(s=>!String.IsNullOrEmpty(s.QuestId)&&p.done.Contains(s.Key)))if(!QuestCaptureImport.Done(store.Value,c.id,step.QuestId,step.Title))Remember(store.Value,c.id,step.QuestId,step.Title);}catch{}}}
 internal static TougliDocument Read(Store store,DofusCharacter c,string slug){return GuideDocumentCache.Read(store,c,slug);}
 internal static void Apply(Store store,TougliDocument doc,DofusCharacter c){QuestCaptureImport.Apply(store.Value,doc,c);var matches=doc.Steps.Where(s=>!String.IsNullOrEmpty(s.QuestId)&&Done(store.Value,c.id,s.QuestId,s.Title)).ToList();if(matches.Count==0)return;var clone=new DofusCharacter{id=c.id,guide=doc.Slug};var p=DofusCompanion.Get(store.Value,clone);foreach(var step in matches)if(!p.done.Contains(step.Key))p.done.Add(step.Key);}
 static void SyncVisited(Store store,DofusCharacter c,string id,string name,bool complete){foreach(var p in store.Value.dofus.progress.Where(x=>x.characterId==c.id).ToList()){if(GanymedeReader.IsGuide(p.guide))continue;try{var doc=Read(store,c,p.guide);if(doc==null)continue;foreach(var step in doc.Steps.Where(s=>!String.IsNullOrEmpty(s.QuestId)&&Same(new CompletedQuest{gameId=id??"",name=QuestInstructionIndex.NameKey(name)},s.QuestId,s.Title))){if(complete&&!p.done.Contains(step.Key))p.done.Add(step.Key);if(!complete)p.done.Remove(step.Key);}}catch{}}}
 internal static void Set(Store store,string id,string name,bool complete){var c=DofusCompanion.Character(store.Value);if(c==null)return;PersonalWorkflows.Change(store,()=>{if(complete){QuestCaptureImport.Keep(store.Value,c.id,id,name);Remember(store.Value,c.id,id,name);}else {QuestCaptureImport.Remove(store.Value,c.id,id,name);}if(!complete)store.Value.completedQuests.RemoveAll(q=>q.character==c.id&&Same(q,id,name));SyncVisited(store,c,id,name,complete);});}
 // Completion follows unfinished steps; arrows retain literal previous/next navigation.
 internal static void Navigate(Store store,TougliDocument doc,int direction,bool complete){
  var c=DofusCompanion.Character(store.Value);var step=DofusCompanion.Current(store.Value,doc);
  if(c==null||step==null)return;
  if(!complete){DofusCompanion.Navigate(store,doc,direction,false);return;}
  if(!step.CanComplete)throw new InvalidOperationException("Configurez le personnage avant de valider la quête.");
  PersonalWorkflows.Change(store,()=>{
   if(String.IsNullOrEmpty(step.QuestId)){QuestCaptureImport.Manual(store.Value,c.id,doc,step,true);}
   else {QuestCaptureImport.Keep(store.Value,c.id,step.QuestId,step.Title);Remember(store.Value,c.id,step.QuestId,step.Title);SyncVisited(store,c,step.QuestId,step.Title,true);Apply(store,doc,c);}
   var p=DofusCompanion.Get(store.Value,c);if(!p.done.Contains(step.Key))p.done.Add(step.Key);
   int index=doc.Steps.IndexOf(step);
   var next=direction>0?doc.Steps.Skip(index+1).Concat(doc.Steps.Take(index+1)).FirstOrDefault(s=>!p.done.Contains(s.Key)):null;
   p.cursor=next!=null?next.Key:doc.Steps[Math.Max(0,Math.Min(doc.Steps.Count-1,index+direction))].Key;
  });
 }
 internal static void NavigationTest(string folder){
  var store=new Store(Path.Combine(folder,"navigation-unfinished"));
  var c=new DofusCharacter{id="navigation",name="Test",guide="navigation-test"};
  store.Value.dofus.characters.Add(c);store.Value.dofus.active=c.id;
  var doc=new TougliDocument{Slug=c.guide,Steps=Enumerable.Range(0,17).Select(i=>new TougliStep{Key="step-"+i,QuestId=i==0?null:(100+i).ToString(),Title="Quest "+i}).ToList()};
  var p=DofusCompanion.Get(store.Value,c);p.done.AddRange(doc.Steps.Skip(1).Select(s=>s.Key));p.cursor=doc.Steps.Last().Key;
  // Screenshot regression: 16/17, last quest already complete, first note omitted.
  Navigate(store,doc,1,true);
  if(p.cursor!="step-0"||p.done.Count!=16)throw new Exception("Final quest did not return to missing step");
  Navigate(store,doc,1,true);
  if(p.done.Count!=17)throw new Exception("Final missing note not completed");
  var reopened=new Store(store.Root);
  if(DofusCompanion.Find(reopened.Value,c).done.Count!=17)throw new Exception("Completion not persisted");
  p.done.Clear();p.done.Add("step-1");p.done.Add("step-2");p.cursor="step-0";
  Navigate(store,doc,1,true);if(p.cursor!="step-3")throw new Exception("Completed steps not skipped");
  Navigate(store,doc,-1,false);if(p.cursor!="step-2")throw new Exception("Previous arrow must remain literal");
  Navigate(store,doc,1,false);if(p.cursor!="step-3"||p.done.Contains("step-3"))throw new Exception("Arrow validated a step");
  p.cursor="step-16";doc.Steps[16].CanComplete=false;
  bool blocked=false;try{Navigate(store,doc,1,true);}catch(InvalidOperationException){blocked=true;}
  if(!blocked||p.done.Contains("step-16"))throw new Exception("Unconfigured step completed");
  File.WriteAllText(Path.Combine(folder,"NAVIGATION_UNFINISHED_PASS.txt"),"PASS: 16/17 final quest regression, missing note, persistence, skip completed steps, literal arrows, blocked steps.");
 }
 internal static void Toggle(Store store,TougliDocument doc){var c=DofusCompanion.Character(store.Value);var step=DofusCompanion.Current(store.Value,doc);if(c==null||step==null)return;if(String.IsNullOrEmpty(step.QuestId)){DofusCompanion.Toggle(store,doc);return;}if(!step.CanComplete)throw new InvalidOperationException("Configurez le personnage avant de valider la quête.");bool complete=!Done(store.Value,c.id,step.QuestId,step.Title);Set(store,step.QuestId,step.Title,complete);if(complete)Apply(store,doc,c);}
 internal static int Count(Store store,DofusCharacter c,string slug){try{var doc=Read(store,c,slug);var p=store.Value.dofus.progress.FirstOrDefault(x=>x.characterId==c.id&&x.guide==slug);if(doc==null)return p==null?0:p.done.Count;return doc.Steps.Count(s=>p!=null&&p.done.Contains(s.Key)||!String.IsNullOrEmpty(s.QuestId)&&Done(store.Value,c.id,s.QuestId,s.Title));}catch{return 0;}}
 internal static void Test(string folder){NavigationTest(folder);var store=new Store(Path.Combine(folder,"shared-quests"));var a=new DofusCharacter{id="a",name="A",guide="parcours-exemple"};var b=new DofusCharacter{id="b",name="B",guide="parcours-exemple"};store.Value.dofus.characters.AddRange(new[]{a,b});store.Value.dofus.active=a.id;TougliReader.Cache(store,a.guide,StandaloneTests.Sample);var doc=TougliReader.Parse(StandaloneTests.Sample,a);var quest=doc.Steps.First(s=>!String.IsNullOrEmpty(s.QuestId));Set(store,quest.QuestId,quest.Title,true);Apply(store,doc,a);if(!DofusCompanion.Find(store.Value,a).done.Contains(quest.Key)||Done(store.Value,b.id,quest.QuestId,quest.Title))throw new Exception("Quête partagée entre personnages");var other=new TougliDocument{Slug="autre-guide",Steps=new List<TougliStep>{new TougliStep{Key="different-step",QuestId=quest.QuestId,Title=quest.Title},new TougliStep{Key="unrelated-note",Title=quest.Title}}};Apply(store,other,a);var progress=DofusCompanion.Get(store.Value,new DofusCharacter{id=a.id,guide=other.Slug});if(!progress.done.Contains("different-step")||progress.done.Contains("unrelated-note"))throw new Exception("Partage de quête hors parcours");if(Done(store.Value,a.id,"999999",quest.Title)||!Done(store.Value,a.id,"",quest.Title))throw new Exception("Identité des quêtes");Set(store,quest.QuestId,quest.Title,false);if(Done(store.Value,a.id,quest.QuestId,quest.Title)||DofusCompanion.Find(store.Value,a).done.Contains(quest.Key))throw new Exception("Annulation commune");var capture=new CaptureQuestChoice{Id=quest.QuestId,Name=quest.Title};var catalog=new CaptureCatalogue{Quests=new List<CaptureQuestChoice>{capture},Documents=new List<TougliDocument>{doc}};QuestCaptureImport.Commit(store,a.id,new List<CaptureQuestChoice>{capture},catalog);var reopened=new Store(store.Root);QuestCaptureImport.Undo(reopened,a.id);Apply(reopened,doc,a);if(Done(reopened.Value,a.id,quest.QuestId,quest.Title)||DofusCompanion.Find(reopened.Value,a).done.Contains(quest.Key))throw new Exception("Import devenu permanent après redémarrage");}
}
internal sealed partial class CompanionWindow {
 internal void SharedQuestPanel(System.Windows.Controls.StackPanel body,TougliStep step){var c=DofusCompanion.Character(store.Value);if(c==null||!GanymedeReader.IsGuide(c.guide))return;var parts=step.Key.Split('/');if(parts.Length<2)return;var quests=InstructionIndex.Items.Where(q=>q.GuideSlug==c.guide&&q.StepId==parts[1]).GroupBy(q=>q.GameQuestId+"/"+QuestInstructionIndex.NameKey(q.QuestName)).Select(g=>g.First()).ToList();if(quests.Count==0)return;var panel=new System.Windows.Controls.StackPanel();panel.Children.Add(Text("Validez une quête ici lorsqu’elle est entièrement terminée. Cette validation est commune aux parcours de ce personnage.",11,true));foreach(var quest in quests){bool done=QuestProgress.Done(store.Value,c.id,quest.GameQuestId,quest.QuestName);panel.Children.Add(Text((done?"✓  ":"")+quest.QuestName,12));panel.Children.Add(Button(done?"Annuler la validation":"Valider cette quête",()=>Safe(()=>QuestProgress.Set(store,quest.GameQuestId,quest.QuestName,!done))));}body.Children.Add(CompanionTheme.Fold("Quêtes associées · progression commune",panel));}
}
