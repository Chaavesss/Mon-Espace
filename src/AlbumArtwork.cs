using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

internal sealed class AlbumArtwork:Border {
 readonly TextBlock fallback;
 byte[] current;
 internal bool HasImage{get;private set;}
 internal AlbumArtwork(double size,Brush surface,Brush ink){
  Width=Height=size;CornerRadius=new CornerRadius(size<80?10:16);
  placeholder=surface;Background=surface;HorizontalAlignment=HorizontalAlignment.Left;VerticalAlignment=VerticalAlignment.Top;
  fallback=new TextBlock{Text="\u266b",FontSize=size*.38,Foreground=ink,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center};
  Child=fallback;
 }
 internal void SetPalette(Brush surface,Brush ink){placeholder=surface;fallback.Foreground=ink;if(!HasImage)Background=surface;}
 internal void Update(byte[] bytes,string album){
  ToolTip=String.IsNullOrWhiteSpace(album)?"Pochette du morceau":album;
  System.Windows.Automation.AutomationProperties.SetName(this,Convert.ToString(ToolTip));
  if(Object.ReferenceEquals(bytes,current)&&HasImage)return;
  current=bytes;HasImage=false;Child=fallback;Background=placeholder??Background;
  if(bytes==null||bytes.Length==0||bytes.Length>4*1024*1024)return;
  try{
   var bitmap=new BitmapImage();
   using(var stream=new MemoryStream(bytes,false)){
    bitmap.BeginInit();bitmap.CacheOption=BitmapCacheOption.OnLoad;bitmap.DecodePixelWidth=512;bitmap.StreamSource=stream;bitmap.EndInit();bitmap.Freeze();
   }
   if(placeholder==null)placeholder=Background;
   var image=new ImageBrush(bitmap){Stretch=Stretch.UniformToFill};image.Freeze();
   Background=image;Child=null;HasImage=true;
  }catch{Child=fallback;HasImage=false;}
 }
 Brush placeholder;
}
