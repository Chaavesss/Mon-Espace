using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
internal sealed class WorkspaceTab {internal string Page,Project,Document;}
internal sealed partial class ModernWorkspace {
 WrapPanel workspaceTabs;Button workspaceSettings;bool showExtraTools;
 readonly List<WorkspaceTab> openTabs=new List<WorkspaceTab>();
 AlbumArtwork miniArtwork;TextBlock miniSong,miniArtist;Button miniPrevious,miniToggle,miniNext;StackPanel miniTransport;bool miniBusy;string miniPlaybackError="";
 Brush FaintLine{get{return new SolidColorBrush(Appearance.Mix(CurrentPalette.Card,CurrentPalette.Ink,.07));}}
 string PageDescription(){
  switch(selected){
   case "Aujourd’hui":return "Le bon endroit pour commencer votre journée.";
   case "Mes tâches":return "Faites de la place à ce qui mérite votre attention.";
   case "Agenda":return "Une vue claire de votre temps et de vos engagements.";
   case "Mes projets":return String.IsNullOrEmpty(openProject)?"Vos idées prennent forme, une étape à la fois.":"Tâches, notes et ressources, réunies au même endroit.";
   case "Mes notes":return "Gardez une idée. Développez la suivante.";
   case "Mon budget":return "Votre argent aujourd’hui, vos prévisions pour demain.";
   case "Ma musique":return "Votre bande-son, à portée de main.";
   case "Documents locaux":return "PDF, images et textes, directement dans votre espace.";
   case "Google Drive":return "Retrouvez vos documents et vos fichiers épinglés.";
   case "Concentration":return "Une seule tâche. Un peu de temps pour avancer.";
   default:return "Votre espace, organisé à votre façon.";
  }
 }
 string NavigationIcon(string page){
  switch(page){
   case "Aujourd’hui":return "M3,10 L10,4 L17,10 M5,9 L5,17 L15,17 L15,9 M9,17 L9,12 L12,12";
   case "Mes tâches":return "M3,6 L5,8 L8,4 M10,6 L17,6 M3,13 L5,15 L8,11 M10,13 L17,13";
   case "Agenda":return "M4,5 L16,5 L16,17 L4,17 Z M7,3 L7,7 M13,3 L13,7 M4,9 L16,9";
   case "Mes projets":return "M3,6 L8,6 L10,8 L17,8 L17,17 L3,17 Z M3,6 L3,4 L8,4 L10,6 L16,6";
   case "Mes notes":return "M5,3 L15,3 L15,17 L5,17 Z M8,7 L12,7 M8,10 L12,10 M8,13 L11,13";
   case "Google Drive":return "M8,3 L12,3 L18,14 L15,18 L3,18 L1,14 Z M8,3 L14,14 L18,14 M1,14 L14,14 M12,3 L6,14 L3,18";
   case "Mon budget":return "M3,6 L17,6 L17,17 L3,17 Z M3,6 L14,3 L14,6 M13,10 L17,10 L17,14 L13,14 Z";
   case "Recherche":return "M13,13 L18,18 M15,9 A6,6 0 1 1 3,9 A6,6 0 1 1 15,9";
   case "Ma musique":return "M8,15 L8,4 L17,2 L17,13 M8,6 L17,4 M8,15 A3,2 0 1 1 2,15 A3,2 0 1 1 8,15 M17,13 A3,2 0 1 1 11,13 A3,2 0 1 1 17,13";
   case "Réglages":return "M3,6 L17,6 M3,14 L17,14 M7,3 L7,9 M13,11 L13,17";
   default:return "M5,5 L9,5 L9,9 L5,9 Z M12,5 L16,5 L16,9 L12,9 Z M5,12 L9,12 L9,16 L5,16 Z M12,12 L16,12 L16,16 L12,16 Z";
  }
 }
 UIElement LineIcon(string page,Brush color){
  return new Viewbox{Width=18,Height=18,Margin=new Thickness(0,0,11,0),Child=new System.Windows.Shapes.Path{Data=Geometry.Parse(NavigationIcon(page)),Stroke=color,StrokeThickness=1.45,StrokeStartLineCap=PenLineCap.Round,StrokeEndLineCap=PenLineCap.Round,StrokeLineJoin=PenLineJoin.Round,Width=20,Height=20}};
 }
 string ShortPage(string page){return page=="Boîte de réception"?"Réception":page=="Documents locaux"?"Fichiers":page=="Aujourd’hui"?"Accueil":page=="Mes tâches"?"Tâches":page=="Mes projets"?"Projets":page=="Mes notes"?"Notes":page=="Mon budget"?"Budget":page=="Google Drive"?"Google Drive":page;}
 Button NavigationButton(string page,string label){
  bool active=selected==page;var button=Btn(label,delegate{Go(page);},false,true);
  var row=new DockPanel();row.Children.Add(LineIcon(page,active?AccentText:Muted));var text=T(label,12,active?Ink:Muted);text.VerticalAlignment=VerticalAlignment.Center;row.Children.Add(text);
  button.Content=row;button.ToolTip=page;button.HorizontalContentAlignment=HorizontalAlignment.Stretch;button.Margin=new Thickness(0,0,0,4);button.Padding=new Thickness(12,9,10,9);
  if(active){button.Background=SelectionBrush;text.FontWeight=FontWeights.SemiBold;}
  return button;
 }
 void RebuildNavigation(){
  nav.Children.Clear();
  var search=NavigationButton("Recherche","Rechercher");search.ToolTip="Rechercher · Ctrl + K";search.Background=SurfaceBrush;search.Margin=new Thickness(0,0,0,14);nav.Children.Add(search);
  nav.Children.Add(T("VOTRE ESPACE",9,Muted,new Thickness(12,0,0,11)));
  foreach(var page in new[]{"Aujourd’hui","Mes tâches","Boîte de réception","Agenda","Mes projets","Mes notes","Documents locaux","Google Drive","Mon budget","Ma musique"})nav.Children.Add(NavigationButton(page,ShortPage(page)));
  var extras=new[]{"Concentration","Habitudes","Bilan de la semaine","Mes modes","Rappels","Mes services"};
  var more=Btn(showExtraTools?"Moins d’outils  −":"Tous mes outils  +",delegate{showExtraTools=!showExtraTools;Refresh();},false,true);more.HorizontalContentAlignment=HorizontalAlignment.Left;more.Foreground=Muted;more.Margin=new Thickness(0,13,0,8);nav.Children.Add(more);
  if(showExtraTools)foreach(string page in extras)nav.Children.Add(NavigationButton(page,page));else if(extras.Contains(selected))nav.Children.Add(NavigationButton(selected,selected));
  if(store.Value.favorites.Count>0)FavoriteSidebar();
 }
 void InstallWorkspaceTabs(Grid main){
  main.Children.Remove(crumb);
  var top=new StackPanel();var line=new DockPanel{Margin=new Thickness(0,0,0,13)};
  var settings=workspaceSettings=Btn("Réglages",delegate{Go("Réglages");},false,true);settings.Padding=new Thickness(10,3,0,3);DockPanel.SetDock(settings,Dock.Right);line.Children.Add(settings);crumb.Margin=new Thickness(0,4,0,0);line.Children.Add(crumb);top.Children.Add(line);
  workspaceTabs=new WrapPanel{Margin=new Thickness(0,0,0,22)};top.Children.Add(workspaceTabs);main.Children.Add(top);
 }
 void UpdateWorkspaceTabs(){
  if(workspaceTabs==null)return;if(workspaceSettings!=null)workspaceSettings.Foreground=Ink;workspaceTabs.Visibility=Visibility.Visible;
  if(!openTabs.Any(t=>t.Page==selected&&t.Project==openProject&&t.Document==(selected=="Documents locaux"?openDocument:"")))openTabs.Add(new WorkspaceTab{Page=selected,Project=openProject,Document=selected=="Documents locaux"?openDocument:""});
  while(openTabs.Count>5)openTabs.RemoveAt(openTabs[0].Page=="Aujourd’hui"?1:0);
  workspaceTabs.Visibility=openTabs.Count>1?Visibility.Visible:Visibility.Collapsed;workspaceTabs.Children.Clear();
  foreach(var entry in openTabs.ToList()){
   var tab=entry;bool active=tab.Page==selected&&tab.Project==openProject&&tab.Document==(selected=="Documents locaux"?openDocument:"");
   var row=new StackPanel{Orientation=Orientation.Horizontal};
   var select=Btn(!String.IsNullOrEmpty(tab.Document)?System.IO.Path.GetFileName(tab.Document):String.IsNullOrEmpty(tab.Project)?ShortPage(tab.Page):tab.Project,delegate{selected=tab.Page;openProject=tab.Project;openDocument=tab.Document??"";RecordTabResume();Refresh();},false,true);
   select.Padding=new Thickness(12,8,7,8);select.Foreground=active?Ink:Muted;row.Children.Add(select);
   if(tab.Page!="Aujourd’hui"){var close=Btn("×",delegate{openTabs.Remove(tab);if(active){var last=openTabs.LastOrDefault();selected=last==null?"Aujourd’hui":last.Page;openProject=last==null?"":last.Project;openDocument=last==null?"":last.Document??"";}Refresh();},false,true);close.ToolTip="Fermer cet onglet";close.Padding=new Thickness(3,6,9,6);close.Margin=new Thickness(0);row.Children.Add(close);}
   var border=new Border{Child=row,CornerRadius=new CornerRadius(9),Background=active?SurfaceBrush:Brushes.Transparent,BorderBrush=active?FaintLine:Brushes.Transparent,BorderThickness=new Thickness(1),Margin=new Thickness(0,0,6,5)};workspaceTabs.Children.Add(border);
  }
 }
 void InstallMiniPlayer(StackPanel now){
  now.Children.Clear();var dock=new DockPanel();
  miniArtwork=new AlbumArtwork(42,SurfaceBrush,Muted){Margin=new Thickness(0,0,12,0)};DockPanel.SetDock(miniArtwork,Dock.Left);dock.Children.Add(miniArtwork);
  var text=new StackPanel{VerticalAlignment=VerticalAlignment.Center};
  miniSong=T("Votre bande-son",12);miniSong.TextWrapping=TextWrapping.NoWrap;miniSong.TextTrimming=TextTrimming.CharacterEllipsis;text.Children.Add(miniSong);
  miniArtist=T("Spotify & routine du matin",10,Muted,new Thickness(0,5,10,0));miniArtist.TextWrapping=TextWrapping.NoWrap;miniArtist.TextTrimming=TextTrimming.CharacterEllipsis;text.Children.Add(miniArtist);dock.Children.Add(text);now.Children.Add(dock);
  // Keep the existing runtime labels for the morning player and spoken summary.
  track.Visibility=Visibility.Collapsed;status.Visibility=Visibility.Collapsed;now.Children.Add(track);now.Children.Add(status);
 }
 void UpdateMiniPlayer(){
  if(miniSong==null)return;UpdateMiniTransport();miniSong.Foreground=Ink;miniArtist.Foreground=Muted;miniArtwork.SetPalette(SurfaceBrush,Muted);
  if(store.Value.settings.PrivacyMode){miniSong.Text="Mode discret";miniArtist.Text="Contenus personnels masqués";miniArtwork.Update(null,"");return;}
  miniSong.Text=desktopState.Available?desktopState.Title:"Votre bande-son";
  miniArtist.Text=desktopState.Available?desktopState.Artist:"Spotify & routine du matin";miniArtwork.Update(desktopState.Available?desktopState.Artwork:null,desktopState.Album);
 }

