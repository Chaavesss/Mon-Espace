using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
internal static class DofusImageRuntime {
 static bool ready;static IntPtr native;
 internal static void Initialize(){if(ready)return;ready=true;AppDomain.CurrentDomain.AssemblyResolve+=Resolve;}
 static Assembly Resolve(object sender,ResolveEventArgs args){string name=new AssemblyName(args.Name).Name;if(!new[]{"SkiaSharp","System.Memory","System.Buffers","System.Runtime.CompilerServices.Unsafe","System.Numerics.Vectors"}.Contains(name))return null;using(var input=Assembly.GetExecutingAssembly().GetManifestResourceStream("MonEspace.Runtime."+name+".dll")){if(input==null)return null;using(var bytes=new MemoryStream()){input.CopyTo(bytes);return Assembly.Load(bytes.ToArray());}}}
 [DllImport("kernel32",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr LoadLibraryEx(string name,IntPtr file,uint flags);
 internal static void EnsureNative(){Initialize();if(native!=IntPtr.Zero)return;byte[] bytes;using(var input=Assembly.GetExecutingAssembly().GetManifestResourceStream("MonEspace.Runtime.libSkiaSharp.dll")){if(input==null)throw new InvalidDataException("Le décodeur d’images intégré est absent.");using(var output=new MemoryStream()){input.CopyTo(output);bytes=output.ToArray();}}string hash=WorkspaceNext.Hash(bytes);string local=Environment.GetEnvironmentVariable("LOCALAPPDATA");if(String.IsNullOrWhiteSpace(local))local=Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);if(String.IsNullOrWhiteSpace(local))local=Path.GetTempPath();string folder=Path.GetFullPath(Path.Combine(local,"MonEspace","Runtime",hash));Directory.CreateDirectory(folder);string file=Path.Combine(folder,"libSkiaSharp.dll");if(!File.Exists(file)||WorkspaceNext.Hash(File.ReadAllBytes(file))!=hash){string temp=file+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllBytes(temp,bytes);if(File.Exists(file))File.Replace(temp,file,null);else File.Move(temp,file);}finally{if(File.Exists(temp))File.Delete(temp);}}native=LoadLibraryEx(file,IntPtr.Zero,0x1100);if(native==IntPtr.Zero)throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(),"Le décodeur WebP n’a pas pu démarrer (Windows "+Marshal.GetLastWin32Error()+").");}
}
