using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
internal sealed partial class CompanionWindow {
 QuestInstructionIndex questIndex;
 bool compactInstructions {get{return store.Value.instructions.compact;}}
 bool autoCopyDestination {get{return store.Value.instructions.autoCopy;}}
 readonly Func<QuestInstructionIndex> instructionIndexLoader;
 internal Task<QuestInstructionIndex> PendingInstructionIndex {get{return instructionLoading;}}
 readonly QuestInstructionIndex loadingIndex=new QuestInstructionIndex();
 Task<QuestInstructionIndex> instructionLoading;int instructionGeneration;
 internal void InvalidateInstructionIndex(){questIndex=null;instructionGeneration++;}
 internal QuestInstructionIndex InstructionIndex {get{if(questIndex!=null)return questIndex;if(testing&&instructionIndexLoader==null)return questIndex=QuestInstructionIndex.Load(store);if(instructionLoading==null)BeginInstructionIndex();return loadingIndex;}}
 void BeginInstructionIndex(){int generation=instructionGeneration;var pending=Task.Run(instructionIndexLoader??(()=>QuestInstructionIndex.Load(store)));instructionLoading=pending;pending.ContinueWith(result=>{var error=result.Exception;if(closing||Dispatcher.HasShutdownStarted)return;try{Dispatcher.BeginInvoke(new Action(()=>{if(closing)return;if(Object.ReferenceEquals(instructionLoading,pending))instructionLoading=null;if(generation==instructionGeneration){questIndex=result.Status==TaskStatus.RanToCompletion?result.Result:new QuestInstructionIndex{Unreadable=1};if(error!=null)message="Certains guides n’ont pas pu être chargés.";}RefreshView();}));}catch(InvalidOperationException){}});}

 async Task<QuestInstructionIndex> ReadyInstructionIndex(){while(!closing&&questIndex==null){if(instructionLoading==null)BeginInstructionIndex();await instructionLoading;await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Background);}return questIndex??loadingIndex;}
 internal void InstructionPanel(StackPanel body,TougliStep step){
 if(step==null||String.IsNullOrWhiteSpace(step.QuestId))return;
 var index=InstructionIndex;var merged=index.Merge(step);var content=new StackPanel();body.Children.Add(content);
 if(merged.Items.Count==0){PersonalInstructions(content,String.IsNullOrWhiteSpace(step.Details)?"Consultez le guide détaillé pour cette quête.":step.Details,step.Key,step.Coordinates);if(questIndex==null&&!testing||preparingInstructions)content.Children.Add(Text("Les consignes complémentaires se préparent automatiquement…",11,true));else {content.Children.Add(Text("Aucune consigne complémentaire fiable n’est encore reliée à cette quête.",11,true));content.Children.Add(Button("Actualiser la bibliothèque",()=>{catalogueAttempted=false;LoadDefaultLibrary();}));}return;}
 string cursorKey=QuestInstructionIndex.CursorKey(store.Value,step,"unified");int position=merged.Position(store.Value,step,cursorKey);var current=merged.Items[position];
 content.Children.Add(CompanionTheme.Label("CONSIGNES · "+(position+1)+" / "+merged.Items.Count));
 if(merged.ConflictingOrder)content.Children.Add(Text("Les références donnent des ordres différents. Vérifiez les consignes avant de vous déplacer.",12,true));
 if(!merged.ConflictingOrder&&merged.UncertainOrder)content.Children.Add(Text("Certains compléments n’ont pas d’ordre commun vérifiable. Consultez les références si nécessaire.",11,true));
 PersonalInstructions(content,current.Details,current.Key,current.Coordinates);
 var controls=new WrapPanel();var previous=Button("← Instruction",()=>SetInstructionCursor(cursorKey,merged.Items[position-1].Key));previous.IsEnabled=position>0;controls.Children.Add(previous);var next=Button("Instruction →",()=>SetInstructionCursor(cursorKey,merged.Items[position+1].Key));next.IsEnabled=position+1<merged.Items.Count;controls.Children.Add(next);content.Children.Add(controls);
 if(!String.IsNullOrWhiteSpace(step.Details)&&!merged.Items.Any(x=>UnifiedQuestInstructions.TextKey(x.Details)==UnifiedQuestInstructions.TextKey(step.Details))){var context=new StackPanel();context.Children.Add(InstructionText(step.Details));content.Children.Add(CompanionTheme.Fold("Préparation et contexte du parcours",context));}
 var references=new StackPanel();foreach(var reference in merged.References[current.Key].GroupBy(x=>x.Url).Select(g=>g.First())){references.Children.Add(Text(reference.Credit,10,true));references.Children.Add(Button("Consulter la référence",()=>Open(reference.Url)));}content.Children.Add(CompanionTheme.Fold("Références et crédits",references));
 }
 void SetInstructionCursor(string key,string value,string coordinates=""){var before=displayedDestination;Safe(()=>PersonalWorkflows.Change(store,()=>{var cursors=store.Value.instructions.cursors;if(!cursors.ContainsKey(key)&&cursors.Count>=5000)cursors.Remove(cursors.Keys.First());cursors[key]=value;}));CopyChangedDestination(before);}
}