 void InstallMiniTransport(Grid grid){
  miniTransport=new StackPanel{Orientation=Orientation.Horizontal,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(12,0,16,0)};
  miniPrevious=PolishedControls.Media(Btn("Précédent",delegate{MiniCommand("previous");},false,true),"previous");
  miniToggle=PolishedControls.Media(Btn("Lecture",delegate{MiniCommand("toggle");},true),"play",true);
  miniNext=PolishedControls.Media(Btn("Suivant",delegate{MiniCommand("next");},false,true),"next");
  miniPrevious.Width=miniPrevious.Height=30;miniNext.Width=miniNext.Height=30;miniToggle.Width=miniToggle.Height=42;
  miniTransport.Children.Add(miniPrevious);miniTransport.Children.Add(miniToggle);miniTransport.Children.Add(miniNext);
  Grid.SetColumn(miniTransport,1);grid.Children.Add(miniTransport);UpdateMiniTransport();
 }
 void UpdateMiniTransport(){
  if(miniToggle==null)return;bool hidden=store.Value.settings.PrivacyMode;
  miniTransport.Visibility=hidden?Visibility.Collapsed:Visibility.Visible;
  miniToggle.Content=desktopState.Playing?"Pause":"Lecture";
  miniToggle.ToolTip=!String.IsNullOrEmpty(miniPlaybackError)?miniPlaybackError:desktopState.Available?Convert.ToString(miniToggle.Content):"Lancez Spotify et un morceau pour activer les commandes.";ToolTipService.SetShowOnDisabled(miniToggle,true);
  miniToggle.Background=Accent;miniToggle.Foreground=new SolidColorBrush(Appearance.TextOn(CurrentPalette.Accent));
  miniPrevious.Foreground=miniNext.Foreground=Ink;
  miniToggle.IsEnabled=!hidden&&!miniBusy&&!musicBusy&&desktopState.Available&&desktopState.CanToggle;
  miniPrevious.IsEnabled=!hidden&&!miniBusy&&!musicBusy&&desktopState.Available&&desktopState.CanPrevious;
  miniNext.IsEnabled=!hidden&&!miniBusy&&!musicBusy&&desktopState.Available&&desktopState.CanNext;
 }
 async void MiniCommand(string action){
  if(miniBusy||musicBusy||store.Value.settings.PrivacyMode)return;miniPlaybackError="";miniBusy=true;UpdateMiniTransport();
  try{await desktopMusic.Command(action);desktopState=await desktopMusic.Read();}
  catch(Exception ex){miniPlaybackError=ex.Message;}
  finally{miniBusy=false;UpdateMiniPlayer();if(selected=="Ma musique")UpdateMusicControls();}
 }
 void SwitchToWidget(){owner.ShowDesktopWidget();if(!owner.IsPreview)owner.Hide();}
 void HomeV2(StackPanel root){
  if(!store.Value.settings.Configured){
   var setup=new StackPanel();setup.Children.Add(T("Donnez un prénom à votre espace, choisissez vos couleurs et retrouvez vos outils.",13,Muted,new Thickness(0,0,0,14)));
   setup.Children.Add(Btn("Configurer mon espace",delegate{owner.SetupWorkspace();},true));var card=Card("Bienvenue chez vous.",setup);card.Margin=new Thickness(0,0,0,18);root.Children.Add(card);
  }
  DayHomeBar(root);
  HomeOverviewV3(root);
  HomeWidgets(root);
 }
 void ProjectsV2(StackPanel root){
  var tools=new WrapPanel{Margin=new Thickness(0,0,0,22)};tools.Children.Add(Btn("+  Nouveau projet",delegate{EditProject(null);},true));tools.Children.Add(Btn("À partir d’un modèle",delegate{Go("Modèles de projets");},false,true));root.Children.Add(tools);
  var grid=new UniformGrid{Columns=2};int visualIndex=0;
  foreach(string name in owner.AllProjects()){
   string project=name;var data=store.Value.projectData.FirstOrDefault(p=>p.name==name);var tasks=store.Value.tasks.Where(t=>t.project==name).ToList();int done=tasks.Count(t=>t.status=="done");
   var body=new StackPanel();bool cover=false;
   if(data!=null&&data.cover!=null){try{var file=ProjectData.PathFor(store,data.cover);if(System.IO.File.Exists(file)){body.Children.Add(CoverBorder(ProjectVisuals.Read(file),110));cover=true;}}catch{}}
   if(!cover){
    var graphic=new Grid{Height=100};int variant=visualIndex++%3;if(variant==1){graphic.Children.Add(new System.Windows.Shapes.Path{Data=Geometry.Parse("M160,0 L260,100 M200,0 L300,100 M240,0 L340,100"),Stroke=new SolidColorBrush(Appearance.Mix(CurrentPalette.Card,CurrentPalette.Accent,.18)),StrokeThickness=18,HorizontalAlignment=HorizontalAlignment.Right});}else if(variant==2){graphic.Children.Add(new Rectangle{Width=85,Height=85,RadiusX=18,RadiusY=18,Stroke=new SolidColorBrush(Appearance.Mix(CurrentPalette.Card,CurrentPalette.Accent,.22)),StrokeThickness=1,RenderTransformOrigin=new Point(.5,.5),RenderTransform=new RotateTransform(25),HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,0,40,0)});}else graphic.Children.Add(new Ellipse{Width=118,Height=118,Stroke=new SolidColorBrush(Appearance.Mix(CurrentPalette.Card,CurrentPalette.Accent,.2)),StrokeThickness=1,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,-28,27,0)});
    var glyph=T(data!=null&&!String.IsNullOrEmpty(data.icon)?data.icon:"↗",31,Accent,new Thickness(20,20,0,0));graphic.Children.Add(glyph);
    body.Children.Add(new Border{Child=graphic,ClipToBounds=true,CornerRadius=new CornerRadius(12),Background=new LinearGradientBrush(Appearance.Mix(CurrentPalette.Card,CurrentPalette.Accent,.12),CurrentPalette.Card,25),Margin=new Thickness(0,0,0,17)});
   }
   var title=T(name,21);title.FontFamily=new FontFamily("Georgia");body.Children.Add(title);
   body.Children.Add(T(data==null||String.IsNullOrWhiteSpace(data.description)?"Un espace pour avancer et garder le fil.":data.description,12,Muted,new Thickness(0,7,0,15)));
   var progress=new DockPanel();var ratio=T(done+" / "+tasks.Count,11,Muted);DockPanel.SetDock(ratio,Dock.Right);progress.Children.Add(ratio);progress.Children.Add(T(tasks.Count==0?"Prêt à commencer":tasks.Count(t=>t.status!="done")+" tâches restantes",11,Muted));body.Children.Add(progress);
   body.Children.Add(new ProgressBar{Value=tasks.Count==0?0:100.0*done/tasks.Count,Maximum=100,Height=4,Foreground=Accent,Background=FaintLine,Margin=new Thickness(0,9,0,16)});
   var open=Btn("Ouvrir le projet  ↗",delegate{OpenProject(project);},false,true);open.Padding=new Thickness(0,7,0,0);open.HorizontalAlignment=HorizontalAlignment.Left;body.Children.Add(open);
   var card=Box(body,new Thickness(20));card.VerticalAlignment=VerticalAlignment.Top;card.Margin=new Thickness(0,0,16,18);grid.Children.Add(card);
  }root.Children.Add(grid);
 }
 void AgendaV2(StackPanel root){
  GoogleCalendarSection(root);
  var toolbar=new WrapPanel{Margin=new Thickness(0,0,0,20)};
  toolbar.Children.Add(Btn("+  Rendez-vous",delegate{EditAppointment(null,agendaDate);},true));
  toolbar.Children.Add(Btn("Aujourd’hui",delegate{agendaDate=DateTime.Today;Refresh();}));
  toolbar.Children.Add(Btn("‹",delegate{agendaDate=agendaDate.AddDays(agendaWeek?-7:-1);Refresh();},false,true));
  toolbar.Children.Add(Btn("›",delegate{agendaDate=agendaDate.AddDays(agendaWeek?7:1);Refresh();},false,true));
  toolbar.Children.Add(Btn("Jour",delegate{agendaWeek=false;Refresh();},!agendaWeek));
  toolbar.Children.Add(Btn("Semaine",delegate{agendaWeek=true;Refresh();},agendaWeek));
  toolbar.Children.Add(Btn("Suppressions récentes",delegate{Go("Suppressions récentes");},false,true));root.Children.Add(toolbar);
  var week=agendaDate.AddDays(-((7+(int)agendaDate.DayOfWeek-1)%7));
  var strip=new UniformGrid{Columns=7,Margin=new Thickness(0,0,0,24)};
  for(int i=0;i<7;i++){var date=week.AddDays(i);var label=new StackPanel();var day=T(date.ToString("ddd",CultureInfo.GetCultureInfo("fr-FR")),10,date==agendaDate?AccentText:Muted);day.TextAlignment=TextAlignment.Center;label.Children.Add(day);var number=T(date.Day.ToString(),21,date==agendaDate?AccentText:Ink,new Thickness(0,6,0,0));number.TextAlignment=TextAlignment.Center;label.Children.Add(number);var button=Btn("",delegate{agendaDate=date;Refresh();},false,true);button.Content=label;button.HorizontalContentAlignment=HorizontalAlignment.Stretch;button.Padding=new Thickness(6,12,6,12);if(date==agendaDate)button.Background=SelectionBrush;strip.Children.Add(button);}root.Children.Add(strip);
  if(agendaWeek){var grid=new UniformGrid{Columns=2};for(int i=0;i<7;i++){var date=week.AddDays(i);var card=Card(date.ToString("dddd d MMMM",CultureInfo.GetCultureInfo("fr-FR")),DaySchedule(date,true));card.Margin=new Thickness(0,0,14,14);card.VerticalAlignment=VerticalAlignment.Top;grid.Children.Add(card);}root.Children.Add(grid);}
  else{
   var grid=new Grid();grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1.7,GridUnitType.Star)});grid.ColumnDefinitions.Add(new ColumnDefinition());
   var day=Card(agendaDate.ToString("dddd d MMMM",CultureInfo.GetCultureInfo("fr-FR")),DaySchedule(agendaDate,false));day.Margin=new Thickness(0,0,18,0);day.VerticalAlignment=VerticalAlignment.Top;grid.Children.Add(day);
   var month=new StackPanel();var calendar=new UniformGrid{Columns=7};
   foreach(string name in new[]{"L","M","M","J","V","S","D"}){var t=T(name,10,Muted,new Thickness(0,0,0,10));t.TextAlignment=TextAlignment.Center;calendar.Children.Add(t);}
   var first=new DateTime(agendaDate.Year,agendaDate.Month,1);int blank=((int)first.DayOfWeek+6)%7;for(int i=0;i<blank;i++)calendar.Children.Add(new Border());
   for(int i=1;i<=DateTime.DaysInMonth(first.Year,first.Month);i++){var date=first.AddDays(i-1);var button=Btn(i.ToString(),delegate{agendaDate=date;Refresh();},false,true);button.Margin=new Thickness(1);button.Padding=new Thickness(2,8,2,8);button.HorizontalContentAlignment=HorizontalAlignment.Center;if(date==agendaDate)button.Background=SelectionBrush;calendar.Children.Add(button);}month.Children.Add(calendar);
   var monthTools=new WrapPanel{Margin=new Thickness(0,14,0,0)};monthTools.Children.Add(Btn("‹ Mois",delegate{agendaDate=agendaDate.AddMonths(-1);Refresh();},false,true));monthTools.Children.Add(Btn("Mois ›",delegate{agendaDate=agendaDate.AddMonths(1);Refresh();},false,true));month.Children.Add(monthTools);
   var card=Card(agendaDate.ToString("MMMM yyyy",CultureInfo.GetCultureInfo("fr-FR")),month);card.VerticalAlignment=VerticalAlignment.Top;Grid.SetColumn(card,1);grid.Children.Add(card);root.Children.Add(grid);
  }
  root.Children.Add(T("Les événements locaux et Google restent distincts ; les événements Google sont modifiés directement sur votre compte.",11,Muted,new Thickness(0,18,0,0)));
 }
 void ReflowBudgetV2(StackPanel root){
  var cards=root.Children.OfType<Border>().Where(b=>Convert.ToString(b.Tag).StartsWith("card:")).ToList();
  var main=cards.FirstOrDefault(b=>Convert.ToString(b.Tag)=="card:Mes opérations");var planned=cards.FirstOrDefault(b=>Convert.ToString(b.Tag)=="card:Mes opérations ponctuelles à venir");
  var side=cards.Where(b=>new[]{"card:Mes enveloppes mensuelles","card:Mes récurrences","card:Prévu jusqu'à la fin du mois"}.Contains(Convert.ToString(b.Tag))).ToList();
  if(main==null||side.Count==0)return;
  root.Children.Remove(main);if(planned!=null)root.Children.Remove(planned);foreach(var card in side)root.Children.Remove(card);
  var columns=new Grid{Margin=new Thickness(0,18,0,0)};columns.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1.55,GridUnitType.Star)});columns.ColumnDefinitions.Add(new ColumnDefinition());
  var journalColumn=new StackPanel{Margin=new Thickness(0,0,18,0)};main.Margin=new Thickness(0,0,0,16);journalColumn.Children.Add(main);if(planned!=null){planned.Margin=new Thickness(0);journalColumn.Children.Add(planned);}columns.Children.Add(journalColumn);
  var sidebar=new StackPanel();foreach(var card in side){card.Margin=new Thickness(0,0,0,16);sidebar.Children.Add(card);}Grid.SetColumn(sidebar,1);columns.Children.Add(sidebar);root.Children.Add(columns);
 }
 void ReflowMusicV2(StackPanel root){
  var cards=root.Children.OfType<Border>().ToList();if(cards.Count!=3)return;foreach(var card in cards)root.Children.Remove(card);
  var grid=new Grid();grid.ColumnDefinitions.Add(new ColumnDefinition());grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1.12,GridUnitType.Star)});
  var player=new StackPanel();cards[0].Margin=new Thickness(0,0,0,16);cards[2].Margin=new Thickness(0);player.Children.Add(cards[0]);player.Children.Add(cards[2]);player.Margin=new Thickness(0,0,18,0);grid.Children.Add(player);
  cards[1].Margin=new Thickness(0);cards[1].VerticalAlignment=VerticalAlignment.Top;Grid.SetColumn(cards[1],1);grid.Children.Add(cards[1]);root.Children.Add(grid);
 }
}

