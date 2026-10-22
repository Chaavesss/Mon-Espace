using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
internal static class ArtworkAcceptance {
 internal static byte[] Demo(){
  using(var bitmap=new System.Drawing.Bitmap(256,256))
  using(var graphics=System.Drawing.Graphics.FromImage(bitmap))
  using(var output=new MemoryStream()){
   graphics.Clear(System.Drawing.Color.FromArgb(47,43,35));
   using(var brush=new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(246,173,66)))graphics.FillEllipse(brush,40,40,176,176);
   using(var brush=new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(47,43,35)))graphics.FillEllipse(brush,90,90,76,76);
   bitmap.Save(output,System.Drawing.Imaging.ImageFormat.Png);return output.ToArray();
  }
 }
 internal static void Verify(){
  var tile=new AlbumArtwork(64,Brushes.Black,Brushes.White);var image=Demo();
  tile.Update(image,"Album exemple");if(!tile.HasImage)throw new Exception("Pochette PNG non chargée");
  tile.Update(image,"Album exemple");if(!tile.HasImage)throw new Exception("Cache pochette");
  tile.Update(new byte[]{1,2,3},"Invalide");if(tile.HasImage||tile.Child==null||tile.Background is ImageBrush)throw new Exception("Ancienne pochette conservée après image invalide");
  tile.Update(image,"Autre album");tile.Update(null,"");if(tile.HasImage||tile.Child==null||tile.Background is ImageBrush)throw new Exception("Pochette conservée sans morceau");
  tile.Update(new byte[4*1024*1024+1],"Trop grande");if(tile.HasImage)throw new Exception("Limite pochette");
 }
}
