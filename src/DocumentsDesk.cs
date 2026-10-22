using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
internal static class WinAwait {
 internal static Task<T> Get<T>(Windows.Foundation.IAsyncOperation<T> operation){return Task.Run(delegate{try{var until=DateTime.UtcNow.AddSeconds(30);while(operation.Status==Windows.Foundation.AsyncStatus.Started){if(DateTime.UtcNow>until){operation.Cancel();throw new TimeoutException("Le document met trop de temps à répondre.");}System.Threading.Thread.Sleep(15);}if(operation.Status!=Windows.Foundation.AsyncStatus.Completed)throw new InvalidOperationException("Ouverture du document impossible.",operation.ErrorCode);return operation.GetResults();}finally{operation.Close();}});}
 internal static Task Done(Windows.Foundation.IAsyncAction action){return Task.Run(delegate{try{var until=DateTime.UtcNow.AddSeconds(30);while(action.Status==Windows.Foundation.AsyncStatus.Started){if(DateTime.UtcNow>until){action.Cancel();throw new TimeoutException("Le rendu met trop de temps à répondre.");}System.Threading.Thread.Sleep(15);}if(action.Status!=Windows.Foundation.AsyncStatus.Completed)throw new InvalidOperationException("Le PDF ne peut pas être affiché.",action.ErrorCode);}finally{action.Close();}});}
}
internal sealed class DocumentDraft {internal string Text,Hash;}
internal sealed partial class ModernWorkspace {
 readonly System.Collections.Generic.Dictionary<string,DocumentDraft> documentDrafts=new System.Collections.Generic.Dictionary<string,DocumentDraft>(StringComparer.OrdinalIgnoreCase);
 string openDocument="",sideDocument="";int pdfJobs;
 void ChooseLocalDocument(bool side=false){using(var picker=new System.Windows.Forms.OpenFileDialog{Title="Ouvrir dans Mon espace",Filter="Documents|*.pdf;*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.txt;*.md;*.csv;*.json;*.log;*.xml;*.html;*.css;*.js;*.cs;*.yml;*.yaml;*.ini|Tous les fichiers|*.*"})if(picker.ShowDialog()==System.Windows.Forms.DialogResult.OK){if(side){sideDocument=picker.FileName;Refresh();}else OpenDocument(picker.FileName);}}
 void OpenDocument(string file){openDocument=Path.GetFullPath(file);RecordResume("document",openDocument);selected="Documents locaux";openProject="";var recent=store.Value.settings.RecentDocuments;recent.RemoveAll(p=>String.Equals(p,openDocument,StringComparison.OrdinalIgnoreCase));recent.Insert(0,openDocument);if(recent.Count>12)recent.RemoveRange(12,recent.Count-12);owner.Save();Refresh();}
 void LocalDocumentsPage(StackPanel body){
  var tools=new WrapPanel{Margin=new Thickness(0,0,0,20)};tools.Children.Add(Btn("+ Ouvrir un fichier",delegate{ChooseLocalDocument();},true));tools.Children.Add(Btn("Google Drive",delegate{Go("Google Drive");},false,true));if(!String.IsNullOrEmpty(openDocument))tools.Children.Add(Btn("À droite",delegate{sideDocument=openDocument;store.Value.settings.Desk.enabled=true;store.Value.settings.Desk.panel="Document";owner.Save();Refresh();},false,true));body.Children.Add(tools);
  if(!String.IsNullOrEmpty(openDocument)){body.Children.Add(DocumentView(openDocument,680));}
  else{body.Children.Add(T("Vos documents, dans votre espace.",24));body.Children.Add(T("Consultez vos PDF et images, et modifiez vos textes sans quitter la fenêtre.",13,Muted,new Thickness(0,10,0,24)));}
  var recent=new StackPanel();foreach(var f in store.Value.settings.RecentDocuments){string file=f;var button=Btn(Path.GetFileName(file),delegate{OpenDocument(file);},false,true);button.HorizontalContentAlignment=HorizontalAlignment.Left;button.ToolTip=file;recent.Children.Add(button);}if(store.Value.settings.RecentDocuments.Count>0){var card=Card("Récemment ouverts",recent);card.Margin=new Thickness(0,20,0,0);body.Children.Add(card);}
 }
 UIElement DocumentView(string file,double height){
  var body=new StackPanel();try{
   var info=new FileInfo(file);if(!info.Exists)throw new FileNotFoundException("Ce fichier n’est plus disponible sur ce PC.");if(info.Length>50L*1024*1024)throw new InvalidDataException("L’aperçu est limité aux documents de 50 Mo.");string ext=info.Extension.ToLowerInvariant();
   if(ext==".pdf"){var panel=new Grid{MinHeight=height};var content=new StackPanel();panel.Children.Add(content);pdfJobs++;Dispatcher.BeginInvoke((Action)(async delegate{try{await LoadPdf(content,file,height);}finally{pdfJobs--;}}));return panel;}
   if(new[]{".png",".jpg",".jpeg",".gif",".bmp"}.Contains(ext)){var image=new BitmapImage();using(var input=File.OpenRead(file)){image.BeginInit();image.CacheOption=BitmapCacheOption.OnLoad;image.DecodePixelWidth=1800;image.StreamSource=input;image.EndInit();}image.Freeze();body.Children.Add(new Image{Source=image,Stretch=Stretch.Uniform,MaxHeight=height});return Box(body);}
   if(!WorkspaceNext.TextExtensions.Contains(ext))throw new InvalidDataException("Ce format n’a pas encore de lecteur intégré. Utilisez son application habituelle.");
   if(info.Length>2*1024*1024)throw new InvalidDataException("L’éditeur de texte est limité à 2 Mo.");
   byte[] original=File.ReadAllBytes(file);DocumentDraft draft;if(!documentDrafts.TryGetValue(file,out draft))draft=new DocumentDraft{Text=WorkspaceNext.DecodeText(original),Hash=WorkspaceNext.Hash(original)};string value=draft.Text;string originalHash=draft.Hash;var editor=new TextBox{Text=value,AcceptsReturn=true,AcceptsTab=true,TextWrapping=TextWrapping.NoWrap,HorizontalScrollBarVisibility=ScrollBarVisibility.Auto,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,Height=height,MaxLength=2000000,FontFamily=new FontFamily("Consolas")};editor.TextChanged+=delegate{draft.Text=editor.Text;draft.Hash=originalHash;documentDrafts[file]=draft;};body.Children.Add(editor);var message=T("Modifications enregistrées uniquement en cliquant sur Enregistrer.",11,Muted,new Thickness(0,10,0,10));body.Children.Add(message);
   body.Children.Add(Btn("Enregistrer le texte",delegate{try{WorkspaceNext.SaveText(file,editor.Text,originalHash);originalHash=WorkspaceNext.Hash(File.ReadAllBytes(file));documentDrafts.Remove(file);message.Text="Enregistré · copie précédente conservée à côté du fichier.";}catch(Exception ex){message.Text=ex.Message;}},true));body.Children.Add(Btn("Recharger le fichier",delegate{if(documentDrafts.ContainsKey(file)&&MessageBox.Show("Abandonner le brouillon et relire le fichier sur le disque ?","Mon espace",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;documentDrafts.Remove(file);Refresh();},false,true));return body;
  }catch(Exception ex){body.Children.Add(T(ex.Message,13,Muted,new Thickness(0,12,0,16)));body.Children.Add(Btn("Ouvrir dans l’application habituelle",delegate{Open(file);},false,true));return Box(body);}
 }
 async Task LoadPdf(StackPanel body,string file,double height){
  bool alive=true;body.Unloaded+=delegate{alive=false;};var label=T("Chargement du PDF…",12,Muted);body.Children.Add(label);
  try{var storage=await WinAwait.Get(StorageFile.GetFileFromPathAsync(Path.GetFullPath(file)));var pdf=await WinAwait.Get(PdfDocument.LoadFromFileAsync(storage));if(!alive)return;uint page=0;bool busy=false;var image=new Image{Stretch=Stretch.Uniform,MaxHeight=height,HorizontalAlignment=HorizontalAlignment.Center};var container=new ScrollViewer{Content=image,Height=height,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};var toolbar=new WrapPanel{Margin=new Thickness(0,0,0,12)};Button back=null,next=null;Func<Task> render=null;
   render=async delegate{if(busy||!alive)return;busy=true;back.IsEnabled=next.IsEnabled=false;label.Text="Page "+(page+1)+" / "+pdf.PageCount;try{using(var p=pdf.GetPage(page))using(var stream=new InMemoryRandomAccessStream()){await WinAwait.Done(p.RenderToStreamAsync(stream,new PdfPageRenderOptions{DestinationWidth=1400}));stream.Seek(0);using(var reader=new DataReader(stream.GetInputStreamAt(0))){uint count=await WinAwait.Get(reader.LoadAsync((uint)stream.Size));if(count>20*1024*1024)throw new InvalidDataException("Cette page est trop lourde.");var bytes=new byte[count];reader.ReadBytes(bytes);if(alive){var bmp=new BitmapImage();using(var memory=new MemoryStream(bytes)){bmp.BeginInit();bmp.CacheOption=BitmapCacheOption.OnLoad;bmp.StreamSource=memory;bmp.EndInit();}bmp.Freeze();image.Source=bmp;}}}}catch(Exception ex){if(alive)label.Text=ex.Message;}finally{busy=false;if(alive){back.IsEnabled=page>0;next.IsEnabled=page+1<pdf.PageCount;}}};
   back=Btn("‹",async delegate{if(page>0){page--;await render();}},false,true);next=Btn("›",async delegate{if(page+1<pdf.PageCount){page++;await render();}},false,true);body.Children.Clear();toolbar.Children.Add(back);toolbar.Children.Add(next);toolbar.Children.Add(Btn("Zoom +",delegate{image.MaxHeight=Math.Min(height*4,image.MaxHeight*1.25);},false,true));toolbar.Children.Add(Btn("Zoom −",delegate{image.MaxHeight=Math.Max(150,image.MaxHeight/1.25);},false,true));toolbar.Children.Add(label);body.Children.Add(toolbar);body.Children.Add(container);await render();
  }catch(Exception ex){if(alive)label.Text=ex.Message;}
 }
 bool ConfirmDocumentDrafts(){
  if(documentDrafts.Count==0)return true;var choice=MessageBox.Show("Des fichiers texte ont été modifiés. Enregistrer les modifications avant de quitter ?","Mon espace",MessageBoxButton.YesNoCancel);if(choice==MessageBoxResult.Cancel)return false;if(choice==MessageBoxResult.No){documentDrafts.Clear();return true;}
  try{foreach(var draft in documentDrafts.ToList()){WorkspaceNext.SaveText(draft.Key,draft.Value.Text,draft.Value.Hash);documentDrafts.Remove(draft.Key);}return true;}catch(Exception ex){MessageBox.Show(ex.Message,"Fichiers non enregistrés");return false;}
 }

}
