
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
internal static class SportNutritionAcceptance {
 internal static void Check(bool yes,string message){if(!yes)throw new Exception(message);}
 internal static void Reject(Action action,string message){bool rejected=false;try{action();}catch(InvalidDataException){rejected=true;}Check(rejected,message);}
 internal static void Core(string folder){
  var store=new Store(Path.Combine(folder,"core"));var d=store.Value.sportNutrition;var day=new DateTime(2026,10,26);
  Check(d.goals.lactoseFree&&d.goals.batchDays==3&&d.recipes.Count==3,"Defaults and lactose preference");
  var curry=d.recipes.First(r=>r.id=="curry");var serving=SportNutrition.RecipeServing(d,curry);
  Check(serving.calories==573.8m&&serving.protein==43.25m,"Exact recipe quantities / four portions");
  d.menu.Add(new MenuEntry{id="meal1",date=day.ToString("yyyy-MM-dd"),meal="Déjeuner",recipeId="curry",servings=2});
  d.menu.Add(new MenuEntry{id="meal2",date=day.AddDays(1).ToString("yyyy-MM-dd"),meal="Dîner",recipeId="curry",servings=1});
  d.menu.Add(new MenuEntry{id="outside",date=day.AddDays(7).ToString("yyyy-MM-dd"),meal="Dîner",recipeId="curry",servings=10});
  var groceries=SportNutrition.Groceries(d,day,day.AddDays(1));Check(groceries.Single(g=>g.Id=="rice").Quantity==240&&groceries.Single(g=>g.Id=="chicken").Quantity==450,"Merged groceries / date boundaries / batch portions");
  var rice=groceries.Single(g=>g.Id=="rice");d.checkedGroceries[rice.Id]=rice.Signature;store.Save();
  Check(new Store(store.Root).Value.sportNutrition.checkedGroceries["rice"]==rice.Signature,"Checkbox persisted");
  Check(SportNutrition.Groceries(d,day.AddDays(10),day.AddDays(11)).Count==0,"Empty shopping period");
  var changed=SportNutrition.Copy(d);changed.menu[0].servings=3;
  Check(SportNutrition.Groceries(changed,day,day.AddDays(1)).Single(g=>g.Id=="rice").Signature!=rice.Signature,"Quantity changes reset checkbox");
  changed.menu[0].date=day.AddDays(14).ToString("yyyy-MM-dd");changed.menu[1].date=day.AddDays(15).ToString("yyyy-MM-dd");changed.menu[0].servings=2;
  Check(SportNutrition.Groceries(changed,day.AddDays(14),day.AddDays(15)).Single(g=>g.Id=="rice").Signature!=rice.Signature,"New shopping period resets checkbox");
  SportNutrition.LogMenu(d,d.menu[0]);Check(d.diary.Single().total.calories==serving.calories*2,"Log recipe snapshot");
  Reject(()=>SportNutrition.LogMenu(d,d.menu[0]),"No duplicate consumption");
  decimal logged=d.diary.Single().total.calories;d.foods.First(f=>f.id=="rice").per100.calories=400;
  Check(d.diary.Single().total.calories==logged,"Past diary independent of food edits");
  d.foods.First(f=>f.id=="rice").per100.calories=356;
  var suggested=SportNutrition.SuggestMenu(d,day,7);Check(suggested.Count==21&&suggested.All(m=>m.servings>0),"Seven-day menu");
  Check(suggested.Where(m=>m.date==day.ToString("yyyy-MM-dd")).Select(m=>m.recipeId+":"+m.servings).SequenceEqual(suggested.Where(m=>m.date==day.AddDays(1).ToString("yyyy-MM-dd")).Select(m=>m.recipeId+":"+m.servings)),"Batch repeat across days");
  Check(suggested.Where(m=>m.meal=="Petit-déjeuner").All(m=>m.recipeId=="breakfast"),"Breakfast recipe compatibility");
  var first=suggested.Take(3);decimal kcal=first.Sum(m=>SportNutrition.RecipeServing(d,d.recipes.First(r=>r.id==m.recipeId)).calories*m.servings);
  Check(Math.Abs(kcal-d.goals.calories)<d.goals.calories*.15m,"Suggested energy near target");
  var noLactose=SportNutrition.Copy(d);noLactose.foods.First(f=>f.id=="coconut").lactoseFree=false;
  Check(SportNutrition.SuggestMenu(noLactose,day,3).All(m=>m.recipeId!="curry"),"Lactose exclusion in automatic menu");
  noLactose.goals.lactoseFree=false;Check(SportNutrition.Allowed(noLactose,noLactose.recipes.First(r=>r.id=="curry")),"Preference switch");
  Reject(()=>SportNutrition.SuggestMenu(new SportNutritionData{recipes=new List<NutritionRecipe>()},day,3),"No compatible recipe");
  var plan=new WorkoutPlan{name="Haut du corps",exercises=new List<WorkoutExercise>{new WorkoutExercise{name="Développé couché",restSeconds=90,sets=new List<WorkoutSet>{new WorkoutSet{reps=10,kg=40},new WorkoutSet{reps=10,kg=40}}}}};
  d.workouts.Add(plan);var session=SportNutrition.Start(d,plan,day);
  Reject(()=>SportNutrition.Start(d,plan,day),"One active workout");
  Reject(()=>SportNutrition.Finish(session),"Reject empty completion");
  session.exercises[0].sets[0].done=true;session.exercises[0].sets[0].kg=42.5m;session.exercises[0].sets[0].reps=8;
  d.restUntilUtcTicks=DateTime.UtcNow.AddSeconds(90).Ticks;store.Save();
  var reloaded=new Store(store.Root).Value.sportNutrition;
  Check(reloaded.sessions.Single().exercises[0].sets[0].kg==42.5m&&reloaded.restUntilUtcTicks==d.restUntilUtcTicks,"Active workout and timer persistence");
  SportNutrition.Finish(session);Check(SportNutrition.Volume(session)==340m,"Only completed sets count");
  Check(plan.exercises[0].sets[0].kg==40&&!plan.exercises[0].sets[0].done,"Template independent of history");
  plan.name="Nouveau nom";plan.exercises[0].name="Autre exercice";Check(session.name=="Haut du corps"&&session.exercises[0].name=="Développé couché","History snapshot");
  store.Save();var export=Path.Combine(folder,"sport-export.json");store.Export(export);store.Value.sportNutrition=new SportNutritionData();store.Import(export);
  Check(store.Value.sportNutrition.sessions.Single().finished&&store.Value.sportNutrition.diary.Count==1&&store.Value.sportNutrition.checkedGroceries.Count==1,"JSON import includes all module data");
  decimal target=store.Value.sportNutrition.goals.calories;Reject(()=>SportNutrition.Change(store,x=>x.goals.calories=-1),"Reject bad goal");
  Check(store.Value.sportNutrition.goals.calories==target&&new Store(store.Root).Value.sportNutrition.goals.calories==target,"Atomic rollback");
  var zip=Path.Combine(folder,"sport-backup.zip");FullBackup.Export(store,zip);using(var backup=FullBackup.Prepare(store,zip)){Check(backup.Data.sportNutrition.sessions.Count==1&&backup.Data.sportNutrition.checkedGroceries.Count==1,"Full backup preserves module");}
  var bad=SportNutrition.Copy(store.Value);bad.sportNutrition.recipes[0].ingredients[0].foodId="missing";
  Reject(()=>Store.Validate(bad),"Dangling ingredient rejected");bad=SportNutrition.Copy(store.Value);bad.sportNutrition.workouts[0].exercises[0].sets[0].reps=0;
  Reject(()=>Store.Validate(bad),"Invalid reps rejected");bad=SportNutrition.Copy(store.Value);bad.sportNutrition.foods.Add(SportNutrition.Copy(bad.sportNutrition.foods[0]));
  Reject(()=>Store.Validate(bad),"Duplicate food rejected");bad=SportNutrition.Copy(store.Value);bad.sportNutrition.goals.batchDays=0;
  Reject(()=>Store.Validate(bad),"Invalid batch rejected");
  var old=Path.Combine(folder,"legacy");Directory.CreateDirectory(old);File.WriteAllText(Path.Combine(old,"state.json"),"{}");Check(new Store(old).Value.sportNutrition.recipes.Count==3,"Migration of old state");
 }
}
internal sealed partial class ModernWorkspace {
 internal void VerifySportUi(string folder){
  selected="Sport & Nutrition";Refresh();
  dialogProbe=delegate(Window w){LogicalControls<TextBox>(w).First().Text="Haut du corps";ClickWizard(w,"Enregistrer");return true;};
  try{EditWorkout(null);}finally{dialogProbe=null;}
  var plan=SN.workouts.Single();dialogProbe=delegate(Window w){var fields=LogicalControls<TextBox>(w).ToList();fields[0].Text="Développé couché";fields[1].Text="3";fields[2].Text="10";fields[3].Text="20";fields[4].Text="90";ClickWizard(w,"Enregistrer");return true;};
  try{EditWorkoutExercise(plan.id,null);}finally{dialogProbe=null;}
  if(SN.workouts.Single().exercises.Count!=1)throw new Exception("Exercise dialog save");
  PreviewSized(Path.Combine(folder,"Sport-seances.png"),1280,1000);
  SportNutrition.Change(store,d=>SportNutrition.Start(d,d.workouts.Single(),DateTime.Today));var active=SN.sessions.Single();
  dialogProbe=delegate(Window w){var fields=LogicalControls<TextBox>(w).ToList();fields[0].Text="12";fields[1].Text="22,5";LogicalControls<CheckBox>(w).First().IsChecked=true;ClickWizard(w,"Repos · 90 s");ClickWizard(w,"Enregistrer & fermer");return true;};
  try{RunWorkout(active.id);}finally{dialogProbe=null;}
  if(new Store(store.Root).Value.sportNutrition.sessions.Single().exercises[0].sets[0].kg!=22.5m||SN.restUntilUtcTicks<=DateTime.UtcNow.Ticks)throw new Exception("Workout draft / rest dialog");
  dialogProbe=delegate(Window w){ClickWizard(w,"Terminer la séance");return true;};try{RunWorkout(active.id);}finally{dialogProbe=null;}
  if(!SN.sessions.Single().finished||SportNutrition.Volume(SN.sessions.Single())!=270)throw new Exception("Workout completion UI");
  sportTab="Progression";Refresh();PreviewSized(Path.Combine(folder,"Sport-progression.png"),1280,1000);
  sportTab="Alimentation";Refresh();dialogProbe=delegate(Window w){LogicalControls<ComboBox>(w).Skip(1).First().SelectedIndex=0;LogicalControls<TextBox>(w).First().Text="150";ClickWizard(w,"Enregistrer");return true;};
  try{EditFoodLog();}finally{dialogProbe=null;}
  if(SN.diary.Single().total.calories!=165)throw new Exception("Food log UI calculation");
  dialogProbe=delegate(Window w){LogicalControls<TextBox>(w).First().Text="200";ClickWizard(w,"Enregistrer");return true;};try{EditFoodLog(SN.diary.Single());}finally{dialogProbe=null;}
  if(SN.diary.Single().total.calories!=220)throw new Exception("Food correction UI");
  dialogProbe=delegate(Window w){LogicalControls<TextBox>(w).First().Text="2700";ClickWizard(w,"Enregistrer");return true;};try{EditNutritionGoals();}finally{dialogProbe=null;}
  if(SN.goals.calories!=2700)throw new Exception("Goals UI");
  PreviewSized(Path.Combine(folder,"Nutrition-journal.png"),1280,1000);
  sportTab="Recettes & aliments";Refresh();PreviewSized(Path.Combine(folder,"Nutrition-recettes.png"),1280,1000);
  dialogProbe=delegate(Window w){LogicalControls<TextBox>(w).First().Text="Poulet coco maison";ClickWizard(w,"Calculer les macros par portion");LogicalControls<Button>(w).Last(b=>Convert.ToString(b.Content)=="×").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));ClickWizard(w,"+ Ingrédient");ClickWizard(w,"Calculer les macros par portion");ClickWizard(w,"Enregistrer");return true;};try{EditNutritionRecipe(SN.recipes.First(r=>r.id=="curry"));}finally{dialogProbe=null;}
  if(SN.recipes.First(r=>r.id=="curry").name!="Poulet coco maison")throw new Exception("Recipe edit UI");
  dialogProbe=delegate(Window w){ClickWizard(w,"Ajouter au planning");return true;};try{SuggestNutritionMenu();}finally{dialogProbe=null;}
  if(SN.menu.Count!=21)throw new Exception("Menu preview save UI");
  sportTab="Menus";Refresh();PreviewSized(Path.Combine(folder,"Nutrition-menus.png"),1280,1000);
  var check=LogicalControls<CheckBox>(this).First(c=>Convert.ToString(c.Content).Contains(" · "));
  check.IsChecked=true;check.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
  if(new Store(store.Root).Value.sportNutrition.checkedGroceries.Count!=1)throw new Exception("Grocery checkbox UI");
  var before=SN.menu.First();SportNutrition.Change(store,d=>SportNutrition.LogMenu(d,d.menu.First()));int count=SN.diary.Count;
  try{SportNutrition.Change(store,d=>SportNutrition.LogMenu(d,d.menu.First()));throw new Exception("Double menu log accepted");}catch(InvalidDataException){}
  if(SN.diary.Count!=count)throw new Exception("Duplicate log rollback");
  store.Value.settings.enhancements.session=new SavedWorkspaceSession{page="Sport & Nutrition",tabs=new List<SavedWorkspaceTab>{new SavedWorkspaceTab{page="Sport & Nutrition"}}};store.Save();selected="Aujourd’hui";RestoreWorkspaceSession(true,true);if(selected!="Sport & Nutrition"||new Store(store.Root).Value.settings.enhancements.session.page!="Sport & Nutrition")throw new Exception("Session tab restore");
  sportTab="Courses";Refresh();PreviewSized(Path.Combine(folder,"Nutrition-courses.png"),1280,1000);
 }
}
internal sealed partial class MainForm {
 internal void VerifySportUi(string folder){rendering=true;modern.VerifySportUi(folder);if(uiTicker!=null)uiTicker.Dispose();modern.CancelMusic();modern.CancelModeLaunch();Stop();if(speech!=null)speech.Dispose();player.Close();}
}
internal static partial class Program {
 internal static int SportTest(string folder){Root=Path.GetFullPath(folder);Directory.CreateDirectory(Root);System.Windows.Forms.Application.EnableVisualStyles();try{SportNutritionAcceptance.Core(Root);using(var form=new MainForm(false))form.VerifySportUi(Root);File.WriteAllText(Path.Combine(Root,"SPORT_PASS.txt"),"PASS: workout drafts, completion, checked-set volume, history snapshots, rest deadlines, recipe nutrition, lactose filter, macro-aware menu, batch repeats, grocery aggregation/period/quantities, duplicate-log rejection, journal corrections/snapshots, migration, validation, rollback, JSON and full backup; actual WPF editor, runner, goals, menu and checkbox controls, rendered previews. No physical desktop interaction tested.");return 0;}catch(Exception ex){File.WriteAllText(Path.Combine(Root,"SPORT_FAIL.txt"),ex.ToString());return 1;}}
}
