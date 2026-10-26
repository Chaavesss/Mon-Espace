
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

internal sealed partial class ModernWorkspace {
 string sportTab="Musculation";DateTime nutritionDay=DateTime.Today,menuStart=DateTime.Today;int menuDays=7;
 static readonly CultureInfo SportCulture=CultureInfo.GetCultureInfo("fr-FR");
 SportNutritionData SN{get{return store.Value.sportNutrition;}}
 string Fmt(decimal n){return n.ToString("0.##",SportCulture);}
 string MacroText(Nutrients n){return Fmt(n.calories)+" kcal · P "+Fmt(n.protein)+" g · G "+Fmt(n.carbs)+" g · L "+Fmt(n.fat)+" g";}
 TextBox SportText(string s){return new TextBox{Text=s,MaxLength=160};}
 TextBox SportNumber(decimal n){return SportText(Fmt(n));}
 decimal SportRead(TextBox t){decimal n;if(!Decimal.TryParse(t.Text.Trim().Replace(',','.'),NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out n))throw new InvalidOperationException("Indiquez un nombre positif (virgule ou point).");return n;}
 int SportInt(TextBox t){decimal n=SportRead(t);if(n!=Decimal.Truncate(n)||n>Int32.MaxValue)throw new InvalidOperationException("Indiquez un nombre entier.");return (int)n;}
 string SportName(TextBox t){if(String.IsNullOrWhiteSpace(t.Text)||t.Text.Trim().Length>160)throw new InvalidOperationException("Indiquez un nom de 1 à 160 caractères.");return t.Text.Trim();}
 void SportChange(Action<SportNutritionData> action){try{SportNutrition.Change(store,action);Refresh();}catch(Exception ex){MessageBox.Show(ex.Message,"Sport & Nutrition");Refresh();}}
 void SportDialogSave(Window w,StackPanel body,StackPanel actions,Action save,string label="Enregistrer"){
  var error=T("",12,Accent,new Thickness(0,12,0,0));body.Children.Add(error);
  actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));
  bool saved=false;actions.Children.Add(Btn(label,delegate{if(saved)return;try{save();saved=true;FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}},true));ShowWindow(w);Refresh();
 }
 void SportNutritionPage(StackPanel root){
  var tabs=new WrapPanel{Margin=new Thickness(0,0,0,20)};
  foreach(string name in new[]{"Musculation","Exercices","Progression","Alimentation","Recettes & aliments","Menus","Courses"}){var tab=name;tabs.Children.Add(Btn(tab,delegate{sportTab=tab;Refresh();},sportTab==tab,sportTab!=tab));}
  root.Children.Add(tabs);
  if(sportTab=="Musculation")WorkoutsPage(root);
  else if(sportTab=="Exercices")ExerciseLibraryPage(root);
  else if(sportTab=="Progression")WorkoutProgressPage(root);
  else if(sportTab=="Alimentation")NutritionDiaryPage(root);
  else if(sportTab=="Recettes & aliments")NutritionLibraryPage(root);
  else NutritionMenuPage(root);
 }
 void WorkoutsPage(StackPanel root){
  var active=SN.sessions.FirstOrDefault(s=>!s.finished);
  if(active!=null){var body=new StackPanel();body.Children.Add(T(active.name+" · "+Finance.Day(active.date).ToString("dd/MM/yyyy"),16));var actions=new WrapPanel();actions.Children.Add(Btn("Reprendre la séance",delegate{RunWorkout(active.id);},true));actions.Children.Add(Btn("Abandonner",delegate{if(MessageBox.Show("Supprimer la séance en cours et ses séries ?","Abandonner",MessageBoxButton.YesNo)==MessageBoxResult.Yes)SportChange(d=>d.sessions.RemoveAll(s=>s.id==active.id));},false,true));body.Children.Add(actions);root.Children.Add(Card("Séance en cours",body));}
  root.Children.Add(Btn("+ Nouvelle séance",delegate{EditWorkout(null);},true));
  root.Children.Add(Btn("Parcourir les exercices",delegate{sportTab="Exercices";Refresh();},false,true));
  if(SN.workouts.Count==0)root.Children.Add(Card("Votre programme commence ici",T("Créez une séance, ajoutez vos exercices, puis lancez-la pour noter les séries réalisées.",13,Muted)));
  foreach(var plan in SN.workouts){
   var p=plan;var body=new StackPanel();
   foreach(var e in p.exercises)body.Children.Add(T(e.name+" · "+e.sets.Count+" séries · "+e.sets[0].reps+" rép. · "+Fmt(e.sets[0].kg)+" kg · repos "+e.restSeconds+" s",12,Muted,new Thickness(0,5,0,5)));
   if(p.exercises.Count==0)body.Children.Add(T("Ajoutez votre premier exercice.",12,Muted));
   var actions=new WrapPanel{Margin=new Thickness(0,12,0,0)};
   var start=Btn("Lancer la séance",delegate{string id="";try{SportNutrition.Change(store,d=>id=SportNutrition.Start(d,d.workouts.First(x=>x.id==p.id),DateTime.Today).id);RunWorkout(id);}catch(Exception ex){MessageBox.Show(ex.Message);Refresh();}},true);
   start.IsEnabled=active==null&&p.exercises.Count>0;actions.Children.Add(start);actions.Children.Add(Btn("+ Bibliothèque",delegate{ChooseLibraryExercise(p.id);},false,true));actions.Children.Add(Btn("+ Exercice personnel",delegate{EditWorkoutExercise(p.id,null);},false,true));actions.Children.Add(Btn("Renommer",delegate{EditWorkout(p);},false,true));actions.Children.Add(Btn("Supprimer",delegate{if(MessageBox.Show("Supprimer ce modèle ? Les séances réalisées seront conservées.","Supprimer",MessageBoxButton.YesNo)==MessageBoxResult.Yes)SportChange(d=>d.workouts.RemoveAll(x=>x.id==p.id));},false,true));body.Children.Add(actions);
   foreach(var exercise in p.exercises){var e=exercise;var row=new WrapPanel();row.Children.Add(Btn("Modifier "+e.name,delegate{EditWorkoutExercise(p.id,e);},false,true));row.Children.Add(Btn("Retirer",delegate{SportChange(d=>d.workouts.First(x=>x.id==p.id).exercises.RemoveAll(x=>x.id==e.id));},false,true));body.Children.Add(row);}
   var card=Card(p.name,body);card.Margin=new Thickness(0,16,0,0);root.Children.Add(card);
  }
 }
 void EditWorkout(WorkoutPlan existing){
  StackPanel body,actions;var w=Dialog(existing==null?"Nouvelle séance":"Renommer la séance",out body,out actions,460);
  var name=SportText(existing==null?"":existing.name);Field(body,"Nom · par exemple Haut du corps",name);
  SportDialogSave(w,body,actions,delegate{var title=SportName(name);SportNutrition.Change(store,d=>{if(existing==null)d.workouts.Add(new WorkoutPlan{name=title});else d.workouts.First(x=>x.id==existing.id).name=title;});});
 }
 void EditWorkoutExercise(string planId,WorkoutExercise existing,ExerciseDefinition definition=null){
  if(existing==null&&definition!=null)existing=ExerciseCatalog.Create(definition);
  StackPanel body,actions;var w=Dialog(existing==null?"Ajouter un exercice":"Modifier l’exercice",out body,out actions,740);
  var name=SportText(existing==null?"":existing.name);Field(body,"Exercice · nom libre",name);
  var sets=SportNumber(existing==null?3:existing.sets.Count);Field(body,"Nombre de séries",sets);
  var reps=SportNumber(existing==null?10:existing.sets[0].reps);Field(body,"Répétitions par série",reps);
  var kg=SportNumber(existing==null?0:existing.sets[0].kg);Field(body,"Charge (kg) · 0 pour poids du corps",kg);
  var rest=SportNumber(existing==null?90:existing.restSeconds);Field(body,"Repos entre séries (secondes)",rest);
  var notes=new TextBox{Text=existing==null?"":existing.notes,MaxLength=2000,Height=65,AcceptsReturn=true,TextWrapping=TextWrapping.Wrap};Field(body,"Consignes (facultatif)",notes);
  body.Children.Add(T("Ces valeurs préparent les séries. Pendant la séance, chaque série peut être ajustée séparément.",11,Muted));
  SportDialogSave(w,body,actions,delegate{int count=SportInt(sets);SportNutrition.Require(count>=1&&count<=30,"Choisissez de 1 à 30 séries.");var e=new WorkoutExercise{id=existing==null?SportNutrition.Id():existing.id,name=SportName(name),restSeconds=SportInt(rest),notes=notes.Text};int r=SportInt(reps);decimal k=SportRead(kg);for(int i=0;i<count;i++)e.sets.Add(new WorkoutSet{reps=r,kg=k});SportNutrition.Change(store,d=>{var p=d.workouts.First(x=>x.id==planId);int index=p.exercises.FindIndex(x=>x.id==e.id);if(index<0)p.exercises.Add(e);else p.exercises[index]=e;});});
 }
 void RunWorkout(string sessionId){
  var original=SN.sessions.First(s=>s.id==sessionId);var draft=SportNutrition.Copy(original);
  StackPanel body,actions;var w=Dialog(draft.name,out body,out actions,820);w.Width=700;
  body.Children.Add(T("Cochez les séries réalisées. Seules ces séries alimentent la progression. Le brouillon reste disponible après enregistrement. Utilisez Enregistrer & fermer pour conserver vos saisies.",12,Muted));
  var clock=T("",20,Accent,new Thickness(0,12,0,12));body.Children.Add(clock);
  Action updateClock=delegate{long remaining=SN.restUntilUtcTicks-DateTime.UtcNow.Ticks;int seconds=(int)Math.Max(0,Math.Ceiling(remaining/(double)TimeSpan.TicksPerSecond));clock.Text=seconds>0?"Repos · "+(seconds/60).ToString("00")+":"+(seconds%60).ToString("00"):"Prêt pour la prochaine série";};
  var timer=new DispatcherTimer{Interval=TimeSpan.FromSeconds(1)};timer.Tick+=delegate{updateClock();};w.Closed+=delegate{timer.Stop();};updateClock();timer.Start();
  var reads=new List<Action>();
  foreach(var exercise in draft.exercises){
   var e=exercise;body.Children.Add(T(e.name,19,Ink,new Thickness(0,15,0,8)));if(!String.IsNullOrWhiteSpace(e.notes))body.Children.Add(T(e.notes,12,Muted));
   int i=0;foreach(var set in e.sets){var t=set;var row=new WrapPanel{Margin=new Thickness(0,4,0,4)};
    row.Children.Add(T((++i)+".",12,Muted,new Thickness(0,12,10,0)));
    var reps=SportNumber(t.reps);reps.Width=80;reps.ToolTip="Répétitions";row.Children.Add(reps);row.Children.Add(T(" rép. ",12,Muted,new Thickness(5,12,5,0)));
    var kg=SportNumber(t.kg);kg.Width=80;kg.ToolTip="Charge en kg";row.Children.Add(kg);row.Children.Add(T(" kg ",12,Muted,new Thickness(5,12,10,0)));
    var done=new CheckBox{Content="Réalisée",IsChecked=t.done,VerticalAlignment=VerticalAlignment.Center};row.Children.Add(done);
    reads.Add(delegate{t.reps=SportInt(reps);t.kg=SportRead(kg);t.done=done.IsChecked==true;});
    body.Children.Add(row);
   }
   body.Children.Add(Btn("Repos · "+e.restSeconds+" s",delegate{try{SportNutrition.Change(store,d=>d.restUntilUtcTicks=DateTime.UtcNow.AddSeconds(e.restSeconds).Ticks);updateClock();}catch(Exception ex){MessageBox.Show(ex.Message);}},false,true));
  }
  var error=T("",12,Accent,new Thickness(0,15,0,0));body.Children.Add(error);
  Action<bool> save=delegate(bool finish){try{foreach(var read in reads)read();if(finish)SportNutrition.Finish(draft);SportNutrition.Change(store,d=>{d.sessions[d.sessions.FindIndex(s=>s.id==sessionId)]=SportNutrition.Copy(draft);if(finish)d.restUntilUtcTicks=0;});FinishFinanceDialog(w);}catch(Exception ex){draft.finished=false;error.Text=ex.Message;}};
  actions.Children.Add(Btn("Enregistrer & fermer",delegate{save(false);},false,true));
  actions.Children.Add(Btn("Terminer la séance",delegate{save(true);},true));
  // Closing through X/Escape preserves the last saved draft.
  ShowWindow(w);Refresh();
 }
 void WorkoutProgressPage(StackPanel root){
  var history=SN.sessions.Where(s=>s.finished).OrderByDescending(s=>s.date).ThenByDescending(s=>SN.sessions.IndexOf(s)).ToList();
  if(history.Count==0){root.Children.Add(Card("Vos premières performances",T("Terminez une séance pour retrouver votre volume, vos charges et vos répétitions ici.",13,Muted)));return;}
  foreach(var group in history.SelectMany(s=>s.exercises.Select(e=>new{Session=s,Exercise=e})).GroupBy(x=>x.Exercise.name.Trim(),StringComparer.OrdinalIgnoreCase)){
   var body=new StackPanel();var records=group.Where(x=>x.Exercise.sets.Any(t=>t.done)).ToList();if(records.Count==0)continue;
   var best=records.SelectMany(x=>x.Exercise.sets.Where(t=>t.done)).Max(t=>t.kg);
   body.Children.Add(T("Meilleure charge · "+Fmt(best)+" kg",21));
   var recent=records.Take(12).ToList();decimal maximum=Math.Max(1,recent.Max(x=>x.Exercise.sets.Where(t=>t.done).Sum(t=>t.kg*t.reps)));
   foreach(var item in recent){var sets=item.Exercise.sets.Where(t=>t.done).ToList();decimal volume=sets.Sum(t=>t.kg*t.reps);body.Children.Add(T(Finance.Day(item.Session.date).ToString("dd/MM/yyyy")+" · "+String.Join(" / ",sets.Select(t=>t.reps+" × "+Fmt(t.kg)+" kg"))+" · volume "+Fmt(volume)+" kg",12,Muted,new Thickness(0,10,0,5)));body.Children.Add(new ProgressBar{Minimum=0,Maximum=(double)maximum,Value=(double)volume,Height=5,Foreground=Accent,Background=Line,BorderThickness=new Thickness(0)});}
   var card=Card(group.Key,body);card.Margin=new Thickness(0,0,0,16);root.Children.Add(card);
  }
  root.Children.Add(T("Historique des séances",22,Ink,new Thickness(0,12,0,12)));
  foreach(var session in history.Take(50)){var s=session;var row=new WrapPanel();row.Children.Add(T(Finance.Day(s.date).ToString("dd/MM/yyyy")+" · "+s.name+" · "+Fmt(SportNutrition.Volume(s))+" kg cumulés",13,Ink,new Thickness(0,12,12,0)));row.Children.Add(Btn("Détail",delegate{WorkoutHistoryDetail(s);},false,true));row.Children.Add(Btn("Supprimer",delegate{if(MessageBox.Show("Supprimer cette séance de l’historique ?","Supprimer",MessageBoxButton.YesNo)==MessageBoxResult.Yes)SportChange(d=>d.sessions.RemoveAll(x=>x.id==s.id));},false,true));root.Children.Add(row);}
 }
 void WorkoutHistoryDetail(WorkoutSession s){
  StackPanel body,actions;var w=Dialog(s.name,out body,out actions,680);
  foreach(var e in s.exercises){body.Children.Add(T(e.name,18,Ink,new Thickness(0,12,0,8)));foreach(var t in e.sets)body.Children.Add(T((t.done?"✓ ":"○ ")+t.reps+" répétitions × "+Fmt(t.kg)+" kg",13,Muted));}
  actions.Children.Add(Btn("Fermer",delegate{w.DialogResult=false;},true));ShowWindow(w);
 }
 void NutritionDayTools(StackPanel root){
  var row=new WrapPanel();row.Children.Add(Btn("‹",delegate{nutritionDay=nutritionDay.AddDays(-1);Refresh();},false,true));row.Children.Add(Btn("Aujourd’hui",delegate{nutritionDay=DateTime.Today;Refresh();},false,true));row.Children.Add(Btn("›",delegate{nutritionDay=nutritionDay.AddDays(1);Refresh();},false,true));
  var picker=new DatePicker{SelectedDate=nutritionDay,Width=150,Margin=new Thickness(8,0,10,0)};picker.SelectedDateChanged+=delegate{if(picker.SelectedDate.HasValue&&picker.SelectedDate.Value!=nutritionDay){nutritionDay=picker.SelectedDate.Value;Refresh();}};row.Children.Add(picker);
  row.Children.Add(Btn("Mes objectifs",delegate{EditNutritionGoals();},false,true));root.Children.Add(row);
 }
 void MacroSummary(StackPanel root,Nutrients n){
  var g=SN.goals;var grid=new UniformGrid{Columns=4,Margin=new Thickness(0,18,0,18)};
  var labels=new[]{"Calories","Protéines","Glucides","Lipides"};var actual=new[]{n.calories,n.protein,n.carbs,n.fat};var target=new[]{g.calories,g.protein,g.carbs,g.fat};
  for(int i=0;i<4;i++){var b=new StackPanel();b.Children.Add(T(labels[i],11,Muted));b.Children.Add(T(Fmt(actual[i])+" / "+Fmt(target[i])+(i==0?" kcal":" g"),18,Ink,new Thickness(0,10,0,8)));b.Children.Add(new ProgressBar{Minimum=0,Maximum=(double)Math.Max(1,target[i]),Value=(double)Math.Min(actual[i],Math.Max(1,target[i])),Height=5,Foreground=Accent,Background=Line,BorderThickness=new Thickness(0)});b.Children.Add(T((actual[i]>target[i]?"Au-dessus : ":"Restant : ")+Fmt(Math.Abs(target[i]-actual[i]))+(i==0?" kcal":" g"),10,Muted,new Thickness(0,8,0,0)));var box=Box(b,new Thickness(14));box.Margin=new Thickness(0,0,8,0);grid.Children.Add(box);}root.Children.Add(grid);
 }
 void NutritionDiaryPage(StackPanel root){
  NutritionDayTools(root);var day=nutritionDay.ToString("yyyy-MM-dd");var logs=SN.diary.Where(l=>l.date==day).ToList();MacroSummary(root,SportNutrition.Sum(logs.Select(l=>l.total)));
  root.Children.Add(T(SN.goals.aim+" · "+(SN.goals.lactoseFree?"Préférence sans lactose":"Tous les aliments")+" · Journal des quantités consommées",12,Muted));
  root.Children.Add(Btn("+ Ajouter un aliment ou un repas",delegate{EditFoodLog();},true));
  if(logs.Count==0)root.Children.Add(Card("Une journée à renseigner",T("Ajoutez vos quantités consommées. Les menus prévus ne sont comptés qu’après validation dans le journal.",12,Muted)));
  foreach(var group in logs.GroupBy(l=>l.meal)){var b=new StackPanel();foreach(var log in group){var l=log;var row=new StackPanel();row.Children.Add(T(l.name+" · quantité/portions "+Fmt(l.servings),14));row.Children.Add(T(MacroText(l.total),12,Muted));var actions=new WrapPanel();actions.Children.Add(Btn("Corriger",delegate{EditFoodLog(l);},false,true));actions.Children.Add(Btn("Retirer",delegate{SportChange(d=>d.diary.RemoveAll(x=>x.id==l.id));},false,true));row.Children.Add(actions);b.Children.Add(row);}var card=Card(group.Key,b);card.Margin=new Thickness(0,16,0,0);root.Children.Add(card);}
  root.Children.Add(T("Les valeurs de départ sont indicatives. Ajustez les aliments avec les valeurs des emballages. Les poids des féculents et viandes de la base sont crus.",11,Muted,new Thickness(0,18,0,0)));
 }
 void EditNutritionGoals(){
  StackPanel body,actions;var w=Dialog("Mes objectifs & préférences",out body,out actions,760);var g=SN.goals;
  var aim=Combo(new[]{"Maintien","Prise de masse","Perte de poids"},g.aim);Field(body,"Objectif personnel",aim);
  var kcal=SportNumber(g.calories);Field(body,"Calories par jour",kcal);var p=SportNumber(g.protein);Field(body,"Protéines (g / jour)",p);var c=SportNumber(g.carbs);Field(body,"Glucides (g / jour)",c);var f=SportNumber(g.fat);Field(body,"Lipides (g / jour)",f);
  var lactose=new CheckBox{Content="Privilégier les aliments et recettes sans lactose",IsChecked=g.lactoseFree};body.Children.Add(lactose);
  var batch=SportNumber(g.batchDays);Field(body,"Batch cooking · répéter les recettes sur combien de jours ? (1 à 14)",batch);
  body.Children.Add(T("Objectifs saisis librement. Le choix maintien / prise de masse / perte de poids ne modifie pas vos chiffres. Énergie des macros : 4 × protéines + 4 × glucides + 9 × lipides ; les calories restent un objectif indépendant.",11,Muted));
  SportDialogSave(w,body,actions,delegate{var goal=new NutritionGoals{aim=Convert.ToString(aim.SelectedItem),calories=SportRead(kcal),protein=SportRead(p),carbs=SportRead(c),fat=SportRead(f),lactoseFree=lactose.IsChecked==true,batchDays=SportInt(batch)};SportNutrition.Change(store,d=>d.goals=goal);});
 }
 void EditFoodLog(FoodLog existing=null){
  StackPanel body,actions;var w=Dialog(existing==null?"Ajouter au journal":"Corriger une consommation",out body,out actions,680);
  var meal=Combo(new[]{"Petit-déjeuner","Déjeuner","Dîner","Collation"},existing==null?"Déjeuner":existing.meal);Field(body,"Repas",meal);
  if(existing!=null){body.Children.Add(T(existing.name+" · "+MacroText(existing.total),14));var amount=SportNumber(existing.servings);Field(body,"Nouvelle quantité / nombre de portions",amount);body.Children.Add(T("Les valeurs seront ajustées proportionnellement à la quantité enregistrée.",12,Muted));
   SportDialogSave(w,body,actions,delegate{decimal q=SportRead(amount);SportNutrition.Require(q>0,"Indiquez une quantité positive.");var changed=SportNutrition.Copy(existing);changed.total=SportNutrition.Proportion(existing.total,q,existing.servings);changed.servings=q;changed.meal=Convert.ToString(meal.SelectedItem);SportNutrition.Change(store,d=>d.diary[d.diary.FindIndex(x=>x.id==existing.id)]=changed);});return;}
  var foods=SN.foods.Where(f=>!SN.goals.lactoseFree||f.lactoseFree).ToList();var recipes=SN.recipes.Where(r=>SportNutrition.Allowed(SN,r)).ToList();
  var names=foods.Select(f=>"Aliment · "+f.name+" ("+f.unit+")").Concat(recipes.Select(r=>"Recette · "+r.name+" (portion)")).ToArray();
  if(names.Length==0){body.Children.Add(T("Ajoutez un aliment ou une recette compatible dans Recettes & aliments.",13,Muted));actions.Children.Add(Btn("Fermer",delegate{w.DialogResult=false;},true));ShowWindow(w);return;}
  var choice=Combo(names,names[0]);Field(body,"Aliment ou recette",choice);
  var quantity=SportNumber(100);Field(body,"Quantité : g / ml / pièces pour un aliment ; portions pour une recette",quantity);
  var preview=T("",13,Accent);body.Children.Add(preview);
  Action calc=delegate{try{decimal q=SportRead(quantity);Nutrients n=choice.SelectedIndex<foods.Count?SportNutrition.Scale(foods[choice.SelectedIndex].per100,q/100m):SportNutrition.Scale(SportNutrition.RecipeServing(SN,recipes[choice.SelectedIndex-foods.Count]),q);preview.Text=MacroText(n);}catch{preview.Text="Vérifiez la quantité.";}};quantity.TextChanged+=delegate{calc();};choice.SelectionChanged+=delegate{quantity.Text=choice.SelectedIndex<foods.Count?"100":"1";calc();};calc();
  SportDialogSave(w,body,actions,delegate{decimal q=SportRead(quantity);SportNutrition.Require(q>0,"Indiquez une quantité positive.");var l=new FoodLog{date=nutritionDay.ToString("yyyy-MM-dd"),meal=Convert.ToString(meal.SelectedItem),servings=q};
   if(choice.SelectedIndex<foods.Count){var f=foods[choice.SelectedIndex];l.name=f.name;l.total=SportNutrition.Scale(f.per100,q/100m);}else{var r=recipes[choice.SelectedIndex-foods.Count];l.name=r.name;l.total=SportNutrition.Scale(SportNutrition.RecipeServing(SN,r),q);}SportNutrition.Change(store,d=>d.diary.Add(l));});
 }
 void NutritionLibraryPage(StackPanel root){
  var tools=new WrapPanel();tools.Children.Add(Btn("+ Recette",delegate{EditNutritionRecipe(null);},true));tools.Children.Add(Btn("+ Aliment",delegate{EditNutritionFood(null);},false,true));tools.Children.Add(Btn("Préférences",delegate{EditNutritionGoals();},false,true));root.Children.Add(tools);
  root.Children.Add(T("Valeurs indicatives à adapter aux emballages. Quantités par recette complète ; macros affichées par portion. La préférence sans lactose filtre les suggestions, les autres recettes restent consultables.",12,Muted,new Thickness(0,14,0,18)));
  foreach(var recipe in SN.recipes){var r=recipe;var b=new StackPanel();b.Children.Add(T(MacroText(SportNutrition.RecipeServing(SN,r))+" / portion",16));b.Children.Add(T(r.portions+" portions · "+(SportNutrition.Allowed(SN,r)?"Compatible avec vos préférences":"Contient un aliment déclaré avec lactose"),11,Muted,new Thickness(0,8,0,8)));
   foreach(var i in r.ingredients){var f=SN.foods.First(x=>x.id==i.foodId);b.Children.Add(T(Fmt(i.quantity)+" "+f.unit+" · "+f.name,12,Muted));}
   b.Children.Add(T(r.instructions,12,Muted,new Thickness(0,10,0,12)));var actions=new WrapPanel();actions.Children.Add(Btn("Modifier",delegate{EditNutritionRecipe(r);},false,true));actions.Children.Add(Btn("Planifier",delegate{AddMenuRecipe(r);},true));actions.Children.Add(Btn("Supprimer",delegate{if(SN.menu.Any(m=>m.recipeId==r.id)){MessageBox.Show("Retirez d’abord cette recette des menus prévus.");return;}if(MessageBox.Show("Supprimer cette recette ?","Supprimer",MessageBoxButton.YesNo)==MessageBoxResult.Yes)SportChange(d=>d.recipes.RemoveAll(x=>x.id==r.id));},false,true));b.Children.Add(actions);var card=Card(r.name,b);card.Margin=new Thickness(0,0,0,16);root.Children.Add(card);}
  root.Children.Add(T("Mes aliments",22,Ink,new Thickness(0,16,0,12)));
  foreach(var food in SN.foods){var f=food;var b=new StackPanel();b.Children.Add(T(f.name+" · "+MacroText(f.per100)+" / 100 "+f.unit,12));b.Children.Add(T(f.aisle+" · "+(f.lactoseFree?"Déclaré sans lactose":"Avec lactose"),11,Muted));var a=new WrapPanel();a.Children.Add(Btn("Modifier",delegate{EditNutritionFood(f);},false,true));a.Children.Add(Btn("Supprimer",delegate{if(SN.recipes.Any(r=>r.ingredients.Any(i=>i.foodId==f.id))){MessageBox.Show("Cet aliment est utilisé dans une recette. Modifiez d’abord les recettes concernées.");return;}SportChange(d=>d.foods.RemoveAll(x=>x.id==f.id));},false,true));b.Children.Add(a);root.Children.Add(b);}
 }
 void EditNutritionFood(NutritionFood existing){
  StackPanel body,actions;var w=Dialog(existing==null?"Nouvel aliment":"Modifier l’aliment",out body,out actions,800);var f=existing??new NutritionFood();
  var name=SportText(f.name);Field(body,"Nom précis · préciser cru / cuit",name);var unit=Combo(new[]{"g","ml","pièce"},f.unit);Field(body,"Unité de quantité",unit);var aisle=SportText(f.aisle);Field(body,"Rayon pour la liste de courses",aisle);
  var kcal=SportNumber(f.per100.calories);Field(body,"Calories pour 100 unités (g / ml / pièces)",kcal);var p=SportNumber(f.per100.protein);Field(body,"Protéines pour 100 unités (g)",p);var c=SportNumber(f.per100.carbs);Field(body,"Glucides pour 100 unités (g)",c);var fat=SportNumber(f.per100.fat);Field(body,"Lipides pour 100 unités (g)",fat);
  var lactose=new CheckBox{Content="Cet aliment est sans lactose",IsChecked=f.lactoseFree};body.Children.Add(lactose);
  body.Children.Add(T("En unité pièce, renseignez les valeurs pour 100 pièces (par exemple 100 œufs). Les consommations déjà enregistrées gardent leurs valeurs d’origine.",11,Muted));
  SportDialogSave(w,body,actions,delegate{var food=new NutritionFood{id=f.id,name=SportName(name),unit=Convert.ToString(unit.SelectedItem),aisle=SportName(aisle),lactoseFree=lactose.IsChecked==true,per100=new Nutrients{calories=SportRead(kcal),protein=SportRead(p),carbs=SportRead(c),fat=SportRead(fat)}};SportNutrition.Change(store,d=>{int index=d.foods.FindIndex(x=>x.id==food.id);if(index<0)d.foods.Add(food);else d.foods[index]=food;});});
 }
 void EditNutritionRecipe(NutritionRecipe existing){
  if(SN.foods.Count==0){MessageBox.Show("Ajoutez d’abord un aliment.");return;}
  var draft=SportNutrition.Copy(existing??new NutritionRecipe());var foods=SN.foods.ToList();
  StackPanel body,actions;var w=Dialog(existing==null?"Nouvelle recette":"Modifier la recette",out body,out actions,830);
  var name=SportText(draft.name);Field(body,"Nom de la recette",name);var portions=SportNumber(draft.portions);Field(body,"Nombre de portions obtenues",portions);var mealType=Combo(new[]{"Tous","Petit-déjeuner","Déjeuner","Dîner","Déjeuner & dîner","Collation"},draft.meal);Field(body,"Type de repas pour les suggestions",mealType);
  var ingredientPanel=new StackPanel();body.Children.Add(ingredientPanel);
  var ingredientReads=new List<Func<RecipeIngredient>>();
  Action render=null;render=delegate{
   ingredientPanel.Children.Clear();ingredientReads.Clear();
   foreach(var item in draft.ingredients){int ingredientIndex=ingredientReads.Count;var row=new WrapPanel{Margin=new Thickness(0,4,0,4)};var food=Combo(foods.Select(f=>f.name+" ("+f.unit+")"),foods.First(f=>f.id==item.foodId).name+" ("+foods.First(f=>f.id==item.foodId).unit+")");food.Width=260;row.Children.Add(food);var amount=SportNumber(item.quantity);amount.Width=90;row.Children.Add(amount);
    ingredientReads.Add(delegate{return new RecipeIngredient{foodId=foods[food.SelectedIndex].id,quantity=SportRead(amount)};});
    row.Children.Add(Btn("×",delegate{try{var current=ingredientReads.Select(r=>r()).ToList();int index=ingredientIndex;current.RemoveAt(index);draft.ingredients=current;render();}catch(Exception ex){MessageBox.Show(ex.Message);}},false,true));ingredientPanel.Children.Add(row);
   }
  };render();
  body.Children.Add(Btn("+ Ingrédient",delegate{try{draft.ingredients=ingredientReads.Select(r=>r()).ToList();draft.ingredients.Add(new RecipeIngredient{foodId=foods[0].id,quantity=100});render();}catch(Exception ex){MessageBox.Show(ex.Message);}},false,true));
  var instructions=new TextBox{Text=draft.instructions,MaxLength=10000,Height=100,AcceptsReturn=true,TextWrapping=TextWrapping.Wrap};Field(body,"Préparation",instructions);
  body.Children.Add(T("Les quantités correspondent à la recette complète. Les portions planifiées ajusteront les courses automatiquement. Les doublons d’un aliment seront additionnés.",11,Muted));
  var preview=T("",12,Accent);body.Children.Add(preview);body.Children.Add(Btn("Calculer les macros par portion",delegate{try{draft.ingredients=ingredientReads.Select(r=>r()).ToList();draft.portions=SportInt(portions);SportNutrition.Require(draft.portions>0,"Nombre de portions positif requis.");preview.Text=MacroText(SportNutrition.RecipeServing(SN,draft));}catch(Exception ex){preview.Text=ex.Message;}},false,true));
  SportDialogSave(w,body,actions,delegate{draft.name=SportName(name);draft.meal=Convert.ToString(mealType.SelectedItem);draft.portions=SportInt(portions);draft.instructions=instructions.Text;draft.ingredients=ingredientReads.Select(r=>r()).ToList();SportNutrition.Change(store,d=>{int index=d.recipes.FindIndex(r=>r.id==draft.id);if(index<0)d.recipes.Add(draft);else d.recipes[index]=draft;});});
 }
 void AddMenuRecipe(NutritionRecipe r){
  if(!SportNutrition.Allowed(SN,r)){MessageBox.Show("Cette recette ne correspond pas à la préférence sans lactose. Modifiez la recette ou vos préférences.");return;}
  StackPanel body,actions;var w=Dialog("Planifier · "+r.name,out body,out actions,560);var date=new DatePicker{SelectedDate=nutritionDay};Field(body,"Date du repas",date);var meal=Combo(new[]{"Petit-déjeuner","Déjeuner","Dîner","Collation"},"Déjeuner");Field(body,"Repas",meal);var portions=SportNumber(1);Field(body,"Nombre de portions consommées à cette date",portions);
  body.Children.Add(T("Pour préparer plusieurs jours, planifiez chaque repas ou utilisez Créer un menu dans Menus.",12,Muted));
  SportDialogSave(w,body,actions,delegate{SportNutrition.Require(date.SelectedDate.HasValue,"Choisissez une date.");var m=new MenuEntry{date=date.SelectedDate.Value.ToString("yyyy-MM-dd"),recipeId=r.id,meal=Convert.ToString(meal.SelectedItem),servings=SportRead(portions)};SportNutrition.Change(store,d=>d.menu.Add(m));});
 }
 void NutritionMenuPage(StackPanel root){
  var tools=new WrapPanel();var start=new DatePicker{SelectedDate=menuStart,Width=150};tools.Children.Add(start);var days=Combo(new[]{"1","3","5","7","14"},menuDays.ToString());days.Width=85;tools.Children.Add(days);tools.Children.Add(Btn("Afficher",delegate{if(start.SelectedDate.HasValue){menuStart=start.SelectedDate.Value;menuDays=Int32.Parse(Convert.ToString(days.SelectedItem));Refresh();}},false,true));tools.Children.Add(Btn("Créer un menu adapté",delegate{SuggestNutritionMenu();},true));tools.Children.Add(Btn("Préférences",delegate{EditNutritionGoals();},false,true));root.Children.Add(tools);
  var end=menuStart.AddDays(menuDays-1);root.Children.Add(T("Du "+menuStart.ToString("dd/MM/yyyy")+" au "+end.ToString("dd/MM/yyyy")+" · "+menuDays+" jours · batch cooking sur "+SN.goals.batchDays+" jours",12,Muted,new Thickness(0,14,0,12)));
  var menus=SN.menu.Where(m=>Finance.Day(m.date)>=menuStart&&Finance.Day(m.date)<=end).OrderBy(m=>m.date).ToList();
  if(menus.Count==0)root.Children.Add(Card("Préparez vos prochains repas",T("Créez un menu adapté ou planifiez vos propres recettes depuis Recettes & aliments.",13,Muted)));
  foreach(var group in menus.Where(m=>sportTab!="Courses").GroupBy(m=>m.date)){
   var b=new StackPanel();var total=SportNutrition.Sum(group.Select(m=>SportNutrition.Scale(SportNutrition.RecipeServing(SN,SN.recipes.First(r=>r.id==m.recipeId)),m.servings)));b.Children.Add(T(MacroText(total),16));
   foreach(var entry in group){var m=entry;var r=SN.recipes.First(x=>x.id==m.recipeId);b.Children.Add(T(m.meal+" · "+r.name+" · "+Fmt(m.servings)+" portion(s)",13,Ink,new Thickness(0,10,0,4)));if(!SportNutrition.Allowed(SN,r))b.Children.Add(T("À adapter : contient un aliment déclaré avec lactose.",12,Accent));var a=new WrapPanel();a.Children.Add(Btn("Consommé → journal",delegate{if(MessageBox.Show("Ajouter ce repas au journal du "+Finance.Day(m.date).ToString("dd/MM")+" ?","Confirmer la consommation",MessageBoxButton.YesNo)==MessageBoxResult.Yes)SportChange(d=>SportNutrition.LogMenu(d,d.menu.First(x=>x.id==m.id)));},false,true));a.Children.Add(Btn("Portions",delegate{EditMenuPortions(m);},false,true));a.Children.Add(Btn("Retirer",delegate{SportChange(d=>d.menu.RemoveAll(x=>x.id==m.id));},false,true));b.Children.Add(a);}
   var card=Card(Finance.Day(group.Key).ToString("dddd dd MMMM",SportCulture),b);card.Margin=new Thickness(0,0,0,16);root.Children.Add(card);
  }
  var groceries=SportNutrition.Groceries(SN,menuStart,end);var shopping=new StackPanel();
  shopping.Children.Add(T("Quantités totales à acheter pour les repas prévus, regroupées par aliment et rayon. Les mêmes aliments doivent réutiliser la même fiche pour être regroupés.",12,Muted,new Thickness(0,0,0,12)));
  foreach(var group in groceries.GroupBy(x=>x.Aisle)){shopping.Children.Add(Eyebrow(group.Key));foreach(var line in group){var l=line;string signature;bool bought=SN.checkedGroceries.TryGetValue(l.Id,out signature)&&signature==l.Signature;var box=new CheckBox{Content=Fmt(l.Quantity)+" "+l.Unit+" · "+l.Name,IsChecked=bought,Margin=new Thickness(0,6,0,6)};box.Click+=delegate{SportChange(d=>{if(box.IsChecked==true)d.checkedGroceries[l.Id]=l.Signature;else d.checkedGroceries.Remove(l.Id);});};shopping.Children.Add(box);}}
  if(groceries.Count==0)shopping.Children.Add(T("Les ingrédients apparaîtront après la planification d’un repas.",12,Muted));else shopping.Children.Add(Btn("Tout décocher",delegate{SportChange(d=>d.checkedGroceries.Clear());},false,true));root.Children.Add(Card("Ma liste de courses",shopping));
  var batch=new StackPanel();foreach(var group in menus.GroupBy(m=>m.recipeId)){var r=SN.recipes.First(x=>x.id==group.Key);decimal portions=group.Sum(m=>m.servings);batch.Children.Add(T(r.name+" · préparer "+Fmt(portions)+" portions (× "+Fmt(portions/r.portions)+" la recette)",13,Ink,new Thickness(0,8,0,6)));}if(menus.Count>0)root.Children.Add(Card("Préparation batch cooking",batch));
 }
 void EditMenuPortions(MenuEntry existing){
  StackPanel body,actions;var w=Dialog("Ajuster les portions",out body,out actions,450);var portions=SportNumber(existing.servings);Field(body,"Portions pour ce repas",portions);body.Children.Add(T("Les macros et les quantités de courses seront recalculées.",12,Muted));SportDialogSave(w,body,actions,delegate{decimal q=SportRead(portions);SportNutrition.Change(store,d=>d.menu.First(m=>m.id==existing.id).servings=q);});
 }
 void SuggestNutritionMenu(){
  List<MenuEntry> suggested;try{suggested=SportNutrition.SuggestMenu(SN,menuStart,menuDays);}catch(Exception ex){MessageBox.Show(ex.Message);return;}
  StackPanel body,actions;var w=Dialog("Proposition de menu",out body,out actions,800);
  body.Children.Add(T("Les recettes et portions sont ajustées selon les calories et macros visées : "+Fmt(SN.goals.calories)+" kcal par jour. Les macros dépendent des recettes : comparez-les à vos objectifs avant d’ajouter. La proposition ajoute des repas au planning existant.",12,Muted));
  var firstDay=suggested.Where(m=>m.date==menuStart.ToString("yyyy-MM-dd")).ToList();var total=SportNutrition.Sum(firstDay.Select(m=>SportNutrition.Scale(SportNutrition.RecipeServing(SN,SN.recipes.First(r=>r.id==m.recipeId)),m.servings)));
  body.Children.Add(T("Premier jour · "+MacroText(total),16,Accent,new Thickness(0,14,0,8)));
  body.Children.Add(T("Objectifs · P "+Fmt(SN.goals.protein)+" g · G "+Fmt(SN.goals.carbs)+" g · L "+Fmt(SN.goals.fat)+" g",12,Muted));
  foreach(var day in suggested.GroupBy(m=>m.date)){body.Children.Add(T(Finance.Day(day.Key).ToString("dd/MM/yyyy"),17,Ink,new Thickness(0,14,0,7)));foreach(var m in day){var r=SN.recipes.First(x=>x.id==m.recipeId);body.Children.Add(T(m.meal+" · "+r.name+" · "+Fmt(m.servings)+" portion(s)",12,Muted));}}
  SportDialogSave(w,body,actions,delegate{SportNutrition.Change(store,d=>d.menu.AddRange(suggested));},"Ajouter au planning");
 }
}
