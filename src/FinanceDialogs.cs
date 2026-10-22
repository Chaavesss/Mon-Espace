using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
internal sealed partial class ModernWorkspace {
 void EditBudgetOverdraft(){
  StackPanel body,actions;var w=Dialog("Découvert autorisé",out body,out actions,450);
  var b=store.Value.budget;var amount=new TextBox{Text=b.overdraftKnown?(b.overdraftCents/100m).ToString("0.00",System.Globalization.CultureInfo.GetCultureInfo("fr-FR")):"",MaxLength=24};Field(body,"Montant autorisé (€), 0 si aucun",amount);
  body.Children.Add(T("Exemple : saisissez 500 pour une limite de −500 €. Ce montant sert de seuil d'alerte et n'est pas ajouté à votre solde.",12,Muted));
  var error=T("",12,Accent,new Thickness(0,12,0,0));body.Children.Add(error);bool saved=false;
  actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));actions.Children.Add(Btn("Enregistrer la limite",delegate{if(saved)return;long cents;if(!Finance.ParseMoney(amount.Text,true,out cents)||cents<0){error.Text="Saisissez un montant positif ou nul, avec au maximum deux décimales.";return;}try{Finance.Change(store,data=>{data.overdraftKnown=true;data.overdraftCents=cents;});saved=true;FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}},true));ShowWindow(w);Refresh();
 }
 void ConfirmBudgetOccurrence(BudgetOccurrence occurrence){
  StackPanel body,actions;var w=Dialog("Confirmer le passage",out body,out actions,630);
  body.Children.Add(T(occurrence.Rule.title+" · prévu le "+occurrence.Date.ToString("dd/MM/yyyy"),16,Ink,new Thickness(0,0,0,15)));
  var candidates=store.Value.budget.entries.Where(e=>e.kind==occurrence.Rule.kind&&String.IsNullOrEmpty(e.occurrence)).OrderByDescending(e=>e.date).Take(100).ToList();
  var labels=new[]{"Créer une nouvelle opération"}.Concat(candidates.Select(e=>Finance.Day(e.date).ToString("dd/MM")+" · "+e.title+" · "+Finance.Money(e.cents))).ToArray();
  var match=Combo(labels,labels[0]);Field(body,"Créer ou associer une opération déjà saisie",match);
  var amount=new TextBox{Text=(occurrence.Rule.cents/100m).ToString("0.00",System.Globalization.CultureInfo.GetCultureInfo("fr-FR")),MaxLength=24};Field(body,"Montant réellement passé (€)",amount);
  var date=new TextBox{Text=DateTime.Today.ToString("dd/MM/yyyy"),MaxLength=10};Field(body,"Date effective · JJ/MM/AAAA",date);
  match.SelectionChanged+=delegate{amount.IsEnabled=date.IsEnabled=match.SelectedIndex==0;};
  body.Children.Add(T("Associer une opération existante la marque comme confirmée sans créer une deuxième dépense ou un deuxième revenu.",11,Muted));var error=T("",12,Accent,new Thickness(0,12,0,0));body.Children.Add(error);bool saved=false;
  actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));actions.Children.Add(Btn("Confirmer le passage",delegate{if(saved)return;long cents=occurrence.Rule.cents;string parsed=DateTime.Today.ToString("yyyy-MM-dd");if(match.SelectedIndex==0&&(!Finance.ParseMoney(amount.Text,false,out cents)||!ReadDate(date,true,out parsed))){error.Text="Vérifiez le montant et la date.";return;}
   try{Finance.Reconcile(store,occurrence,DateTime.Today,Finance.Day(parsed),cents,match.SelectedIndex==0?null:candidates[match.SelectedIndex-1].id);saved=true;FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}
  },true));ShowWindow(w);Refresh();
 }
 void FinishFinanceDialog(Window window){if(dialogProbe==null&&previewPrefix==null)window.DialogResult=true;}
 void EditBudgetBalance(){
  StackPanel body,actions;var w=Dialog("Renseigner mon solde",out body,out actions,540);
  var b=store.Value.budget;var amount=new TextBox{Text=b.hasBalance?(Finance.Balance(b,DateTime.Today)/100m).ToString("0.00",System.Globalization.CultureInfo.GetCultureInfo("fr-FR")):"",MaxLength=24};Field(body,"Solde réel actuel (€), négatif si découvert",amount);
  body.Children.Add(T("Ce montant devient votre nouveau point de départ aujourd'hui. Toutes les opérations déjà saisies sont considérées comme incluses : elles ne seront pas déduites une deuxième fois.",12,Muted,new Thickness(0,0,0,14)));
  body.Children.Add(T("Les opérations ajoutées ensuite, datées d'aujourd'hui ou d'un jour suivant, modifieront le solde estimé. Les ajouts antidatés avant ce point de départ servent seulement à compléter l'historique.",11,Muted));
  var error=T("",12,Accent,new Thickness(0,14,0,0));body.Children.Add(error);bool saved=false;
  actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));actions.Children.Add(Btn("Enregistrer le solde",delegate{if(saved)return;long cents;if(!Finance.ParseMoney(amount.Text,true,out cents)){error.Text="Saisissez un montant en euros, avec au maximum deux décimales.";return;}try{Finance.Change(store,data=>Finance.SetBalance(data,cents,DateTime.Today));saved=true;FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}},true));ShowWindow(w);Refresh();
 }
 void EditBudgetEntry(BudgetEntry existing,string entryKind){
  StackPanel body,actions;var w=Dialog(existing==null?(entryKind=="income"?"Ajouter un revenu":"Ajouter une dépense"):"Modifier l'opération",out body,out actions,760);
  var title=new TextBox{Text=existing==null?"":existing.title,MaxLength=200};Field(body,"Libellé",title);
  var amount=new TextBox{Text=existing==null?"":(existing.cents/100m).ToString("0.00",System.Globalization.CultureInfo.GetCultureInfo("fr-FR")),MaxLength=24};Field(body,"Montant (€), toujours positif",amount);
  var date=new TextBox{Text=existing==null?DateTime.Today.ToString("dd/MM/yyyy"):Finance.Day(existing.date).ToString("dd/MM/yyyy"),MaxLength=10};Field(body,"Date de passage · JJ/MM/AAAA",date);
  var kind=Combo(new[]{"Dépense","Revenu"},entryKind=="income"?"Revenu":"Dépense");kind.IsEnabled=existing==null||String.IsNullOrEmpty(existing.occurrence);Field(body,"Type",kind);
  var category=Combo(BudgetCategories(),existing==null?(entryKind=="income"?"Salaire":"Courses"):existing.category);Field(body,"Catégorie",category);
  var project=Combo(new[]{"Sans projet"}.Concat(owner.AllProjects()),existing==null?String.IsNullOrEmpty(openProject)?"Sans projet":openProject:String.IsNullOrEmpty(existing.project)?"Sans projet":existing.project);Field(body,"Projet (facultatif)",project);
  var notes=new TextBox{Text=existing==null?"":existing.notes,AcceptsReturn=true,Height=70,MaxLength=10000,TextWrapping=TextWrapping.Wrap};Field(body,"Notes (facultatif)",notes);
  var error=T("",12,Accent);body.Children.Add(error);bool saved=false;
  if(existing!=null)actions.Children.Add(Btn("Supprimer",delegate{if(MessageBox.Show("Supprimer cette opération ? Une prévision ou récurrence liée redeviendra à confirmer.","Supprimer l'opération",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;try{Finance.Change(store,b=>b.entries.RemoveAll(e=>e.id==existing.id));saved=true;FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}},false,true));
  actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));actions.Children.Add(Btn("Enregistrer l'opération",delegate{if(saved)return;long cents;string parsed;if(String.IsNullOrWhiteSpace(title.Text)||!Finance.ParseMoney(amount.Text,false,out cents)||!ReadDate(date,true,out parsed)){error.Text="Vérifiez le libellé, le montant positif et la date.";return;}
   var item=new BudgetEntry{project=project.SelectedIndex<=0?"":Convert.ToString(project.SelectedItem),id=existing==null?Guid.NewGuid().ToString():existing.id,title=title.Text.Trim(),cents=cents,date=parsed,kind=kind.SelectedIndex==1?"income":"expense",category=Convert.ToString(category.SelectedItem),notes=notes.Text,occurrence=existing==null?"":existing.occurrence};
   try{Finance.AddEntry(store,item,DateTime.Today);saved=true;FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}
  },true));ShowWindow(w);Refresh();
 }
 void EditBudgetRule(BudgetRule existing){
  StackPanel body,actions;var w=Dialog(existing==null?"Nouvelle récurrence":"Modifier la récurrence",out body,out actions,790);
  var title=new TextBox{Text=existing==null?"":existing.title,MaxLength=200};Field(body,"Libellé : loyer, salaire, abonnement…",title);
  var amount=new TextBox{Text=existing==null?"":(existing.cents/100m).ToString("0.00",System.Globalization.CultureInfo.GetCultureInfo("fr-FR")),MaxLength=24};Field(body,"Montant (€), toujours positif",amount);
  var kind=Combo(new[]{"Dépense","Revenu"},existing!=null&&existing.kind=="income"?"Revenu":"Dépense");Field(body,"Type",kind);
  var category=Combo(BudgetCategories(),existing==null?"Abonnements":existing.category);Field(body,"Catégorie",category);
  var start=new TextBox{Text=existing==null?DateTime.Today.ToString("dd/MM/yyyy"):Finance.Day(existing.start).ToString("dd/MM/yyyy"),MaxLength=10};Field(body,"Premier passage · JJ/MM/AAAA",start);
  var repeat=Combo(new[]{"Chaque semaine","Chaque mois","Chaque année"},existing==null||existing.repeat=="monthly"?"Chaque mois":existing.repeat=="weekly"?"Chaque semaine":"Chaque année");Field(body,"Répétition",repeat);
  var end=new TextBox{Text=existing==null||String.IsNullOrEmpty(existing.end)?"":Finance.Day(existing.end).ToString("dd/MM/yyyy"),MaxLength=10};Field(body,"Dernier passage autorisé (facultatif)",end);var remind=new CheckBox{Content="Rappel actif pour cette récurrence",IsChecked=existing==null||existing.remind};body.Children.Add(remind);
  body.Children.Add(T("Pour un passage le 31, les mois plus courts utilisent leur dernier jour. Confirmez le passage dans le budget quand l'opération est réellement effectuée.",11,Muted,new Thickness(0,0,0,8)));
  var error=T("",12,Accent);body.Children.Add(error);bool saved=false;
  if(existing!=null)actions.Children.Add(Btn("Supprimer",delegate{if(MessageBox.Show("Supprimer cette récurrence ? Les opérations déjà confirmées seront conservées.","Supprimer la récurrence",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;try{Finance.Change(store,b=>b.rules.RemoveAll(r=>r.id==existing.id));saved=true;FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}},false,true));
  actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));actions.Children.Add(Btn("Enregistrer la récurrence",delegate{if(saved)return;long cents;string from,to;if(String.IsNullOrWhiteSpace(title.Text)||!Finance.ParseMoney(amount.Text,false,out cents)||!ReadDate(start,true,out from)||!ReadDate(end,false,out to)){error.Text="Vérifiez le libellé, le montant et les dates.";return;}
   var rule=new BudgetRule{id=existing==null?Guid.NewGuid().ToString():existing.id,title=title.Text.Trim(),cents=cents,kind=kind.SelectedIndex==1?"income":"expense",category=Convert.ToString(category.SelectedItem),start=from,end=to,repeat=new[]{"weekly","monthly","yearly"}[repeat.SelectedIndex],remind=remind.IsChecked==true};
   try{Finance.Change(store,b=>{b.rules.RemoveAll(r=>r.id==rule.id);b.rules.Add(rule);});saved=true;FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}
  },true));ShowWindow(w);Refresh();
 }
 void EditBudgetCap(BudgetCap existing){
  StackPanel body,actions;var w=Dialog("Budget par catégorie",out body,out actions,470);
  var category=new TextBox{Text=existing==null?"Courses":existing.category,MaxLength=60};Field(body,"Catégorie (vous pouvez créer la vôtre)",category);
  var amount=new TextBox{Text=existing==null?"":(existing.cents/100m).ToString("0.00",System.Globalization.CultureInfo.GetCultureInfo("fr-FR")),MaxLength=24};Field(body,"Plafond mensuel (€)",amount);
  body.Children.Add(T("Cette enveloppe est un objectif mensuel. Elle ne crée aucune dépense et ne modifie pas le solde.",11,Muted));var error=T("",12,Accent,new Thickness(0,12,0,0));body.Children.Add(error);bool saved=false;
  if(existing!=null)actions.Children.Add(Btn("Retirer",delegate{try{Finance.Change(store,b=>b.caps.RemoveAll(c=>String.Equals(c.category,existing.category,StringComparison.OrdinalIgnoreCase)));saved=true;FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}},false,true));
  actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));actions.Children.Add(Btn("Enregistrer le budget",delegate{if(saved)return;long cents;if(String.IsNullOrWhiteSpace(category.Text)||!Finance.ParseMoney(amount.Text,false,out cents)){error.Text="Indiquez une catégorie et un montant positif.";return;}try{Finance.Change(store,b=>{if(existing!=null)b.caps.RemoveAll(c=>String.Equals(c.category,existing.category,StringComparison.OrdinalIgnoreCase));b.caps.RemoveAll(c=>String.Equals(c.category,category.Text.Trim(),StringComparison.OrdinalIgnoreCase));b.caps.Add(new BudgetCap{category=category.Text.Trim(),cents=cents});});saved=true;FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}},true));ShowWindow(w);Refresh();
 }
}
