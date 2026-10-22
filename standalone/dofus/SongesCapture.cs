using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
internal sealed class SongesGameWindow {internal IntPtr Handle;internal int ProcessId;internal string Title;public override string ToString(){return Title+" · "+ProcessId;}}
internal static class SongesCapture {
 delegate bool EnumCallback(IntPtr window,IntPtr parameter);
 [StructLayout(LayoutKind.Sequential)]struct Rect {public int left,top,right,bottom;}
 [StructLayout(LayoutKind.Sequential)]struct Point {public int x,y;}
 [DllImport("user32.dll")]static extern bool EnumWindows(EnumCallback callback,IntPtr parameter);
 [DllImport("user32.dll")]static extern bool IsWindowVisible(IntPtr window);
 [DllImport("user32.dll")]static extern bool IsIconic(IntPtr window);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern int GetWindowText(IntPtr window,StringBuilder title,int count);
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr window,out int pid);
 [DllImport("user32.dll")]static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")]static extern bool GetClientRect(IntPtr window,out Rect rect);
 [DllImport("user32.dll")]static extern bool ClientToScreen(IntPtr window,ref Point point);
 [DllImport("user32.dll")]internal static extern bool SetProcessDPIAware();
 internal static List<SongesGameWindow> List(){var list=new List<SongesGameWindow>();EnumWindows(delegate(IntPtr window,IntPtr unused){if(!IsWindowVisible(window))return true;var name=new StringBuilder(512);GetWindowText(window,name,name.Capacity);int pid;GetWindowThreadProcessId(window,out pid);try{using(var p=Process.GetProcessById(pid)){if(p.ProcessName.IndexOf("dofus",StringComparison.OrdinalIgnoreCase)>=0&&name.Length>0)list.Add(new SongesGameWindow{Handle=window,ProcessId=pid,Title=name.ToString()});}}catch{}return true;},IntPtr.Zero);return list;}
 internal static Bitmap Frame(SongesGameWindow selected){if(selected==null||GetForegroundWindow()!=selected.Handle||IsIconic(selected.Handle))return null;int pid;GetWindowThreadProcessId(selected.Handle,out pid);if(pid!=selected.ProcessId)return null;Rect rect;var point=new Point();if(!GetClientRect(selected.Handle,out rect)||!ClientToScreen(selected.Handle,ref point))return null;int width=rect.right-rect.left,height=rect.bottom-rect.top;if(width<500||height<300||width>8000||height>5000)return null;var bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb);try{using(var g=Graphics.FromImage(bitmap))g.CopyFromScreen(point.x,point.y,0,0,new Size(width,height),CopyPixelOperation.SourceCopy);if(GetForegroundWindow()!=selected.Handle){bitmap.Dispose();return null;}return bitmap;}catch{bitmap.Dispose();throw;}}
}
