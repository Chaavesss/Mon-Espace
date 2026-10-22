using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
internal static class PolishedControls {
 internal static Button Media(Button button,string kind,bool prominent=false){
  string shape=kind=="previous"?"M3,3 L5,3 L5,17 L3,17 Z M17,3 L7,10 L17,17 Z":kind=="next"?"M15,3 L17,3 L17,17 L15,17 Z M3,3 L13,10 L3,17 Z":kind=="stop"?"M4,4 L16,4 L16,16 L4,16 Z":kind=="settings"?"M3,5 L17,5 L17,7 L3,7 Z M3,13 L17,13 L17,15 L3,15 Z M7,2 L9,2 L9,10 L7,10 Z M12,10 L14,10 L14,18 L12,18 Z":kind=="audio"?"M2,7 L6,7 L11,3 L11,17 L6,13 L2,13 Z M14,5 L16,7 L17,10 L16,13 L14,15 L13,13 L15,10 L13,7 Z":"M5,2 L18,10 L5,18 Z";
  string template=@"<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='Button'>
<Border x:Name='Face' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding Foreground}' BorderThickness='0' CornerRadius='100' RenderTransformOrigin='.5,.5'>
<Border.RenderTransform><ScaleTransform ScaleX='1' ScaleY='1'/></Border.RenderTransform>
<Viewbox Width='20' Height='20' HorizontalAlignment='Center' VerticalAlignment='Center'><Canvas Width='20' Height='20'><Path x:Name='Glyph' Fill='{TemplateBinding Foreground}' Data='"+shape+@"'/></Canvas></Viewbox>
</Border><ControlTemplate.Triggers>
<Trigger Property='Content' Value='Pause'><Setter TargetName='Glyph' Property='Data' Value='M4,3 L8,3 L8,17 L4,17 Z M12,3 L16,3 L16,17 L12,17 Z'/></Trigger>
<Trigger Property='IsMouseOver' Value='True'><Setter TargetName='Face' Property='Opacity' Value='.8'/></Trigger>
<Trigger Property='IsPressed' Value='True'><Setter TargetName='Face' Property='RenderTransform'><Setter.Value><ScaleTransform ScaleX='.93' ScaleY='.93'/></Setter.Value></Setter></Trigger>
<Trigger Property='IsKeyboardFocused' Value='True'><Setter TargetName='Face' Property='BorderThickness' Value='2'/></Trigger>
<Trigger Property='IsEnabled' Value='False'><Setter TargetName='Face' Property='Opacity' Value='.35'/></Trigger>
</ControlTemplate.Triggers></ControlTemplate>";
  button.Template=(ControlTemplate)XamlReader.Parse(template);button.Width=button.Height=prominent?52:38;button.Padding=new Thickness(0);button.MinWidth=0;button.MinHeight=0;button.VerticalAlignment=VerticalAlignment.Center;
  button.SetBinding(FrameworkElement.ToolTipProperty,new System.Windows.Data.Binding("Content"){Source=button});return button;
 }
}
