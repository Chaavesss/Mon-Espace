using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
internal static class Installation {
 internal static string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }
 internal static string Target(string root){return Path.Combine(root,"Versions",Version,"MonEspace.exe");}
 static bool SameFile(string a,string b){using(var hash=SHA256.Create())using(var left=File.OpenRead(a))using(var right=File.OpenRead(b)){return Convert.ToBase64String(hash.ComputeHash(left))==Convert.ToBase64String(hash.ComputeHash(right));}}
 internal static void Copy(string source,string destination){
  Directory.CreateDirectory(Path.GetDirectoryName(destination));
  if(File.Exists(destination)&&SameFile(source,destination))return;
  string temporary=destination+"."+Guid.NewGuid().ToString("N")+".tmp";
  try{
   File.Copy(source,temporary,false);
   if(!SameFile(source,temporary))throw new IOException("Le fichier de l’application n’a pas été copié entièrement.");
   if(File.Exists(destination))File.Replace(temporary,destination,null);else File.Move(temporary,destination);
  }finally{try{if(File.Exists(temporary))File.Delete(temporary);}catch{}}
 }
 internal static string Diagnostic(string phase,Exception error,string root,string destination){
  string text="Mon espace n’a pas pu terminer l’étape : "+phase+".\n\n"+error.Message+"\n\nDestination :\n"+destination+"\n\nVos données restent dans :\n"+Path.Combine(root,"Data");
  try{string log=Path.Combine(Path.GetTempPath(),"MonEspace-installation-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".txt");File.WriteAllText(log,"Version: "+Version+"\r\nÉtape: "+phase+"\r\nSource: "+Assembly.GetExecutingAssembly().Location+"\r\nDestination: "+destination+"\r\n"+error);text+="\n\nDiagnostic :\n"+log;}catch{}
  return text;
 }
 internal static void Test(string folder){
  Directory.CreateDirectory(folder);string source=Path.Combine(folder,"source.exe"),target=Target(folder);File.WriteAllBytes(source,new byte[]{1,4,8,16});
  Copy(source,target);if(!SameFile(source,target))throw new Exception("Installation incomplète");
  using(var locked=new FileStream(target,FileMode.Open,FileAccess.Read,FileShare.Read))Copy(source,target);
  if(Directory.Exists(Path.Combine(folder,"Bin")))throw new Exception("Ancien dossier Bin modifié");
  string data=Path.Combine(folder,"Data");Directory.CreateDirectory(data);string state=Path.Combine(data,"state.json");File.WriteAllText(state,"donnees-utilisateur");
  File.WriteAllBytes(source,new byte[]{2,5,9,17});Copy(source,target);if(!SameFile(source,target)||File.ReadAllText(state)!="donnees-utilisateur")throw new Exception("Mise à jour ou préservation des données");
  string blocked=Path.Combine(folder,"blocked");File.WriteAllText(blocked,"occupied");bool rejected=false;try{Copy(source,Path.Combine(blocked,"child.exe"));}catch(IOException){rejected=true;}if(!rejected)throw new Exception("Erreur de destination ignorée");
  if(Directory.GetFiles(Path.GetDirectoryName(target),"*.tmp").Length>0)throw new Exception("Fichier temporaire restant");
 }
}