internal sealed partial class ModernWorkspace {
 StackPanel HomeTimelineV2(bool compact){
  var body=new StackPanel();string today=DateTime.Today.ToString("yyyy-MM-dd");
  var appointments=store.Value.events.Where(e=>e.date==today).OrderBy(e=>e.time).ToList();
  foreach(var appointment in appointments.Take(compact?2:4)){
   var item=appointment;var row=new DockPanel{Margin=new Thickness(0,0,0,14)};
   var time=T(item.time,12,Accent,new Thickness(0,3,15,0));time.Width=46;DockPanel.SetDock(time,Dock.Left);row.Children.Add(time);
   var info=new StackPanel();var open=Btn(item.title,delegate{EditAppointment(item,DateTime.Today);},false,true);open.Padding=new Thickness(0);open.HorizontalContentAlignment=HorizontalAlignment.Left;info.Children.Add(open);info.Children.Add(T(item.duration+" min"+(String.IsNullOrEmpty(item.project)?"":" · "+item.project),10,Muted,new Thickness(0,5,0,0)));row.Children.Add(info);body.Children.Add(row);
  }
  if(appointments.Count==0){body.Children.Add(T("Du temps pour ce qui compte.",18,Ink,new Thickness(0,4,0,9)));body.Children.Add(T("Aucun rendez-vous aujourd’hui. Gardez une place pour vos priorités.",12,Muted,new Thickness(0,0,0,18)));}
  var due=store.Value.tasks.Count(t=>t.status!="done"&&t.due==today);
  if(due>0)body.Children.Add(T(due+" tâche(s) à organiser dans votre journée",11,Muted,new Thickness(0,2,0,12)));
  body.Children.Add(Btn("+  Rendez-vous",delegate{EditAppointment(null,DateTime.Today);},false,true));return body;
 }
 StackPanel ProjectListV2(int limit){
  var result=new StackPanel();var config=store.Value.settings.HomeBlocks.FirstOrDefault(b=>b.id=="projects");
  var grid=new UniformGrid{Columns=config!=null&&config.column==2?Math.Min(limit,3):1};
  foreach(string name in owner.AllProjects().Take(limit<=0?100:limit)){
   string project=name;var tasks=store.Value.tasks.Where(t=>t.project==name).ToList();int done=tasks.Count(t=>t.status=="done");
   var body=new StackPanel{Margin=new Thickness(0,0,14,0)};var button=Btn(ProjectHeading(name),delegate{OpenProject(project);},false,true);button.Padding=new Thickness(0,2,0,8);button.HorizontalContentAlignment=HorizontalAlignment.Left;PinMenu(button,"project",name,name);body.Children.Add(button);
   body.Children.Add(T(done+" / "+tasks.Count+" terminées",10,Muted,new Thickness(0,0,0,9)));
   body.Children.Add(new ProgressBar{Value=tasks.Count==0?0:100.0*done/tasks.Count,Maximum=100,Height=4,Foreground=Accent,Background=FaintLine});grid.Children.Add(body);
  }result.Children.Add(grid);return result;
 }
 void BudgetToolbarV2(StackPanel root){
  var tools=new WrapPanel{Margin=new Thickness(0,0,0,16)};
  tools.Children.Add(Btn("+ Dépense",delegate{EditBudgetEntry(null,"expense");},true));tools.Children.Add(Btn("+ Revenu",delegate{EditBudgetEntry(null,"income");}));
  var menu=new ContextMenu{Resources=(ResourceDictionary)System.Windows.Markup.XamlReader.Parse(ResourcesText())};
  Action<string,Action> add=delegate(string text,Action action){var item=new MenuItem{Header=text};item.Click+=delegate{action();};menu.Items.Add(item);};
  add("Renseigner mon solde",delegate{EditBudgetBalance();});add("Découvert autorisé",delegate{EditBudgetOverdraft();});add("Prévoir une dépense",delegate{EditPlannedExpense(null);});add("Créer une récurrence",delegate{EditBudgetRule(null);});add("Budget par catégorie",delegate{EditBudgetCap(null);});
  Button manage=null;manage=Btn("Gérer mon budget  ⌄",delegate{menu.PlacementTarget=manage;menu.IsOpen=true;},false,true);manage.ContextMenu=menu;tools.Children.Add(manage);root.Children.Add(tools);
 }
}
