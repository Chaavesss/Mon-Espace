using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;
public sealed class ProjectTemplate {
 public string id="",name="",description="";
 public List<TemplateTask> tasks=new List<TemplateTask>();
 public List<TemplateNote> notes=new List<TemplateNote>();
 public override string ToString(){return name;}
}
public sealed class TemplateTask {public string title="",priority="Normale",notes="";}
public sealed class TemplateNote {public string title="",body="";}
internal static class ProjectTemplates {
 static JavaScriptSerializer Json(){return new JavaScriptSerializer{MaxJsonLength=8*1024*1024};}
 internal static T Copy<T>(T value){var j=Json();return j.Deserialize<T>(j.Serialize(value));}
 internal static List<ProjectTemplate> Defaults(){
  return new List<ProjectTemplate>{
   Make("builtin-work","Travail","Du besoin initial à la livraison.","Définir le besoin","Réunir les documents","Réaliser le travail","Vérifier le résultat","Finaliser et transmettre"),
   Make("builtin-admin","Administratif","Un dossier suivi jusqu'à sa résolution.","Identifier les démarches","Rassembler les justificatifs","Remplir et envoyer le dossier","Vérifier la réponse","Classer les documents"),
   Make("builtin-personal","Projet personnel","Faire avancer une idée, étape par étape.","Définir l'objectif","Lister les étapes","Préparer les ressources","Passer à l'action","Faire le bilan")
  };
 }
 static ProjectTemplate Make(string id,string name,string description,params string[] titles){return new ProjectTemplate{id=id,name=name,description=description,tasks=titles.Select(t=>new TemplateTask{title=t}).ToList()};}
 internal static void Validate(Data data){
  if(data.projectTemplates==null)data.projectTemplates=new List<ProjectTemplate>();
  if(data.projectTemplates.Count>50)throw new InvalidDataException("Maximum 50 modèles personnels.");
  var ids=new HashSet<string>();
  foreach(var template in data.projectTemplates){
   if(template==null)throw new InvalidDataException("Modèle invalide.");
   Check(template);
   if(String.IsNullOrWhiteSpace(template.id)||!ids.Add(template.id))throw new InvalidDataException("Identifiant de modèle invalide ou dupliqué.");
  }
 }
 static void Check(ProjectTemplate t){
  if(t==null||String.IsNullOrWhiteSpace(t.name)||t.name.Length>80||(t.description??"").Length>10000||t.tasks==null||t.notes==null||t.tasks.Count>500||t.notes.Count>100)throw new InvalidDataException("Modèle invalide : 500 tâches et 100 notes maximum.");
  if(t.tasks.Any(v=>v==null||String.IsNullOrWhiteSpace(v.title)||v.title.Length>500||!new[]{"Normale","Haute","Basse"}.Contains(v.priority)||(v.notes??"").Length>10000))throw new InvalidDataException("Tâche du modèle invalide.");
  if(t.notes.Any(n=>n==null||String.IsNullOrWhiteSpace(n.title)||n.title.Length>200||(n.body??"").Length>300000))throw new InvalidDataException("Note du modèle invalide.");
 }
 internal static ProjectTemplate FromProject(Data data,string project,bool includeNotes){
  var source=data.projectData.FirstOrDefault(p=>p.name==project);
  return new ProjectTemplate{id=Guid.NewGuid().ToString(),name=project,description=source==null?"":source.description,
   tasks=data.tasks.Where(t=>t.project==project).Select(t=>new TemplateTask{title=t.title,priority=t.priority,notes=t.notes}).ToList(),
   notes=includeNotes?data.pages.Where(n=>n.project==project&&!n.trashed).Select(n=>new TemplateNote{title=n.title,body=n.body}).ToList():new List<TemplateNote>()};
 }
 internal static void Atomic(Store store,Action action){var before=Copy(store.Value);try{action();store.Save();}catch{store.Value=before;throw;}}
 internal static void Save(Store store,ProjectTemplate template){
  Check(template);Atomic(store,delegate{
   var copy=Copy(template);if(String.IsNullOrEmpty(copy.id)||copy.id.StartsWith("builtin-",StringComparison.Ordinal))copy.id=Guid.NewGuid().ToString();
   store.Value.projectTemplates.RemoveAll(t=>t.id==copy.id);store.Value.projectTemplates.Add(copy);
  });
 }
 internal static void Delete(Store store,string id){Atomic(store,delegate{store.Value.projectTemplates.RemoveAll(t=>t.id==id);});}
 internal static string Create(Store store,ProjectTemplate template,string name,IEnumerable<int> selected,bool notes){
  Check(template);name=(name??"").Trim();
  if(name.Length==0||name.Length>80)throw new InvalidDataException("Choisissez un nom de projet (80 caractères maximum).");
  var existing=store.Value.settings.Projects.Concat(store.Value.projectData.Select(p=>p.name)).Concat(store.Value.tasks.Select(t=>t.project)).Concat(store.Value.pages.Select(n=>n.project));
  if(existing.Any(p=>String.Equals(p,name,StringComparison.OrdinalIgnoreCase)))throw new InvalidDataException("Ce nom de projet existe déjà. Choisissez-en un autre.");
  var indices=selected.Distinct().ToList();if(indices.Any(i=>i<0||i>=template.tasks.Count))throw new InvalidDataException("Sélection de tâches invalide.");
  Atomic(store,delegate{
   store.Value.settings.Projects.Add(name);store.Value.projectData.Add(new ProjectSpace{name=name,description=template.description??""});
   foreach(int i in indices){var t=template.tasks[i];store.Value.tasks.Add(new WorkTask{id=Guid.NewGuid().ToString(),title=t.title,project=name,priority=t.priority,status="todo",due="",notes=t.notes??""});}
   if(notes)foreach(var n in template.notes)store.Value.pages.Add(new NotePage{id=Guid.NewGuid().ToString(),title=n.title,body=n.body??"",project=name,updated=DateTime.Now.ToString("o")});
  });return name;
 }
 internal static void Test(string folder){
  var store=new Store(Path.Combine(folder,"templates"));
  var template=Defaults()[0];Save(store,template);
  var saved=new Store(store.Root).Value.projectTemplates.Single();if(saved.id.StartsWith("builtin-")||saved.tasks.Count!=5)throw new Exception("Persistance modèle");
  Create(store,saved,"Chantier A",new[]{0,2},true);Create(store,saved,"Chantier B",new[]{0,2},true);
  if(store.Value.tasks.Count!=4||store.Value.tasks.Select(t=>t.id).Distinct().Count()!=4||store.Value.tasks.Any(t=>t.status!="todo"||t.due!=""||t.repeat!="none"))throw new Exception("Création indépendante");
  store.Value.tasks[0].status="done";store.Value.tasks[0].due="2026-01-01";store.Value.pages.Add(new NotePage{id="visible",project="Chantier A",title="Brief",body="Contenu"});store.Value.pages.Add(new NotePage{id="trash",project="Chantier A",title="Corbeille",trashed=true});store.Save();
  var own=FromProject(store.Value,"Chantier A",true);if(own.notes.Count!=1||own.tasks.Count!=2)throw new Exception("Capture projet");
  Save(store,own);own.tasks[0].title="Mutation";if(store.Value.projectTemplates.Last().tasks[0].title=="Mutation")throw new Exception("Modèle non isolé");
  Create(store,store.Value.projectTemplates.Last(),"Copie",new[]{0},true);var task=store.Value.tasks.Last();if(task.status!="todo"||task.due!=""||store.Value.pages.Last().title!="Brief")throw new Exception("Réinitialisation projet");
  bool rejected=false;try{Create(store,saved,"chantier a",new[]{0},false);}catch(InvalidDataException){rejected=true;}if(!rejected)throw new Exception("Doublon accepté");
  var jsonPath=Path.Combine(folder,"templates-export.json");store.Export(jsonPath);store.Value.projectTemplates.Clear();store.Import(jsonPath);if(store.Value.projectTemplates.Count!=2)throw new Exception("Import modèles");
  string backup=Path.Combine(folder,"templates.mespace");FullBackup.Export(store,backup);var target=new Store(Path.Combine(folder,"templates-restored"));using(var plan=FullBackup.Prepare(target,backup))FullBackup.Commit(target,plan);if(new Store(target.Root).Value.projectTemplates.Count!=2)throw new Exception("Sauvegarde complète modèles");
  string path=Path.Combine(store.Root,"state.json");File.Delete(path);Directory.CreateDirectory(path);int count=store.Value.tasks.Count;rejected=false;try{Create(store,saved,"Échec",new[]{0},false);}catch(IOException){rejected=true;}catch(UnauthorizedAccessException){rejected=true;}Directory.Delete(path);if(!rejected||store.Value.tasks.Count!=count||store.Value.settings.Projects.Contains("Échec"))throw new Exception("Retour après échec création");store.Save();
  Delete(store,store.Value.projectTemplates.First().id);if(new Store(store.Root).Value.projectTemplates.Count!=1||store.Value.tasks.Count!=count)throw new Exception("Suppression modèle");
  var legacy=new Data();legacy.projectTemplates=null;Validate(legacy);if(legacy.projectTemplates==null)throw new Exception("Ancien format");
 }
}
