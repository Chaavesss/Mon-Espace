using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Control;
internal sealed class DesktopMusicState {
 internal string Album="";internal byte[] Artwork;
 internal string Title="",Artist="",Diagnostic="",Message="Spotify n'est pas détecté. Lancez un morceau dans Spotify, puis réduisez sa fenêtre.";
 internal bool Available,Playing,CanToggle,CanNext,CanPrevious;
}
internal sealed class DesktopMusic {
 string artworkKey="";byte[] artwork;DateTime artworkUntil=DateTime.MinValue;
 async Task<byte[]> ReadArtwork(Windows.Media.Control.GlobalSystemMediaTransportControlsSessionMediaProperties media){
  string key=media.Title+"\n"+media.Artist+"\n"+media.AlbumTitle;
  if(key==artworkKey&&DateTime.UtcNow<artworkUntil)return artwork;
  artworkKey=key;artwork=null;artworkUntil=DateTime.UtcNow.AddSeconds(15);
  try{
   if(media.Thumbnail==null)return null;
   using(var stream=await Await(media.Thumbnail.OpenReadAsync())){
    if(stream.Size==0||stream.Size>4*1024*1024)return null;
    using(var reader=new Windows.Storage.Streams.DataReader(stream)){
     uint count=await Await(reader.LoadAsync((uint)stream.Size));
     if(count==0||count>4*1024*1024)return null;
     var bytes=new byte[count];reader.ReadBytes(bytes);artwork=bytes;
    }
   }
  }catch{artwork=null;}return artwork;
 }
 static Task<T> Await<T>(Windows.Foundation.IAsyncOperation<T> operation){return Task.Run(delegate{try{var until=DateTime.UtcNow.AddSeconds(5);while(operation.Status==Windows.Foundation.AsyncStatus.Started){if(DateTime.UtcNow>until){operation.Cancel();throw new TimeoutException("Spotify ne répond pas.");}System.Threading.Thread.Sleep(20);}if(operation.Status!=Windows.Foundation.AsyncStatus.Completed)throw new InvalidOperationException("La commande multimédia a échoué.",operation.ErrorCode);return operation.GetResults();}finally{operation.Close();}});}
 internal static bool IsSpotify(string id){return !String.IsNullOrEmpty(id)&&id.IndexOf("spotify",StringComparison.OrdinalIgnoreCase)>=0;}
 async Task<GlobalSystemMediaTransportControlsSession> Session(){
  var manager=await Await(GlobalSystemMediaTransportControlsSessionManager.RequestAsync());
  return manager.GetSessions().FirstOrDefault(s=>IsSpotify(s.SourceAppUserModelId));
 }
 internal async Task<DesktopMusicState> Read(){
  var state=new DesktopMusicState();
  try{var session=await Session();if(session==null){artwork=null;artworkKey="";return state;}var media=await Await(session.TryGetMediaPropertiesAsync());var playback=session.GetPlaybackInfo();var controls=playback.Controls;
   state.Available=true;state.Title=media.Title;state.Artist=media.Artist;state.Playing=playback.PlaybackStatus==GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
   state.CanToggle=controls.IsPlayPauseToggleEnabled;state.CanNext=controls.IsNextEnabled;state.CanPrevious=controls.IsPreviousEnabled;state.Message=state.Playing?"Lecture Spotify en cours":"Spotify en pause";
   state.Album=media.AlbumTitle;state.Artwork=await ReadArtwork(media);
  }catch(Exception ex){state.Diagnostic=ex.ToString();state.Message="Les commandes multimédias Windows sont indisponibles dans cette session. Vous pouvez utiliser la connexion Spotify.";}return state;
 }
 internal async Task Command(string action){
  var session=await Session();if(session==null)throw new InvalidOperationException("Lancez d'abord Spotify et un morceau sur ce PC.");
  var controls=session.GetPlaybackInfo().Controls;bool result=false;
  if(action=="toggle"&&controls.IsPlayPauseToggleEnabled)result=await Await(session.TryTogglePlayPauseAsync());
  else if(action=="next"&&controls.IsNextEnabled)result=await Await(session.TrySkipNextAsync());
  else if(action=="previous"&&controls.IsPreviousEnabled)result=await Await(session.TrySkipPreviousAsync());
  if(!result)throw new InvalidOperationException("Spotify ne permet pas cette commande pour le morceau en cours.");
 }
}
