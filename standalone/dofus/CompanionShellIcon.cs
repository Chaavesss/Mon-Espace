using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

internal static class CompanionShellIcon {
 [DllImport("shell32.dll", CharSet=CharSet.Unicode)]
 static extern void SHChangeNotify(uint eventId,uint flags,string item1,IntPtr item2);
 internal static string Export(string folder) {
  byte[] bytes;
  using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("MonEspace.Icon")) {
   if(stream==null)throw new InvalidDataException("Icône du compagnon absente.");
   using(var output=new MemoryStream()){stream.CopyTo(output);bytes=output.ToArray();}
  }
  string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();
  Directory.CreateDirectory(folder);
  string file=Path.Combine(folder,"compagnon-"+hash+".ico");
  if(!File.Exists(file)||!File.ReadAllBytes(file).SequenceEqual(bytes))File.WriteAllBytes(file,bytes);
  return file;
 }
 internal static bool UpdateShortcut(string file,string target,string icon,bool create) {
  if(!create&&!File.Exists(file))return false;
  object shell=null,link=null;
  try {
   var type=Type.GetTypeFromProgID("WScript.Shell");shell=Activator.CreateInstance(type);
   link=type.InvokeMember("CreateShortcut",BindingFlags.InvokeMethod,null,shell,new object[]{file});
   var lt=link.GetType();
   if(!create) {
    string existing=(string)lt.InvokeMember("TargetPath",BindingFlags.GetProperty,null,link,null);
    if(!String.Equals(existing,target,StringComparison.OrdinalIgnoreCase))return false;
   }
   lt.InvokeMember("TargetPath",BindingFlags.SetProperty,null,link,new object[]{target});
   lt.InvokeMember("WorkingDirectory",BindingFlags.SetProperty,null,link,new object[]{Path.GetDirectoryName(target)});
   lt.InvokeMember("Description",BindingFlags.SetProperty,null,link,new object[]{"Compagnon Dofus · guides Tougli"});
   lt.InvokeMember("IconLocation",BindingFlags.SetProperty,null,link,new object[]{icon+",0"});
   lt.InvokeMember("Save",BindingFlags.InvokeMethod,null,link,null);
   SHChangeNotify(0x2000,0x5,file,IntPtr.Zero);
   return true;
  } finally {if(link!=null)Marshal.FinalReleaseComObject(link);if(shell!=null)Marshal.FinalReleaseComObject(shell);}
 }
 internal static void Refresh(bool install) {
  string icon=Export(Path.Combine(CompanionProgram.Root,"App","Icons"));
  string desktop=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),"Compagnon Dofus.lnk");
  string menu=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs),"Compagnon Dofus");
  if(install)Directory.CreateDirectory(menu);
  UpdateShortcut(desktop,CompanionProgram.Installed,icon,install);
  UpdateShortcut(Path.Combine(menu,"Compagnon Dofus.lnk"),CompanionProgram.Installed,icon,install);
  string pinned=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),@"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar");
  if(Directory.Exists(pinned))foreach(string file in Directory.GetFiles(pinned,"*.lnk")){
   try{UpdateShortcut(file,CompanionProgram.Installed,icon,false);}
   catch(IOException){}catch(UnauthorizedAccessException){}catch(COMException){}
  }
 }
 internal static void Test(string folder) {
  string test=Path.Combine(folder,"shell-icon");Directory.CreateDirectory(test);
  string icon=Export(test);
  using(var actual=new System.Drawing.Icon(icon,32,32))if(actual.Width!=32)throw new Exception("Icône Windows invalide");
  if(Export(test)!=icon)throw new Exception("Chemin icône instable");
  string link=Path.Combine(test,"Compagnon.lnk");
  UpdateShortcut(link,CompanionProgram.Installed,icon,true);
  object shell=null,item=null;
  try{
   var type=Type.GetTypeFromProgID("WScript.Shell");shell=Activator.CreateInstance(type);
   item=type.InvokeMember("CreateShortcut",BindingFlags.InvokeMethod,null,shell,new object[]{link});
   string location=(string)item.GetType().InvokeMember("IconLocation",BindingFlags.GetProperty,null,item,null);
   if(location.Replace(" ","")!=(icon+",0").Replace(" ",""))throw new Exception("Raccourci sans nouvelle icône");
  }finally{if(item!=null)Marshal.FinalReleaseComObject(item);if(shell!=null)Marshal.FinalReleaseComObject(shell);}
  byte[] before=File.ReadAllBytes(link);
  if(UpdateShortcut(link,Path.Combine(test,"Other.exe"),icon,false)||!before.SequenceEqual(File.ReadAllBytes(link)))throw new Exception("Raccourci étranger modifié");
  File.WriteAllText(icon,"corrupted");Export(test);
  using(var repaired=new System.Drawing.Icon(icon)){}
 }
}
