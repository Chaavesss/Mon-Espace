using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
internal static class AppIdentity {
 internal static System.Drawing.Icon Icon(){
  using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("MonEspace.Icon")){
   if(stream==null)return (System.Drawing.Icon)System.Drawing.SystemIcons.Application.Clone();
   using(var icon=new System.Drawing.Icon(stream,32,32))return (System.Drawing.Icon)icon.Clone();
  }
 }
 internal static UIElement Mark(){
  using(var icon=Icon()){
   var bitmap=System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle,Int32Rect.Empty,System.Windows.Media.Imaging.BitmapSizeOptions.FromWidthAndHeight(32,32));
   bitmap.Freeze();return new Image{Source=bitmap,Width=24,Height=24,Stretch=Stretch.Uniform,VerticalAlignment=VerticalAlignment.Center,HorizontalAlignment=HorizontalAlignment.Center};
  }
 }
}
