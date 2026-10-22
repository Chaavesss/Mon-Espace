using System;
using System.IO;
using System.Drawing;
internal static class SongesOcrAcceptance {
 internal static int Run(string folder){try{var engine=new SongesOcr();using(var start=new Bitmap(Path.Combine(folder,"start.png")))using(var win=new Bitmap(Path.Combine(folder,"victory.png")))using(var blank=new Bitmap(2048,1217)){var a=engine.Read(start).GetAwaiter().GetResult();var b=engine.Read(win).GetAwaiter().GetResult();var c=engine.Read(blank).GetAwaiter().GetResult();if(!a.EndTurn||a.Victory||!b.Victory||b.EndTurn||c.EndTurn||c.Victory||a.Depth!=5||b.Depth!=9)throw new Exception("Reconnaissance des repères fournis : start="+a.EndTurn+" depth="+a.Depth+" victory="+b.Victory+" depth="+b.Depth);File.WriteAllText(Path.Combine(folder,"SONGES_OCR_PASS.txt"),"PASS: actual user-provided FIN DE TOUR glyphs, yellow VICTOIRE banner and depth labels, on neutral test frames. Blank negative frame. Live capture on user's game still needs verification.");}return 0;}catch(Exception ex){File.WriteAllText(Path.Combine(folder,"SONGES_OCR_FAIL.txt"),ex.ToString());return 1;}}
}
