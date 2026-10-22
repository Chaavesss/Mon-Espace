using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
internal static class PlannedAcceptance {
 static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
 internal static void Test(string folder){
  var today=new DateTime(2028,1,20);var store=new Store(Path.Combine(folder,"planned-core"));Finance.Change(store,b=>Finance.SetBalance(b,100000,today));
  var plan=new BudgetRule{id="planned",title="Réparation",cents=25000,start="2028-01-25",repeat="once",category="Carburant",notes="Devis"};
  PlannedExpenses.Save(store,plan);
  Check(Finance.Balance(store.Value.budget,today)==100000&&Finance.Spent(store.Value.budget,today,null)==0,"Prévision modifie le réel");
  Check(Finance.Forecast(store.Value.budget,today,new DateTime(2028,1,24)).End==100000&&Finance.Forecast(store.Value.budget,today,new DateTime(2028,1,25)).End==75000,"Date de la prévision");
  Check(Finance.Pending(store.Value.budget,today,today.AddYears(1)).Count==1,"Prévision répétée");
  plan.cents=10000;PlannedExpenses.Save(store,plan);Check(store.Value.budget.rules.Count==1&&Finance.Forecast(store.Value.budget,today,today.AddMonths(1)).End==90000,"Modification prévision");
  var occurrence=Finance.Pending(store.Value.budget,today,today.AddMonths(1)).Single();
  Finance.Reconcile(store,occurrence,today,today,12000,null);Finance.Reconcile(store,occurrence,today,today,12000,null);
  Check(store.Value.budget.entries.Count==1&&store.Value.budget.entries[0].notes=="Devis"&&Finance.Balance(store.Value.budget,today)==88000&&Finance.Forecast(store.Value.budget,today,today.AddMonths(1)).End==88000,"Paiement anticipé, réel ou doublon");
  bool blocked=false;try{PlannedExpenses.Save(store,plan);}catch(InvalidDataException){blocked=true;}Check(blocked,"Modification prévision payée");blocked=false;try{PlannedExpenses.Cancel(store,plan.id);}catch(InvalidDataException){blocked=true;}Check(blocked,"Annulation prévision payée");
  var overdue=new BudgetRule{id="late",title="Facture en attente",cents=5000,start="2028-01-19",repeat="once"};PlannedExpenses.Save(store,overdue);Finance.Change(store,b=>Finance.SetBalance(b,50000,today));Check(Finance.Forecast(store.Value.budget,today,today.AddMonths(1)).End==45000,"Prévision non payée perdue après recalage du solde");
  Finance.AddEntry(store,new BudgetEntry{id="manual-plan",title="Facture payée",date="2028-01-20",cents=5000},today);var late=Finance.Pending(store.Value.budget,today,today.AddMonths(1)).Single();Finance.Reconcile(store,late,today,today,5000,"manual-plan");Check(store.Value.budget.entries.Count==2&&Finance.Forecast(store.Value.budget,today,today.AddMonths(1)).End==45000,"Association prévision à écriture existante");
  var cancel=new BudgetRule{id="cancel",title="Achat annulé",cents=9000,start="2028-02-01",repeat="once"};PlannedExpenses.Save(store,cancel);PlannedExpenses.Cancel(store,cancel.id);Check(Finance.Forecast(store.Value.budget,today,today.AddMonths(1)).End==45000&&store.Value.budget.entries.Count==2,"Annulation prévision");
  var expected=new BudgetRule{id="income-plan",title="Remboursement attendu",cents=2000,kind="income",start="2028-01-30",repeat="once"};PlannedExpenses.Save(store,expected);Check(Finance.Forecast(store.Value.budget,today,today.AddMonths(1)).End==47000,"Revenu ponctuel");
  var export=Path.Combine(folder,"planned-export.json");store.Export(export);store.Value.budget=new BudgetData();store.Import(export);Check(store.Value.budget.rules.Count==3&&Finance.Forecast(store.Value.budget,today,today.AddMonths(1)).End==47000,"Import prévisions");
  var archive=Path.Combine(folder,"planned.mespace");FullBackup.Export(store,archive);var target=new Store(Path.Combine(folder,"planned-restored"));using(var restore=FullBackup.Prepare(target,archive))FullBackup.Commit(target,restore);Check(new Store(target.Root).Value.budget.rules.Count==3,"Restauration prévisions");
  string state=Path.Combine(store.Root,"state.json");File.Delete(state);Directory.CreateDirectory(state);blocked=false;try{PlannedExpenses.Save(store,new BudgetRule{id="failed",title="Échec",cents=100,start="2028-02-01",repeat="once"});}catch(IOException){blocked=true;}catch(UnauthorizedAccessException){blocked=true;}Directory.Delete(state);Check(blocked&&!store.Value.budget.rules.Any(r=>r.id=="failed"),"Retour après échec prévision");store.Save();
 }
}
internal sealed partial class ModernWorkspace {
 internal void VerifyPlannedUi(string folder){
  int count=store.Value.budget.rules.Count;
  dialogProbe=delegate(Window w){ClickWizard(w,"Enregistrer la prévision");if(store.Value.budget.rules.Count!=count)throw new Exception("Prévision vide acceptée");var fields=LogicalControls<TextBox>(w).ToList();fields[0].Text="Achat prévu de test";fields[1].Text="50,25";fields[2].Text=DateTime.Today.AddDays(1).ToString("dd/MM/yyyy");fields[3].Text="Détails de l'achat";ClickWizard(w,"Enregistrer la prévision");ClickWizard(w,"Enregistrer la prévision");return true;};
  try{EditPlannedExpense(null);}finally{dialogProbe=null;}
  var planned=store.Value.budget.rules.Single(r=>r.repeat=="once");if(planned.cents!=5025||planned.notes!="Détails de l'achat"||store.Value.budget.rules.Count!=count+1)throw new Exception("Saisie prévision ponctuelle");
  var data=Finance.Copy(store.Value.budget);dialogProbe=delegate{return false;};try{EditPlannedExpense(planned);}finally{dialogProbe=null;}if(store.Value.budget.rules.Count!=data.rules.Count||store.Value.budget.rules.Last().cents!=data.rules.Last().cents)throw new Exception("Abandon modification prévision");
  int entries=store.Value.budget.entries.Count;dialogProbe=delegate(Window w){LogicalControls<TextBox>(w).First().Text="49,90";ClickWizard(w,"Confirmer le passage");return true;};
  try{ConfirmBudgetOccurrence(new BudgetOccurrence{Rule=planned,Date=Finance.Day(planned.start)});}finally{dialogProbe=null;}
  if(store.Value.budget.entries.Count!=entries+1||store.Value.budget.entries.Last().cents!=4990||!PlannedExpenses.IsPaid(store.Value.budget,planned))throw new Exception("Confirmation anticipée UI");
  PlannedExpenses.Save(store,new BudgetRule{id=Guid.NewGuid().ToString(),title="Réparation prévue (exemple de test)",cents=18000,start=DateTime.Today.AddDays(2).ToString("yyyy-MM-dd"),repeat="once",category="Carburant"});
  previewPrefix=Path.Combine(folder,"Budget");try{EditPlannedExpense(null);}finally{previewPrefix=null;}
 }
}
