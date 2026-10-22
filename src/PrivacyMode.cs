using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
internal sealed partial class ModernWorkspace {
 Button privacyButton;readonly List<Window> sensitiveDialogs=new List<Window>();
 bool? ShowWindow(Window window){
  if(store.Value.settings.PrivacyMode){window.Close();return false;}
  sensitiveDialogs.Add(window);try{return ShowWindowCore(window);}finally{sensitiveDialogs.Remove(window);}
 }
 internal void HideSensitiveDialogs(){foreach(var window in sensitiveDialogs.ToList())try{window.DialogResult=false;}catch{try{window.Close();}catch{}}}
 bool RenderPrivacy(){RefreshNoticeBadge(false);
  if(!store.Value.settings.PrivacyMode){if(privacyButton!=null)privacyButton.Content="Discret";if(themeFoot!=null)themeFoot.Visibility=Visibility.Visible;return false;}
  if(workspaceTabs!=null)workspaceTabs.Visibility=Visibility.Collapsed;UpdateMiniPlayer();ApplyAppearance();owner.Text="Mon espace · Mode discret";brand.Text="Mon espace";profile.Text="Mode discret";crumb.Text="Mon espace / Affichage masqué";track.Text="Mode discret";status.Text="Contenus personnels masqués";countdownBox.Visibility=Visibility.Collapsed;if(privacyButton!=null)privacyButton.Content="Afficher";
  if(themeFoot!=null)themeFoot.Visibility=Visibility.Collapsed;nav.Children.Clear();nav.Children.Add(T("MODE DISCRET",10,Muted));nav.Children.Add(T("Les informations personnelles sont masquées dans Mon espace.",12,Muted,new Thickness(0,16,0,20)));nav.Children.Add(Btn("Afficher mes données",delegate{owner.TogglePrivacyMode();},true));
  var root=new StackPanel{Margin=new Thickness(0,50,0,0)};root.Children.Add(T("Un écran tranquille.",32));root.Children.Add(T("Les tâches, notes, projets, montants et titres musicaux sont masqués. La capture rapide, la voix et les nouvelles notifications sont suspendues.",14,Muted,new Thickness(0,22,0,22)));root.Children.Add(Btn("Désactiver le mode discret",delegate{owner.TogglePrivacyMode();},true));content.Content=root;return true;
 }
 internal void ResumeMusicAfterPrivacy(){if(musicTimer!=null&&!owner.IsPreview)musicTimer.Start();}
}
internal sealed partial class MainForm {
 internal void TogglePrivacyMode(){
  bool next=!store.Value.settings.PrivacyMode;
  try{PersonalWorkflows.Change(store,delegate{store.Value.settings.PrivacyMode=next;});}
  catch(Exception ex){notice.Text="Mode discret non enregistré : "+ex.Message;return;}
  if(next){CancelCountdown();Stop();modern.HideSensitiveDialogs();modern.CancelModeLaunch();modern.CancelGoogle();modern.CancelMusic();}
  else{Text=store.Value.settings.AppTitle;modern.ResumeMusicAfterPrivacy();}
  modern.Refresh();if(desktopWidget!=null)desktopWidget.RefreshView();
 }
}
internal sealed partial class DesktopWidget {
 bool DrawPrivateWidget(){
  if(!store.Value.settings.PrivacyMode)return false;Title="Widget · Mon espace";var body=new StackPanel{Margin=new Thickness(20)};
  body.Children.Add(Text("Mode discret",22));var text=Text("Vos contenus sont masqués.",13,true);text.Margin=new Thickness(0,20,0,25);body.Children.Add(text);body.Children.Add(Button("Afficher mes données",delegate{host.TogglePrivacyMode();}));body.Children.Add(Button("Fermer le widget",delegate{Close();}));
  Content=new Border{Background=Brush(palette.Card),BorderBrush=Brush(palette.Border),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(16),Child=body};return true;
 }
}
