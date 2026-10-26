using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

internal sealed class PublicExercise {
 public string id,name,level,equipment,category;
 public string[] primaryMuscles,secondaryMuscles,instructions,images;
}
internal sealed class FrenchExercise { public string name,instructions; }
internal static partial class ExerciseCatalog {
 internal const string SourceCommit="f00c92c7dcf1216a928a52c3706c7ce8e2f71ed5";
 static readonly Dictionary<string,string> Muscles=new Dictionary<string,string>{
 {"abdominals","Abdominaux"},{"abductors","Abducteurs"},{"adductors","Adducteurs"},{"biceps","Biceps"},{"calves","Mollets"},{"chest","Pectoraux"},{"forearms","Avant-bras"},{"glutes","Fessiers"},{"hamstrings","Ischio-jambiers"},{"lats","Dorsaux"},{"lower back","Lombaires"},{"middle back","Milieu du dos"},{"neck","Cou"},{"quadriceps","Quadriceps"},{"shoulders","Épaules"},{"traps","Trapèzes"},{"triceps","Triceps"}};
 static string Translate(string s,Dictionary<string,string> map,string fallback){string value;return s!=null&&map.TryGetValue(s,out value)?value:fallback;}
 static ExerciseDefinition[] Load(){
  // Called after the curated array is initialized; mapping keeps existing exercise IDs and names.
  var names=new Dictionary<string,string>{
   {"Barbell Bench Press - Medium Grip","bench"},{"Dumbbell Bench Press","db-bench"},{"Incline Dumbbell Press","incline"},{"Cable Crossover","fly"},{"Butterfly","pec-deck"},{"Pushups","pushup"},{"Pullups","pullup"},{"Wide-Grip Lat Pulldown","pulldown"},{"Seated Cable Rows","seated-row"},{"One-Arm Dumbbell Row","db-row"},{"Bent Over Barbell Row","bar-row"},{"Straight-Arm Pulldown","pullover"},{"Dumbbell Shoulder Press","shoulder-db"},{"Leverage Shoulder Press","shoulder-machine"},{"Side Lateral Raise","lateral"},{"Seated Bent-Over Rear Delt Raise","rear-fly"},{"Face Pull","facepull"},{"Dumbbell Bicep Curl","curl"},{"Hammer Curls","hammer"},{"EZ-Bar Curl","bar-curl"},{"Standing Biceps Cable Curl","cable-curl"},{"Triceps Pushdown - Rope Attachment","pushdown"},{"Standing Dumbbell Triceps Extension","overhead"},{"Dips - Triceps Version","dips"},{"Barbell Squat","squat"},{"Goblet Squat","goblet"},{"Leg Press","press"},{"Dumbbell Rear Lunge","lunge"},{"Leg Extensions","leg-extension"},{"Romanian Deadlift","rdl"},{"Seated Leg Curl","leg-curl"},{"Barbell Hip Thrust","hip-thrust"},{"Butt Lift (Bridge)","bridge"},{"Thigh Abductor","abduction"},{"Standing Calf Raises","calf"},{"Crunches","crunch"},{"Cable Crunch","cable-crunch"},{"Reverse Crunch","reverse-crunch"},{"Ab Roller","ab-wheel"}};
  var equipment=new Dictionary<string,string>{{"body only","Poids du corps"},{"machine","Machine"},{"other","Autre matériel"},{"foam roll","Rouleau de massage"},{"kettlebells","Kettlebell"},{"dumbbell","Haltères"},{"cable","Poulie"},{"barbell","Barre"},{"bands","Élastiques"},{"medicine ball","Médecine-ball"},{"exercise ball","Ballon"},{"e-z curl bar","Barre EZ"}};
  var levels=new Dictionary<string,string>{{"beginner","Débutant"},{"intermediate","Intermédiaire"},{"expert","Avancé"}};
  var categories=new Dictionary<string,string>{{"strength","Musculation"},{"stretching","Étirements"},{"plyometrics","Pliométrie"},{"strongman","Strongman"},{"powerlifting","Force athlétique"},{"cardio","Cardio"},{"olympic weightlifting","Haltérophilie"}};
  PublicExercise[] source;using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("ExerciseCatalog.json")){if(stream==null)throw new InvalidDataException("Catalogue d'exercices absent du programme.");using(var reader=new StreamReader(stream))source=new JavaScriptSerializer{MaxJsonLength=8*1024*1024}.Deserialize<PublicExercise[]>(reader.ReadToEnd());}
  Dictionary<string,FrenchExercise> french;using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("ExerciseFrench.json")){if(stream==null)throw new InvalidDataException("Traductions d'exercices absentes du programme.");using(var reader=new StreamReader(stream))french=new JavaScriptSerializer{MaxJsonLength=8*1024*1024}.Deserialize<Dictionary<string,FrenchExercise>>(reader.ReadToEnd());}
  var result=new List<ExerciseDefinition>();var used=new HashSet<string>();
  foreach(var x in source){
   string localId;ExerciseDefinition translated=null;if(names.TryGetValue(x.name,out localId))translated=Curated.FirstOrDefault(c=>c.Id==localId);
   if(translated!=null)used.Add(translated.Id);
   FrenchExercise local;if(!french.TryGetValue(x.id,out local)||String.IsNullOrWhiteSpace(local.name)||String.IsNullOrWhiteSpace(local.instructions))throw new InvalidDataException("Traduction incomplète : "+x.id);
   var e=new ExerciseDefinition{Id=translated==null?"fedb:"+x.id:translated.Id,Name=translated==null?(local==null?x.name:local.name):translated.Name,OriginalName=x.name,SourceId=x.id,
    Muscle=String.Join(", ",(x.primaryMuscles??new string[0]).Select(m=>Translate(m,Muscles,m))),Secondary=String.Join(", ",(x.secondaryMuscles??new string[0]).Select(m=>Translate(m,Muscles,m))),Equipment=Translate(x.equipment,equipment,"Non renseigné"),Level=Translate(x.level,levels,"Non renseigné"),Category=Translate(x.category,categories,"Autre"),Images=x.images??new string[0],OriginalSteps=String.Join("\n\n",x.instructions??new string[0]),Reps=translated==null?10:translated.Reps,Rest=translated==null?90:translated.Rest};
   e.AutomaticTranslation=translated==null&&local!=null;
   e.Steps=translated==null?(local==null?"Consignes d'origine (anglais)\n\n"+e.OriginalSteps:local.instructions):translated.Steps;
   if(String.IsNullOrWhiteSpace(e.OriginalSteps)&&translated==null)e.Steps="La source ne fournit pas de consignes pour ce mouvement.";
   if(e.Muscle=="")e.Muscle="Non renseigné";if(e.Secondary=="")e.Secondary="Non renseignés";
   if(translated!=null)e.Equipment=translated.Equipment;
   result.Add(e);
  }
  result.AddRange(Curated.Where(e=>!used.Contains(e.Id)));return result.ToArray();
 }
 static ZipArchive photos;
 internal static ZipArchive Photos{get{if(photos==null){var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("ExercisePhotos.zip");if(stream==null)throw new InvalidDataException("Photos d'exercices absentes du programme.");photos=new ZipArchive(stream,ZipArchiveMode.Read);}return photos;}}
 internal static BitmapSource Photo(string path,int width){
  var entry=Photos.GetEntry(path);if(entry==null)throw new InvalidDataException("Photo absente : "+path);
  using(var stream=entry.Open())using(var buffer=new MemoryStream()){stream.CopyTo(buffer);buffer.Position=0;var image=new BitmapImage();image.BeginInit();image.CacheOption=BitmapCacheOption.OnLoad;image.DecodePixelWidth=width;image.StreamSource=buffer;image.EndInit();image.Freeze();return image;}
 }
 internal static string Notes(ExerciseDefinition e){var text=e.Steps;return text.Length<=2000?text:text.Substring(0,1880)+"\n\nSuite des consignes dans la fiche de la bibliothèque : "+e.OriginalName.Substring(0,Math.Min(65,e.OriginalName.Length));}
}
internal sealed partial class ModernWorkspace {
 UIElement ExercisePhoto(string path,int height){
  try{var image=new Image{Source=ExerciseCatalog.Photo(path,400),Height=height,MaxWidth=300,Stretch=Stretch.Uniform,HorizontalAlignment=HorizontalAlignment.Left,Margin=new Thickness(0,0,10,12)};System.Windows.Automation.AutomationProperties.SetName(image,"Photo de démonstration de l'exercice");return image;}
  catch(Exception){return T("Photo indisponible",12,Muted);}
 }
}
