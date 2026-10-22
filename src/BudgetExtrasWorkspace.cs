using System;
using System.Linq;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
internal sealed partial class ModernWorkspace {
 void BudgetExtraLinks(StackPanel root){
  var tools=new WrapPanel{Margin=new Thickness(0,0,0,15)};tools.Children.Add(Btn("Objectifs d'épargne",delegate{Go("Mon épargne");},false,true));tools.Children.Add(Btn("Bilan mensuel",delegate{Go("Bilan budget");},false,true));tools.Children.Add(Btn("Rappels d'échéances",delegate{Go("Rappels budget");},false,true));root.Children.Add(tools);
 }
 void SavingsPage(StackPanel root){
  var tools=new WrapPanel{Margin=new Thickness(0,0,0,16)};tools.Children.Add(Btn("Mon budget",delegate{Go("Mon budget");},false,true));tools.Children.Add(Btn("+ Objectif d'épargne",delegate{EditSavingsGoal(null);},true));root.Children.Add(tools);
  root.Children.Add(T("Suivez les montants que vous avez mis de côté. Aucun virement n'est effectué et ce suivi ne modifie ni votre solde courant, ni votre prévisionnel.",12,Muted,new Thickness(0,0,0,20)));
  if(store.Value.budget.goals.Count==0)root.Children.Add(Card("Un projet à financer ?",T("Créez par exemple un objectif Vacances, Nouveau PC ou Réserve de sécurité.",13,Muted)));
  foreach(var goal in store.Value.budget.goals){
   var item=goal;long saved=BudgetExtras.Saved(item);var body=new StackPanel();body.Children.Add(T(Finance.Money(saved)+" / "+Finance.Money(item.targetCents),23));
   body.Children.Add(new ProgressBar{Minimum=0,Maximum=100,Value=Math.Min(100,100.0*saved/item.targetCents),Height=5,Foreground=Accent,Background=Line,BorderThickness=new Thickness(0),Margin=new Thickness(0,13,0,10)});
   body.Children.Add(T(saved>=item.targetCents?"Objectif atteint !":"Reste à mettre de côté : "+Finance.Money(item.targetCents-saved),12,saved>=item.targetCents?Accent:Muted));
   if(!String.IsNullOrEmpty(item.deadline))body.Children.Add(T("Date cible : "+Finance.Day(item.deadline).ToString("dd/MM/yyyy")+(Finance.Day(item.deadline)<DateTime.Today&&saved<item.targetCents?" · À réévaluer":""),11,Muted,new Thickness(0,8,0,0)));
   if(!String.IsNullOrEmpty(item.notes))body.Children.Add(T(item.notes,12,Muted,new Thickness(0,10,0,0)));
   var actions=new WrapPanel{Margin=new Thickness(0,15,0,12)};actions.Children.Add(Btn("Mettre de côté / Retirer",delegate{EditSavingsMove(item);},true));actions.Children.Add(Btn("Modifier l'objectif",delegate{EditSavingsGoal(item);},false,true));body.Children.Add(actions);
   foreach(var move in item.moves.AsEnumerable().Reverse().Take(5))body.Children.Add(T(Finance.Day(move.date).ToString("dd/MM/yyyy")+" · "+(move.cents>=0?"+":"−")+Finance.Money(Math.Abs(move.cents))+(String.IsNullOrWhiteSpace(move.label)?"":" · "+move.label),11,Muted,new Thickness(0,5,0,0)));
   var card=Card(item.name,body);card.Margin=new Thickness(0,0,0,16);root.Children.Add(card);
  }
 }
 void EditSavingsGoal(BudgetGoal existing){
  StackPanel body,actions;var w=Dialog(existing==null?"Nouvel objectif d'épargne":"Modifier l'objectif d'épargne",out body,out actions,690);
  var name=new TextBox{Text=existing==null?"":existing.name,MaxLength=120};Field(body,"Nom de l'objectif",name);
  var target=new TextBox{Text=existing==null?"":(existing.targetCents/100m).ToString("0.00",CultureInfo.GetCultureInfo("fr-FR")),MaxLength=24};Field(body,"Montant cible (€)",target);
  var initial=new TextBox{Text=existing==null?"0":(existing.initialCents/100m).ToString("0.00",CultureInfo.GetCultureInfo("fr-FR")),MaxLength=24,IsReadOnly=existing!=null};Field(body,"Montant déjà mis de côté au départ (€)",initial);
  var deadline=new TextBox{Text=existing==null||String.IsNullOrEmpty(existing.deadline)?"":Finance.Day(existing.deadline).ToString("dd/MM/yyyy"),MaxLength=10};Field(body,"Date cible · JJ/MM/AAAA (facultatif)",deadline);
  var notes=new TextBox{Text=existing==null?"":existing.notes,AcceptsReturn=true,Height=75,TextWrapping=TextWrapping.Wrap,MaxLength=10000};Field(body,"Notes",notes);
  body.Children.Add(T("Après création, utilisez Mettre de côté / Retirer pour actualiser le montant suivi.",11,Muted));var error=T("",12,Accent,new Thickness(0,10,0,0));body.Children.Add(error);bool saved=false;
  if(existing!=null)actions.Children.Add(Btn("Supprimer",delegate{if(MessageBox.Show("Supprimer cet objectif et son historique de suivi ? Votre solde courant restera inchangé.","Supprimer l'objectif",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;try{Finance.Change(store,b=>b.goals.RemoveAll(g=>g.id==existing.id));saved=true;FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}},false,true));
  actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));actions.Children.Add(Btn("Enregistrer l'objectif",delegate{if(saved)return;long goalCents,initialCents;string date;if(String.IsNullOrWhiteSpace(name.Text)||!Finance.ParseMoney(target.Text,false,out goalCents)||!Finance.ParseMoney(initial.Text,true,out initialCents)||initialCents<0||!ReadDate(deadline,false,out date)){error.Text="Vérifiez le nom, les montants et la date.";return;}
   try{BudgetExtras.SaveGoal(store,new BudgetGoal{id=existing==null?Guid.NewGuid().ToString():existing.id,name=name.Text.Trim(),targetCents=goalCents,initialCents=initialCents,deadline=date,notes=notes.Text});saved=true;FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}
  },true));ShowWindow(w);Refresh();
 }
 void EditSavingsMove(BudgetGoal goal){
  StackPanel body,actions;var w=Dialog("Actualiser mon épargne",out body,out actions,610);body.Children.Add(T(goal.name+" · montant suivi "+Finance.Money(BudgetExtras.Saved(goal)),16,Ink,new Thickness(0,0,0,15)));
  var kind=Combo(new[]{"Mettre de côté","Retirer"},"Mettre de côté");Field(body,"Mouvement de suivi",kind);var amount=new TextBox{MaxLength=24};Field(body,"Montant (€), toujours positif",amount);
  var date=new TextBox{Text=DateTime.Today.ToString("dd/MM/yyyy"),MaxLength=10};Field(body,"Date · JJ/MM/AAAA",date);var label=new TextBox{MaxLength=200};Field(body,"Commentaire (facultatif)",label);
  body.Children.Add(T("Ce mouvement actualise uniquement cet objectif. Il ne crée aucune opération dans votre compte courant.",11,Muted));var error=T("",12,Accent,new Thickness(0,12,0,0));body.Children.Add(error);bool saved=false;
  actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));actions.Children.Add(Btn("Enregistrer le mouvement",delegate{if(saved)return;long cents;string parsed;if(!Finance.ParseMoney(amount.Text,false,out cents)||!ReadDate(date,true,out parsed)){error.Text="Vérifiez le montant et la date.";return;}try{BudgetExtras.MoveSavings(store,goal.id,kind.SelectedIndex==0?cents:-cents,label.Text,Finance.Day(parsed),DateTime.Today);saved=true;FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}},true));ShowWindow(w);Refresh();
 }
 void BudgetReportPage(StackPanel root){
  var tools=new WrapPanel{Margin=new Thickness(0,0,0,15)};tools.Children.Add(Btn("Mon budget",delegate{Go("Mon budget");},false,true));tools.Children.Add(Btn("‹",delegate{if(budgetMonth.Year>1900){budgetMonth=budgetMonth.AddMonths(-1);Refresh();}},false,true));tools.Children.Add(Btn("Ce mois",delegate{budgetMonth=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1);Refresh();},false,true));tools.Children.Add(Btn("›",delegate{if(budgetMonth.Year<2200){budgetMonth=budgetMonth.AddMonths(1);Refresh();}},false,true));tools.Children.Add(T(budgetMonth.ToString("MMMM yyyy",CultureInfo.GetCultureInfo("fr-FR")),20,Ink,new Thickness(10,6,0,0)));root.Children.Add(tools);
  var report=BudgetExtras.Month(store.Value.budget,budgetMonth);var previous=BudgetExtras.Month(store.Value.budget,budgetMonth.AddMonths(-1));
  root.Children.Add(T("Ce bilan utilise les opérations saisies. Les prévisions et le suivi d'épargne restent séparés. Un mois non renseigné ne permet pas de conclure à une absence de dépenses.",12,Muted,new Thickness(0,0,0,20)));
  var summary=new UniformGrid{Columns=3};foreach(var item in new[]{new[]{"Revenus saisis",Finance.Money(report.Income)},new[]{"Dépenses saisies",Finance.Money(report.Expenses)},new[]{"Différence revenus − dépenses",Finance.Money(report.Net)}}){var body=new StackPanel();body.Children.Add(T(item[0],11,Muted));body.Children.Add(T(item[1],23,Ink,new Thickness(0,10,0,0)));var box=Box(body,new Thickness(16));box.Margin=new Thickness(0,0,10,0);summary.Children.Add(box);}root.Children.Add(summary);
  root.Children.Add(T("Par rapport au mois précédent : "+(report.Expenses>=previous.Expenses?"+":"−")+Finance.Money(Math.Abs(report.Expenses-previous.Expenses))+" de dépenses saisies.",12,Muted,new Thickness(0,16,0,20)));
  var series=Enumerable.Range(0,6).Select(i=>BudgetExtras.Month(store.Value.budget,budgetMonth.AddMonths(i-5))).ToList();long max=Math.Max(1,series.Max(m=>Math.Max(m.Income,m.Expenses)));
  var chart=new UniformGrid{Columns=6};foreach(var month in series){var tile=new StackPanel{Margin=new Thickness(4)};var bars=new Grid{Height=110,Margin=new Thickness(0,0,0,8)};bars.ColumnDefinitions.Add(new ColumnDefinition());bars.ColumnDefinitions.Add(new ColumnDefinition());var income=new System.Windows.Shapes.Rectangle{Width=20,Height=110.0*month.Income/max,Fill=Accent,VerticalAlignment=VerticalAlignment.Bottom};var expenses=new System.Windows.Shapes.Rectangle{Width=20,Height=110.0*month.Expenses/max,Fill=Muted,VerticalAlignment=VerticalAlignment.Bottom};Grid.SetColumn(expenses,1);bars.Children.Add(income);bars.Children.Add(expenses);tile.Children.Add(bars);tile.Children.Add(T(month.Month.ToString("MMM yy",CultureInfo.GetCultureInfo("fr-FR")),11,Muted));tile.Children.Add(T(Finance.Money(month.Income),10,Accent,new Thickness(0,6,0,0)));tile.Children.Add(T(Finance.Money(month.Expenses),10,Muted,new Thickness(0,4,0,0)));chart.Children.Add(tile);}
  var chartBody=new StackPanel();chartBody.Children.Add(T("Revenus : couleur d'accent · Dépenses : gris",11,Muted,new Thickness(0,0,0,18)));chartBody.Children.Add(chart);root.Children.Add(Card("Les six derniers mois",chartBody));
  var categories=new StackPanel();if(report.Categories.Count==0)categories.Children.Add(T("Aucune dépense saisie pour ce mois.",12,Muted));foreach(var category in report.Categories){var row=new StackPanel{Margin=new Thickness(0,0,0,16)};row.Children.Add(T(category.Name+" · "+Finance.Money(category.Cents)+" · "+(100m*category.Cents/Math.Max(1,report.Expenses)).ToString("0",CultureInfo.InvariantCulture)+" %",12));row.Children.Add(new ProgressBar{Minimum=0,Maximum=100,Value=100.0*category.Cents/Math.Max(1,report.Expenses),Height=4,Foreground=Accent,Background=Line,BorderThickness=new Thickness(0),Margin=new Thickness(0,9,0,0)});categories.Children.Add(row);}var card=Card("Répartition des dépenses",categories);card.Margin=new Thickness(0,16,0,0);root.Children.Add(card);
 }
 void BudgetReminderPage(StackPanel root){
  root.Children.Add(Btn("Mon budget",delegate{Go("Mon budget");},false,true));var settings=store.Value.budget.reminders;var body=new StackPanel();
  var enabled=new CheckBox{Content="Activer les rappels d'échéances du budget",IsChecked=settings.enabled};body.Children.Add(enabled);var income=new CheckBox{Content="Inclure aussi les revenus attendus",IsChecked=settings.includeIncome};body.Children.Add(income);
  int[] leads={0,1,2,3,7,14};string[] labels={"Le jour même","1 jour avant","2 jours avant","3 jours avant","7 jours avant","14 jours avant"};var days=Combo(labels,labels[Array.IndexOf(leads,settings.daysBefore)]);Field(body,"Quand me prévenir ?",days);
  var time=new TextBox{Text=settings.time,MaxLength=5};Field(body,"Heure locale · HH:mm",time);var minimum=new TextBox{Text=(settings.minimumCents/100m).ToString("0.00",CultureInfo.GetCultureInfo("fr-FR")),MaxLength=24};Field(body,"Montant minimum à rappeler (€), 0 pour tous",minimum);
  body.Children.Add(T("Les rappels concernent les opérations encore à confirmer et dont l'option Rappel est active. À la réouverture, une échéance à venir peut encore être signalée si son rappel a été manqué.",11,Muted,new Thickness(0,0,0,12)));
  body.Children.Add(T("Mon espace doit rester actif, même réduit près de l'horloge. Windows peut masquer les notifications en mode Ne pas déranger.",11,Muted));if(!store.Value.settings.Notifications)body.Children.Add(T("Les notifications générales sont désactivées dans Réglages.",12,Accent,new Thickness(0,12,0,0)));
  var error=T("",12,Accent,new Thickness(0,12,0,10));body.Children.Add(error);body.Children.Add(Btn("Enregistrer les rappels",delegate{long cents;if(!Schedule.ValidTime(time.Text.Trim())||!Finance.ParseMoney(minimum.Text,true,out cents)||cents<0){error.Text="Vérifiez l'heure et le montant minimum positif ou nul.";return;}try{Finance.Change(store,b=>b.reminders=new BudgetReminderOptions{enabled=enabled.IsChecked==true,includeIncome=income.IsChecked==true,daysBefore=leads[days.SelectedIndex],time=time.Text.Trim(),minimumCents=cents});error.Text="Rappels enregistrés.";}catch(Exception ex){error.Text=ex.Message;}},true));root.Children.Add(Card("Mes rappels budget",body));
  BudgetReminderOverview(root);
 }
 void BudgetReminderOverview(StackPanel root){
  var upcoming=Finance.Pending(store.Value.budget,DateTime.Today,DateTime.Today.AddDays(14)).Where(o=>o.Date>=DateTime.Today&&o.Rule.remind).Take(10).ToList();if(upcoming.Count==0)return;
  var body=new StackPanel();foreach(var item in upcoming)body.Children.Add(T(item.Date.ToString("dd/MM")+" · "+item.Rule.title+" · "+Finance.Money(item.Rule.cents),12,Muted,new Thickness(0,6,0,6)));body.Children.Add(Btn("Ouvrir mon budget",delegate{Go("Mon budget");},false,true));var card=Card("Échéances budget des 14 prochains jours",body);card.Margin=new Thickness(0,16,0,16);root.Children.Add(card);
 }
 internal void OpenBudget(){if(owner.Enabled)Go("Mon budget");}
}
