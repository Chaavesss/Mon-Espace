using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

// Embedded brand glyphs. No network access is needed to display these marks.
internal static class BundledLogos {
    static Brush Color(string value){return (Brush)new BrushConverter().ConvertFromString(value);}
    internal static System.Drawing.Bitmap Get(string host){
        var visual=new DrawingVisual();bool known=true;
        using(var d=visual.RenderOpen()){
            d.PushTransform(new ScaleTransform(64.0/24,64.0/24));
            if(host=="discord.com")Shape(d,"M20.317 4.37a19.791 19.791 0 0 0-4.885-1.515.074.074 0 0 0-.079.037c-.211.375-.445.865-.608 1.25a18.27 18.27 0 0 0-5.487 0 12.64 12.64 0 0 0-.617-1.25.077.077 0 0 0-.079-.037A19.736 19.736 0 0 0 3.677 4.37a.07.07 0 0 0-.032.027C.533 9.046-.32 13.58.099 18.057a.082.082 0 0 0 .031.055 19.9 19.9 0 0 0 5.993 3.03.078.078 0 0 0 .084-.028c.462-.63.874-1.295 1.226-1.994a.076.076 0 0 0-.041-.106 13.107 13.107 0 0 1-1.872-.892.077.077 0 0 1-.008-.128 10.2 10.2 0 0 0 .372-.292.074.074 0 0 1 .077-.01c3.928 1.793 8.18 1.793 12.061 0a.074.074 0 0 1 .078.01c.12.098.246.196.373.292a.077.077 0 0 1-.006.127 12.299 12.299 0 0 1-1.873.892.077.077 0 0 0-.04.107c.36.698.772 1.362 1.225 1.993a.076.076 0 0 0 .084.028 19.839 19.839 0 0 0 6.002-3.03.077.077 0 0 0 .031-.054c.5-5.177-.838-9.674-3.549-13.66a.061.061 0 0 0-.031-.03zM8.02 15.331c-1.183 0-2.157-1.085-2.157-2.419 0-1.333.955-2.419 2.157-2.419 1.211 0 2.176 1.096 2.157 2.419 0 1.334-.955 2.419-2.157 2.419zm7.975 0c-1.183 0-2.157-1.085-2.157-2.419 0-1.333.955-2.419 2.157-2.419 1.211 0 2.176 1.096 2.157 2.419 0 1.334-.946 2.419-2.157 2.419z", "#5865F2");
            else if(host=="github.com")Shape(d,"M12 .297C5.37.297 0 5.67 0 12.297c0 5.303 3.438 9.8 8.205 11.385.6.113.82-.258.82-.577 0-.285-.01-1.04-.015-2.04-3.338.724-4.043-1.61-4.043-1.61-.546-1.387-1.333-1.756-1.333-1.756-1.09-.745.083-.729.083-.729 1.205.084 1.838 1.237 1.838 1.237 1.07 1.834 2.807 1.304 3.492.997.108-.775.418-1.305.762-1.605-2.665-.305-5.467-1.334-5.467-5.931 0-1.31.469-2.381 1.236-3.221-.124-.303-.536-1.523.117-3.176 0 0 1.008-.322 3.301 1.23a11.49 11.49 0 0 1 3.003-.404c1.018.005 2.042.138 3.003.404 2.291-1.552 3.297-1.23 3.297-1.23.655 1.653.243 2.873.12 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222 0 1.606-.015 2.898-.015 3.293 0 .319.216.694.825.576C20.565 22.092 24 17.592 24 12.297c0-6.627-5.373-12-12-12", "#F0F0F0");
            else if(host=="twitch.tv")Shape(d,"M2.149 0 .537 4.119v16.835h5.732V24h3.224l3.224-3.224h4.657l6.089-6.448V0H2.149zm19.165 13.254-3.582 3.582H12l-3.224 3.224v-3.224H3.94V1.612h17.374v11.642zM17.732 6.448h1.612v4.835h-1.612zM12.358 6.448h1.612v4.835h-1.612z", "#9146FF");
            else if(host=="google.com"){
                Shape(d,"M0 5.457v13.909c0 .904.733 1.636 1.636 1.636h3.819V9.55L0 5.457Z","#4285F4");
                Shape(d,"M18.545 9.55v11.452h3.819A1.636 1.636 0 0 0 24 19.366V5.457l-5.455 4.093Z","#34A853");
                Shape(d,"M5.455 4.64 12 9.548l6.545-4.91v6.82L12 16.64l-6.545-4.91V4.64Z","#EA4335");
                Shape(d,"M0 5.457c0-2.023 2.31-3.18 3.927-1.964L5.455 4.64v6.819L0 7.366V5.457Z","#C5221F");
                Shape(d,"m18.545 4.638 1.528-1.145C21.69 2.278 24 3.434 24 5.457v1.91l-5.455 4.091v-6.82Z","#FBBC04");
            }
            else if(host=="instagram.com"){
                var gradient=new LinearGradientBrush();gradient.StartPoint=new Point(0,1);gradient.EndPoint=new Point(1,0);gradient.GradientStops.Add(new GradientStop(System.Windows.Media.Colors.Gold,0));gradient.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromRgb(242,74,90),.45));gradient.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromRgb(170,47,183),.75));gradient.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromRgb(77,91,211),1));d.DrawRoundedRectangle(gradient,null,new Rect(0,0,24,24),5.5,5.5);var pen=new Pen(Brushes.White,1.8);d.DrawRoundedRectangle(null,pen,new Rect(4.5,4.5,15,15),4.5,4.5);d.DrawEllipse(null,pen,new Point(12,12),3.8,3.8);d.DrawEllipse(Brushes.White,null,new Point(17,7),1.05,1.05);
            }
            else if(host=="spotify.com"){
                d.DrawEllipse(Color("#1DB954"),null,new Point(12,12),12,12);Stroke(d,"M5 9C9 7 15.5 7 19.5 9.4",2.1);Stroke(d,"M5.8 12.5C9.8 10.8 14.9 11 18.3 13",1.8);Stroke(d,"M6.6 16C10 14.6 14 14.8 17 16.4",1.5);
            }
            else known=false;d.Pop();
        }
        if(!known)return null;var bitmap=new RenderTargetBitmap(64,64,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using(var buffer=new MemoryStream()){encoder.Save(buffer);buffer.Position=0;using(var source=new System.Drawing.Bitmap(buffer))return new System.Drawing.Bitmap(source);}
    }
    static void Shape(DrawingContext d,string path,string color){d.DrawGeometry(Color(color),null,Geometry.Parse(path));}
    static void Stroke(DrawingContext d,string path,double size){d.DrawGeometry(null,new Pen(Brushes.Black,size){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round},Geometry.Parse(path));}
}
