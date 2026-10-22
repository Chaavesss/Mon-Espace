using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Forms=System.Windows.Forms;
internal sealed partial class MainForm {
 const int CaptureHotkey=0x4D45;bool hotkeyRegistered,captureOpen;
 internal string CaptureStatus{get{return !store.Value.settings.QuickCapture?"Raccourci désactivé.":hotkeyRegistered?"Raccourci prêt : Ctrl + Alt + N.":"Raccourci indisponible sur ce PC : utilisez le bouton Capture rapide.";}}
 [DllImport("user32.dll",SetLastError=true)]static extern bool RegisterHotKey(IntPtr window,int id,uint modifiers,uint key);
 [DllImport("user32.dll")]static extern bool UnregisterHotKey(IntPtr window,int id);
 [DllImport("user32.dll")]static extern bool SetForegroundWindow(IntPtr window);
 protected override void OnShown(EventArgs e){if(!rendering){UpdateCaptureHotkey();InitializeReminders();}base.OnShown(e);if(!rendering&&store.Value.settings.Widget.openOnStartup)ShowDesktopWidget();}
 protected override void OnHandleDestroyed(EventArgs e){if(hotkeyRegistered){UnregisterHotKey(Handle,CaptureHotkey);hotkeyRegistered=false;}base.OnHandleDestroyed(e);}
 internal void UpdateCaptureHotkey(){if(hotkeyRegistered){UnregisterHotKey(Handle,CaptureHotkey);hotkeyRegistered=false;}if(store.Value.settings.QuickCapture)hotkeyRegistered=RegisterHotKey(Handle,CaptureHotkey,0x0001|0x0002|0x4000,0x4E);}
 internal void ShowQuickCapture(){if(store.Value.settings.PrivacyMode)return;if(captureOpen||closing||modern==null||!Enabled)return;captureOpen=true;try{modern.Capture();}finally{captureOpen=false;}}
}
internal sealed partial class ModernWorkspace {
 internal void PreviewHomeFeatures(string path){selected="Aujourd’hui";openProject="";Refresh();Preview(path+"-accueil.png");previewPrefix=path;EditHome();Capture();previewPrefix=null;}
}
