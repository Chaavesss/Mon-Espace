using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

internal static class SiteIcons {
    internal static Bitmap Placeholder(){return Globe();}
    internal static Bitmap Get(string host){return BundledLogos.Get(host)??Load(host);}
    static readonly HashSet<string> Known=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"discord.com","github.com","wispbyte.com","instagram.com","twitch.tv","google.com","spotify.com"};
    static readonly object Gate=new object();
    static readonly Dictionary<string,Bitmap> Memory=new Dictionary<string,Bitmap>();
    internal static string Brand(string url){Uri u;if(!Uri.TryCreate(url,UriKind.Absolute,out u)||(u.Scheme!="http"&&u.Scheme!="https"))return null;string h=u.DnsSafeHost.ToLowerInvariant();foreach(string host in Known)if(h==host||h.EndsWith("."+host,StringComparison.Ordinal))return host;return null;}
    internal static void Bind(Button button,string url){button.TextImageRelation=TextImageRelation.ImageBeforeText;button.ImageAlign=ContentAlignment.MiddleLeft;button.Padding=new Padding(10,4,10,4);button.MinimumSize=new Size(125,42);button.Image=Globe();button.Disposed+=delegate{if(button.Image!=null)button.Image.Dispose();};string brand=Brand(url);if(brand==null)return;
        if(button.IsHandleCreated)Apply(button,brand);else button.HandleCreated+=delegate{Apply(button,brand);};
    }
    static void Apply(Button button,string brand){ThreadPool.QueueUserWorkItem(delegate{try{Bitmap image=Load(brand);if(image==null||button.IsDisposed||!button.IsHandleCreated)return;button.BeginInvoke((Action)delegate{if(button.IsDisposed)return;var old=button.Image;button.Image=new Bitmap(image);if(old!=null)old.Dispose();});}catch{}});}
    static Bitmap Load(string host){lock(Gate){Bitmap cached;if(Memory.TryGetValue(host,out cached))return cached;string dir=Path.Combine(Program.Root,"IconCache"),path=Path.Combine(dir,host+".png");Directory.CreateDirectory(dir);if(File.Exists(path)){try{using(var source=Image.FromFile(path)){cached=Scale(source);Memory[host]=cached;return cached;}}catch{}}
        try{ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;string domain=host=="google.com"?"mail.google.com":host;var request=(HttpWebRequest)WebRequest.Create("https://www.google.com/s2/favicons?domain="+Uri.EscapeDataString(domain)+"&sz=64");request.Timeout=6000;request.ReadWriteTimeout=6000;request.UserAgent="MonEspace/1.0";request.MaximumAutomaticRedirections=3;
            using(var response=request.GetResponse())using(var input=response.GetResponseStream())using(var buffer=new MemoryStream()){var chunk=new byte[8192];int count;while((count=input.Read(chunk,0,chunk.Length))>0){if(buffer.Length+count>256*1024)return null;buffer.Write(chunk,0,count);}buffer.Position=0;using(var source=Image.FromStream(buffer)){if(source.Width>512||source.Height>512)return null;cached=Scale(source);cached.Save(path,ImageFormat.Png);Memory[host]=cached;return cached;}}
        }catch{return null;}
    }}
    static Bitmap Scale(Image source){var target=new Bitmap(24,24);using(var g=Graphics.FromImage(target)){g.SmoothingMode=SmoothingMode.AntiAlias;g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.DrawImage(source,0,0,24,24);}return target;}
    static Bitmap Globe(){var image=new Bitmap(24,24);using(var g=Graphics.FromImage(image))using(var pen=new Pen(Color.FromArgb(179,168,149),1.5f)){g.SmoothingMode=SmoothingMode.AntiAlias;g.DrawEllipse(pen,3,3,18,18);g.DrawEllipse(pen,8,3,8,18);g.DrawLine(pen,3,12,21,12);g.DrawArc(pen,4,6,16,7,0,180);}return image;}
}
