using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Markup;
using Forms=System.Windows.Forms;
public sealed class DesktopWidgetOptions {
 public bool openOnStartup=false,topmost=true,positionSaved=false;public double left,top;public double width=340,height=560;
}
internal static class WidgetModel {
 internal static void Validate(Settings settings){
  if(settings.Widget==null)settings.Widget=new DesktopWidgetOptions();var widget=settings.Widget;if(Double.IsNaN(widget.width)||Double.IsInfinity(widget.width)||widget.width<300||widget.width>2000)widget.width=340;if(Double.IsNaN(widget.height)||Double.IsInfinity(widget.height)||widget.height<400||widget.height>2000)widget.height=560;
  if(Double.IsNaN(widget.left)||Double.IsNaN(widget.top)||Double.IsInfinity(widget.left)||Double.IsInfinity(widget.top)||Math.Abs(widget.left)>100000||Math.Abs(widget.top)>100000){widget.left=widget.top=0;widget.positionSaved=false;}
 }
 internal static List<WorkTask> Tasks(Data data){return data.tasks.Where(t=>t.status!="done").OrderBy(t=>String.IsNullOrEmpty(t.due)?"9999-12-31":t.due).ThenBy(t=>t.priority=="Haute"?0:1).ThenBy(t=>t.status=="doing"?0:1).ThenBy(t=>t.title).Take(3).ToList();}
 internal static Point Clamp(Point desired,Size size,IEnumerable<Rect> screens){
  var areas=screens.ToList();if(areas.Count==0)areas.Add(new Rect(0,0,1280,720));
  var target=areas.OrderBy(a=>{double x=Math.Max(a.Left,Math.Min(desired.X,a.Right)),y=Math.Max(a.Top,Math.Min(desired.Y,a.Bottom));return (desired.X-x)*(desired.X-x)+(desired.Y-y)*(desired.Y-y);}).First();
  return new Point(Math.Max(target.Left,Math.Min(desired.X,target.Right-size.Width)),Math.Max(target.Top,Math.Min(desired.Y,target.Bottom-size.Height)));
 }
}
internal sealed partial class DesktopWidget:Window {
 DispatcherTimer sizeTimer;readonly MainForm host;readonly Store store;readonly DispatcherTimer timer;readonly DesktopMusic music=new DesktopMusic();
 AlbumArtwork widgetArtwork;ThemePalette palette;TextBlock song,artist,message;Button previous,toggle,next;bool polling,commandBusy,stopping;string signature="";DesktopMusicState playback=new DesktopMusicState();
 internal DesktopWidget(MainForm owner,Store data){
  host=owner;store=data;Title="Widget · "+store.Value.settings.AppTitle;Width=store.Value.settings.Widget.width;Height=store.Value.settings.Widget.height;MinWidth=300;MinHeight=400;WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.CanResizeWithGrip;AllowsTransparency=true;Background=Brushes.Transparent;ShowInTaskbar=false;Topmost=store.Value.settings.Widget.topmost;
  timer=new DispatcherTimer{Interval=TimeSpan.FromSeconds(1)};timer.Tick+=delegate{RefreshView();UpdateWidgetFocusClock();if(++musicTicks%3==0)PollMusic();};
  Loaded+=delegate{if(!host.IsPreview){Place();timer.Start();PollMusic();}};Closed+=delegate{stopping=true;timer.Stop();FlushWidgetSize();host.WidgetClosed(this);};
  SizeChanged+=delegate{if(!IsLoaded||host.IsPreview||stopping)return;if(sizeTimer==null){sizeTimer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(500)};sizeTimer.Tick+=delegate{sizeTimer.Stop();host.SaveWidgetPreference(delegate(DesktopWidgetOptions value){value.width=Width;value.height=Height;});};}sizeTimer.Stop();sizeTimer.Start();};RefreshView();
 }
 Brush Brush(Color color){return new SolidColorBrush(color);}
 TextBlock Text(string value,double size=12,bool muted=false){return new TextBlock{Text=value,FontSize=size,Foreground=Brush(muted?palette.Muted:palette.Ink),TextWrapping=TextWrapping.Wrap};}
 Button Button(string label,Action action,bool primary=false){
  var button=new Button{Content=label,Padding=new Thickness(10,8,10,8),Margin=new Thickness(0,0,7,0),Foreground=Brush(primary?Appearance.TextOn(palette.Accent):palette.Ink),Background=Brush(primary?palette.Accent:palette.Surface)};
  button.Click+=delegate{action();};return button;
 }
 internal void RefreshView(){
  if(stopping)return;var json=new System.Web.Script.Serialization.JavaScriptSerializer();
  string current=store.Value.settings.PrivacyMode.ToString()+store.Value.dofus.active+json.Serialize(store.Value.focus)+json.Serialize(WidgetModel.Tasks(store.Value))+json.Serialize(store.Value.settings.Appearance)+json.Serialize(store.Value.settings.Widget)+store.Value.settings.AppTitle+DateTime.Today.ToString("yyyy-MM-dd");
  Topmost=store.Value.settings.Widget.topmost;if(current==signature)return;signature=current;
  palette=Appearance.Palette(store.Value.settings.Appearance);host.StyleDesktopWidget(this);if(DrawPrivateWidget())return;Title="Widget · "+store.Value.settings.AppTitle;
  var frame=new Border{Background=Brush(palette.Card),BorderBrush=Brush(palette.Border),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(16),Padding=new Thickness(18)};Content=frame;
  var layout=new Grid();layout.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});layout.RowDefinitions.Add(new RowDefinition());layout.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});frame.Child=layout;
  var header=new DockPanel{Margin=new Thickness(0,0,0,16)};var close=Button("×",delegate{Close();});close.Margin=new Thickness(0);DockPanel.SetDock(close,Dock.Right);header.Children.Add(close);
  var heading=new StackPanel();var name=Text(store.Value.settings.AppTitle,20);name.FontFamily=new FontFamily("Georgia");heading.Children.Add(name);var date=Text(DateTime.Today.ToString("dddd d MMM",System.Globalization.CultureInfo.GetCultureInfo("fr-FR")),10,true);date.Margin=new Thickness(0,6,0,0);heading.Children.Add(date);heading.MouseLeftButtonDown+=delegate(object sender,MouseButtonEventArgs e){if(e.LeftButton==MouseButtonState.Pressed&&!host.IsPreview){DragMove();KeepOnScreen();SavePosition();}};heading.Cursor=Cursors.SizeAll;header.Children.Add(heading);layout.Children.Add(header);
  var body=new StackPanel();var scroll=new ScrollViewer{Content=body,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};Grid.SetRow(scroll,1);layout.Children.Add(scroll);
  DrawWidgetFocus(body);body.Children.Add(Text("MES PROCHAINES TÂCHES",10,true));var tasks=WidgetModel.Tasks(store.Value);
  if(tasks.Count==0){var empty=Text("Tout est clair pour le moment.",12,true);empty.Margin=new Thickness(0,16,0,18);body.Children.Add(empty);}
  foreach(var item in tasks){
   string id=item.id;var row=new Grid{Margin=new Thickness(0,12,0,0)};row.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(25)});row.ColumnDefinitions.Add(new ColumnDefinition());
   var done=new CheckBox{IsChecked=false,ToolTip="Terminer cette tâche",VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(0,9,0,0)};done.Click+=delegate{try{host.CompleteWidgetTask(id);RefreshView();}catch(Exception ex){message.Text="Enregistrement impossible : "+ex.Message;done.IsChecked=false;}};row.Children.Add(done);
   var info=new StackPanel();Grid.SetColumn(info,1);var edit=Button(item.title,delegate{host.OpenWidgetTask(id);});edit.Padding=new Thickness(0,3,0,5);edit.Background=Brushes.Transparent;edit.HorizontalContentAlignment=HorizontalAlignment.Left;edit.Content=Text(item.title,12);info.Children.Add(edit);
   string detail=item.project+(String.IsNullOrEmpty(item.due)?"":" · "+Schedule.Day(item.due).ToString("dd/MM")+(Schedule.Day(item.due)<DateTime.Today?" · En retard":""))+(item.status=="doing"?" · En cours":"");info.Children.Add(Text(detail,10,true));row.Children.Add(info);body.Children.Add(row);
  }
  var actions=new WrapPanel{Margin=new Thickness(0,20,0,18)};actions.Children.Add(Button("+ Capture rapide",delegate{host.ShowQuickCapture();},true));actions.Children.Add(Button("Ouvrir l'espace",delegate{host.ShowWorkspaceFromWidget();}));if(store.Value.dofus.characters.Count>0)actions.Children.Add(Button("Compagnon Dofus",delegate{host.OpenDofusFromWidget();}));body.Children.Add(actions);
  var musicPanel=new StackPanel();musicPanel.Children.Add(Text("SPOTIFY",10,true));var nowPlaying=new DockPanel{Margin=new Thickness(0,10,0,0)};widgetArtwork=new AlbumArtwork(58,Brush(palette.Surface),Brush(palette.Muted)){Margin=new Thickness(0,0,12,0)};DockPanel.SetDock(widgetArtwork,Dock.Left);nowPlaying.Children.Add(widgetArtwork);var metadata=new StackPanel{VerticalAlignment=VerticalAlignment.Center};song=Text("",13);song.MaxHeight=38;song.TextTrimming=TextTrimming.CharacterEllipsis;metadata.Children.Add(song);artist=Text("",10,true);artist.Margin=new Thickness(0,4,0,0);metadata.Children.Add(artist);nowPlaying.Children.Add(metadata);musicPanel.Children.Add(nowPlaying);
  var transport=new WrapPanel{Margin=new Thickness(0,12,0,0)};previous=Button("Précédent",delegate{SendCommand("previous");});toggle=Button("Lecture",delegate{SendCommand("toggle");},true);next=Button("Suivant",delegate{SendCommand("next");});PolishedControls.Media(previous,"previous");PolishedControls.Media(toggle,"play",true);PolishedControls.Media(next,"next");transport.Children.Add(previous);transport.Children.Add(toggle);transport.Children.Add(next);musicPanel.Children.Add(transport);body.Children.Add(musicPanel);
  message=Text("",10,true);message.Margin=new Thickness(0,12,0,10);body.Children.Add(message);UpdatePlayback();
  var options=new StackPanel{Margin=new Thickness(0,12,0,0)};var pin=new CheckBox{Content="Garder au premier plan",IsChecked=store.Value.settings.Widget.topmost,Foreground=Brush(palette.Muted),FontSize=11,Margin=new Thickness(0,0,0,7)};pin.Click+=delegate{host.SaveWidgetPreference(delegate(DesktopWidgetOptions settings){settings.topmost=pin.IsChecked==true;});pin.IsChecked=store.Value.settings.Widget.topmost;Topmost=store.Value.settings.Widget.topmost;};options.Children.Add(pin);
  var automatic=new CheckBox{Content="Ouvrir avec Mon espace",IsChecked=store.Value.settings.Widget.openOnStartup,Foreground=Brush(palette.Muted),FontSize=11,Margin=new Thickness(0)};automatic.Click+=delegate{host.SaveWidgetPreference(delegate(DesktopWidgetOptions settings){settings.openOnStartup=automatic.IsChecked==true;});automatic.IsChecked=store.Value.settings.Widget.openOnStartup;};options.Children.Add(automatic);Grid.SetRow(options,2);layout.Children.Add(options);
 }
 void UpdatePlayback(){if(song==null||stopping||store.Value.settings.PrivacyMode)return;widgetArtwork.Update(playback.Artwork,playback.Album);song.Text=String.IsNullOrWhiteSpace(playback.Title)?"Aucun morceau détecté":playback.Title;artist.Text=playback.Artist;previous.IsEnabled=playback.CanPrevious&&!commandBusy;toggle.IsEnabled=playback.CanToggle&&!commandBusy;next.IsEnabled=playback.CanNext&&!commandBusy;toggle.Content=playback.Playing?"Pause":"Lecture";if(!playback.Available)message.Text="Lancez Spotify et un morceau, puis réduisez sa fenêtre.";else if(message.Text=="Lancez Spotify et un morceau, puis réduisez sa fenêtre.")message.Text="";}
 async void PollMusic(){if(polling||stopping||host.IsPreview||store.Value.settings.PrivacyMode)return;polling=true;try{playback=await music.Read();if(!stopping)UpdatePlayback();}catch{if(!stopping)message.Text="Commandes Spotify indisponibles dans cette session.";}finally{polling=false;}}
 async void SendCommand(string action){if(commandBusy||stopping)return;commandBusy=true;UpdatePlayback();try{await music.Command(action);if(!stopping)message.Text="";}catch(Exception ex){if(!stopping)message.Text=ex.Message;}finally{commandBusy=false;if(!stopping){UpdatePlayback();PollMusic();}}}
 IEnumerable<Rect> ScreenAreas(){
  double scale=1;using(var graphics=System.Drawing.Graphics.FromHwnd(IntPtr.Zero))scale=graphics.DpiX/96.0;
  return Forms.Screen.AllScreens.Select(s=>new Rect(s.WorkingArea.X/scale,s.WorkingArea.Y/scale,s.WorkingArea.Width/scale,s.WorkingArea.Height/scale)).ToList();
 }
 void Place(){var areas=ScreenAreas().ToList();var area=areas.FirstOrDefault();if(area.Width<=0)area=new Rect(0,0,1280,720);Height=Math.Max(400,Math.Min(store.Value.settings.Widget.height,area.Height-16));Width=Math.Max(300,Math.Min(store.Value.settings.Widget.width,area.Width-16));var options=store.Value.settings.Widget;var desired=options.positionSaved?new Point(options.left,options.top):new Point(area.Right-Width-20,area.Top+20);var point=WidgetModel.Clamp(desired,new Size(Width,Height),areas);Left=point.X;Top=point.Y;}
 void KeepOnScreen(){var point=WidgetModel.Clamp(new Point(Left,Top),new Size(Width,Height),ScreenAreas());Left=point.X;Top=point.Y;}
 void SavePosition(){host.SaveWidgetPreference(delegate(DesktopWidgetOptions options){options.left=Left;options.top=Top;options.positionSaved=true;});}
 internal void RecoverPosition(){Place();}
 void FlushWidgetSize(){if(sizeTimer!=null&&sizeTimer.IsEnabled){sizeTimer.Stop();if(!host.IsPreview)host.SaveWidgetPreference(delegate(DesktopWidgetOptions value){value.width=Width;value.height=Height;});}}
 internal void Shutdown(){stopping=true;timer.Stop();FlushWidgetSize();Close();}
 internal void TestPlayback(DesktopMusicState state){playback=state;UpdatePlayback();}
 internal void Render(string path){var element=(FrameworkElement)Content;element.Width=Width;element.Height=Height;element.Measure(new Size(Width,Height));element.Arrange(new Rect(0,0,Width,Height));element.UpdateLayout();var bmp=new RenderTargetBitmap((int)Width,(int)Height,96,96,PixelFormats.Pbgra32);bmp.Render(element);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bmp));using(var file=File.Create(path))png.Save(file);}
}
internal sealed partial class MainForm {
 DesktopWidget desktopWidget;
 internal void ShowDesktopWidget(){if(closing||modern==null)return;if(desktopWidget!=null){desktopWidget.RecoverPosition();desktopWidget.Show();desktopWidget.Activate();return;}desktopWidget=new DesktopWidget(this,store);desktopWidget.Show();}
 internal void WidgetClosed(DesktopWidget widget){if(desktopWidget==widget)desktopWidget=null;}
 internal void CloseDesktopWidget(){if(desktopWidget!=null){var widget=desktopWidget;desktopWidget=null;widget.Shutdown();}}
 internal void SaveWidgetPreference(Action<DesktopWidgetOptions> change){
  var json=new System.Web.Script.Serialization.JavaScriptSerializer();var before=json.Deserialize<DesktopWidgetOptions>(json.Serialize(store.Value.settings.Widget));
  try{change(store.Value.settings.Widget);store.Save();}catch(Exception ex){store.Value.settings.Widget=before;notice.Text="Réglage du widget non enregistré : "+ex.Message;}
 }
 internal void ShowWorkspaceFromWidget(){RestoreWorkspace();}
 internal void OpenWidgetTask(string id){var task=store.Value.tasks.FirstOrDefault(t=>t.id==id);if(task==null)return;RestoreWorkspace();Edit(task);}
 internal void CompleteWidgetTask(string id){TaskBoard.Move(store,id,"done");modern.Refresh();}
 internal void StyleDesktopWidget(Window window){modern.StyleDesktopWidget(window);}
}
internal sealed partial class ModernWorkspace {
 internal void StyleDesktopWidget(Window window){window.Resources=(ResourceDictionary)XamlReader.Parse(ResourcesText());}
}
