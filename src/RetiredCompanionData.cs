using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
internal static class RetiredCompanionData {
 // Preserve retired JSON payloads without loading the companion or image runtime.
 internal static void Validate(Data data){
  if(data.portraits==null)data.portraits=new Dictionary<string,string>();
  if(data.portraits.Count>30||data.portraits.Any(x=>String.IsNullOrEmpty(x.Key)||x.Key.Length>100||!Regex.IsMatch(x.Value??"","^[a-f0-9]{64}$")))throw new InvalidDataException("Portraits invalides.");
 }
 internal static void Test(string folder){
  var store=new Store(Path.Combine(folder,"retired-companion"));
  var json=new System.Web.Script.Serialization.JavaScriptSerializer();
  store.Value.dofus=json.DeserializeObject("{\"active\":\"legacy\",\"characters\":[{\"id\":\"legacy\"}],\"progress\":[{\"done\":[\"quest-1\"]}]}");
  store.Value.instructions=json.DeserializeObject("{\"cursors\":{\"quest-1\":2}}");
  string progress=json.Serialize(store.Value.dofus),instructions=json.Serialize(store.Value.instructions);
  store.Value.settings.enhancements.session.page="Compagnon Dofus";
  store.Value.settings.enhancements.session.tabs.Add(new SavedWorkspaceTab{page="Compagnon Dofus"});
  store.Value.modes.Add(new LaunchMode{name="Ancien mode",dofusView="both"});store.Save();
  var reopened=new Store(store.Root);
  if(json.Serialize(reopened.Value.dofus)!=progress||json.Serialize(reopened.Value.instructions)!=instructions)throw new Exception("Anciennes données Dofus perdues");
  if(reopened.Value.settings.enhancements.session.page!="Aujourd’hui"||reopened.Value.settings.enhancements.session.tabs.Any(t=>t.page=="Compagnon Dofus")||reopened.Value.modes[0].dofusView!="off")throw new Exception("Migration compagnon incomplète");
  string exported=Path.Combine(folder,"retired-export.json");reopened.Export(exported);reopened.Import(exported);
  if(json.Serialize(reopened.Value.dofus)!=progress)throw new Exception("Import ancien compagnon perdu");
  var assembly=typeof(RetiredCompanionData).Assembly;
  if(assembly.GetManifestResourceNames().Any(n=>n.StartsWith("MonEspace.Runtime.")||n.StartsWith("MonEspace.Dofus."))||assembly.GetReferencedAssemblies().Any(a=>a.Name=="SkiaSharp")||assembly.GetType("DofusOverlay")!=null||assembly.GetType("DofusCompanion")!=null)throw new Exception("Moteur du compagnon encore embarqué");
 }
}
