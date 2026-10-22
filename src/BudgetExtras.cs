using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
public sealed class BudgetGoal {
 public string id="",name="",deadline="",notes="";public long targetCents,initialCents;
 public List<SavingsMove> moves=new List<SavingsMove>();
}
public sealed class SavingsMove {public string id="",date="",label="";public long cents;}
public sealed class BudgetReminderOptions {public bool enabled=true,includeIncome=false;public int daysBefore=3;public string time="09:00";public long minimumCents;}
internal sealed class BudgetNotice {internal BudgetOccurrence Occurrence;internal string Key{get{return Occurrence.Key+"/budget";}}}
internal sealed class BudgetCategoryTotal {internal string Name;internal long Cents;}
internal sealed class BudgetMonthReport {
 internal DateTime Month;internal long Income,Expenses;internal long Net{get{return Income-Expenses;}}
 internal List<BudgetCategoryTotal> Categories=new List<BudgetCategoryTotal>();
}
internal static class BudgetExtras {
 internal static long Saved(BudgetGoal goal){return goal.initialCents+goal.moves.Sum(m=>m.cents);}
 internal static void Validate(BudgetData b){
  if(b.goals==null)b.goals=new List<BudgetGoal>();if(b.reminders==null)b.reminders=new BudgetReminderOptions();if(b.remindersSent==null)b.remindersSent=new List<string>();
  if(b.goals.Count>100||b.remindersSent.Count>5000||b.remindersSent.Any(s=>String.IsNullOrWhiteSpace(s)||s.Length>160)||b.remindersSent.Distinct().Count()!=b.remindersSent.Count)throw new InvalidDataException("Objectifs ou rappels budget invalides.");
  var ids=new HashSet<string>();foreach(var goal in b.goals){
   if(goal==null||String.IsNullOrWhiteSpace(goal.id)||goal.id.Length>100||!ids.Add(goal.id)||String.IsNullOrWhiteSpace(goal.name)||goal.name.Length>120||goal.targetCents<=0||goal.targetCents>Finance.MaxMoney||goal.initialCents<0||goal.initialCents>Finance.MaxMoney||(goal.notes??"").Length>10000||(!String.IsNullOrEmpty(goal.deadline)&&!Finance.DateOk(goal.deadline))||goal.moves==null||goal.moves.Count>2000)throw new InvalidDataException("Objectif d'épargne invalide.");
   var moves=new HashSet<string>();foreach(var move in goal.moves)if(move==null||String.IsNullOrWhiteSpace(move.id)||move.id.Length>100||!moves.Add(move.id)||!Finance.DateOk(move.date)||move.cents==0||move.cents>Finance.MaxMoney||move.cents< -Finance.MaxMoney||(move.label??"").Length>200)throw new InvalidDataException("Mouvement d'épargne invalide.");
   if(Saved(goal)<0||Saved(goal)>Finance.MaxMoney)throw new InvalidDataException("Le montant suivi pour un objectif doit rester positif ou nul.");
  }
  if(!new[]{0,1,2,3,7,14}.Contains(b.reminders.daysBefore)||!Schedule.ValidTime(b.reminders.time)||b.reminders.minimumCents<0||b.reminders.minimumCents>Finance.MaxMoney)throw new InvalidDataException("Paramètres des rappels budget invalides.");
 }
 internal static void SaveGoal(Store store,BudgetGoal goal){Finance.Change(store,b=>{var old=b.goals.FirstOrDefault(g=>g.id==goal.id);if(old!=null){goal.moves=old.moves.ToList();goal.initialCents=old.initialCents;}b.goals.RemoveAll(g=>g.id==goal.id);b.goals.Add(goal);});}
 internal static void MoveSavings(Store store,string id,long cents,string label,DateTime date,DateTime today){
  if(date.Date>today.Date)throw new InvalidDataException("Renseignez une mise de côté ou un retrait déjà effectué.");
  Finance.Change(store,b=>{var goal=b.goals.FirstOrDefault(g=>g.id==id);if(goal==null)throw new InvalidDataException("Objectif introuvable.");goal.moves.Add(new SavingsMove{id=Guid.NewGuid().ToString(),date=date.ToString("yyyy-MM-dd"),cents=cents,label=label??""});});
 }
 internal static BudgetMonthReport Month(BudgetData budget,DateTime month){
  var entries=budget.entries.Where(e=>Finance.Day(e.date).Year==month.Year&&Finance.Day(e.date).Month==month.Month).ToList();
  return new BudgetMonthReport{Month=new DateTime(month.Year,month.Month,1),Income=entries.Where(e=>e.kind=="income").Sum(e=>e.cents),Expenses=entries.Where(e=>e.kind=="expense").Sum(e=>e.cents),Categories=entries.Where(e=>e.kind=="expense").GroupBy(e=>e.category,StringComparer.OrdinalIgnoreCase).Select(g=>new BudgetCategoryTotal{Name=g.Key,Cents=g.Sum(e=>e.cents)}).OrderByDescending(c=>c.Cents).ToList()};
 }
 internal static List<BudgetNotice> Due(BudgetData budget,DateTime now){
  var settings=budget.reminders;var result=new List<BudgetNotice>();if(!settings.enabled)return result;
  var sent=new HashSet<string>(budget.remindersSent);var pending=Finance.Pending(budget,now.Date,now.Date.AddDays(settings.daysBefore));
  foreach(var item in pending){
   if(!item.Rule.remind||item.Date<now.Date||(!settings.includeIncome&&item.Rule.kind=="income")||item.Rule.cents<settings.minimumCents)continue;
   var at=item.Date.AddDays(-settings.daysBefore).Add(TimeSpan.Parse(settings.time,System.Globalization.CultureInfo.InvariantCulture));var notice=new BudgetNotice{Occurrence=item};
   if(now>=at&&!sent.Contains(notice.Key))result.Add(notice);
  }return result;
 }
 internal static void Mark(BudgetData budget,IEnumerable<BudgetNotice> notices){foreach(var notice in notices)if(!budget.remindersSent.Contains(notice.Key))budget.remindersSent.Add(notice.Key);if(budget.remindersSent.Count>5000)budget.remindersSent.RemoveRange(0,budget.remindersSent.Count-5000);}
}
