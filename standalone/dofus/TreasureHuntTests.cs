using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
internal static class TreasureHuntTests {
 static void Check(bool value,string reason){if(!value)throw new Exception("Chasse : "+reason);}
 internal static void Run(string folder){
  var poi=new TreasureHunt.Poi{id=1,name=new TreasureHunt.Names{fr="Théière à rayures"}};
  var maps=new[]{new TreasureHunt.Map{posX=-20,posY=-36,pois=new[]{poi}},new TreasureHunt.Map{posX=-23,posY=-36,pois=new[]{poi}},new TreasureHunt.Map{posX=-26,posY=-36,pois=new[]{poi}},new TreasureHunt.Map{posX=-25,posY=-35,pois=new[]{poi}},new TreasureHunt.Map{posX=-25,posY=-37,pois=new[]{poi}}};
  var found=TreasureHunt.Select(maps,-25,-36,0);Check(found.Count==1&&found[0].X==-23&&found[0].Distance==2,"indice le plus proche à l’est");
  Check(TreasureHunt.Select(maps,-25,-36,4).Single().X==-26,"ouest");Check(TreasureHunt.Select(maps,-25,-36,6).Single().Y==-37,"nord");Check(TreasureHunt.Select(maps,-25,-36,2).Single().Y==-35,"sud");
  Check(TreasureHunt.Matches(found[0].Name,"theiere")&&found[0].Travel=="/travel -23 -36","accents et commande");
  Check(found[0].ToString()==found[0].Name,"libellé du sélecteur");
  var page=TreasureHunt.Parse("{\"total\":1,\"skip\":0,\"data\":[{\"posX\":-23,\"posY\":-36,\"pois\":[{\"id\":1,\"name\":{\"fr\":\"Théière\"}}]}]}");Check(TreasureHunt.Select(page.data,-25,-36,0).Count==1,"réponse JSON réelle");
  Check(!TreasureHunt.Valid(Int32.MinValue,0,0),"coordonnées invalides");
  int copies=0;string copied="";Action<string> clipboard=value=>{copies++;copied=value;};int requestedX=0,requestedY=0,requestedDirection=-1;var panel=new TreasureHuntPanel((x,y,d,ct)=>{requestedX=x;requestedY=y;requestedDirection=d;return Task.FromResult(found);},clipboard);panel.X.Text="-25";panel.Y.Text="-36";panel.Load(0).GetAwaiter().GetResult();panel.Search.Text="theiere";Check(panel.Results.Items.Count==1,"filtre interface");panel.Results.SelectedIndex=0;Check(panel.Copy.IsEnabled&&panel.Destination.Text.Contains("-23"),"destination sélectionnée");Check(panel.X.Text=="-23"&&panel.Y.Text=="-36"&&panel.Copy.IsEnabled,"position actualisée dès sélection sans effacer la copie");Check(copies==1&&copied=="/travel -23 -36","copie automatique exacte sans clic");panel.Search.Text="thé";Check(copies==1,"filtre ne réécrit pas le presse-papiers");panel.Copy.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Check(copies==2,"recopie manuelle");panel.Load(6).GetAwaiter().GetResult();Check(requestedX==-23&&requestedY==-36&&requestedDirection==6&&panel.Search.Text==""&&!panel.Copy.IsEnabled,"prochaine recherche depuis arrivée et filtre réinitialisé");panel.Results.SelectedIndex=0;panel.X.Text="3";Check(!panel.Copy.IsEnabled&&panel.Results.Items.Count==0,"correction manuelle invalide les anciens résultats");
  var locked=new TreasureHuntPanel((x,y,d,ct)=>Task.FromResult(found),value=>{throw new System.Runtime.InteropServices.ExternalException();});locked.Load(0).GetAwaiter().GetResult();locked.Results.SelectedIndex=0;Check(locked.Copy.IsEnabled&&locked.X.Text=="-23"&&locked.Status.Text.Contains("indisponible"),"presse-papiers occupé sans blocage");
  var pending=new TaskCompletionSource<System.Collections.Generic.List<TreasureClue>>();var stale=new TreasureHuntPanel((x,y,d,ct)=>pending.Task);var operation=stale.Load(0);stale.X.Text="2";pending.SetResult(found);
  System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(new Action(()=>{}),System.Windows.Threading.DispatcherPriority.ApplicationIdle);
  Check(stale.Results.Items.Count==0&&!stale.Copy.IsEnabled,"réponse ancienne ignorée");
  var window=new CompanionWindow(new Store(Path.Combine(folder,"treasure-isolated")),true);window.treasurePanel=new TreasureHuntPanel((x,y,d,ct)=>Task.FromResult(found),clipboard);window.ShowTreasure();
  // Inject deterministic fixture data; screenshots never depend on network availability.
  window.treasurePanel.X.Text="-25";window.treasurePanel.Y.Text="-36";
  window.treasurePanel.Load(0).GetAwaiter().GetResult();window.treasurePanel.Results.SelectedIndex=0;window.RefreshView();Check(window.treasurePanel.Copy.IsEnabled,"état conservé après rafraîchissement");window.Render(Path.Combine(folder,"treasure-hunt.png"));window.Close();
  File.WriteAllText(Path.Combine(folder,"TREASURE_PASS.txt"),"PASS: directions, closest clue, JSON, accent search, next position, stale response and overlay rendering.");
 }
}
