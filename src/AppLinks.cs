using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

internal static class AppLinks {
    internal static bool IsDiscord(string url){Uri uri;if(!Uri.TryCreate(url,UriKind.Absolute,out uri)||(uri.Scheme!="https"&&uri.Scheme!="http"))return false;string host=uri.DnsSafeHost.ToLowerInvariant();return host=="discord.com"||host=="www.discord.com"||host=="discordapp.com"||host=="www.discordapp.com";}
    internal static void Open(string url){
        if(IsDiscord(url)&&TryDiscord())return;
        Process.Start(new ProcessStartInfo(url){UseShellExecute=true});
    }
    internal static bool TryDiscord(){
        string local=Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        foreach(string name in new[]{"Discord","DiscordPTB","DiscordCanary"}){
            string folder=Path.Combine(local,name),updater=Path.Combine(folder,"Update.exe");
            if(File.Exists(updater))try{Process.Start(new ProcessStartInfo(updater,"--processStart "+name+".exe"){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=folder});return true;}catch{}
        }
        try{using(var registration=Registry.ClassesRoot.OpenSubKey(@"discord\shell\open\command")){if(registration!=null){Process.Start(new ProcessStartInfo("discord:"){UseShellExecute=true});return true;}}}catch{}
        return false;
    }
}
