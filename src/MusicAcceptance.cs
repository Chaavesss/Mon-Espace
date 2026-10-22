using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
internal sealed class TestSpotifyStorage:SpotifyStorage {
 internal SpotifyConfig config=new SpotifyConfig{clientId="0123456789abcdef0123456789abcdef"};
 internal SpotifySession session=new SpotifySession{access_token="test-old",refresh_token="test-refresh",expiresTicks=DateTime.UtcNow.AddHours(1).Ticks};
 internal TestSpotifyStorage():base(""){}
 internal override SpotifyConfig Config{get{return config;}}
 internal override SpotifySession Session{get{return session;}}
 internal override void SaveConfig(SpotifyConfig value){config=value;session=null;}
 internal override void SaveSession(SpotifySession value){session=value;}
 internal override void Disconnect(){session=null;}
}
internal sealed class TestSpotifyHttp:HttpMessageHandler {
 internal Queue<HttpResponseMessage> Responses=new Queue<HttpResponseMessage>();internal List<string> Requests=new List<string>();
 protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){token.ThrowIfCancellationRequested();Requests.Add(request.Method+" "+request.RequestUri+" "+(request.Headers.Authorization==null?"":request.Headers.Authorization.ToString())+" "+(request.Content==null?"":await request.Content.ReadAsStringAsync()));if(Responses.Count==0)throw new Exception("Unexpected Spotify request");return Responses.Dequeue();}
}
internal static class MusicAcceptance {
 static HttpResponseMessage Reply(int code,string body){return new HttpResponseMessage((HttpStatusCode)code){Content=new StringContent(body)};}
 internal static async Task Api(){
  var storage=new TestSpotifyStorage();var handler=new TestSpotifyHttp();var client=new SpotifyClient(storage,()=>new HttpClient(handler,false));
  string auth=SpotifyClient.AuthUrl(storage.Config.clientId,"http://127.0.0.1:1234/","state","verifier");
  if(!auth.Contains("code_challenge_method=S256")||auth.Contains("client_secret")||!auth.Contains("playlist-read-private")||SpotifyClient.ValidClient("bad")||SpotifyClient.ValidUri("https://bad","track"))throw new Exception("Configuration OAuth");
  handler.Responses.Enqueue(Reply(401,"{}"));handler.Responses.Enqueue(Reply(200,"{\"access_token\":\"test-new\",\"expires_in\":3600}"));handler.Responses.Enqueue(Reply(200,"{\"items\":[{\"name\":\"Dofus\",\"uri\":\"spotify:playlist:1234567890123456789012\"}],\"next\":\"next-page\",\"total\":51}"));
  var playlists=await client.Playlists(0,CancellationToken.None);
  if(playlists.items.Count!=1||playlists.items[0].name!="Dofus"||handler.Requests.Count!=3||!handler.Requests[2].Contains("Bearer test-new")||!handler.Requests[1].Contains("grant_type=refresh_token")||storage.Session.refresh_token!="test-refresh")throw new Exception("Rafraîchissement de connexion");
  handler.Responses.Enqueue(Reply(200,"{\"items\":[{\"track\":null},{\"track\":{\"name\":\"Titre\",\"uri\":\"spotify:track:1234567890123456789012\",\"artists\":[{\"name\":\"Artiste\"}]}}],\"next\":null}"));
  var liked=await client.Liked(50,CancellationToken.None);if(liked.items.Count!=2||!handler.Requests.Last().Contains("offset=50"))throw new Exception("Pagination et titres");
  handler.Responses.Enqueue(Reply(204,""));await client.Play(playlists.items[0].uri,true,CancellationToken.None);if(!handler.Requests.Last().Contains("PUT")||!handler.Requests.Last().Contains("context_uri"))throw new Exception("Lecture playlist");
  handler.Responses.Enqueue(Reply(204,""));await client.Play(liked.items[1].track.uri,false,CancellationToken.None);if(!handler.Requests.Last().Contains("\"uris\""))throw new Exception("Lecture titre");
  foreach(string command in new[]{"play","pause","next","previous"}){handler.Responses.Enqueue(Reply(204,""));await client.Command(command,CancellationToken.None);}
  int count=handler.Requests.Count;bool bad=false;try{await client.Play("spotify:track:bad",false,CancellationToken.None);}catch(InvalidDataException){bad=true;}if(!bad||handler.Requests.Count!=count)throw new Exception("URI invalide");
  foreach(int status in new[]{403,404,429}){handler.Responses.Enqueue(Reply(status,"{}"));bad=false;try{await client.Playlists(0,CancellationToken.None);}catch(InvalidOperationException){bad=true;}if(!bad)throw new Exception("Erreur HTTP ignorée");}
  using(var cancelled=new CancellationTokenSource()){cancelled.Cancel();bad=false;try{await client.Playlists(0,cancelled.Token);}catch(OperationCanceledException){bad=true;}if(!bad)throw new Exception("Annulation HTTP");}
  if(!DesktopMusic.IsSpotify("Spotify.exe")||!DesktopMusic.IsSpotify("SpotifyAB.SpotifyMusic_xyz!Spotify")||DesktopMusic.IsSpotify("Opera.exe"))throw new Exception("Ciblage Spotify");
 }
}
internal sealed partial class ModernWorkspace {
 internal void VerifyMiniPlayerUi(string folder){
  var original=desktopState;var page=selected;bool privacy=store.Value.settings.PrivacyMode;
  try{
   desktopState=new DesktopMusicState{Available=true,Playing=true,CanToggle=true,CanNext=true,CanPrevious=true,Title="Morceau de test",Artist="Artiste",Artwork=ArtworkAcceptance.Demo()};Go("Aujourd’hui");
   if(Convert.ToString(miniToggle.Content)!="Pause"||!miniPrevious.IsEnabled||!miniNext.IsEnabled||!miniArtwork.HasImage)throw new Exception("Mini-lecteur sans état ou pochette");
   var transport=miniTransport;Go("Agenda");if(miniTransport!=transport||Convert.ToString(miniToggle.Content)!="Pause")throw new Exception("Commandes perdues lors de la navigation");
   desktopState.Playing=false;desktopState.CanNext=false;UpdateMiniPlayer();if(Convert.ToString(miniToggle.Content)!="Lecture"||miniNext.IsEnabled||!miniPrevious.IsEnabled)throw new Exception("Capacités du mini-lecteur");
   miniBusy=true;UpdateMiniPlayer();if(miniToggle.IsEnabled||miniPrevious.IsEnabled||miniNext.IsEnabled)throw new Exception("Double commande du mini-lecteur");miniBusy=false;
   store.Value.settings.PrivacyMode=true;UpdateMiniPlayer();if(miniTransport.Visibility!=Visibility.Collapsed||miniToggle.IsEnabled||miniSong.Text!="Mode discret"||miniArtwork.HasImage)throw new Exception("Mini-lecteur visible en mode discret");
   store.Value.settings.PrivacyMode=false;desktopState=new DesktopMusicState();UpdateMiniPlayer();if(miniToggle.IsEnabled||miniPrevious.IsEnabled||miniNext.IsEnabled||miniArtwork.HasImage)throw new Exception("Mini-lecteur actif sans Spotify");
   foreach(int width in new[]{960,1040,1280}){Go("Aujourd’hui");PreviewSized(Path.Combine(folder,"Mini-lecteur-"+width+".png"),width,720);}
   using(var icon=AppIdentity.Icon()){if(icon.Width<16||icon.Height<16)throw new Exception("Icône application invalide");}
  }finally{miniBusy=false;store.Value.settings.PrivacyMode=privacy;desktopState=original;Go(page);}
 }
 internal void VerifyMusicUi(string folder){
  VerifyMiniPlayerUi(folder);ArtworkAcceptance.Verify();spotifyClient=new SpotifyClient(new TestSpotifyStorage());
  desktopState=new DesktopMusicState{Available=true,Playing=true,CanToggle=true,CanNext=true,CanPrevious=true,Title="Exemple de morceau",Artist="Artiste",Album="Pochette de démonstration",Artwork=ArtworkAcceptance.Demo(),Message="Aperçu de test — Spotify en arrière-plan"};
  musicPlaylists.Add(new SpotifyPlaylist{name="Playlist Dofus (exemple)",uri="spotify:playlist:1234567890123456789012"});
  Go("Ma musique");if(musicToggle.Content.ToString()!="Pause"||!musicNext.IsEnabled||!musicPrevious.IsEnabled)throw new Exception("Commandes lecteur");if(!musicArtwork.HasImage)throw new Exception("Pochette du lecteur absente");Preview(Path.Combine(folder,"Musique.png"));desktopState.Artwork=new byte[]{1,2,3};UpdateMusicControls();if(musicArtwork.HasImage)throw new Exception("Pochette corrompue affichée");
  desktopState=new DesktopMusicState();UpdateMusicControls();if(musicArtwork.HasImage||musicToggle.IsEnabled||musicNext.IsEnabled||musicPrevious.IsEnabled)throw new Exception("Commandes sans Spotify");
  bool seen=false;dialogProbe=delegate(Window w){if(!LogicalControls<TextBlock>(w).Any(t=>t.Text.Contains("127.0.0.1/")))throw new Exception("Guide de connexion");seen=true;return false;};try{ConfigureSpotify();}finally{dialogProbe=null;}if(!seen)throw new Exception("Configuration absente");
  spotifyClient.Storage.Disconnect();musicPlaylists.Clear();Refresh();Preview(Path.Combine(folder,"Musique-deconnectee.png"));CancelMusic();
 }
}
internal sealed partial class MainForm {
 internal void VerifyMusicUi(string folder){rendering=true;modern.VerifyMusicUi(folder);if(uiTicker!=null)uiTicker.Dispose();Stop();if(speech!=null)speech.Dispose();player.Close();}
}
internal static partial class Program {
 internal static int MusicTest(string folder){
  Root=Path.GetFullPath(folder);Directory.CreateDirectory(Root);System.Windows.Forms.Application.EnableVisualStyles();
  try{
   Task.Run(async()=>await MusicAcceptance.Api()).GetAwaiter().GetResult();
   try{var state=Task.Run(async()=>await new DesktopMusic().Read()).GetAwaiter().GetResult();File.WriteAllText(Path.Combine(Root,"WINDOWS_MEDIA_CHECK.txt"),state.Available?"Spotify session detected. No playback command sent.":state.Message+" No playback command sent.\n"+state.Diagnostic);}catch(Exception ex){File.WriteAllText(Path.Combine(Root,"WINDOWS_MEDIA_CHECK.txt"),"Windows media unavailable in host: "+ex.GetType().Name);}
   using(var form=new MainForm(false))form.VerifyMusicUi(Root);
   var storage=new SpotifyStorage(Path.Combine(Root,"encrypted-vault"));try{storage.SaveConfig(new SpotifyConfig{clientId="0123456789abcdef0123456789abcdef"});storage.SaveSession(new SpotifySession{access_token="test-secret",refresh_token="test-refresh"});if(storage.Session.access_token!="test-secret"||System.Text.Encoding.UTF8.GetString(File.ReadAllBytes(Path.Combine(Root,"encrypted-vault","session.bin"))).Contains("test-secret"))throw new Exception("Spotify encryption");storage.Disconnect();if(storage.Session!=null)throw new Exception("Spotify disconnect");File.WriteAllText(Path.Combine(Root,"SPOTIFY_VAULT_CHECK.txt"),"PASS: Windows DPAPI roundtrip, no plaintext, disconnect.");}catch(System.Security.Cryptography.CryptographicException){File.WriteAllText(Path.Combine(Root,"SPOTIFY_VAULT_CHECK.txt"),"SKIPPED: DPAPI unavailable for host profile. No plaintext fallback.");}
   File.WriteAllText(Path.Combine(Root,"MUSIC_PASS.txt"),"PASS: mocked Spotify HTTP calls, PKCE, scopes, token refresh and 401 retry, playlist and saved track parsing, paging, playlist/track playback request, four transport commands, URI rejection, 403/404/429 handling, cancellation, local Spotify session targeting; album PNG decoding, invalid and oversized cover fallback, removal on track change/disconnection; WPF player state, no-session controls, configuration guide and rendered previews. No real Spotify account connected and no real playback command sent. DPAPI and Windows media availability recorded separately.");return 0;
  }catch(Exception ex){File.WriteAllText(Path.Combine(Root,"MUSIC_FAIL.txt"),ex.ToString());return 1;}
 }
}
