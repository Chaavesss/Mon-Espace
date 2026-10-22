using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
internal static class BudgetExtrasAcceptance {
 static void Check(bool value,string label){if(!value)throw new Exception(label);}
 internal static void Test(string folder){
  var store=new Store(Path.Combine(folder,"budget-extras"));var today=new DateTime(2028,1,22);
  Finance.Change(store,b=>Finance.SetBalance(b,50000,today));
  var goal=new BudgetGoal{id="goal",name="Vacances",targetCents=50000,initialCents=10000,deadline="2028-07-01"};
  BudgetExtras.SaveGoal(store,goal);BudgetExtras.MoveSavings(store,goal.id,2500,"Ajout",today,today);BudgetExtras.MoveSavings(store,goal.id,-1000,"Retrait",today,today);
  Check(BudgetExtras.Saved(store.Value.budget.goals.Single())==11500&&Finance.Balance(store.Value.budget,today)==50000&&store.Value.budget.entries.Count==0,"Épargne et solde courant séparés");
  BudgetExtras.SaveGoal(store,new BudgetGoal{id=goal.id,name="Voyage",targetCents=60000,initialCents=999999});Check(BudgetExtras.Saved(store.Value.budget.goals.Single())==11500&&store.Value.budget.goals.Single().moves.Count==2,"Modification objectif préserve le suivi");
  bool bad=false;try{BudgetExtras.MoveSavings(store,goal.id,-20000,"Trop",today,today);}catch(InvalidDataException){bad=true;}Check(bad&&BudgetExtras.Saved(store.Value.budget.goals.Single())==11500,"Retrait excessif");
  bad=false;try{BudgetExtras.MoveSavings(store,goal.id,100,"Futur",today.AddDays(1),today);}catch(InvalidDataException){bad=true;}Check(bad,"Épargne future");
  Finance.AddEntry(store,new BudgetEntry{id="income",title="Salaire",date="2028-01-22",kind="income",cents=200000,category="Salaire"},today);
  Finance.AddEntry(store,new BudgetEntry{id="expense",title="Courses",date="2028-01-22",cents=5000,category="Courses"},today);Finance.AddEntry(store,new BudgetEntry{id="expense2",title="Complément",date="2028-01-22",cents=1000,category="courses"},today);
  PlannedExpenses.Save(store,new BudgetRule{id="forecast",title="Prévision exclue du bilan",start="2028-01-31",repeat="once",cents=99000});
  var report=BudgetExtras.Month(store.Value.budget,today);Check(report.Income==200000&&report.Expenses==6000&&report.Net==194000&&report.Categories.Count==1&&report.Categories[0].Cents==6000,"Bilan réel et catégories");Check(BudgetExtras.Month(store.Value.budget,today.AddMonths(-1)).Expenses==0,"Mois vide");
  Finance.Change(store,b=>{b.rules.Clear();b.rules.Add(new BudgetRule{id="rent",title="Loyer",start="2028-01-25",cents=90000});b.rules.Add(new BudgetRule{id="salary-rule",title="Salaire",start="2028-01-25",cents=200000,kind="income"});b.reminders=new BudgetReminderOptions{daysBefore=3,time="09:00"};});
  Check(BudgetExtras.Due(store.Value.budget,today.AddHours(8).AddMinutes(59)).Count==0&&BudgetExtras.Due(store.Value.budget,today.AddHours(9)).Count==1,"Heure et anticipation rappel");
  Check(BudgetExtras.Due(store.Value.budget,today.AddDays(2).AddHours(16)).Count==1,"Rattrapage rappel");
  Finance.Change(store,b=>b.reminders.includeIncome=true);Check(BudgetExtras.Due(store.Value.budget,today.AddHours(9)).Count==2,"Rappel revenus optionnel");
  Finance.Change(store,b=>b.reminders.minimumCents=100000);Check(BudgetExtras.Due(store.Value.budget,today.AddHours(9)).Count==1,"Seuil rappel");
  Finance.Change(store,b=>{b.reminders.minimumCents=0;b.reminders.includeIncome=false;b.rules[0].remind=false;});Check(BudgetExtras.Due(store.Value.budget,today.AddHours(9)).Count==0,"Rappel désactivé pour une échéance");
  Finance.Change(store,b=>b.rules[0].remind=true);var notices=BudgetExtras.Due(store.Value.budget,today.AddHours(9));BudgetExtras.Mark(store.Value.budget,notices);store.Save();Check(BudgetExtras.Due(new Store(store.Root).Value.budget,today.AddHours(9)).Count==0,"Rappel dédupliqué après rechargement");
  Finance.Change(store,b=>b.remindersSent.Clear());var occurrence=Finance.Pending(store.Value.budget,today,today.AddDays(3)).First(o=>o.Rule.id=="rent");Finance.Reconcile(store,occurrence,new DateTime(2028,1,25),new DateTime(2028,1,25),90000,null);Check(BudgetExtras.Due(store.Value.budget,new DateTime(2028,1,25,9,0,0)).Count==0,"Échéance confirmée encore rappelée");
  Finance.Change(store,b=>b.reminders.enabled=false);Check(BudgetExtras.Due(store.Value.budget,today.AddHours(9)).Count==0,"Rappels désactivés");
  var path=Path.Combine(folder,"extras-export.json");store.Export(path);store.Value.budget=new BudgetData();store.Import(path);Check(store.Value.budget.goals.Single().name=="Voyage"&&!store.Value.budget.reminders.enabled,"Import épargne et rappels");
  var archive=Path.Combine(folder,"extras.mespace");FullBackup.Export(store,archive);var restored=new Store(Path.Combine(folder,"extras-restored"));using(var plan=FullBackup.Prepare(restored,archive))FullBackup.Commit(restored,plan);Check(BudgetExtras.Saved(new Store(restored.Root).Value.budget.goals.Single())==11500,"Sauvegarde complète objectifs");
  string state=Path.Combine(store.Root,"state.json");File.Delete(state);Directory.CreateDirectory(state);bad=false;try{BudgetExtras.MoveSavings(store,goal.id,100,"Échec",today,today);}catch(IOException){bad=true;}catch(UnauthorizedAccessException){bad=true;}Directory.Delete(state);Check(bad&&BudgetExtras.Saved(store.Value.budget.goals.Single())==11500,"Retour après échec épargne");store.Save();
  var legacy=new Data();legacy.budget.goals=null;legacy.budget.reminders=null;legacy.budget.remindersSent=null;Store.Validate(legacy);Check(legacy.budget.goals.Count==0&&legacy.budget.reminders.daysBefore==3,"Migration épargne/rappels");
  var zeroLead=new BudgetData();zeroLead.rules.Add(new BudgetRule{id="today",title="Aujourd'hui",start="2028-01-22",repeat="once",cents=1000});zeroLead.reminders.daysBefore=0;Check(BudgetExtras.Due(zeroLead,today.AddHours(8)).Count==0&&BudgetExtras.Due(zeroLead,today.AddHours(9)).Count==1,"Rappel le jour même");
  var invalid=new Data();invalid.budget.reminders.daysBefore=99;bad=false;try{Store.Validate(invalid);}catch(InvalidDataException){bad=true;}Check(bad,"Paramètres rappel invalides");
 }
}
internal sealed partial class ModernWorkspace {
 internal void VerifyBudgetExtrasUi(string folder){
  long beforeBalance=Finance.Balance(store.Value.budget,DateTime.Today);int entries=store.Value.budget.entries.Count;
  dialogProbe=delegate(Window w){ClickWizard(w,"Enregistrer l'objectif");if(store.Value.budget.goals.Count!=0)throw new Exception("Objectif vide accepté");var fields=LogicalControls<TextBox>(w).ToList();fields[0].Text="Vacances (test)";fields[1].Text="1000";fields[2].Text="100";fields[3].Text="";fields[4].Text="Objectif de test";ClickWizard(w,"Enregistrer l'objectif");ClickWizard(w,"Enregistrer l'objectif");return true;};try{EditSavingsGoal(null);}finally{dialogProbe=null;}
  var goal=store.Value.budget.goals.Single();dialogProbe=delegate(Window w){var fields=LogicalControls<TextBox>(w).ToList();fields[0].Text="25";fields[1].Text=DateTime.Today.ToString("dd/MM/yyyy");fields[2].Text="Mise de côté test";ClickWizard(w,"Enregistrer le mouvement");ClickWizard(w,"Enregistrer le mouvement");return true;};try{EditSavingsMove(goal);}finally{dialogProbe=null;}
  if(BudgetExtras.Saved(store.Value.budget.goals.Single())!=12500||Finance.Balance(store.Value.budget,DateTime.Today)!=beforeBalance||store.Value.budget.entries.Count!=entries)throw new Exception("Épargne UI ou double soumission");
  string before=new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(store.Value.budget);dialogProbe=delegate{return false;};try{EditSavingsGoal(goal);EditSavingsMove(goal);}finally{dialogProbe=null;}if(before!=new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(store.Value.budget))throw new Exception("Annulation épargne");
  Go("Mon épargne");Preview(Path.Combine(folder,"Epargne.png"));
  Go("Rappels budget");var root=(DependencyObject)((ScrollViewer)content.Content).Content;var fields2=LogicalControls<TextBox>(root).ToList();fields2[0].Text="10:30";fields2[1].Text="100";
  var days=LogicalControls<ComboBox>(root).First();days.SelectedIndex=1;LogicalControls<Button>(root).First(b=>Convert.ToString(b.Content)=="Enregistrer les rappels").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
  if(store.Value.budget.reminders.time!="10:30"||store.Value.budget.reminders.daysBefore!=1||store.Value.budget.reminders.minimumCents!=10000)throw new Exception("Paramètres rappels UI");Preview(Path.Combine(folder,"Rappels-budget.png"));
  Finance.Change(store,b=>{for(int i=0;i<6;i++){var date=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1).AddMonths(-i);b.entries.Add(new BudgetEntry{id=Guid.NewGuid().ToString(),title="Revenu exemple",date=date.ToString("yyyy-MM-dd"),kind="income",cents=200000+i*10000,category="Salaire"});b.entries.Add(new BudgetEntry{id=Guid.NewGuid().ToString(),title="Dépense exemple",date=date.ToString("yyyy-MM-dd"),cents=80000+i*5000,category="Logement"});}});
  Go("Bilan budget");Preview(Path.Combine(folder,"Bilan-mensuel.png"));
 }
}
