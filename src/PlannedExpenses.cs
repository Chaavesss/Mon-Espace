using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
internal static class PlannedExpenses {
 internal static bool IsPaid(BudgetData data,BudgetRule rule){return data.entries.Any(e=>!String.IsNullOrEmpty(e.occurrence)&&e.occurrence.StartsWith(rule.id+"/",StringComparison.Ordinal));}
 internal static void Save(Store store,BudgetRule planned){
  if(planned.repeat!="once")throw new InvalidDataException("L'opération prévue doit être ponctuelle.");
  if(IsPaid(store.Value.budget,planned))throw new InvalidDataException("Cette opération est déjà passée. Modifiez l'écriture dans votre historique.");
  Finance.Change(store,b=>{b.rules.RemoveAll(r=>r.id==planned.id);b.rules.Add(planned);});
 }
 internal static void Cancel(Store store,string id){
  var item=store.Value.budget.rules.FirstOrDefault(r=>r.id==id&&r.repeat=="once");
  if(item==null)return;if(IsPaid(store.Value.budget,item))throw new InvalidDataException("Cette opération est déjà confirmée.");
  Finance.Change(store,b=>b.rules.RemoveAll(r=>r.id==id));
 }
}
internal sealed partial class ModernWorkspace {
 bool showPaidPlans;
 void PlannedBudgetSection(StackPanel root){
  var body=new StackPanel();body.Children.Add(T("Achats, factures ou rentrées d'argent ponctuels. Ils restent prévus jusqu'à confirmation de leur passage.",12,Muted,new Thickness(0,0,0,12)));
  var tools=new WrapPanel();tools.Children.Add(Btn("+ Prévoir une opération",delegate{EditPlannedExpense(null);},true));tools.Children.Add(Btn(showPaidPlans?"Masquer les opérations passées":"Afficher les opérations passées",delegate{showPaidPlans=!showPaidPlans;Refresh();},false,true));body.Children.Add(tools);
  var plans=store.Value.budget.rules.Where(r=>r.repeat=="once"&&(showPaidPlans||!PlannedExpenses.IsPaid(store.Value.budget,r))).OrderBy(r=>r.start).ToList();
  if(plans.Count==0)body.Children.Add(T("Aucune opération ponctuelle en attente.",12,Muted,new Thickness(0,14,0,0)));
  foreach(var planned in plans){
   var item=planned;bool paid=PlannedExpenses.IsPaid(store.Value.budget,item);var row=new DockPanel{Margin=new Thickness(0,14,0,6)};var actions=new WrapPanel();
   if(paid)actions.Children.Add(Btn("Voir l'opération",delegate{var entry=store.Value.budget.entries.FirstOrDefault(e=>!String.IsNullOrEmpty(e.occurrence)&&e.occurrence.StartsWith(item.id+"/",StringComparison.Ordinal));if(entry!=null)EditBudgetEntry(entry,entry.kind);},false,true));
   else{actions.Children.Add(Btn("Confirmer passée",delegate{ConfirmBudgetOccurrence(new BudgetOccurrence{Rule=item,Date=Finance.Day(item.start)});},false,true));actions.Children.Add(Btn("Modifier",delegate{EditPlannedExpense(item);},false,true));}
   DockPanel.SetDock(actions,Dock.Right);row.Children.Add(actions);var info=new StackPanel();info.Children.Add(T(item.title+" · "+(item.kind=="income"?"+":"−")+Finance.Money(item.cents),13,paid?Muted:Ink));info.Children.Add(T(Finance.Day(item.start).ToString("dd/MM/yyyy")+" · "+item.category+(paid?" · Confirmée":Finance.Day(item.start)<DateTime.Today?" · En attente de confirmation":" · Prévue"),11,Muted,new Thickness(0,4,0,0)));row.Children.Add(info);body.Children.Add(row);
  }
  var card=Card("Mes opérations ponctuelles à venir",body);card.Margin=new Thickness(0,16,0,0);root.Children.Add(card);
 }
 void EditPlannedExpense(BudgetRule existing){
  StackPanel body,actions;var w=Dialog(existing==null?"Prévoir une opération ponctuelle":"Modifier l'opération prévue",out body,out actions,740);
  var title=new TextBox{Text=existing==null?"":existing.title,MaxLength=200};Field(body,"Libellé : réparation, achat, facture…",title);
  var amount=new TextBox{Text=existing==null?"":(existing.cents/100m).ToString("0.00",System.Globalization.CultureInfo.GetCultureInfo("fr-FR")),MaxLength=24};Field(body,"Montant prévu (€), toujours positif",amount);
  var date=new TextBox{Text=existing==null?DateTime.Today.AddDays(1).ToString("dd/MM/yyyy"):Finance.Day(existing.start).ToString("dd/MM/yyyy"),MaxLength=10};Field(body,"Date prévue · JJ/MM/AAAA",date);
  var kind=Combo(new[]{"Dépense","Revenu"},existing!=null&&existing.kind=="income"?"Revenu":"Dépense");Field(body,"Type",kind);
  var category=Combo(BudgetCategories(),existing==null?"Autres":existing.category);Field(body,"Catégorie",category);
  var remind=new CheckBox{Content="Rappel actif pour cette opération prévue",IsChecked=existing==null||existing.remind};body.Children.Add(remind);var notes=new TextBox{Text=existing==null?"":existing.notes,Height=75,AcceptsReturn=true,MaxLength=10000,TextWrapping=TextWrapping.Wrap};Field(body,"Notes (facultatif)",notes);
  body.Children.Add(T("Le montant entre dans le prévisionnel à la date choisie. Votre solde et vos dépenses réelles ne changent qu'après confirmation. Vous pourrez ajuster la date et le montant réellement passés.",11,Muted,new Thickness(0,0,0,8)));
  var error=T("",12,Accent);body.Children.Add(error);bool saved=false;
  if(existing!=null)actions.Children.Add(Btn("Annuler cette prévision",delegate{if(MessageBox.Show("Annuler cette opération prévue ? Elle sera retirée du prévisionnel.","Annuler la prévision",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;try{PlannedExpenses.Cancel(store,existing.id);saved=true;FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}},false,true));
  actions.Children.Add(Btn("Fermer",delegate{w.DialogResult=false;},false,true));actions.Children.Add(Btn("Enregistrer la prévision",delegate{if(saved)return;long cents;string parsed;if(String.IsNullOrWhiteSpace(title.Text)||!Finance.ParseMoney(amount.Text,false,out cents)||!ReadDate(date,true,out parsed)){error.Text="Vérifiez le libellé, le montant positif et la date.";return;}
   var planned=new BudgetRule{id=existing==null?Guid.NewGuid().ToString():existing.id,title=title.Text.Trim(),cents=cents,start=parsed,end="",repeat="once",kind=kind.SelectedIndex==1?"income":"expense",category=Convert.ToString(category.SelectedItem),notes=notes.Text,remind=remind.IsChecked==true};
   try{PlannedExpenses.Save(store,planned);saved=true;FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}
  },true));ShowWindow(w);Refresh();
 }
}
