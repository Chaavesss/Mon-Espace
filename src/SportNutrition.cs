
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Script.Serialization;

public sealed class SportNutritionData {
 public NutritionGoals goals=new NutritionGoals();
 public List<WorkoutPlan> workouts=new List<WorkoutPlan>();
 public List<WorkoutSession> sessions=new List<WorkoutSession>();
 public List<NutritionFood> foods=SportNutrition.DefaultFoods();
 public List<NutritionRecipe> recipes=SportNutrition.DefaultRecipes();
 public List<FoodLog> diary=new List<FoodLog>();
 public List<MenuEntry> menu=new List<MenuEntry>();
 public Dictionary<string,string> checkedGroceries=new Dictionary<string,string>();
 public long restUntilUtcTicks;
}
public sealed class NutritionGoals {public decimal calories=2600,protein=150,carbs=320,fat=80;public bool lactoseFree=true;public int batchDays=3;public string aim="Maintien";}
public sealed class WorkoutPlan {public string id=SportNutrition.Id(),name="";public List<WorkoutExercise> exercises=new List<WorkoutExercise>();}
public sealed class WorkoutExercise {public string id=SportNutrition.Id(),name="",notes="";public int restSeconds=90;public List<WorkoutSet> sets=new List<WorkoutSet>();}
public sealed class WorkoutSet {public int reps=10;public decimal kg;public bool done;}
public sealed class WorkoutSession {public string id=SportNutrition.Id(),planId="",name="",date=DateTime.Today.ToString("yyyy-MM-dd");public bool finished;public List<WorkoutExercise> exercises=new List<WorkoutExercise>();}
public sealed class Nutrients {public decimal calories,protein,carbs,fat;}
public sealed class NutritionFood {public string id=SportNutrition.Id(),name="",unit="g",aisle="Autres";public bool lactoseFree=true;public Nutrients per100=new Nutrients();}
public sealed class RecipeIngredient {public string foodId="";public decimal quantity;}
public sealed class NutritionRecipe {public string id=SportNutrition.Id(),name="",instructions="",meal="Tous";public int portions=1;public List<RecipeIngredient> ingredients=new List<RecipeIngredient>();}
public sealed class FoodLog {public string id=SportNutrition.Id(),date="",meal="",name="",sourceMenuId="";public decimal servings=1;public Nutrients total=new Nutrients();}
public sealed class MenuEntry {public string id=SportNutrition.Id(),date="",meal="",recipeId="";public decimal servings=1;}
internal sealed class GroceryLine {internal string Id,Name,Unit,Aisle,Period;internal decimal Quantity;internal string Signature{get{return Period+":"+Quantity.ToString(CultureInfo.InvariantCulture);}}}
internal static class SportNutrition {
 internal static string Id(){return Guid.NewGuid().ToString("N");}
 internal static T Copy<T>(T value){var j=new JavaScriptSerializer{MaxJsonLength=8*1024*1024};return j.Deserialize<T>(j.Serialize(value));}
 internal static void Change(Store store,Action<SportNutritionData> action){PersonalWorkflows.Change(store,delegate{action(store.Value.sportNutrition);});}
 internal static void Require(bool valid,string message){if(!valid)throw new InvalidDataException(message);}
 static bool Name(string s){return !String.IsNullOrWhiteSpace(s)&&s.Length<=160;}
 static bool IdOk(string s){return !String.IsNullOrEmpty(s)&&s.Length<=100;}
 static void Macros(Nutrients n){Require(n!=null&&n.calories>=0&&n.calories<=100000&&n.protein>=0&&n.protein<=10000&&n.carbs>=0&&n.carbs<=10000&&n.fat>=0&&n.fat<=10000,"Valeurs nutritionnelles invalides.");}
 static void Exercises(List<WorkoutExercise> list) {
  Require(list!=null&&list.Count<=40,"40 exercices maximum.");
  var ids=new HashSet<string>();
  foreach(var e in list){Require(e!=null&&IdOk(e.id)&&ids.Add(e.id)&&Name(e.name)&&(e.notes??"").Length<=2000&&e.restSeconds>=0&&e.restSeconds<=3600&&e.sets!=null&&e.sets.Count>=1&&e.sets.Count<=30,"Exercice invalide (1 à 30 séries, repos de 0 à 3600 s).");
   foreach(var s in e.sets)Require(s!=null&&s.reps>=1&&s.reps<=1000&&s.kg>=0&&s.kg<=2000,"Série invalide (1 à 1000 répétitions, 0 à 2000 kg).");
  }
 }
 internal static void Validate(Data data){
  if(data.sportNutrition==null)data.sportNutrition=new SportNutritionData();var d=data.sportNutrition;
  Require(d.goals!=null&&d.workouts!=null&&d.sessions!=null&&d.foods!=null&&d.recipes!=null&&d.diary!=null&&d.menu!=null&&d.checkedGroceries!=null,"Données Sport & Nutrition incomplètes.");
  var g=d.goals;Require(g.calories>=500&&g.calories<=10000&&g.protein>=0&&g.protein<=1000&&g.carbs>=0&&g.carbs<=2000&&g.fat>=0&&g.fat<=1000&&g.batchDays>=1&&g.batchDays<=14&&new[]{"Maintien","Prise de masse","Perte de poids"}.Contains(g.aim),"Objectifs invalides.");
  Require(d.workouts.Count<=100&&d.sessions.Count<=2000&&d.foods.Count<=2000&&d.recipes.Count<=500&&d.diary.Count<=10000&&d.menu.Count<=2000&&d.checkedGroceries.Count<=2000,"Limite de données Sport & Nutrition atteinte.");
  Require(d.restUntilUtcTicks>=0&&d.restUntilUtcTicks<=DateTime.MaxValue.Ticks,"Minuteur invalide.");
  var ids=new HashSet<string>();foreach(var w in d.workouts){Require(w!=null&&IdOk(w.id)&&ids.Add(w.id)&&Name(w.name),"Séance invalide.");Exercises(w.exercises);}
  ids.Clear();foreach(var s in d.sessions){Require(s!=null&&IdOk(s.id)&&ids.Add(s.id)&&Name(s.name)&&Finance.DateOk(s.date)&&s.exercises!=null&&(!s.finished||s.exercises.Any(e=>e!=null&&e.sets!=null&&e.sets.Any(t=>t!=null&&t.done))),"Historique de séance invalide.");Exercises(s.exercises);}
  Require(d.sessions.Count(s=>!s.finished)<=1,"Une seule séance peut être en cours.");
  ids.Clear();foreach(var f in d.foods){Require(f!=null&&IdOk(f.id)&&ids.Add(f.id)&&Name(f.name)&&new[]{"g","ml","pièce"}.Contains(f.unit)&&Name(f.aisle),"Aliment invalide.");Macros(f.per100);}
  Require(d.foods.Select(f=>f.name.Trim().ToUpperInvariant()+"|"+f.unit).Distinct().Count()==d.foods.Count,"Cet aliment existe déjà avec cette unité. Réutilisez sa fiche pour regrouper les courses.");
  ids.Clear();foreach(var r in d.recipes){Require(r!=null&&IdOk(r.id)&&ids.Add(r.id)&&Name(r.name)&&r.portions>=1&&r.portions<=100&&new[]{"Tous","Petit-déjeuner","Déjeuner","Dîner","Déjeuner & dîner","Collation"}.Contains(r.meal)&&(r.instructions??"").Length<=10000&&r.ingredients!=null&&r.ingredients.Count>=1&&r.ingredients.Count<=100,"Recette invalide.");
   foreach(var i in r.ingredients)Require(i!=null&&d.foods.Any(f=>f.id==i.foodId)&&i.quantity>0&&i.quantity<=100000,"Ingrédient invalide.");
  }
  ids.Clear();foreach(var m in d.menu)Require(m!=null&&IdOk(m.id)&&ids.Add(m.id)&&Finance.DateOk(m.date)&&Name(m.meal)&&d.recipes.Any(r=>r.id==m.recipeId)&&m.servings>0&&m.servings<=100,"Menu invalide.");
  ids.Clear();foreach(var l in d.diary){Require(l!=null&&IdOk(l.id)&&ids.Add(l.id)&&Finance.DateOk(l.date)&&Name(l.meal)&&Name(l.name)&&l.servings>0&&l.servings<=100000&&(l.sourceMenuId??"").Length<=100,"Journal alimentaire invalide.");Macros(l.total);}
  foreach(var c in d.checkedGroceries)Require(IdOk(c.Key)&&c.Value!=null&&c.Value.Length<=60,"Case de courses invalide.");
 }
 internal static Nutrients Scale(Nutrients n,decimal factor){return new Nutrients{calories=n.calories*factor,protein=n.protein*factor,carbs=n.carbs*factor,fat=n.fat*factor};}
 internal static Nutrients Proportion(Nutrients n,decimal numerator,decimal denominator){return new Nutrients{calories=n.calories*numerator/denominator,protein=n.protein*numerator/denominator,carbs=n.carbs*numerator/denominator,fat=n.fat*numerator/denominator};}
 internal static Nutrients Sum(IEnumerable<Nutrients> list){var n=new Nutrients();foreach(var x in list){n.calories+=x.calories;n.protein+=x.protein;n.carbs+=x.carbs;n.fat+=x.fat;}return n;}
 internal static Nutrients RecipeTotal(SportNutritionData d,NutritionRecipe r){return Sum(r.ingredients.Select(i=>Scale(d.foods.First(f=>f.id==i.foodId).per100,i.quantity/100m)));}
 internal static Nutrients RecipeServing(SportNutritionData d,NutritionRecipe r){return Scale(RecipeTotal(d,r),1m/r.portions);}
 internal static bool Allowed(SportNutritionData d,NutritionRecipe r){return !d.goals.lactoseFree||r.ingredients.All(i=>d.foods.First(f=>f.id==i.foodId).lactoseFree);}
 internal static WorkoutSession Start(SportNutritionData d,WorkoutPlan plan,DateTime day){
  Require(!d.sessions.Any(active=>!active.finished),"Terminez ou abandonnez la séance en cours.");Require(plan.exercises.Count>0,"Ajoutez au moins un exercice.");
  var s=new WorkoutSession{planId=plan.id,name=plan.name,date=day.ToString("yyyy-MM-dd"),exercises=Copy(plan.exercises)};
  foreach(var e in s.exercises)foreach(var set in e.sets)set.done=false;d.sessions.Add(s);return s;
 }
 internal static void Finish(WorkoutSession s){Require(s.exercises.Any(e=>e.sets.Any(t=>t.done)),"Validez au moins une série.");s.finished=true;}
 internal static decimal Volume(WorkoutSession s){return s.exercises.Sum(e=>e.sets.Where(t=>t.done).Sum(t=>t.kg*t.reps));}
 internal static List<GroceryLine> Groceries(SportNutritionData d,DateTime start,DateTime end){
  var amounts=new Dictionary<string,decimal>();
  foreach(var m in d.menu.Where(m=>Finance.Day(m.date)>=start.Date&&Finance.Day(m.date)<=end.Date)){
   var r=d.recipes.First(x=>x.id==m.recipeId);foreach(var i in r.ingredients){if(!amounts.ContainsKey(i.foodId))amounts[i.foodId]=0;amounts[i.foodId]+=i.quantity*m.servings/r.portions;}
  }
  return amounts.Select(a=>{var f=d.foods.First(x=>x.id==a.Key);return new GroceryLine{Id=f.id,Name=f.name,Unit=f.unit,Aisle=f.aisle,Period=start.ToString("yyyyMMdd")+"-"+end.ToString("yyyyMMdd"),Quantity=Math.Round(a.Value,2)};}).OrderBy(x=>x.Aisle).ThenBy(x=>x.Name).ToList();
 }
 // Produces a preview; caller explicitly adds it to the planner. Existing entries are preserved.

