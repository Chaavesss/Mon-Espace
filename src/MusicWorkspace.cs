using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
internal sealed partial class ModernWorkspace {
 SpotifyClient spotifyClient;DesktopMusic desktopMusic=new DesktopMusic();DesktopMusicState desktopState=new DesktopMusicState();
 DispatcherTimer musicTimer;CancellationTokenSource musicCancel;bool musicBusy,musicPolling;
 AlbumArtwork musicArtwork;TextBlock musicAlbum;TextBlock musicTitle,musicArtist,musicInfo;Button musicToggle,musicNext,musicPrevious;
 List<SpotifyPlaylist> musicPlaylists=new List<SpotifyPlaylist>();List<SpotifySavedTrack> musicLiked=new List<SpotifySavedTrack>();
 bool showLiked=false,musicHasNext=false;int musicOffset=0;string musicMessage="";
 SpotifyClient Spotify{get{if(spotifyClient==null)spotifyClient=new SpotifyClient(new SpotifyStorage(Path.Combine(Path.GetDirectoryName(store.Root),"SpotifyConnection")));return spotifyClient;}}
 internal void CancelMusic(){if(musicCancel!=null)musicCancel.Cancel();if(musicTimer!=null)musicTimer.Stop();}
 void InstallMusic(){
  musicTimer=new DispatcherTimer{Interval=TimeSpan.FromSeconds(2)};musicTimer.Tick+=delegate{PollMusic();if(selected=="Agenda"&&!store.Value.settings.PrivacyMode&&!googleBusy&&CalendarSession()!=null&&DateTime.UtcNow-calendarAttempt>TimeSpan.FromMinutes(2))SyncGoogleCalendar();};
  Loaded+=delegate{if(!owner.IsPreview)musicTimer.Start();};Unloaded+=delegate{CancelMusic();};
 }
 async void PollMusic(){
  if(musicPolling||owner.IsPreview||store.Value.settings.PrivacyMode)return;musicPolling=true;
  try{desktopState=await desktopMusic.Read();if(selected=="Ma musique")UpdateMusicControls();UpdateMiniPlayer();}
  catch{desktopState=new DesktopMusicState{Message="Commandes Windows indisponibles dans cette session."};if(selected=="Ma musique")UpdateMusicControls();UpdateMiniPlayer();}
  finally{musicPolling=false;}
 }
 void UpdateMusicControls(){
  if(musicTitle==null||store.Value.settings.PrivacyMode)return;musicArtwork.Update(desktopState.Artwork,desktopState.Album);musicAlbum.Text=desktopState.Album;musicAlbum.Visibility=String.IsNullOrWhiteSpace(desktopState.Album)?Visibility.Collapsed:Visibility.Visible;
  musicTitle.Text=String.IsNullOrWhiteSpace(desktopState.Title)?"Votre musique Spotify":desktopState.Title;musicArtist.Text=desktopState.Artist;
  musicInfo.Text=desktopState.Message;musicToggle.Content=desktopState.Playing?"Pause":"Lecture";
  musicToggle.IsEnabled=desktopState.CanToggle&&!musicBusy;musicNext.IsEnabled=desktopState.CanNext&&!musicBusy;musicPrevious.IsEnabled=desktopState.CanPrevious&&!musicBusy;
 }
 async void MusicAction(Func<CancellationToken,Task> action){
  if(musicBusy||miniBusy)return;musicBusy=true;UpdateMiniTransport();musicMessage="Opération en cours…";musicCancel=new CancellationTokenSource();if(selected=="Ma musique")Refresh();
  try{await action(musicCancel.Token);musicMessage="";}
  catch(OperationCanceledException){musicMessage="Opération annulée.";}
  catch(Exception ex){musicMessage=ex.Message;}
  finally{musicBusy=false;UpdateMiniTransport();musicCancel.Dispose();musicCancel=null;if(selected=="Ma musique"){Refresh();PollMusic();}}
 }
 async Task LoadMusicLibrary(bool reset,CancellationToken token){
  int offset=reset?0:musicOffset;
  if(showLiked){var page=await Spotify.Liked(offset,token);if(reset)musicLiked.Clear();musicLiked.AddRange(page.items.Where(t=>t!=null&&t.track!=null));musicOffset=offset+page.items.Count;musicHasNext=!String.IsNullOrEmpty(page.next)&&page.items.Count>0;}
  else{var page=await Spotify.Playlists(offset,token);if(reset)musicPlaylists.Clear();musicPlaylists.AddRange(page.items.Where(p=>p!=null));musicOffset=offset+page.items.Count;musicHasNext=!String.IsNullOrEmpty(page.next)&&page.items.Count>0;}
 }
 void MusicPage(StackPanel root){
  var player=new StackPanel();var nowPlaying=new DockPanel{Margin=new Thickness(0,0,0,20)};musicArtwork=new AlbumArtwork(112,SurfaceBrush,Muted){Margin=new Thickness(0,0,20,0)};DockPanel.SetDock(musicArtwork,Dock.Left);nowPlaying.Children.Add(musicArtwork);var metadata=new StackPanel{VerticalAlignment=VerticalAlignment.Center};musicTitle=T("Votre musique Spotify",21);musicTitle.TextTrimming=System.Windows.TextTrimming.CharacterEllipsis;metadata.Children.Add(musicTitle);musicArtist=T("",12,Muted,new Thickness(0,7,0,0));metadata.Children.Add(musicArtist);musicAlbum=T("",11,Muted,new Thickness(0,7,0,0));metadata.Children.Add(musicAlbum);nowPlaying.Children.Add(metadata);player.Children.Add(nowPlaying);
  var controls=new WrapPanel();musicPrevious=Btn("Précédent",delegate{MusicAction(token=>desktopMusic.Command("previous"));});musicToggle=Btn("Lecture",delegate{MusicAction(token=>desktopMusic.Command("toggle"));},true);musicNext=Btn("Suivant",delegate{MusicAction(token=>desktopMusic.Command("next"));});
  PolishedControls.Media(musicPrevious,"previous");PolishedControls.Media(musicToggle,"play",true);PolishedControls.Media(musicNext,"next");controls.Children.Add(musicPrevious);controls.Children.Add(musicToggle);controls.Children.Add(musicNext);controls.Children.Add(Btn("Actualiser",delegate{PollMusic();},false,true));controls.Children.Add(Btn("Lancer Spotify",delegate{owner.OpenSpotify();},false,true));player.Children.Add(controls);
  musicInfo=T("",11,Muted,new Thickness(0,14,0,0));player.Children.Add(musicInfo);root.Children.Add(Card("Lecteur Spotify",player));UpdateMusicControls();
  var library=new StackPanel();library.Children.Add(T("Retrouvez vos playlists et titres enregistrés, puis lancez-les sur votre lecteur Spotify actif.",12,Muted,new Thickness(0,0,0,14)));
  var tools=new WrapPanel();var configure=Btn("Configurer la connexion",delegate{ConfigureSpotify();});tools.Children.Add(configure);
  bool connected=false;try{connected=Spotify.Storage.Session!=null;}catch(Exception ex){musicMessage="Connexion locale indisponible : "+ex.Message;}
  if(connected){
   tools.Children.Add(Btn("Déconnecter",delegate{if(musicBusy)return;try{Spotify.Storage.Disconnect();musicPlaylists.Clear();musicLiked.Clear();musicHasNext=false;musicMessage="Compte déconnecté de ce PC.";Refresh();}catch(Exception ex){MessageBox.Show(ex.Message);}},false,true));
   tools.Children.Add(Btn("Mes playlists",delegate{showLiked=false;MusicAction(token=>LoadMusicLibrary(true,token));},!showLiked));
   tools.Children.Add(Btn("Titres enregistrés",delegate{showLiked=true;MusicAction(token=>LoadMusicLibrary(true,token));},showLiked));
  }else tools.Children.Add(Btn("Connecter Spotify",delegate{MusicAction(async token=>{await Spotify.Connect(Open,token);await LoadMusicLibrary(true,token);});},true));
  foreach(var button in tools.Children.OfType<Button>())button.IsEnabled=!musicBusy;library.Children.Add(tools);
  if(musicBusy){library.Children.Add(T("Opération en cours…",12,Muted,new Thickness(0,14,0,0)));library.Children.Add(Btn("Annuler",delegate{if(musicCancel!=null)musicCancel.Cancel();},false,true));}
  if(!String.IsNullOrEmpty(musicMessage))library.Children.Add(T(musicMessage,12,Accent,new Thickness(0,14,0,0)));
  if(!connected)library.Children.Add(T("La bibliothèque nécessite une connexion à votre compte et une configuration Spotify. Le lecteur Windows ci-dessus fonctionne séparément.",11,Muted,new Thickness(0,16,0,0)));
  if(connected&&!musicBusy){
   if(showLiked){foreach(var saved in musicLiked){var item=saved.track;var row=new DockPanel{Margin=new Thickness(0,12,0,4)};var play=Btn("Lire",delegate{MusicAction(token=>Spotify.Play(item.uri,false,token));},false,true);play.IsEnabled=SpotifyClient.ValidUri(item.uri,"track");DockPanel.SetDock(play,Dock.Right);row.Children.Add(play);var info=new StackPanel();info.Children.Add(T(item.name,13));info.Children.Add(T(String.Join(", ",(item.artists??new List<SpotifyArtist>()).Where(a=>a!=null).Select(a=>a.name)),11,Muted));row.Children.Add(info);library.Children.Add(row);}}
   else foreach(var playlist in musicPlaylists){var item=playlist;var row=new DockPanel{Margin=new Thickness(0,12,0,4)};var play=Btn("Lire",delegate{MusicAction(token=>Spotify.Play(item.uri,true,token));},false,true);play.IsEnabled=SpotifyClient.ValidUri(item.uri,"playlist");DockPanel.SetDock(play,Dock.Right);row.Children.Add(play);row.Children.Add(T(item.name,13,Ink,new Thickness(0,10,8,0)));library.Children.Add(row);}
   if(musicHasNext)library.Children.Add(Btn("Charger la suite",delegate{MusicAction(token=>LoadMusicLibrary(false,token));},false,true));
   if((showLiked?musicLiked.Count:musicPlaylists.Count)==0)library.Children.Add(T("Choisissez Mes playlists ou Titres enregistrés pour charger votre bibliothèque.",11,Muted,new Thickness(0,16,0,0)));
   var remote=new WrapPanel{Margin=new Thickness(0,14,0,0)};remote.Children.Add(Btn("Reprendre via Spotify",delegate{MusicAction(token=>Spotify.Command("play",token));},false,true));remote.Children.Add(Btn("Pause via Spotify",delegate{MusicAction(token=>Spotify.Command("pause",token));},false,true));remote.Children.Add(Btn("Précédent via Spotify",delegate{MusicAction(token=>Spotify.Command("previous",token));},false,true));remote.Children.Add(Btn("Suivant via Spotify",delegate{MusicAction(token=>Spotify.Command("next",token));},false,true));library.Children.Add(remote);
  }
  var box=Card("Ma bibliothèque Spotify",library);box.Margin=new Thickness(0,16,0,0);root.Children.Add(box);
  var local=new StackPanel();local.Children.Add(T("Votre MP3 du matin reste lu directement par Mon espace.",12,Muted,new Thickness(0,0,0,12)));local.Children.Add(Btn("Musique & voix du matin",delegate{owner.AudioSettings();}));var localBox=Card("Ma routine",local);localBox.Margin=new Thickness(0,16,0,0);root.Children.Add(localBox);ReflowMusicV2(root);
 }
 void ConfigureSpotify(){
  StackPanel body,actions;var w=Dialog("Configurer Spotify",out body,out actions,680);
  body.Children.Add(T("1. Créez une application dans Spotify for Developers avec votre compte Premium.\n2. Activez Web API et ajoutez l'adresse de retour http://127.0.0.1/ (sans port).\n3. Ajoutez votre compte et celui de votre frère aux utilisateurs autorisés si vous partagez cette intégration.\n4. Copiez uniquement le Client ID ci-dessous. Aucun Client Secret n'est nécessaire.",12,Muted,new Thickness(0,0,0,18)));
  body.Children.Add(Btn("Ouvrir Spotify for Developers",delegate{Open("https://developer.spotify.com/dashboard");},false,true));
  var client=new TextBox{MaxLength=32};try{var config=Spotify.Storage.Config;if(config!=null)client.Text=config.clientId;}catch{}
  Field(body,"Client ID",client);var error=T("",12,Accent);body.Children.Add(error);
  body.Children.Add(T("La connexion s'ouvre dans votre navigateur habituel. Les autorisations restent chiffrées sur ce PC et ne sont pas incluses dans vos sauvegardes.",11,Muted,new Thickness(0,15,0,0)));
  actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));actions.Children.Add(Btn("Enregistrer",delegate{if(!SpotifyClient.ValidClient(client.Text.Trim())){error.Text="Saisissez les 32 caractères du Client ID.";return;}try{Spotify.Storage.SaveConfig(new SpotifyConfig{clientId=client.Text.Trim()});musicPlaylists.Clear();musicLiked.Clear();musicHasNext=false;w.DialogResult=true;}catch(Exception ex){error.Text=ex.Message;}},true));ShowWindow(w);Refresh();
 }
}
internal sealed partial class MainForm {internal bool IsPreview{get{return rendering;}}}
