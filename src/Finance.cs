using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Web.Script.Serialization;
public sealed class BudgetData {public List<BudgetGoal> goals=new List<BudgetGoal>();public BudgetReminderOptions reminders=new BudgetReminderOptions();public List<string> remindersSent=new List<string>();
 public bool overdraftKnown;public long overdraftCents;
 public bool hasBalance;public long balanceCents;public string balanceDate="";
 public List<string> includedIds=new List<string>();
 public List<BudgetEntry> entries=new List<BudgetEntry>();
 public List<BudgetRule> rules=new List<BudgetRule>();
 public List<BudgetCap> caps=new List<BudgetCap>();
}
public sealed class BudgetEntry {public string project="";public string id="",title="",date="",kind="expense",category="Autres",notes="",occurrence="";public long cents;}
public sealed class BudgetRule {public bool remind=true;public string notes="";public string id="",title="",start="",end="",kind="expense",category="Abonnements",repeat="monthly";public long cents;}
public sealed class BudgetCap {public string category="";public long cents;}
internal sealed class BudgetOccurrence {internal BudgetRule Rule;internal DateTime Date;internal string Key{get{return Rule.id+"/"+Date.ToString("yyyy-MM-dd");}}}
internal sealed class BudgetForecast {internal long End,Minimum;internal DateTime MinimumDate;internal List<BudgetOccurrence> Pending=new List<BudgetOccurrence>();}
internal static class Finance {
 internal static readonly string[] Categories={"Courses","Carburant","Logement","Abonnements","Loisirs","Santé","Impôts","Salaire","Autres"};
 internal const long MaxMoney=100000000000L;
 internal static string Money(long cents){return (cents/100m).ToString("N2",CultureInfo.GetCultureInfo("fr-FR"))+" €";}
 internal static bool ParseMoney(string value,bool signed,out long cents){
  cents=0;if(value==null)return false;value=value.Trim().Replace("\u00a0","").Replace("\u202f","").Replace(" ","").Replace(",",".");
  decimal amount;if(!Decimal.TryParse(value,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out amount)||Decimal.Round(amount,2)!=amount||amount>MaxMoney/100m||amount< -MaxMoney/100m||(!signed&&amount<=0))return false;
  cents=(long)(amount*100);return true;
 }
 internal static DateTime Day(string date){return DateTime.ParseExact(date,"yyyy-MM-dd",CultureInfo.InvariantCulture);}
 internal static bool DateOk(string date){DateTime value;return DateTime.TryParseExact(date,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out value)&&value.Year>=1900&&value.Year<=2200;}
 static bool Text(string text,int max){return !String.IsNullOrWhiteSpace(text)&&text.Length<=max;}
 static bool Kind(string kind){return kind=="income"||kind=="expense";}
 internal static void Validate(Data data){
  if(data.budget==null)data.budget=new BudgetData();var b=data.budget;BudgetExtras.Validate(b);
  if(b.entries==null||b.rules==null||b.caps==null||b.includedIds==null||b.entries.Count>10000||(b.rules.Count>600||b.rules.Count(r=>r!=null&&r.repeat!="once")>100||b.rules.Count(r=>r!=null&&r.repeat=="once")>500)||b.caps.Count>50||b.includedIds.Count>10000||b.includedIds.Any(id=>!Text(id,100))||b.includedIds.Distinct().Count()!=b.includedIds.Count)throw new InvalidDataException("Données budget invalides ou trop volumineuses.");
  if(b.balanceCents>MaxMoney||b.balanceCents< -MaxMoney||(b.hasBalance&&!DateOk(b.balanceDate)))throw new InvalidDataException("Solde de référence invalide.");
  if(b.overdraftCents<0||b.overdraftCents>MaxMoney)throw new InvalidDataException("Découvert autorisé invalide.");
  var ids=new HashSet<string>();var occurrences=new HashSet<string>();
  foreach(var e in b.entries){
   if(e==null||!Text(e.id,100)||!ids.Add(e.id)||!Text(e.title,200)||!Text(e.category,60)||!DateOk(e.date)||!Kind(e.kind)||e.cents<=0||e.cents>MaxMoney||(e.notes??"").Length>10000||(e.occurrence??"").Length>120)throw new InvalidDataException("Opération budget invalide.");
   if(!String.IsNullOrEmpty(e.occurrence)&&!occurrences.Add(e.occurrence))throw new InvalidDataException("Échéance budgétaire déjà confirmée.");
  }
  ids.Clear();foreach(var r in b.rules)if(r==null||!Text(r.id,100)||!ids.Add(r.id)||!Text(r.title,200)||!Text(r.category,60)||!DateOk(r.start)||(!String.IsNullOrEmpty(r.end)&&(!DateOk(r.end)||Day(r.end)<Day(r.start)))||!Kind(r.kind)||!new[]{"once","weekly","monthly","yearly"}.Contains(r.repeat)||(r.notes??"").Length>10000||r.cents<=0||r.cents>MaxMoney)throw new InvalidDataException("Prélèvement ou revenu récurrent invalide.");
  var categories=new HashSet<string>(StringComparer.OrdinalIgnoreCase);foreach(var cap in b.caps)if(cap==null||!Text(cap.category,60)||!categories.Add(cap.category)||cap.cents<=0||cap.cents>MaxMoney)throw new InvalidDataException("Budget de catégorie invalide.");
 }
 internal static BudgetData Copy(BudgetData value){var j=new JavaScriptSerializer{MaxJsonLength=8*1024*1024};return j.Deserialize<BudgetData>(j.Serialize(value));}
 internal static bool ExceedsOverdraft(BudgetData b,long balance){return b.overdraftKnown&&balance< -b.overdraftCents;}
 internal static string OverdraftSummary(BudgetData b,long minimum){
  if(!b.overdraftKnown)return "Votre limite de découvert n'est pas renseignée.";
  return ExceedsOverdraft(b,minimum)?"Limite autorisée dépassée de "+Money(-b.overdraftCents-minimum)+".":"Le solde reste dans votre limite de découvert autorisé ("+Money(b.overdraftCents)+").";
 }
 internal static void Change(Store store,Action<BudgetData> change){
  var before=Copy(store.Value.budget);bool first=!before.hasBalance&&before.entries.Count==0&&before.rules.Count==0&&before.caps.Count==0;
  var widget=store.Value.settings.HomeBlocks.FirstOrDefault(v=>v.id=="finance");bool visible=widget!=null&&widget.visible;
  try{change(store.Value.budget);if(first&&widget!=null)widget.visible=true;store.Save();}
  catch{store.Value.budget=before;if(widget!=null)widget.visible=visible;throw;}
 }
 internal static void SetBalance(BudgetData b,long cents,DateTime today){b.hasBalance=true;b.balanceCents=cents;b.balanceDate=today.ToString("yyyy-MM-dd");b.includedIds=b.entries.Where(e=>Day(e.date)<=today.Date).Select(e=>e.id).ToList();}
 internal static long Signed(BudgetEntry e){return e.kind=="income"?e.cents:-e.cents;}
 internal static long Balance(BudgetData b,DateTime today){
  if(!b.hasBalance)return 0;var included=new HashSet<string>(b.includedIds);
  return b.balanceCents+b.entries.Where(e=>!included.Contains(e.id)&&Day(e.date)>=Day(b.balanceDate)&&Day(e.date)<=today.Date).Sum(e=>Signed(e));
 }
 internal static DateTime OccurrenceDate(BudgetRule r,int index){
  var start=Day(r.start);if(r.repeat=="once"){if(index!=0)throw new InvalidDataException("Une opération ponctuelle ne se répète pas.");return start;}if(r.repeat=="weekly")return start.AddDays(7.0*index);
  if(r.repeat=="yearly"){int year=start.Year+index;return new DateTime(year,start.Month,Math.Min(start.Day,DateTime.DaysInMonth(year,start.Month)));}
  var month=new DateTime(start.Year,start.Month,1).AddMonths(index);return new DateTime(month.Year,month.Month,Math.Min(start.Day,DateTime.DaysInMonth(month.Year,month.Month)));
 }
 internal static List<BudgetOccurrence> Pending(BudgetData b,DateTime from,DateTime until){
  var paid=new HashSet<string>(b.entries.Where(e=>!String.IsNullOrEmpty(e.occurrence)).Select(e=>e.occurrence));var list=new List<BudgetOccurrence>();
  foreach(var r in b.rules){
   if(r.repeat=="once"){var once=new BudgetOccurrence{Rule=r,Date=Day(r.start)};if(once.Date<=until&&!paid.Contains(once.Key))list.Add(once);continue;}
   var start=Day(r.start);int index=Math.Max(0,r.repeat=="weekly"?(int)(from-start).TotalDays/7:r.repeat=="yearly"?from.Year-start.Year:(from.Year-start.Year)*12+from.Month-start.Month);
   for(;;index++){var date=OccurrenceDate(r,index);if(date>until||(!String.IsNullOrEmpty(r.end)&&date>Day(r.end)))break;if(date<from)continue;var item=new BudgetOccurrence{Rule=r,Date=date};if(!paid.Contains(item.Key))list.Add(item);}
  }return list.OrderBy(o=>o.Date).ThenBy(o=>o.Rule.kind=="expense"?0:1).ToList();
 }
 internal static BudgetForecast Forecast(BudgetData b,DateTime today,DateTime until){
  var result=new BudgetForecast{End=Balance(b,today),Minimum=Balance(b,today),MinimumDate=today.Date};
  DateTime from=b.hasBalance?Day(b.balanceDate):today.Date;result.Pending=Pending(b,from,until.Date);
  foreach(var group in result.Pending.GroupBy(o=>o.Date<today.Date?today.Date:o.Date).OrderBy(g=>g.Key)){
   result.End+=group.Sum(o=>o.Rule.kind=="income"?o.Rule.cents:-o.Rule.cents);
   if(result.End<result.Minimum){result.Minimum=result.End;result.MinimumDate=group.Key;}
  }return result;
 }
 internal static long Spent(BudgetData b,DateTime month,string category){return b.entries.Where(e=>e.kind=="expense"&&Day(e.date).Year==month.Year&&Day(e.date).Month==month.Month&&(category==null||String.Equals(e.category,category,StringComparison.OrdinalIgnoreCase))).Sum(e=>e.cents);}
 internal static void AddEntry(Store store,BudgetEntry entry,DateTime today){
  if(Day(entry.date)>today.Date)throw new InvalidDataException("Saisissez une opération déjà passée. Pour une opération future, utilisez une prévision ponctuelle ou une récurrence.");
  Change(store,b=>{b.entries.RemoveAll(e=>e.id==entry.id);b.entries.Add(entry);});
 }
 internal static void Confirm(Store store,BudgetOccurrence occurrence,DateTime today){
  Reconcile(store,occurrence,today,today,occurrence.Rule.cents,null);
 }
 internal static void Reconcile(Store store,BudgetOccurrence occurrence,DateTime today,DateTime date,long cents,string existingId){
  if(occurrence.Date>today.Date&&occurrence.Rule.repeat!="once")throw new InvalidDataException("Confirmez cette opération le jour de son passage.");
  if(store.Value.budget.entries.Any(e=>e.occurrence==occurrence.Key))return;
  if(existingId!=null){Change(store,b=>{var entry=b.entries.FirstOrDefault(e=>e.id==existingId);if(entry==null||!String.IsNullOrEmpty(entry.occurrence)||entry.kind!=occurrence.Rule.kind)throw new InvalidDataException("Cette opération ne peut pas être associée.");entry.occurrence=occurrence.Key;});return;}
  AddEntry(store,new BudgetEntry{id=Guid.NewGuid().ToString(),title=occurrence.Rule.title,kind=occurrence.Rule.kind,category=occurrence.Rule.category,cents=cents,date=date.ToString("yyyy-MM-dd"),occurrence=occurrence.Key,notes=occurrence.Rule.notes??""},today);
 }
}
