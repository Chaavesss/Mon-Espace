using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

internal sealed class TreasureClue {
 public int Id, X, Y, Distance;
 public string Name;
 public string Display { get { return Name; } }
 internal string Travel { get { return "/travel "+X+" "+Y; } }
}
internal static class TreasureHunt {
 internal sealed class Page { public int total, skip; public Map[] data; }
 internal sealed class Map { public int posX,posY; public Poi[] pois; }
 internal sealed class Poi { public int id; public Names name; }
 internal sealed class Names { public string fr; }
 internal static bool Valid(int x,int y,int direction){return Math.Abs((long)x)<=200&&Math.Abs((long)y)<=200&&(direction==0||direction==2||direction==4||direction==6);}
 internal static List<TreasureClue> Select(IEnumerable<Map> maps,int x,int y,int direction){
  if(!Valid(x,y,direction))throw new InvalidDataException("Coordonnées ou direction invalides.");
  var clues=new Dictionary<int,TreasureClue>();
  foreach(var map in maps){
   int dx=map.posX-x,dy=map.posY-y;
   bool aligned=direction==0?dy==0&&dx>0:direction==4?dy==0&&dx<0:direction==2?dx==0&&dy>0:dx==0&&dy<0;
   int distance=Math.Abs(dx)+Math.Abs(dy);if(!aligned||distance>10||map.pois==null)continue;
   foreach(var poi in map.pois){
    if(poi==null||poi.name==null||String.IsNullOrWhiteSpace(poi.name.fr))continue;
    TreasureClue old;if(clues.TryGetValue(poi.id,out old)&&old.Distance<=distance)continue;
    clues[poi.id]=new TreasureClue{Id=poi.id,Name=poi.name.fr,X=map.posX,Y=map.posY,Distance=distance};
   }
  }
  return clues.Values.OrderBy(c=>c.Name,StringComparer.CurrentCultureIgnoreCase).ToList();
 }
 internal static Page Parse(string json){var page=new JavaScriptSerializer{MaxJsonLength=2*1024*1024}.Deserialize<Page>(json);if(page==null||page.data==null||page.total<0||page.skip<0)throw new InvalidDataException("Réponse DofusDB invalide.");return page;}
 internal static bool Matches(string name,string query){return CultureInfo.GetCultureInfo("fr-FR").CompareInfo.IndexOf(name,query.Trim(),CompareOptions.IgnoreCase|CompareOptions.IgnoreNonSpace)>=0;}
 internal static async Task<List<TreasureClue>> Fetch(int x,int y,int direction,CancellationToken token){
  if(!Valid(x,y,direction))throw new InvalidDataException("Coordonnées ou direction invalides.");
  ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
  using(var http=new HttpClient(new HttpClientHandler{AllowAutoRedirect=false}){Timeout=TimeSpan.FromSeconds(15)}){
   http.DefaultRequestHeaders.UserAgent.ParseAdd("CompagnonDofus/1.8.5");
   var maps=new List<Map>();int skip=0;
   for(int attempt=0;attempt<10;attempt++){
    string url="https://api.dofusdb.fr/treasure-hunt?x="+x+"&y="+y+"&direction="+direction+"&$limit=50&$skip="+skip;
    using(var response=await http.GetAsync(url,HttpCompletionOption.ResponseHeadersRead,token).ConfigureAwait(false)){
     if((int)response.StatusCode==403||(int)response.StatusCode==503)throw new IOException("DofusDB demande une vérification ou est indisponible. Utilisez le bouton DofusDB.");
     if((int)response.StatusCode==429)throw new IOException("Trop de recherches. Patientez avant de réessayer.");
     if(response.StatusCode==HttpStatusCode.NotFound)return new List<TreasureClue>();
     response.EnsureSuccessStatusCode();
     if(response.Content.Headers.ContentLength>2*1024*1024)throw new InvalidDataException("Réponse trop volumineuse.");
     using(var input=await response.Content.ReadAsStreamAsync().ConfigureAwait(false))using(var output=new MemoryStream()){
      byte[] buffer=new byte[8192];int count;
      while((count=await input.ReadAsync(buffer,0,buffer.Length,token).ConfigureAwait(false))>0){if(output.Length+count>2*1024*1024)throw new InvalidDataException("Réponse trop volumineuse.");output.Write(buffer,0,count);}
      var page=Parse(Encoding.UTF8.GetString(output.ToArray()));
      if(page.skip!=skip)throw new InvalidDataException("Pagination DofusDB invalide.");
      maps.AddRange(page.data);skip+=page.data.Length;
      if(skip>=page.total)return Select(maps,x,y,direction);
      if(page.data.Length==0)throw new InvalidDataException("Résultats DofusDB incomplets.");
     }
    }
   }
   throw new InvalidDataException("Trop de résultats DofusDB. Précisez votre position.");
  }
 }
}
