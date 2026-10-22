using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
public sealed class Data { public DofusData dofus=new DofusData(); }
internal sealed class Store {
 internal readonly string Root; internal Data Value;
 internal static readonly JavaScriptSerializer Json=new JavaScriptSerializer{MaxJsonLength=8*1024*1024,RecursionLimit=64};
 internal Store(string root){Root=root;Directory.CreateDirectory(root);var file=Path.Combine(root,"state.json");if(File.Exists(file)&&new FileInfo(file).Length>8*1024*1024)throw new InvalidDataException("Sauvegarde trop volumineuse.");Value=File.Exists(file)?Json.Deserialize<Data>(File.ReadAllText(file)):new Data();if(Value==null)throw new InvalidDataException("Sauvegarde invalide.");DofusCompanion.Validate(Value);}
 internal void Save(){DofusCompanion.Validate(Value);var file=Path.Combine(Root,"state.json");var tmp=file+".tmp";try{File.WriteAllText(tmp,Json.Serialize(Value),new UTF8Encoding(false));if(File.Exists(file))File.Replace(tmp,file,file+".previous");else File.Move(tmp,file);}finally{if(File.Exists(tmp))File.Delete(tmp);}}
}
internal static class PersonalWorkflows {
 internal static void Change(Store store,Action action){var before=Store.Json.Serialize(store.Value);try{action();store.Save();}catch{store.Value=Store.Json.Deserialize<Data>(before);throw;}}
}
internal static class WorkspaceNext {
 internal static string Hash(byte[] bytes){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
}
