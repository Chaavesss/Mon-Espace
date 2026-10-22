using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Web.Script.Serialization;
internal sealed class SpotifyConfig {public string clientId="";}
internal sealed class SpotifySession {public string access_token="",refresh_token="";public long expiresTicks;}
internal sealed class SpotifyTokens {public string access_token="",refresh_token="";public int expires_in=3600;}
internal sealed class SpotifyPlaylist {public string id="",name="",uri="";}
internal sealed class SpotifyArtist {public string name="";}
internal sealed class SpotifyTrack {public string id="",name="",uri="";public List<SpotifyArtist> artists=new List<SpotifyArtist>();}
internal sealed class SpotifySavedTrack {public SpotifyTrack track;}
internal sealed class SpotifyPage<T> {public List<T> items=new List<T>();public int total,offset,limit=50;public string next;}
internal class SpotifyStorage {
 readonly GoogleVault vault;
 internal SpotifyStorage(string root){vault=new GoogleVault(root);}
 internal virtual SpotifyConfig Config{get{return vault.Read<SpotifyConfig>("client");}}
 internal virtual SpotifySession Session{get{return vault.Read<SpotifySession>("session");}}
 internal virtual void SaveConfig(SpotifyConfig config){vault.Write("client",config);vault.ClearSession();}
 internal virtual void SaveSession(SpotifySession session){vault.Write("session",session);}
 internal virtual void Disconnect(){vault.ClearSession();}
}
internal sealed class SpotifyClient {
 internal readonly SpotifyStorage Storage;readonly Func<HttpClient> httpFactory;
 static readonly JavaScriptSerializer Json=new JavaScriptSerializer{MaxJsonLength=4*1024*1024};
 const string Scope="playlist-read-private playlist-read-collaborative user-library-read user-read-playback-state user-modify-playback-state";
 internal SpotifyClient(SpotifyStorage storage,Func<HttpClient> factory=null){Storage=storage;httpFactory=factory??(()=>new HttpClient(new HttpClientHandler{AllowAutoRedirect=false}){Timeout=TimeSpan.FromSeconds(25)});}
 internal static bool ValidClient(string id){return id!=null&&System.Text.RegularExpressions.Regex.IsMatch(id,@"^[a-fA-F0-9]{32}$");}
 internal static bool ValidUri(string uri,string kind){return uri!=null&&System.Text.RegularExpressions.Regex.IsMatch(uri,@"^spotify:"+kind+@":[A-Za-z0-9]{22}$");}
 internal static string AuthUrl(string id,string redirect,string state,string verifier){if(!ValidClient(id))throw new InvalidDataException("Client ID Spotify invalide (32 caractères).");return "https://accounts.spotify.com/authorize?"+GoogleDriveClient.Form(new Dictionary<string,string>{{"client_id",id},{"response_type","code"},{"redirect_uri",redirect},{"state",state},{"scope",Scope},{"code_challenge_method","S256"},{"code_challenge",GoogleDriveClient.Challenge(verifier)}});}
 async Task<SpotifyTokens> Tokens(Dictionary<string,string> fields,CancellationToken token){
  using(var http=httpFactory())using(var response=await http.PostAsync("https://accounts.spotify.com/api/token",new FormUrlEncodedContent(fields),token)){
   if(!response.IsSuccessStatusCode)throw new InvalidOperationException("Connexion Spotify refusée. Vérifiez la configuration ou reconnectez votre compte.");
   var result=Json.Deserialize<SpotifyTokens>(await response.Content.ReadAsStringAsync());if(result==null||String.IsNullOrEmpty(result.access_token))throw new InvalidDataException("Réponse Spotify incomplète.");return result;
  }
 }
 internal async Task Connect(Action<string> browser,CancellationToken cancel){
  var config=Storage.Config;if(config==null||!ValidClient(config.clientId))throw new InvalidOperationException("Renseignez d'abord le Client ID Spotify.");
  string state=GoogleDriveClient.Random(),verifier=GoogleDriveClient.Random();var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();
  using(var timeout=CancellationTokenSource.CreateLinkedTokenSource(cancel)){timeout.CancelAfter(TimeSpan.FromMinutes(5));try{
   string redirect="http://127.0.0.1:"+((IPEndPoint)listener.LocalEndpoint).Port+"/";var callback=GoogleDriveClient.WaitCallback(listener,state,timeout.Token);
   browser(AuthUrl(config.clientId,redirect,state,verifier));string code=await callback;
   var tokens=await Tokens(new Dictionary<string,string>{{"grant_type","authorization_code"},{"client_id",config.clientId},{"code",code},{"code_verifier",verifier},{"redirect_uri",redirect}},timeout.Token);
   if(String.IsNullOrEmpty(tokens.refresh_token))throw new InvalidDataException("Spotify n'a pas fourni de connexion persistante.");
   timeout.Token.ThrowIfCancellationRequested();Storage.SaveSession(new SpotifySession{access_token=tokens.access_token,refresh_token=tokens.refresh_token,expiresTicks=DateTime.UtcNow.AddSeconds(Math.Max(30,tokens.expires_in)).Ticks});
  }finally{timeout.Cancel();listener.Stop();}}
 }
 async Task<string> Access(bool force,CancellationToken token){
  var session=Storage.Session;var config=Storage.Config;if(session==null||config==null)throw new InvalidOperationException("Connectez votre compte Spotify pour afficher votre bibliothèque.");
  if(force||session.expiresTicks<DateTime.UtcNow.AddMinutes(1).Ticks){var refreshed=await Tokens(new Dictionary<string,string>{{"grant_type","refresh_token"},{"client_id",config.clientId},{"refresh_token",session.refresh_token}},token);session.access_token=refreshed.access_token;if(!String.IsNullOrEmpty(refreshed.refresh_token))session.refresh_token=refreshed.refresh_token;session.expiresTicks=DateTime.UtcNow.AddSeconds(Math.Max(30,refreshed.expires_in)).Ticks;token.ThrowIfCancellationRequested();Storage.SaveSession(session);}return session.access_token;
 }
 async Task<string> Request(HttpMethod method,string path,string body,CancellationToken token){
  for(int attempt=0;attempt<2;attempt++){string access=await Access(attempt==1,token);
   using(var http=httpFactory())using(var request=new HttpRequestMessage(method,"https://api.spotify.com/v1/"+path)){
    request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",access);if(body!=null)request.Content=new StringContent(body,Encoding.UTF8,"application/json");
    using(var response=await http.SendAsync(request,token)){
     if(response.StatusCode==HttpStatusCode.Unauthorized&&attempt==0)continue;
     if(!response.IsSuccessStatusCode){
      if(response.StatusCode==HttpStatusCode.NotFound)throw new InvalidOperationException("Aucun lecteur Spotify actif : lancez Spotify et un morceau, puis réduisez la fenêtre.");
      if((int)response.StatusCode==429)throw new InvalidOperationException("Spotify demande de patienter. Réessayez dans quelques instants.");
      if(response.StatusCode==HttpStatusCode.Forbidden)throw new InvalidOperationException("Spotify refuse cet accès. Vérifiez Premium, les comptes autorisés et les permissions de connexion.");
      throw new InvalidOperationException("Spotify indisponible (code "+(int)response.StatusCode+").");
     }return await response.Content.ReadAsStringAsync();
    }
   }
  }throw new InvalidOperationException("Reconnectez votre compte Spotify.");
 }
 static void Offset(int offset){if(offset<0||offset>100000)throw new InvalidDataException("Page Spotify invalide.");}
 internal async Task<SpotifyPage<SpotifyPlaylist>> Playlists(int offset,CancellationToken token){Offset(offset);var p=Json.Deserialize<SpotifyPage<SpotifyPlaylist>>(await Request(HttpMethod.Get,"me/playlists?limit=50&offset="+offset,null,token));if(p==null)throw new InvalidDataException("Bibliothèque Spotify vide ou invalide.");p.items=p.items??new List<SpotifyPlaylist>();return p;}
 internal async Task<SpotifyPage<SpotifySavedTrack>> Liked(int offset,CancellationToken token){Offset(offset);var p=Json.Deserialize<SpotifyPage<SpotifySavedTrack>>(await Request(HttpMethod.Get,"me/tracks?limit=50&offset="+offset,null,token));if(p==null)throw new InvalidDataException("Titres Spotify invalides.");p.items=p.items??new List<SpotifySavedTrack>();return p;}
 internal async Task Play(string uri,bool playlist,CancellationToken token){
  if(!ValidUri(uri,playlist?"playlist":"track"))throw new InvalidDataException("Lien Spotify invalide.");
  string body=playlist?Json.Serialize(new{context_uri=uri}):Json.Serialize(new{uris=new[]{uri}});
  await Request(HttpMethod.Put,"me/player/play",body,token);
 }
 internal async Task Command(string action,CancellationToken token){string path;HttpMethod method;if(action=="play"||action=="pause"){path="me/player/"+action;method=HttpMethod.Put;}else if(action=="next"||action=="previous"){path="me/player/"+action;method=HttpMethod.Post;}else throw new InvalidDataException("Commande Spotify invalide.");await Request(method,path,null,token);}
}
