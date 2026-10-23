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
 internal static bool Candidate(int pid,int ownPid,string processName,string title){if(pid<=0||pid==ownPid||String.IsNullOrWhiteSpace(title))return false;string name=(processName??"").ToLowerInvariant();return name=="dofus"||name=="dofusunity"||name=="dofus unity"||name=="dofusretro"||name=="dofus retro"||name=="dofusinvoker";}
 internal static bool Valid(SongesGameWindow selected){if(selected==null||selected.Handle==IntPtr.Zero||!IsWindowVisible(selected.Handle))return false;int pid;GetWindowThreadProcessId(selected.Handle,out pid);if(pid!=selected.ProcessId)return false;try{using(var own=Process.GetCurrentProcess())using(var p=Process.GetProcessById(pid))return Candidate(pid,own.Id,p.ProcessName,selected.Title);}catch{return false;}}
 internal static List<SongesGameWindow> List(){var list=new List<SongesGameWindow>();int ownPid;using(var own=Process.GetCurrentProcess())ownPid=own.Id;EnumWindows(delegate(IntPtr window,IntPtr unused){if(!IsWindowVisible(window))return true;int pid;GetWindowThreadProcessId(window,out pid);if(pid<=0||pid==ownPid)return true;var name=new StringBuilder(512);GetWindowText(window,name,name.Capacity);try{using(var p=Process.GetProcessById(pid)){if(Candidate(pid,ownPid,p.ProcessName,name.ToString()))list.Add(new SongesGameWindow{Handle=window,ProcessId=pid,Title=name.ToString()});}}catch{}return true;},IntPtr.Zero);list.Sort((a,b)=>StringComparer.CurrentCultureIgnoreCase.Compare(a.Title,b.Title));return list;}
 internal static int Selection(List<SongesGameWindow> list,SongesGameWindow previous){int index=previous==null?-1:list.FindIndex(w=>w.Handle==previous.Handle&&w.ProcessId==previous.ProcessId);return index>=0?index:list.Count==1?0:-1;}
 internal static void Test(){if(Candidate(1,1,"Dofus","DOFUS")||Candidate(2,1,"CompagnonDofus","Compagnon Dofus · Songes")||Candidate(2,1,"MonEspace","DOFUS")||Candidate(2,1,"Ankama Launcher","DOFUS")||Candidate(2,1,"chrome","DOFUS")||Candidate(2,1,"DofusTools","DOFUS")||Candidate(2,1,"Dofus"," ")||!Candidate(2,1,"DOFUS","Personnage - DOFUS")||!Candidate(2,1,"DofusUnity","DOFUS"))throw new Exception("Filtre de fenêtres DOFUS");var a=new SongesGameWindow{Handle=new IntPtr(1),ProcessId=2};var b=new SongesGameWindow{Handle=new IntPtr(2),ProcessId=3};var list=new List<SongesGameWindow>{a,b};if(Selection(list,null)!=-1||Selection(list,b)!=1||Selection(new List<SongesGameWindow>{a},null)!=0||Selection(new List<SongesGameWindow>(),a)!=-1||Selection(list,new SongesGameWindow{Handle=a.Handle,ProcessId=99})!=-1)throw new Exception("Sélection multi-compte DOFUS");using(var own=Process.GetCurrentProcess()){if(List().Exists(w=>w.ProcessId==own.Id))throw new Exception("Le compagnon se capture lui-même");}if(Valid(null)||Frame(null)!=null)throw new Exception("Capture sans jeu");}
 internal static Bitmap Frame(SongesGameWindow selected){if(!Valid(selected)||GetForegroundWindow()!=selected.Handle||IsIconic(selected.Handle))return null;int pid;GetWindowThreadProcessId(selected.Handle,out pid);if(pid!=selected.ProcessId)return null;Rect rect;var point=new Point();if(!GetClientRect(selected.Handle,out rect)||!ClientToScreen(selected.Handle,ref point))return null;int width=rect.right-rect.left,height=rect.bottom-rect.top;if(width<500||height<300||width>8000||height>5000)return null;var bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb);try{using(var g=Graphics.FromImage(bitmap))g.CopyFromScreen(point.x,point.y,0,0,new Size(width,height),CopyPixelOperation.SourceCopy);if(GetForegroundWindow()!=selected.Handle){bitmap.Dispose();return null;}return bitmap;}catch{bitmap.Dispose();throw;}}
}
