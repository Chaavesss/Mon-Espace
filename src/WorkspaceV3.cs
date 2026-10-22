using System;
using System.Linq;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
internal sealed partial class ModernWorkspace {
 TextBlock Eyebrow(string text){var t=T(text.ToUpper(CultureInfo.GetCultureInfo("fr-FR")),10,Muted);t.FontWeight=FontWeights.SemiBold;return t;}
 Border Pill(string label,Brush foreground=null){return new Border{Background=SelectionBrush,CornerRadius=new CornerRadius(20),Padding=new Thickness(10,5,10,5),Child=T(label,10,foreground??AccentText)};}
 void PageHeaderV3(StackPanel wrapper){
  var header=new Grid{Margin=new Thickness(0,0,10,28)};header.ColumnDefinitions.Add(new ColumnDefinition());header.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
  var labels=new StackPanel();
  var date=Eyebrow(selected=="Aujourd’hui"?DateTime.Now.ToString("dddd d MMMM",CultureInfo.GetCultureInfo("fr-FR")):"MON ESPACE / "+ShortPage(selected));labels.Children.Add(date);
  string greeting=DateTime.Now.Hour<12?"Bonjour":DateTime.Now.Hour<18?"Bon après-midi":"Bonsoir";
  string title=selected=="Documents locaux"&&!String.IsNullOrEmpty(openDocument)?System.IO.Path.GetFileName(openDocument):selected=="Aujourd’hui"?greeting+(String.IsNullOrWhiteSpace(store.Value.settings.DisplayName)?"":" "+store.Value.settings.DisplayName)+".":selected=="Mes projets"&&!String.IsNullOrEmpty(openProject)?openProject:selected;
  var h=T(title,34,Ink,new Thickness(0,8,0,8));h.FontFamily=new FontFamily("Georgia");labels.Children.Add(h);
  if(updateFound!=null)labels.Children.Add(Btn("Version "+updateFound.version+" disponible",delegate{Go("Mises à jour");},false,true));labels.Children.Add(T(selected=="Aujourd’hui"?"Tout ce qui compte, au même endroit.":PageDescription(),13,Muted));header.Children.Add(labels);
  var actions=new StackPanel{Orientation=Orientation.Horizontal,VerticalAlignment=VerticalAlignment.Center};
  if(selected=="Aujourd’hui"){var customize=Btn("Personnaliser",delegate{EditHome();},false,true);customize.ToolTip="Choisir et organiser les blocs de l’accueil";actions.Children.Add(customize);}
  actions.Children.Add(Btn(store.Value.settings.Desk.enabled?"Un panneau":"Deux panneaux",delegate{ToggleDesk();},false,true));actions.Children.Add(Btn("+  Capturer",delegate{owner.ShowQuickCapture();},true));if(store.Value.settings.Desk.enabled){header.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});header.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});Grid.SetColumnSpan(labels,2);Grid.SetRow(actions,1);actions.Margin=new Thickness(0,14,0,0);}else Grid.SetColumn(actions,1);header.Children.Add(actions);wrapper.Children.Add(header);
 }
 void HomeOverviewV3(StackPanel root){
  var active=DayDashboard.Tasks(store.Value).Where(t=>t.status!="done").ToList();string today=DateTime.Today.ToString("yyyy-MM-dd");
  var focus=DayDashboard.Priorities(store.Value,DateTime.Today).FirstOrDefault(t=>t.status!="done");
  var next=NextDayAppointment(DateTime.Now);
  if(store.Value.settings.ShowBanner){
   var layout=new Grid{Margin=new Thickness(0,0,10,20)};layout.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1.65,GridUnitType.Star)});layout.ColumnDefinitions.Add(new ColumnDefinition());
   var main=new Grid{MinHeight=150};main.ColumnDefinitions.Add(new ColumnDefinition());main.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(72)});
   var copy=new StackPanel();var eyebrow=Eyebrow("Votre prochaine avancée");eyebrow.Foreground=AccentText;copy.Children.Add(eyebrow);
   var title=T(focus==null?"Laissez une place à vos idées.":focus.title,26,Ink,new Thickness(0,15,0,10));title.FontFamily=new FontFamily("Georgia");title.LineHeight=32;copy.Children.Add(title);
   copy.Children.Add(T(focus==null?"Une première tâche suffit pour commencer.":(String.IsNullOrEmpty(focus.project)?"Personnel":focus.project)+"  ·  "+(focus.priority=="Haute"?"Priorité haute":"À votre rythme"),12,Muted,new Thickness(0,0,0,22)));
   var begin=Btn(focus==null?"+  Créer ma première tâche":"Me concentrer  →",delegate{if(focus==null)owner.Edit(null);else Go("Concentration");},true);begin.HorizontalAlignment=HorizontalAlignment.Left;copy.Children.Add(begin);main.Children.Add(copy);
   var ornament=new Grid{Width=62,Height=100,VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(0,28,0,0)};
   ornament.Children.Add(new Ellipse{Width=60,Height=60,Stroke=new SolidColorBrush(Appearance.Mix(CurrentPalette.Card,CurrentPalette.Accent,.4)),StrokeThickness=1.2});
   ornament.Children.Add(new Ellipse{Width=35,Height=35,Stroke=new SolidColorBrush(Appearance.Mix(CurrentPalette.Card,CurrentPalette.Accent,.28)),StrokeThickness=1.2});
   ornament.Children.Add(new Ellipse{Width=9,Height=9,Fill=Accent,HorizontalAlignment=HorizontalAlignment.Right,VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(0,24,1,0)});Grid.SetColumn(ornament,1);main.Children.Add(ornament);
   var hero=Box(main,new Thickness(24));hero.Background=new LinearGradientBrush(Appearance.Mix(CurrentPalette.Card,CurrentPalette.Accent,.15),CurrentPalette.Card,35);hero.BorderThickness=new Thickness(1);hero.BorderBrush=new SolidColorBrush(Appearance.Mix(CurrentPalette.Card,CurrentPalette.Accent,.2));hero.Margin=new Thickness(0,0,16,0);layout.Children.Add(hero);
   var upcoming=new StackPanel();upcoming.Children.Add(Eyebrow("À l’horizon"));
   var time=T(next==null?"À votre rythme":next.time,30,Ink,new Thickness(0,18,0,9));time.FontFamily=new FontFamily("Georgia");upcoming.Children.Add(time);
   upcoming.Children.Add(T(next==null?"Votre agenda respire.":next.title,15,Ink,new Thickness(0,0,0,9)));
   upcoming.Children.Add(T(next==null?"Profitez-en pour réserver un moment pour vous.":Schedule.Day(next.date).ToString("dddd d MMMM",CultureInfo.GetCultureInfo("fr-FR")),12,Muted,new Thickness(0,0,0,20)));
   upcoming.Children.Add(Btn("Ouvrir l’agenda  ↗",delegate{Go("Agenda");},false,true));
   var nextCard=Box(upcoming,new Thickness(26));Grid.SetColumn(nextCard,1);layout.Children.Add(nextCard);root.Children.Add(layout);
  }
  if(store.Value.settings.ShowStats){
   var stats=new UniformGrid{Columns=4,Margin=new Thickness(0,0,0,24)};
   string[][] values={new[]{active.Count(t=>!String.IsNullOrEmpty(t.due)&&String.CompareOrdinal(t.due,today)<=0).ToString(),"À faire aujourd’hui","Mes tâches"},new[]{active.Count(t=>t.status=="doing").ToString(),"En cours","Mes tâches"},new[]{(store.Value.events.Count(e=>e.date==today)+GoogleTodayCount()).ToString(),"Rendez-vous","Agenda"},new[]{owner.AllProjects().Count.ToString(),"Projets ouverts","Mes projets"}};
   foreach(var value in values){string target=value[2];var grid=new DockPanel();var number=T(value[0],25,Ink,new Thickness(0,0,12,0));number.FontFamily=new FontFamily("Georgia");DockPanel.SetDock(number,Dock.Left);grid.Children.Add(number);var label=T(value[1],11,Muted);label.VerticalAlignment=VerticalAlignment.Center;grid.Children.Add(label);var button=Btn("",delegate{Go(target);},false,true);button.Content=grid;button.HorizontalContentAlignment=HorizontalAlignment.Stretch;button.Background=SurfaceBrush;button.Margin=new Thickness(0,0,10,0);button.Padding=new Thickness(16,13,12,13);stats.Children.Add(button);}root.Children.Add(stats);
  }
 }
 void SettingAction(StackPanel body,string title,string detail,Action action){
  var row=new Grid();row.ColumnDefinitions.Add(new ColumnDefinition());row.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
  var text=new StackPanel();text.Children.Add(T(title,13));text.Children.Add(T(detail,11,Muted,new Thickness(0,5,14,0)));row.Children.Add(text);var button=Btn("Ouvrir  ↗",action,false,true);Grid.SetColumn(button,1);button.VerticalAlignment=VerticalAlignment.Center;row.Children.Add(button);
  body.Children.Add(new Border{Child=row,Padding=new Thickness(0,13,0,13),BorderBrush=FaintLine,BorderThickness=new Thickness(0,0,0,1)});
 }
 void PreferencesV3(StackPanel root){
  var grid=new Grid();grid.ColumnDefinitions.Add(new ColumnDefinition());grid.ColumnDefinitions.Add(new ColumnDefinition());
  var personal=new StackPanel();SettingAction(personal,"Apparence & thèmes","Couleurs, fond et dégradés",delegate{EditAppearance();});SettingAction(personal,"Mon accueil","Choix, ordre et taille des blocs",delegate{EditHome();});SettingAction(personal,"Mon profil & mes raccourcis","Prénom, applications et liens favoris",delegate{owner.Personalize();});SettingAction(personal,"Assistant de configuration","Reprendre la configuration de cet espace",delegate{owner.SetupWorkspace();});
  var personalCard=Card("Un espace à votre image",personal);personalCard.Margin=new Thickness(0,0,16,18);grid.Children.Add(personalCard);
  var tools=new StackPanel();SettingAction(tools,"Documents intégrés","PDF, images et textes dans vos onglets",delegate{Go("Documents locaux");});SettingAction(tools,"Mises à jour","Vérifier et télécharger une version",delegate{Go("Mises à jour");});SettingAction(tools,"Widget de bureau","Retrouver l’essentiel dans une petite fenêtre",delegate{owner.ShowDesktopWidget();});SettingAction(tools,"Musique & voix","Routine du matin et synthèse vocale",delegate{owner.AudioSettings();});SettingAction(tools,"Google Drive & Sheets","Connexion et documents épinglés",delegate{Go("Google Drive");});SettingAction(tools,"Suppressions récentes","Récupérer une tâche ou un rendez-vous",delegate{Go("Suppressions récentes");});
  var toolsCard=Card("Vos outils",tools);toolsCard.Margin=new Thickness(0,0,0,18);Grid.SetColumn(toolsCard,1);grid.Children.Add(toolsCard);root.Children.Add(grid);
  var startupBody=new StackPanel();var startup=new CheckBox{Content="Ouvrir à ma connexion Windows",IsChecked=Program.Startup()};startup.Click+=delegate{try{Program.SetStartup(startup.IsChecked==true);}catch(Exception e){MessageBox.Show(e.Message);}};startupBody.Children.Add(startup);
  var auto=new CheckBox{Content="Lancer ma routine à la connexion",IsChecked=store.Value.settings.AutoMorning};auto.Click+=delegate{store.Value.settings.AutoMorning=auto.IsChecked==true;if(!store.Value.settings.AutoMorning)owner.CancelCountdown();owner.Save();};startupBody.Children.Add(auto);
  var once=new CheckBox{Content="Une seule routine automatique par jour",IsChecked=store.Value.settings.OnceDaily};once.Click+=delegate{store.Value.settings.OnceDaily=once.IsChecked==true;owner.Save();};startupBody.Children.Add(once);
  var capture=new CheckBox{Content="Activer la capture rapide · Ctrl + Alt + N",IsChecked=store.Value.settings.QuickCapture};capture.Click+=delegate{store.Value.settings.QuickCapture=capture.IsChecked==true;owner.UpdateCaptureHotkey();owner.Save();Refresh();};startupBody.Children.Add(capture);startupBody.Children.Add(T(owner.CaptureStatus,11,Muted,new Thickness(0,9,0,0)));
  var startupCard=Card("Dès le démarrage",startupBody);startupCard.Margin=new Thickness(0,0,0,18);root.Children.Add(startupCard);
  var backups=new StackPanel();BackupSettings(backups);SettingAction(backups,"Exporter mes données","Conserver une copie au format JSON",delegate{owner.Export();});SettingAction(backups,"Importer des données","Retrouver des données depuis un fichier JSON",delegate{owner.Import();});backups.Children.Add(Btn("Ouvrir mes sauvegardes",delegate{Open(System.IO.Path.Combine(store.Root,"Backups"));},false,true));root.Children.Add(Card("Vos données",backups));
  var agenda=new StackPanel();AddAgendaSettings(agenda);var agendaCard=Card("Agenda & rappels",agenda);agendaCard.Margin=new Thickness(0,18,0,0);root.Children.Add(agendaCard);
 }
 void NotesV3(StackPanel root){
  var tools=new WrapPanel{Margin=new Thickness(0,0,0,20)};tools.Children.Add(Btn("+  Nouvelle page",delegate{EditNote(null,"");},true));tools.Children.Add(Btn(showTrash?"Toutes mes pages":"Corbeille",delegate{showTrash=!showTrash;Refresh();},false,true));
  var search=new TextBox{Text=noteQuery,Width=230,Margin=new Thickness(10,0,8,0),ToolTip="Rechercher dans les titres et le contenu"};search.VerticalContentAlignment=VerticalAlignment.Center;search.KeyDown+=delegate(object sender,System.Windows.Input.KeyEventArgs e){if(e.Key==System.Windows.Input.Key.Enter){noteQuery=search.Text;Refresh();e.Handled=true;}};tools.Children.Add(search);tools.Children.Add(Btn("Rechercher",delegate{noteQuery=search.Text;Refresh();}));root.Children.Add(tools);
  var pages=store.Value.pages.Where(n=>n.trashed==showTrash&&(String.IsNullOrWhiteSpace(noteQuery)||(n.title+" "+n.body+" "+n.project).IndexOf(noteQuery,StringComparison.OrdinalIgnoreCase)>=0)).OrderByDescending(n=>n.updated).ToList();
  if(pages.Count==0){root.Children.Add(Card(showTrash?"La corbeille est vide":"Une place pour vos idées",T("Créez une page, ajoutez du texte, des titres et des cases à cocher.",13,Muted)));return;}
  var grid=new UniformGrid{Columns=2};
  foreach(var item in pages){var note=item;var body=new StackPanel();var top=new DockPanel{Margin=new Thickness(0,0,0,18)};var icon=LineIcon("Mes notes",AccentText);DockPanel.SetDock(icon,Dock.Left);top.Children.Add(icon);top.Children.Add(T(String.IsNullOrEmpty(note.project)?"Note personnelle":note.project,11,Muted));body.Children.Add(top);
   var title=T(note.title,22,Ink,new Thickness(0,0,0,13));title.FontFamily=new FontFamily("Georgia");body.Children.Add(title);
   var snippet=new StackPanel{MinHeight=76};foreach(string line in note.body.Replace("\r\n","\n").Split('\n').Where(l=>!String.IsNullOrWhiteSpace(l)).Take(3)){
    string text=line;bool checklist=text.StartsWith("- [ ] ")||text.StartsWith("- [x] ")||text.StartsWith("- [X] ");if(checklist)text="○  "+text.Substring(6);else if(text.StartsWith("## "))text=text.Substring(3);else if(text.StartsWith("# "))text=text.Substring(2);else if(text.StartsWith("- "))text="•  "+text.Substring(2);
    var row=T(text,12,Muted,new Thickness(0,0,0,7));row.MaxHeight=36;row.TextTrimming=TextTrimming.CharacterEllipsis;snippet.Children.Add(row);
   }body.Children.Add(snippet);
   var open=Btn(showTrash?"Restaurer":"Lire la page  ↗",delegate{if(showTrash){note.trashed=false;Persist();}else ReadNote(note);},false,true);open.HorizontalAlignment=HorizontalAlignment.Left;open.Padding=new Thickness(0,12,0,0);body.Children.Add(open);
   var card=Box(body);card.Margin=new Thickness(0,0,16,16);card.VerticalAlignment=VerticalAlignment.Top;grid.Children.Add(card);
  }root.Children.Add(grid);
 }

}