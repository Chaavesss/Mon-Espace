using System;
using System.Windows;
using System.Windows.Controls;
internal sealed partial class DesktopWidget {
 TextBlock widgetFocusClock;int musicTicks;
 void DrawWidgetFocus(StackPanel body){
  widgetFocusClock=null;var f=store.Value.focus;
  if(!FocusClock.Active(f)&&f.state!="finished"){var start=Button("Se concentrer",delegate{host.OpenFocusFromWidget();});start.Margin=new Thickness(0,0,0,16);body.Children.Add(start);return;}
  var panel=new StackPanel{Margin=new Thickness(0,0,0,20)};panel.Children.Add(Text(f.state=="finished"?"SESSION TERMINÉE":"CONCENTRATION",10,true));
  var title=Text(f.taskTitle,13);title.Margin=new Thickness(0,8,0,4);panel.Children.Add(title);widgetFocusClock=Text("",29);widgetFocusClock.FontFamily=new System.Windows.Media.FontFamily("Georgia");panel.Children.Add(widgetFocusClock);
  var controls=new WrapPanel{Margin=new Thickness(0,9,0,0)};if(FocusClock.Active(f))controls.Children.Add(Button(f.state=="paused"?"Reprendre":"Pause",delegate{try{host.ToggleFocusSession();RefreshView();}catch(Exception ex){message.Text=ex.Message;}},true));controls.Children.Add(Button("Voir la session",delegate{host.OpenFocusFromWidget();}));panel.Children.Add(controls);body.Children.Add(panel);UpdateWidgetFocusClock();
 }
 void UpdateWidgetFocusClock(){if(widgetFocusClock==null||stopping||store.Value.settings.PrivacyMode)return;var f=store.Value.focus;widgetFocusClock.Text=f.state=="finished"?"C'est fait.":FocusClock.Display(f,DateTime.UtcNow)+(f.state=="paused"?" · Pause":"");}
}
