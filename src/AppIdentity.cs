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
  var glyph=new System.Windows.Shapes.Path{Data=Geometry.Parse("M5,18 L5,11 C5,5 12,5 12,11 L12,18 M12,11 C12,5 19,5 19,11 L19,18"),StrokeThickness=2.3,StrokeStartLineCap=PenLineCap.Round,StrokeEndLineCap=PenLineCap.Round};
  glyph.SetResourceReference(System.Windows.Shapes.Shape.StrokeProperty,"Accent");
  var grid=new Grid{Width=24,Height=24,VerticalAlignment=VerticalAlignment.Center,HorizontalAlignment=HorizontalAlignment.Center};grid.Children.Add(glyph);
  var dot=new System.Windows.Shapes.Ellipse{Width=2.2,Height=2.2,HorizontalAlignment=HorizontalAlignment.Right,VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(0,4,1,0)};dot.SetResourceReference(System.Windows.Shapes.Shape.FillProperty,"Accent");grid.Children.Add(dot);return grid;
 }
}
