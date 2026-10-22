using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
internal static class FinanceAcceptance {
 static void OverdraftTest(){var b=new BudgetData();Check(!Finance.ExceedsOverdraft(b,-1),"Limite non renseignée");b.overdraftKnown=true;b.overdraftCents=50000;Check(!Finance.ExceedsOverdraft(b,10000)&&!Finance.ExceedsOverdraft(b,-50000)&&Finance.ExceedsOverdraft(b,-50001),"Seuil exact du découvert");b.overdraftCents=0;Check(Finance.ExceedsOverdraft(b,-1)&&!Finance.ExceedsOverdraft(b,0),"Aucun découvert autorisé");var d=new Data();d.budget.overdraftCents=-1;bool rejected=false;try{Store.Validate(d);}catch(InvalidDataException){rejected=true;}Check(rejected,"Limite négative acceptée");}
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static BudgetEntry Entry(string id,string date,long cents,string kind="expense"){return new BudgetEntry{id=id,title=id,date=date,cents=cents,kind=kind,category="Courses"};}
 internal static void Test(string folder){
  BudgetExtrasAcceptance.Test(folder);PlannedAcceptance.Test(folder);OverdraftTest();long amount;Check(Finance.ParseMoney("1 234,56",false,out amount)&&amount==123456,"Montant français");Check(Finance.ParseMoney("-22.50",true,out amount)&&amount== -2250,"Solde négatif");Check(!Finance.ParseMoney("1,234",false,out amount)&&!Finance.ParseMoney("-10",false,out amount)&&!Finance.ParseMoney("1e6",false,out amount)&&!Finance.ParseMoney("0",false,out amount),"Montants invalides");
  var store=new Store(Path.Combine(folder,"budget-core"));var today=new DateTime(2028,1,31);
  store.Value.budget.entries.Add(Entry("before","2028-01-31",2000));Finance.SetBalance(store.Value.budget,100000,today);
  Check(Finance.Balance(store.Value.budget,today)==100000,"Solde compte une dépense deux fois");
  Finance.AddEntry(store,Entry("today","2028-01-31",1234),today);Check(Finance.Balance(store.Value.budget,today)==98766,"Dépense le jour du solde");
  Finance.AddEntry(store,Entry("past","2028-01-30",3000),today);Check(Finance.Balance(store.Value.budget,today)==98766,"Ajout antidaté modifie le solde");
  Finance.AddEntry(store,Entry("income","2028-01-31",10000,"income"),today);Check(Finance.Balance(store.Value.budget,today)==108766,"Revenu");
  Finance.AddEntry(store,Entry("today","2028-01-31",1500),today);Check(Finance.Balance(store.Value.budget,today)==108500,"Modification dépense");
  Check(Finance.Spent(store.Value.budget,today,"courses")==6500,"Dépenses et catégories");
  bool rejected=false;try{Finance.AddEntry(store,Entry("future","2028-02-01",1000),today);}catch(InvalidDataException){rejected=true;}Check(rejected,"Opération future acceptée");
  var rule=new BudgetRule{id="monthly",title="Abonnement",start="2028-01-31",repeat="monthly",cents=5000};
  Check(Finance.OccurrenceDate(rule,1)==new DateTime(2028,2,29)&&Finance.OccurrenceDate(rule,2)==new DateTime(2028,3,31),"Ancre 31 et février bissextile");
  rule.repeat="yearly";rule.start="2028-02-29";Check(Finance.OccurrenceDate(rule,1)==new DateTime(2029,2,28)&&Finance.OccurrenceDate(rule,4)==new DateTime(2032,2,29),"Annuel bissextile");rule.repeat="weekly";Check(Finance.OccurrenceDate(rule,1)==new DateTime(2028,3,7),"Hebdomadaire");
  rule.repeat="monthly";rule.start="2028-01-31";store.Value.budget.rules.Add(rule);store.Save();
  var projection=Finance.Forecast(store.Value.budget,today,new DateTime(2028,3,31));Check(projection.Pending.Count==3&&projection.End==93500&&projection.Minimum==93500,"Prévision récurrences");
  Finance.Confirm(store,projection.Pending[0],today);Finance.Confirm(store,projection.Pending[0],today);
  Check(store.Value.budget.entries.Count(e=>e.occurrence==projection.Pending[0].Key)==1,"Confirmation dupliquée");Check(Finance.Forecast(store.Value.budget,today,new DateTime(2028,3,31)).End==93500,"Confirmation déduite deux fois");
  var recurring=store.Value.budget.rules[0];var feb=new BudgetOccurrence{Rule=recurring,Date=new DateTime(2028,2,29)};var later=new DateTime(2028,3,1);
  Finance.AddEntry(store,Entry("manual","2028-02-29",4500),later);int count=store.Value.budget.entries.Count;Finance.Reconcile(store,feb,later,later,5000,"manual");Check(store.Value.budget.entries.Count==count&&Finance.Pending(store.Value.budget,today,later).Count==0,"Association opération existante");
  Finance.Change(store,b=>{b.caps.Add(new BudgetCap{category="Courses",cents=20000});Finance.SetBalance(b,120000,later);});
  Check(Finance.Balance(store.Value.budget,later)==120000,"Recalage du solde");
  var march=new BudgetOccurrence{Rule=store.Value.budget.rules[0],Date=new DateTime(2028,3,31)};Finance.Reconcile(store,march,new DateTime(2028,3,31),new DateTime(2028,3,31),5500,null);Check(Finance.Balance(store.Value.budget,new DateTime(2028,3,31))==114500,"Montant réel différent");
  var negative=new BudgetData();Finance.SetBalance(negative,10000,today);negative.rules.Add(new BudgetRule{id="rent",title="Loyer",start="2028-02-01",end="2028-02-01",cents=20000});negative.rules.Add(new BudgetRule{id="salary",title="Salaire",kind="income",start="2028-02-15",end="2028-02-15",cents=50000});var forecast=Finance.Forecast(negative,today,new DateTime(2028,2,29));Check(forecast.Minimum== -10000&&forecast.MinimumDate==new DateTime(2028,2,1)&&forecast.End==40000,"Découvert intermédiaire");
  Check(Finance.Forecast(negative,new DateTime(2028,2,2),new DateTime(2028,2,29)).Pending.Count==2,"Échéance en retard perdue");
  Finance.Change(store,b=>{b.overdraftKnown=true;b.overdraftCents=50000;});string exported=Path.Combine(folder,"budget-export.json");store.Export(exported);store.Value.budget=new BudgetData();store.Import(exported);Check(store.Value.budget.caps.Count==1&&store.Value.budget.hasBalance&&store.Value.budget.overdraftKnown&&store.Value.budget.overdraftCents==50000,"Import budget");
  string archive=Path.Combine(folder,"budget.mespace");FullBackup.Export(store,archive);var target=new Store(Path.Combine(folder,"budget-restored"));using(var plan=FullBackup.Prepare(target,archive))FullBackup.Commit(target,plan);Check(new Store(target.Root).Value.budget.entries.Count==store.Value.budget.entries.Count&&new Store(target.Root).Value.budget.overdraftCents==50000,"Sauvegarde complète budget");
  var before=Finance.Copy(store.Value.budget);string path=Path.Combine(store.Root,"state.json");File.Delete(path);Directory.CreateDirectory(path);rejected=false;try{Finance.Change(store,b=>Finance.SetBalance(b,999999,today));}catch(IOException){rejected=true;}catch(UnauthorizedAccessException){rejected=true;}Directory.Delete(path);Check(rejected&&store.Value.budget.balanceCents==before.balanceCents&&store.Value.budget.balanceDate==before.balanceDate,"Retour après échec sauvegarde");store.Save();
  var legacy=new Data();legacy.budget=null;Store.Validate(legacy);Check(legacy.budget!=null&&!legacy.budget.hasBalance,"Migration ancien format");
  var invalid=new Data();invalid.budget.entries.Add(Entry("bad","2028-01-31",-1));rejected=false;try{Store.Validate(invalid);}catch(InvalidDataException){rejected=true;}Check(rejected,"Dépense négative importée");
 }
}
internal sealed partial class ModernWorkspace {
 internal void VerifyFinanceUi(string folder){
  int original=store.Value.budget.entries.Count;dialogProbe=delegate(Window w){ClickWizard(w,"Enregistrer l'opération");if(store.Value.budget.entries.Count!=original)throw new Exception("Opération vide acceptée");var fields=LogicalControls<TextBox>(w).ToList();fields[0].Text="Courses de test";fields[1].Text="12,34";fields[2].Text=DateTime.Today.ToString("dd/MM/yyyy");fields[3].Text="Ticket conservé";ClickWizard(w,"Enregistrer l'opération");ClickWizard(w,"Enregistrer l'opération");return true;};try{EditBudgetEntry(null,"expense");}finally{dialogProbe=null;}
  if(new Store(store.Root).Value.budget.entries.Count!=1||store.Value.budget.entries[0].cents!=1234||store.Value.budget.entries[0].notes!="Ticket conservé")throw new Exception("Saisie de dépense");
  dialogProbe=delegate(Window w){LogicalControls<TextBox>(w).First().Text="1000,00";ClickWizard(w,"Enregistrer le solde");return true;};try{EditBudgetBalance();}finally{dialogProbe=null;}if(Finance.Balance(store.Value.budget,DateTime.Today)!=100000)throw new Exception("Saisie solde");
  dialogProbe=delegate(Window w){var fields=LogicalControls<TextBox>(w).ToList();fields[0].Text="Courses";fields[1].Text="200";ClickWizard(w,"Enregistrer le budget");return true;};try{EditBudgetCap(null);}finally{dialogProbe=null;}
  dialogProbe=delegate(Window w){var fields=LogicalControls<TextBox>(w).ToList();fields[0].Text="Abonnement de test";fields[1].Text="10";ClickWizard(w,"Enregistrer la récurrence");return true;};try{EditBudgetRule(null);}finally{dialogProbe=null;}
  dialogProbe=delegate(Window w){LogicalControls<TextBox>(w).First().Text="500";ClickWizard(w,"Enregistrer la limite");return true;};try{EditBudgetOverdraft();}finally{dialogProbe=null;}if(!store.Value.budget.overdraftKnown||store.Value.budget.overdraftCents!=50000||Finance.Balance(store.Value.budget,DateTime.Today)!=100000)throw new Exception("Limite de découvert ou solde modifié");
  var occurrence=Finance.Pending(store.Value.budget,DateTime.Today,DateTime.Today).First();
  dialogProbe=delegate(Window w){LogicalControls<ComboBox>(w).First().SelectedIndex=1;ClickWizard(w,"Confirmer le passage");return true;};try{ConfirmBudgetOccurrence(occurrence);}finally{dialogProbe=null;}
  if(store.Value.budget.entries.Count!=1||String.IsNullOrEmpty(store.Value.budget.entries[0].occurrence))throw new Exception("Association dans la fenêtre de confirmation");
  var json=new System.Web.Script.Serialization.JavaScriptSerializer();string before=json.Serialize(store.Value);
  dialogProbe=delegate{return false;};try{EditBudgetEntry(null,"income");EditBudgetOverdraft();EditBudgetBalance();EditBudgetRule(null);EditBudgetCap(null);}finally{dialogProbe=null;}if(before!=json.Serialize(store.Value))throw new Exception("Annulation budget");
  if(!store.Value.settings.HomeBlocks.Any(b=>b.id=="finance"&&b.visible))throw new Exception("Bloc budget accueil");
  VerifyPlannedUi(folder);Go("Mon budget");Preview(Path.Combine(folder,"Budget.png"));
  previewPrefix=Path.Combine(folder,"Budget");try{EditBudgetEntry(null,"expense");EditBudgetBalance();EditBudgetRule(null);EditBudgetCap(null);}finally{previewPrefix=null;}
  Go("Aujourd’hui");Preview(Path.Combine(folder,"Accueil-budget.png"));VerifyBudgetExtrasUi(folder);
 }
}
internal sealed partial class MainForm {
 internal void VerifyFinanceUi(string folder){rendering=true;modern.VerifyFinanceUi(folder);if(uiTicker!=null)uiTicker.Dispose();modern.CancelMusic();Stop();if(speech!=null)speech.Dispose();player.Close();}
}
internal static partial class Program {
 internal static int FinanceTest(string folder){Root=Path.GetFullPath(folder);Directory.CreateDirectory(Root);System.Windows.Forms.Application.EnableVisualStyles();try{FinanceAcceptance.Test(Root);using(var form=new MainForm(false))form.VerifyFinanceUi(Root);File.WriteAllText(Path.Combine(Root,"FINANCE_PASS.txt"),"PASS: integer-cent money parsing, current balance checkpoint, same-day entries, historical entries, income, editing, monthly categories, reject future entries, monthly 31st/leap/yearly/weekly recurrence, forecast and intermediate overdraft, overdue occurrences, actual confirmation/deduplication, matching existing transactions, different actual amounts, balance recalibration, JSON export/import, complete backup restore, save-failure rollback, migration and invalid import; WPF manual input/validation/double-submit/cancel, balance/cap/rule/confirmation dialogs, home widget and rendered previews. No bank connection.");return 0;}catch(Exception ex){File.WriteAllText(Path.Combine(Root,"FINANCE_FAIL.txt"),ex.ToString());return 1;}}
}
