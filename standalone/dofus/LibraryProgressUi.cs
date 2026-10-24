using System;
using System.Windows.Controls;
using System.Windows.Threading;
internal sealed partial class CompanionWindow {
 async void RefreshLibraryProgress(ListBox list,GuideChoice[] items,DofusCharacter character,int generation){try{foreach(var item in items){await System.Windows.Threading.Dispatcher.Yield(DispatcherPriority.Background);if(closing||generation!=libraryGeneration||!Object.ReferenceEquals(list.ItemsSource,items))return;int done=QuestProgress.Count(store,character,item.Slug);item.Completed=GuideCompletion.IsComplete(store,character,item.Slug);item.Detail=(GanymedeReader.IsGuide(item.Slug)?"Ganymède":"Tougli")+" · "+done+" étapes validées";list.Items.Refresh();}}catch(System.Threading.Tasks.TaskCanceledException){}}
}