 static decimal[] Vector(Nutrients n){return new[]{n.calories,n.protein,n.carbs,n.fat};}
 static bool ForMeal(NutritionRecipe r,string meal){return r.meal=="Tous"||r.meal==meal||(r.meal=="Déjeuner & dîner"&&(meal=="Déjeuner"||meal=="Dîner"));}
 internal static List<MenuEntry> SuggestMenu(SportNutritionData d,DateTime start,int days){
  Require(days>=1&&days<=14,"Choisissez de 1 à 14 jours.");
  var available=d.recipes.Where(r=>Allowed(d,r)&&RecipeServing(d,r).calories>0).ToList();
  var meals=new[]{"Petit-déjeuner","Déjeuner","Dîner"};
  Require(meals.All(m=>available.Any(r=>ForMeal(r,m))),"Ajoutez des recettes compatibles pour le petit-déjeuner, le déjeuner et le dîner.");
  var target=new[]{d.goals.calories,d.goals.protein,d.goals.carbs,d.goals.fat};
  var weight=new[]{6m,2m,1m,1m};var result=new List<MenuEntry>();
  for(int block=0;block<days;block+=d.goals.batchDays){
   var chosen=meals.Select(m=>available.Where(r=>ForMeal(r,m)).Skip((block/d.goals.batchDays)%available.Count).FirstOrDefault()??available.First(r=>ForMeal(r,m))).ToArray();
   var quantities=new[]{1m,1m,1m};
   for(int pass=0;pass<6;pass++)for(int slot=0;slot<3;slot++){
    var other=Vector(Sum(Enumerable.Range(0,3).Where(i=>i!=slot).Select(i=>Scale(RecipeServing(d,chosen[i]),quantities[i]))));
    decimal best=Decimal.MaxValue,bestQ=1;NutritionRecipe bestRecipe=chosen[slot];
    foreach(var recipe in available.Where(r=>ForMeal(r,meals[slot]))){
     var v=Vector(RecipeServing(d,recipe));decimal numerator=0,denominator=0;
     for(int k=0;k<4;k++){decimal norm=Math.Max(1,target[k]),w=weight[k]/(norm*norm);numerator+=w*v[k]*(target[k]-other[k]);denominator+=w*v[k]*v[k];}
     decimal mealEnergy=d.goals.calories*(slot==0?.25m:.375m);decimal lower=Math.Max(.1m,Math.Min(10m,.75m*mealEnergy/v[0])),upper=Math.Max(lower,Math.Min(10m,1.25m*mealEnergy/v[0]));decimal q=Math.Round(Math.Max(lower,Math.Min(upper,numerator/Math.Max(.00000001m,denominator))),2),score=0;
     for(int k=0;k<4;k++){decimal diff=(other[k]+q*v[k]-target[k])/Math.Max(1,target[k]);score+=weight[k]*diff*diff;}
     // Prefer variety between batch blocks while retaining the nutrition score.
     score+=.025m*result.Count(m=>m.recipeId==recipe.id&&m.meal==meals[slot]);
     if(score<best){best=score;bestQ=q;bestRecipe=recipe;}
    }chosen[slot]=bestRecipe;quantities[slot]=bestQ;
   }
   for(int day=block;day<Math.Min(days,block+d.goals.batchDays);day++)for(int meal=0;meal<3;meal++)result.Add(new MenuEntry{date=start.AddDays(day).ToString("yyyy-MM-dd"),meal=meals[meal],recipeId=chosen[meal].id,servings=quantities[meal]});
  }return result;
 }
 internal static void LogMenu(SportNutritionData d,MenuEntry m){Require(!d.diary.Any(l=>l.sourceMenuId==m.id),"Ce repas est déjà dans le journal. Corrigez-le depuis Alimentation.");var r=d.recipes.First(x=>x.id==m.recipeId);d.diary.Add(new FoodLog{date=m.date,meal=m.meal,name=r.name,sourceMenuId=m.id,servings=m.servings,total=Scale(RecipeServing(d,r),m.servings)});}
 static NutritionFood Food(string id,string name,string aisle,decimal kcal,decimal p,decimal c,decimal f){return new NutritionFood{id=id,name=name,aisle=aisle,per100=new Nutrients{calories=kcal,protein=p,carbs=c,fat=f}};}
 internal static List<NutritionFood> DefaultFoods(){return new List<NutritionFood>{
  Food("chicken","Poulet cru","Viandes",110,23,0,2),Food("rice","Riz cru","Épicerie",356,7,78,1),
  Food("zucchini","Courgettes","Légumes",17,1.2m,2,0.3m),Food("coconut","Lait de coco","Épicerie",180,1.5m,3,18),
  Food("pasta","Pâtes crues","Épicerie",350,12,70,2),Food("beef","Bœuf haché 5 % cru","Viandes",130,21,0,5),
  Food("tomato","Tomates concassées","Épicerie",25,1.2m,4,0.2m),Food("beans","Haricots rouges égouttés","Épicerie",100,7,15,0.5m),
  Food("bread","Pain complet","Boulangerie",245,9,41,4),Food("eggs","Œufs sans coquille","Œufs",145,13,1,10),
  Food("oil","Huile d’olive","Épicerie",900,0,0,100)};}
 internal static List<NutritionRecipe> DefaultRecipes(){return new List<NutritionRecipe>{
  new NutritionRecipe{id="breakfast",meal="Petit-déjeuner",name="Œufs & pain complet",portions=1,ingredients=new List<RecipeIngredient>{I("eggs",150),I("bread",100)},instructions="Cuire les œufs et servir avec le pain. Quantités d’œufs pesées sans coquille."},
  new NutritionRecipe{id="curry",meal="Déjeuner & dîner",name="Poulet coco, riz & courgettes",portions=4,ingredients=new List<RecipeIngredient>{I("chicken",600),I("rice",320),I("zucchini",800),I("coconut",200)},instructions="Cuire le poulet à cœur avec les courgettes, le lait de coco et les épices de votre choix. Cuire le riz selon le paquet, puis répartir en portions."},
  new NutritionRecipe{id="chili",meal="Déjeuner & dîner",name="Chili & riz",portions=4,ingredients=new List<RecipeIngredient>{I("beef",600),I("rice",320),I("beans",400),I("tomato",600),I("oil",20)},instructions="Faire revenir la viande, ajouter tomates et haricots égouttés. Mijoter avec les épices de votre choix. Servir avec le riz cuit séparément."}
 };}
 static RecipeIngredient I(string food,decimal quantity){return new RecipeIngredient{foodId=food,quantity=quantity};}
}
