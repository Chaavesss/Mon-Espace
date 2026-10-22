using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
internal static class WorkspaceNext {
 internal static readonly string[] TextExtensions={".txt",".md",".csv",".json",".log",".xml",".html",".css",".js",".cs",".yml",".yaml",".ini"};
 internal static string Hash(byte[] bytes){using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
 internal static string DecodeText(byte[] bytes){if(bytes.Length>=2&&bytes[0]==255&&bytes[1]==254)return Encoding.Unicode.GetString(bytes,2,bytes.Length-2);if(bytes.Length>=2&&bytes[0]==254&&bytes[1]==255)return Encoding.BigEndianUnicode.GetString(bytes,2,bytes.Length-2);int skip=bytes.Length>=3&&bytes[0]==239&&bytes[1]==187&&bytes[2]==191?3:0;return new UTF8Encoding(false,true).GetString(bytes,skip,bytes.Length-skip);}
 internal static void SaveText(string file,string text,string expected){
  var previous=File.ReadAllBytes(file);if(Hash(previous)!=expected)throw new IOException("Le fichier a changé en dehors de Mon espace. Votre brouillon est conservé : rechargez le fichier avant de le remplacer.");
  string temp=file+"."+Guid.NewGuid().ToString("N")+".tmp",backup=file+".avant-mon-espace-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+".bak";
  try{Encoding encoding=new UTF8Encoding(false);if(previous.Length>=2&&previous[0]==255&&previous[1]==254)encoding=Encoding.Unicode;else if(previous.Length>=2&&previous[0]==254&&previous[1]==255)encoding=Encoding.BigEndianUnicode;else if(previous.Length>=3&&previous[0]==239&&previous[1]==187&&previous[2]==191)encoding=new UTF8Encoding(true);if(encoding.GetByteCount(text)>2*1024*1024)throw new InvalidDataException("Le texte dépasse la limite de 2 Mo.");File.WriteAllText(temp,text,encoding);File.Replace(temp,file,backup);}finally{if(File.Exists(temp))File.Delete(temp);}
 }
 internal static void Validate(Data data){
  var s=data.settings;if(s.Desk==null)s.Desk=new DeskOptions();if(!new[]{"Notes","Agenda","Tâches","Document"}.Contains(s.Desk.panel))s.Desk.panel="Notes";if(Double.IsNaN(s.Desk.ratio)||Double.IsInfinity(s.Desk.ratio)||s.Desk.ratio<.2||s.Desk.ratio>.6)s.Desk.ratio=.35;if(s.RecentDocuments==null)s.RecentDocuments=new List<string>();s.RecentDocuments=s.RecentDocuments.Where(p=>!String.IsNullOrWhiteSpace(p)&&p.Length<4096).Distinct(StringComparer.OrdinalIgnoreCase).Take(12).ToList();if(s.Updates==null)s.Updates=new UpdateOptions();
  foreach(var project in data.projectData){if(project.budgetCents<0||project.budgetCents>Finance.MaxMoney)throw new InvalidDataException("Budget de projet invalide.");if(project.launchModeId==null)project.launchModeId="";}
  foreach(var entry in data.budget.entries){if(entry.project==null)entry.project="";if(entry.project.Length>80)throw new InvalidDataException("Projet d’opération invalide.");}
 }
}
