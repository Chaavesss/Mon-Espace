using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

internal sealed class ExerciseDefinition {
 internal string Id,Name,Muscle,Secondary,Equipment,Steps;
 internal string OriginalName="",Level="",Category="",SourceId="",OriginalSteps="";
 internal string[] Images=new string[0];
 internal bool AutomaticTranslation;
 internal int Reps,Rest;
}
internal static partial class ExerciseCatalog {
 // Original text, no external images or network dependency. Defaults are editable starting points.
 static ExerciseDefinition E(string id,string name,string muscle,string secondary,string equipment,int reps,int rest,string steps){return new ExerciseDefinition{Id=id,Name=name,Muscle=muscle,Secondary=secondary,Equipment=equipment,Reps=reps,Rest=rest,Steps=steps};}
 internal static readonly ExerciseDefinition[] Curated={
 E("bench","Développé couché barre","Pectoraux","Triceps, épaules","Barre",10,120,"Allongez-vous sur le banc, pieds stables au sol. Gardez les omoplates rapprochées. Descendez la barre de façon contrôlée vers la poitrine, puis poussez sans décoller les fesses."),
 E("db-bench","Développé couché haltères","Pectoraux","Triceps, épaules","Haltères",10,120,"Installez les haltères au-dessus de la poitrine, pieds stables. Descendez les coudes dans une amplitude confortable. Remontez sans entrechoquer les haltères."),
 E("incline","Développé incliné haltères","Pectoraux","Épaules, triceps","Haltères",10,120,"Réglez le banc sur une inclinaison modérée. Gardez le haut du dos en appui. Descendez les haltères de chaque côté du haut de la poitrine puis poussez en contrôlant."),
 E("fly","Écarté à la poulie","Pectoraux","Épaules","Poulie",12,90,"Placez un pied devant l'autre, buste stable et coudes légèrement fléchis. Rapprochez les mains devant la poitrine. Revenez lentement sans forcer l'étirement des épaules."),
 E("pec-deck","Pec deck","Pectoraux","Épaules","Machine",12,90,"Réglez le siège pour placer les poignées à hauteur de poitrine. Gardez le dos contre le dossier. Rapprochez les bras puis revenez sans laisser tomber la charge."),
 E("pushup","Pompes","Pectoraux","Triceps, abdominaux","Poids du corps",10,90,"Posez les mains légèrement plus larges que les épaules. Maintenez le corps aligné. Descendez la poitrine vers le sol puis repoussez ; utilisez un appui surélevé si nécessaire."),
 E("pullup","Tractions pronation","Dos","Biceps, avant-bras","Barre de traction",8,120,"Saisissez la barre, paumes vers l'avant. Gainez le buste et tirez les coudes vers le bas. Revenez de façon contrôlée, sans élan ; utilisez une assistance si nécessaire."),
 E("pulldown","Tirage vertical","Dos","Biceps","Poulie",10,120,"Calez les cuisses et gardez le buste stable. Tirez la barre devant vous vers le haut de la poitrine. Laissez les bras remonter lentement sans tirer derrière la nuque."),
 E("seated-row","Tirage horizontal assis","Dos","Biceps, arrière d'épaule","Poulie",10,90,"Asseyez-vous avec le dos stable et les genoux légèrement fléchis. Tirez la poignée vers le ventre en guidant les coudes en arrière. Revenez sans balancer le buste."),
 E("db-row","Rowing un bras haltère","Dos","Biceps, arrière d'épaule","Haltères",10,90,"Prenez un appui stable sur le banc. Gardez le dos dans une position confortable et tirez le coude vers la hanche. Redescendez sans tourner le buste. Répétez de chaque côté."),
 E("bar-row","Rowing barre","Dos","Biceps, arrière d'épaule","Barre",10,120,"Inclinez le buste depuis les hanches, genoux légèrement fléchis. Gardez la barre proche du corps. Tirez vers le ventre puis redescendez sans donner d'élan avec le dos."),
 E("pullover","Pull-over à la poulie","Dos","Triceps","Poulie",12,90,"Face à la poulie haute, inclinez légèrement le buste. Gardez les coudes peu fléchis et ramenez la poignée vers les cuisses. Revenez lentement sans creuser le dos."),
 E("shoulder-db","Développé épaules haltères","Épaules","Triceps","Haltères",10,120,"Asseyez-vous avec le dos soutenu. Placez les haltères près des épaules. Poussez vers le haut dans une amplitude confortable puis redescendez en gardant le buste stable."),
 E("shoulder-machine","Développé épaules machine","Épaules","Triceps","Machine",10,120,"Réglez le siège pour aligner les poignées avec les épaules. Gardez le dos contre le dossier. Poussez puis revenez lentement, sans verrouiller brutalement les coudes."),
 E("lateral","Élévations latérales","Épaules","Trapèzes","Haltères",12,90,"Tenez les haltères le long du corps, coudes légèrement fléchis. Levez les bras sur les côtés jusqu'à une hauteur confortable. Redescendez lentement sans balancer le buste."),
 E("rear-fly","Oiseau haltères","Épaules","Haut du dos","Haltères",12,90,"Inclinez le buste avec un appui stable ou utilisez un banc incliné. Écartez les bras avec les coudes légèrement fléchis. Revenez en contrôlant sans lancer les haltères."),
 E("facepull","Face pull","Épaules","Haut du dos","Poulie",12,90,"Réglez la corde à hauteur du visage. Tirez vers le visage en écartant les mains et les coudes. Gardez le buste stable puis revenez lentement."),
 E("curl","Curl haltères","Biceps","Avant-bras","Haltères",12,90,"Gardez les coudes près du buste. Fléchissez les bras sans avancer les épaules ni balancer le dos. Redescendez lentement jusqu'à une extension confortable."),
 E("hammer","Curl marteau","Biceps","Avant-bras","Haltères",12,90,"Tenez les haltères paumes face à face. Fléchissez les coudes près du buste. Redescendez sans modifier la position des poignets ni donner d'élan."),
 E("bar-curl","Curl barre EZ","Biceps","Avant-bras","Barre",10,90,"Choisissez une prise confortable sur la barre EZ. Gardez les coudes près du corps. Montez la barre sans balancer le buste puis redescendez lentement."),
 E("cable-curl","Curl à la poulie","Biceps","Avant-bras","Poulie",12,90,"Face à la poulie basse, gardez les épaules et les coudes stables. Fléchissez les bras puis revenez lentement sans laisser tomber la charge."),
 E("pushdown","Extension triceps à la corde","Triceps","Avant-bras","Poulie",12,90,"Placez les coudes près du buste. Abaissez la corde en tendant les bras. Revenez lentement sans déplacer les épaules ni balancer le corps."),
 E("overhead","Extension triceps au-dessus de la tête","Triceps","Épaules","Haltères",12,90,"Tenez un haltère à deux mains au-dessus de la tête. Fléchissez les coudes pour descendre derrière la tête dans une amplitude confortable. Retendez les bras sans cambrer le dos."),
 E("dips","Dips","Triceps","Pectoraux, épaules","Barres parallèles",8,120,"Prenez appui sur les barres, épaules stables. Descendez en fléchissant les coudes dans une amplitude confortable. Repoussez sans rebond ; utilisez une assistance si nécessaire."),
 E("squat","Squat barre","Quadriceps","Fessiers, abdominaux","Barre",8,150,"Installez la barre sur le haut du dos et stabilisez les pieds. Fléchissez hanches et genoux en gardant les pieds au sol. Remontez en poussant dans le sol, dans une amplitude maîtrisée."),
 E("goblet","Goblet squat","Quadriceps","Fessiers, abdominaux","Haltères",10,120,"Tenez un haltère devant la poitrine. Descendez entre les jambes en fléchissant les hanches et genoux. Gardez les pieds stables puis remontez sans arrondir volontairement le dos."),
 E("press","Presse à cuisses","Quadriceps","Fessiers","Machine",12,120,"Réglez le siège pour garder le bassin en appui. Descendez le plateau dans une amplitude qui ne décolle pas le bassin. Repoussez sans verrouiller brutalement les genoux."),
 E("lunge","Fentes arrière haltères","Quadriceps","Fessiers, équilibre","Haltères",10,120,"Faites un pas en arrière et fléchissez les deux jambes. Gardez le pied avant stable. Poussez dans ce pied pour revenir, puis répétez de chaque côté."),
 E("leg-extension","Leg extension","Quadriceps","Quadriceps","Machine",12,90,"Alignez les genoux avec l'axe de la machine. Placez le rouleau au-dessus des chevilles. Tendez les jambes de façon contrôlée puis revenez sans laisser tomber la charge."),
 E("rdl","Soulevé de terre roumain","Ischio-jambiers","Fessiers, dos","Barre",10,150,"Gardez les genoux légèrement fléchis et la barre proche des jambes. Reculez les hanches pour incliner le buste. Revenez debout en avançant les hanches, sans tirer avec le dos."),
 E("leg-curl","Leg curl assis","Ischio-jambiers","Mollets","Machine",12,90,"Réglez les appuis et alignez les genoux avec l'axe de la machine. Fléchissez les jambes sans décoller le bassin. Revenez lentement en gardant la charge sous contrôle."),
 E("hip-thrust","Hip thrust barre","Fessiers","Ischio-jambiers","Barre",10,120,"Placez le haut du dos contre un banc stable et protégez les hanches du contact avec la barre. Poussez dans les pieds pour relever le bassin. Redescendez sans cambrer le bas du dos."),
 E("bridge","Pont fessier au sol","Fessiers","Ischio-jambiers","Poids du corps",12,90,"Allongez-vous, genoux fléchis et pieds au sol. Relevez le bassin en contractant les fessiers. Redescendez lentement sans exagérer la cambrure."),
 E("abduction","Abduction de hanches machine","Fessiers","Hanches","Machine",15,90,"Asseyez-vous avec le dos et le bassin stables. Écartez les jambes contre les coussins. Revenez lentement sans rebond."),
 E("calf","Mollets debout","Mollets","Équilibre","Machine",15,90,"Placez l'avant des pieds sur le support et stabilisez le buste. Montez sur la pointe des pieds puis redescendez lentement dans une amplitude confortable, sans rebond."),
 E("crunch","Crunch au sol","Abdominaux","Abdominaux","Poids du corps",15,60,"Allongez-vous, genoux fléchis. Enroulez légèrement le haut du buste en rapprochant les côtes du bassin. Revenez sans tirer sur la nuque."),
 E("cable-crunch","Crunch à la poulie","Abdominaux","Abdominaux","Poulie",12,90,"À genoux devant la poulie haute, gardez la corde près de la tête. Enroulez le buste sans tirer avec les bras. Revenez lentement en gardant le bassin stable."),
 E("reverse-crunch","Crunch inversé","Abdominaux","Hanches","Poids du corps",12,60,"Allongez-vous et fléchissez les jambes. Enroulez légèrement le bassin vers les côtes sans lancer les jambes. Reposez lentement le bassin."),
 E("ab-wheel","Roulette abdominale","Abdominaux","Épaules, dos","Roulette",8,90,"À genoux sur un support confortable, faites rouler la roue vers l'avant sur une courte distance maîtrisée. Gardez le tronc gainé sans creuser le bas du dos. Revenez sans élan.")
 };
 static ExerciseDefinition[] all;
 internal static ExerciseDefinition[] All{get{return all??(all=Load());}}
 internal static IEnumerable<ExerciseDefinition> Search(string query,string muscle,string equipment){return All.Where(e=>(String.IsNullOrEmpty(muscle)||muscle=="Tous les muscles"||(e.Muscle+", "+e.Secondary).Split(new[]{", "},StringSplitOptions.None).Contains(muscle))&&(String.IsNullOrEmpty(equipment)||equipment=="Tout le matériel"||e.Equipment==equipment)&&(String.IsNullOrWhiteSpace(query)||CultureInfo.GetCultureInfo("fr-FR").CompareInfo.IndexOf(e.Name+" "+e.OriginalName+" "+e.Muscle+" "+e.Secondary+" "+e.Equipment,query.Trim(),CompareOptions.IgnoreCase|CompareOptions.IgnoreNonSpace)>=0));}
 internal static WorkoutExercise Create(ExerciseDefinition e){return new WorkoutExercise{name=e.Name,notes=Notes(e),restSeconds=e.Rest,sets=Enumerable.Range(0,3).Select(i=>new WorkoutSet{reps=e.Reps}).ToList()};}
}

