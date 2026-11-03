using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
internal sealed partial class CompanionWindow {
 CancellationTokenSource catalogueRefreshCancel;
 bool preparingInstructions;CancellationTokenSource instructionPreparationCancel;DateTime nextInstructionRetry;
 async void StartInstructionPreparation(bool retry=false){
 if(testing||closing||preparingInstructions||!retry&&DateTime.UtcNow<nextInstructionRetry)return;
 var guides=GuideLibrary.Read(store).Where(g=>GanymedeReader.IsGuide(g.Slug)).ToList();if(guides.Count==0||guides.All(g=>GuideDownloads.Available(store,g.Slug)))return;
 preparingInstructions=true;instructionPreparationCancel=new CancellationTokenSource();var token=instructionPreparationCancel.Token;int lastReady=-1;
 try{
 var result=await Task.Run(()=>GuideDownloads.Prepare(store,guides,async (guide,ct)=>GanymedeReader.Normalise(await DownloadAddress(GanymedeReader.Url(guide.Slug),ct).ConfigureAwait(false)),progress=>{
 if(closing||token.IsCancellationRequested||progress.Ready==lastReady)return;lastReady=progress.Ready;
 Dispatcher.BeginInvoke(new Action(()=>{if(!closing&&libraryNotice!=null)libraryNotice.Text="Consignes : "+progress.Ready+" / "+progress.Total+" références disponibles.";}));
 },token));
 if(closing)return;InvalidateInstructionIndex();await ReadyInstructionIndex();nextInstructionRetry=result.Failed>0?DateTime.UtcNow.AddMinutes(5):DateTime.MinValue;
 message=result.Failed==0?"Consignes réunies automatiquement dans vos parcours.":"Les consignes disponibles sont réunies. "+result.Failed+" références restent indisponibles ; une nouvelle tentative sera possible.";if(libraryNotice!=null)libraryNotice.Text=message;
 }catch(OperationCanceledException){}catch(Exception ex){nextInstructionRetry=DateTime.UtcNow.AddMinutes(5);if(!closing){message="Les parcours restent disponibles. Certaines consignes n’ont pas pu être préparées : "+ex.Message;if(libraryNotice!=null)libraryNotice.Text=message;}}
 finally{preparingInstructions=false;instructionPreparationCancel.Dispose();instructionPreparationCancel=null;if(!closing){RefreshView();StartInstructionPreparation();}}
 }
}
