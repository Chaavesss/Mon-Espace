using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
internal sealed class GoogleCalendarInfo {public string id="",summary="",accessRole="reader";public bool primary;}
internal sealed class GoogleCalendarList {public List<GoogleCalendarInfo> items=new List<GoogleCalendarInfo>();public string nextPageToken="";}
internal sealed class GoogleEventTime {public string date="",dateTime="",timeZone="";}
internal sealed class GoogleCalendarEvent {public string id="",summary="",description="",status="",etag="",recurringEventId="";public GoogleEventTime start=new GoogleEventTime(),end=new GoogleEventTime();}
internal sealed class GoogleEventList {public List<GoogleCalendarEvent> items=new List<GoogleCalendarEvent>();public string nextPageToken="";}
internal sealed class GoogleCalendarClient {
 readonly Func<string,string,object,string,CancellationToken,Task<string>> send;static readonly JavaScriptSerializer Json=new JavaScriptSerializer{MaxJsonLength=4*1024*1024};
 internal GoogleCalendarClient(Func<string,string,object,string,CancellationToken,Task<string>> transport){send=transport;}
 internal static bool CanWrite(GoogleCalendarInfo calendar){return calendar!=null&&(calendar.accessRole=="owner"||calendar.accessRole=="writer");}
 internal static DateTime LocalStart(GoogleCalendarEvent item){DateTime date;if(!String.IsNullOrEmpty(item.start.date)&&DateTime.TryParseExact(item.start.date,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out date))return date;DateTimeOffset instant;if(DateTimeOffset.TryParse(item.start.dateTime,CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind,out instant))return instant.LocalDateTime;throw new InvalidOperationException("Date d’événement Google invalide.");}
 static string Base(string calendar){return "calendar/v3/calendars/"+Uri.EscapeDataString(calendar)+"/events";}
 internal async Task<List<GoogleCalendarInfo>> Calendars(CancellationToken token){var all=new List<GoogleCalendarInfo>();string next="";do{var page=Json.Deserialize<GoogleCalendarList>(await send("calendar/v3/users/me/calendarList?maxResults=100&pageToken="+Uri.EscapeDataString(next),"GET",null,"",token));all.AddRange(page.items??new List<GoogleCalendarInfo>());next=page.nextPageToken??"";if(all.Count>2000)throw new InvalidOperationException("Trop d’agendas.");}while(next!="");return all;}
 internal async Task<List<GoogleCalendarEvent>> Events(string calendar,DateTime from,DateTime until,CancellationToken token){
  var all=new List<GoogleCalendarEvent>();string next="";do{var path=Base(calendar)+"?singleEvents=true&orderBy=startTime&maxResults=250&timeMin="+Uri.EscapeDataString(new DateTimeOffset(from).ToString("o"))+"&timeMax="+Uri.EscapeDataString(new DateTimeOffset(until).ToString("o"))+"&pageToken="+Uri.EscapeDataString(next);var page=Json.Deserialize<GoogleEventList>(await send(path,"GET",null,"",token));all.AddRange((page.items??new List<GoogleCalendarEvent>()).Where(e=>e.status!="cancelled"));next=page.nextPageToken??"";if(all.Count>5000)throw new InvalidOperationException("Trop d’événements à afficher pour cette période.");}while(next!="");return all.OrderBy(LocalStart).ToList();
 }
 internal async Task Save(string calendar,GoogleCalendarEvent old,string title,string notes,DateTime start,DateTime end,bool allDay,CancellationToken token){
  if(String.IsNullOrWhiteSpace(title)||end<=start)throw new InvalidOperationException("Vérifiez le titre et la fin de l’événement.");if(!allDay&&(TimeZoneInfo.Local.IsInvalidTime(start)||TimeZoneInfo.Local.IsInvalidTime(end)))throw new InvalidOperationException("Cette heure n’existe pas lors du passage à l’heure d’été.");
  var payload=new Dictionary<string,object>{{"summary",title},{"description",notes??""},{"start",allDay?(object)new{date=start.ToString("yyyy-MM-dd")}:new{dateTime=new DateTimeOffset(start).ToString("o")}},{"end",allDay?(object)new{date=end.ToString("yyyy-MM-dd")}:new{dateTime=new DateTimeOffset(end).ToString("o")}}};
  await send(Base(calendar)+(old==null?"":"/"+Uri.EscapeDataString(old.id))+"?sendUpdates=none",old==null?"POST":"PATCH",payload,old==null?"":old.etag,token);
 }
 internal Task Delete(string calendar,GoogleCalendarEvent item,CancellationToken token){return send(Base(calendar)+"/"+Uri.EscapeDataString(item.id)+"?sendUpdates=none","DELETE",null,item.etag,token);}
}