internal sealed partial class ModernWorkspace {
 string exerciseQuery="",exerciseMuscle="Tous les muscles",exerciseEquipment="Tout le matériel";
 string exerciseLevel="Tous les niveaux",exerciseCategory="Toutes les catégories";int exercisePage;
 void ExerciseLibraryPage(StackPanel root){
  root.Children.Add(T(ExerciseCatalog.All.Length+" exercices · fiches d'exécution · ajout à vos séances",16));
  root.Children.Add(T("Choisissez un mouvement, consultez ses consignes puis personnalisez vos séries, répétitions et repos. Les charges restent à renseigner.",12,Muted,new Thickness(0,8,0,14)));
  root.Children.Add(T("Source : Free Exercise DB · photos hors connexion · fiches en français, avec traduction automatique signalée et texte anglais consultable.",11,Muted,new Thickness(0,0,0,12)));
  var results=new StackPanel();root.Children.Add(ExerciseLibraryFilters(results,null));root.Children.Add(results);
  RenderExerciseResults(results,null);
  var personal=SN.workouts.SelectMany(p=>p.exercises).Where(e=>!ExerciseCatalog.All.Any(c=>c.Name.Equals(e.name,StringComparison.OrdinalIgnoreCase))).GroupBy(e=>e.name,StringComparer.OrdinalIgnoreCase).ToList();
  if(personal.Count>0){var b=new StackPanel();foreach(var group in personal){var e=group.First();b.Children.Add(T(e.name,14));b.Children.Add(T(e.notes,12,Muted));}root.Children.Add(Card("Mes exercices personnels",b));}
 }
 UIElement ExerciseLibraryFilters(StackPanel results,string planId){
  var filters=new WrapPanel{Margin=new Thickness(0,0,0,16)};
  var search=SportText(exerciseQuery);search.Width=240;search.ToolTip="Rechercher un exercice ou un muscle";System.Windows.Automation.AutomationProperties.SetName(search,"Rechercher un exercice");filters.Children.Add(search);
  var muscles=Combo(new[]{"Tous les muscles"}.Concat(ExerciseCatalog.All.SelectMany(e=>(e.Muscle+", "+e.Secondary).Split(new[]{", "},StringSplitOptions.None)).Where(x=>x!="Non renseignés").Distinct().OrderBy(x=>x)).ToArray(),exerciseMuscle);muscles.Width=170;filters.Children.Add(muscles);
  var equipment=Combo(new[]{"Tout le matériel"}.Concat(ExerciseCatalog.All.Select(e=>e.Equipment).Distinct().OrderBy(x=>x)).ToArray(),exerciseEquipment);equipment.Width=180;filters.Children.Add(equipment);
  var level=Combo(new[]{"Tous les niveaux"}.Concat(ExerciseCatalog.All.Select(e=>e.Level).Where(x=>x!="").Distinct().OrderBy(x=>x)).ToArray(),exerciseLevel);level.Width=170;filters.Children.Add(level);
  var category=Combo(new[]{"Toutes les catégories"}.Concat(ExerciseCatalog.All.Select(e=>e.Category).Where(x=>x!="").Distinct().OrderBy(x=>x)).ToArray(),exerciseCategory);category.Width=210;filters.Children.Add(category);
  Action filter=delegate{exerciseQuery=search.Text;exerciseMuscle=Convert.ToString(muscles.SelectedItem);exerciseEquipment=Convert.ToString(equipment.SelectedItem);exerciseLevel=Convert.ToString(level.SelectedItem);exerciseCategory=Convert.ToString(category.SelectedItem);exercisePage=0;RenderExerciseResults(results,planId);};
  search.TextChanged+=delegate{filter();};muscles.SelectionChanged+=delegate{filter();};equipment.SelectionChanged+=delegate{filter();};level.SelectionChanged+=delegate{filter();};category.SelectionChanged+=delegate{filter();};
  return filters;
 }
 void RenderExerciseResults(StackPanel results,string planId){
  results.Children.Clear();var found=ExerciseCatalog.Search(exerciseQuery,exerciseMuscle,exerciseEquipment).Where(e=>(exerciseLevel=="Tous les niveaux"||e.Level==exerciseLevel)&&(exerciseCategory=="Toutes les catégories"||e.Category==exerciseCategory)).ToList();int pages=Math.Max(1,(found.Count+23)/24);exercisePage=Math.Max(0,Math.Min(exercisePage,pages-1));results.Children.Add(T(found.Count+" résultat(s) · page "+(exercisePage+1)+" / "+pages,12,Muted,new Thickness(0,0,0,12)));
  if(found.Count==0)results.Children.Add(T("Aucun exercice ne correspond. Essayez un autre muscle, matériel ou mot-clé. Vous pouvez aussi créer un exercice personnel dans une séance.",13,Muted));
  foreach(var definition in found.Skip(exercisePage*24).Take(24)){var e=definition;var b=new StackPanel();if(e.Images.Length>0)b.Children.Add(ExercisePhoto(e.Images[0],110));b.Children.Add(T(e.Muscle+" · "+e.Equipment,13,Accent));b.Children.Add(T(e.Level+" · "+e.Category,11,Muted));b.Children.Add(T("Aussi sollicités : "+e.Secondary,12,Muted,new Thickness(0,6,0,6)));var actions=new WrapPanel();actions.Children.Add(Btn("Voir la fiche",delegate{ShowExerciseDefinition(e,planId);},false,true));actions.Children.Add(Btn("Ajouter à une séance",delegate{AddCatalogExercise(e,planId);},true));b.Children.Add(actions);var card=Card(e.Name,b);card.Margin=new Thickness(0,0,0,12);results.Children.Add(card);}
  if(pages>1){var nav=new WrapPanel();var previous=Btn("Page précédente",delegate{exercisePage--;RenderExerciseResults(results,planId);},false,true);previous.IsEnabled=exercisePage>0;nav.Children.Add(previous);var next=Btn("Page suivante",delegate{exercisePage++;RenderExerciseResults(results,planId);},false,true);next.IsEnabled=exercisePage<pages-1;nav.Children.Add(next);results.Children.Add(nav);}
 }
 void ChooseLibraryExercise(string planId){
  StackPanel body,actions;var w=Dialog("Bibliothèque d'exercices",out body,out actions,800);w.Width=740;
  var results=new StackPanel();body.Children.Add(ExerciseLibraryFilters(results,planId));body.Children.Add(results);RenderExerciseResults(results,planId);
  actions.Children.Add(Btn("Fermer",delegate{FinishFinanceDialog(w);},false,true));ShowWindow(w);Refresh();
 }
 void ShowExerciseDefinition(ExerciseDefinition e,string planId){
  StackPanel body,actions;var w=Dialog(e.Name,out body,out actions,620);
  body.Children.Add(T(e.Muscle+" · "+e.Equipment,17,Accent));body.Children.Add(T("Muscles secondaires : "+e.Secondary,13,Muted,new Thickness(0,10,0,15)));
  if(e.Images.Length>0){var photos=new WrapPanel();foreach(var path in e.Images)photos.Children.Add(ExercisePhoto(path,190));body.Children.Add(photos);}
  body.Children.Add(T(e.Level+" · "+e.Category,12,Muted));
  body.Children.Add(T("Exécution",20));body.Children.Add(T(e.Steps,14,Ink,new Thickness(0,12,0,15)));
  if(e.AutomaticTranslation)body.Children.Add(T("Traduction automatique depuis l'anglais · formulation non révisée individuellement.",11,Muted));
  if(e.OriginalSteps!=""&&e.OriginalSteps!=e.Steps){var original=new Expander{Header="Consignes d'origine (anglais)",Content=T(e.OriginalSteps,13,Muted)};body.Children.Add(original);}
  if(e.SourceId!="")body.Children.Add(T("Free Exercise DB · Unlicense · "+e.OriginalName,11,Muted,new Thickness(0,10,0,10)));
  body.Children.Add(T("Point de départ modifiable : 3 séries de "+e.Reps+" répétitions · repos "+e.Rest+" s. Choisissez une charge adaptée à votre maîtrise du mouvement.",12,Muted));
  actions.Children.Add(Btn("Fermer",delegate{FinishFinanceDialog(w);},false,true));actions.Children.Add(Btn("Ajouter à une séance",delegate{AddCatalogExercise(e,planId);},true));ShowWindow(w);
 }
 void AddCatalogExercise(ExerciseDefinition definition,string planId){
  if(planId!=null){EditWorkoutExercise(planId,null,definition);return;}
  if(SN.workouts.Count==0){EditWorkout(null);if(SN.workouts.Count==0)return;}
  StackPanel body,actions;var w=Dialog("Choisir la séance",out body,out actions,450);
  var plans=SN.workouts.ToList();var picker=new ComboBox{ItemsSource=plans.Select(p=>p.name).ToList(),SelectedIndex=0};Field(body,"Ajouter "+definition.Name+" à",picker);
  actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));string selectedPlan=null;
  actions.Children.Add(Btn("Continuer",delegate{if(picker.SelectedIndex<0)return;selectedPlan=plans[picker.SelectedIndex].id;FinishFinanceDialog(w);},true));ShowWindow(w);
  if(selectedPlan!=null)EditWorkoutExercise(selectedPlan,null,definition);
 }
}
