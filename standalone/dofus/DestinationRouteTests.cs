using System;
using System.IO;
using System.Linq;
internal static class DestinationRouteTests {
 internal static void Run(string folder){
 var route=DestinationRoute.Build("Départ de quête : [0,-1]\nObjectif : [+2,−3]\nPNJ : [2,4]\nCombat : [4,5]\nDestination intermédiaire : [6,7]\nFin de quête : [0,-1]");
 if(route.Count!=6||route.Select(d=>d.Role).Distinct().Count()!=6||route[1].Command!="/travel 2,-3"||route[0].Key==route[5].Key)throw new Exception("Itinéraire : rôles, signes ou retour sur une position");
 if(DestinationRoute.Build("Va en [1,2] puis [3,4].").Any(d=>d.Role!="Rôle non précisé")||DestinationRoute.Build("PNJ : [1,2] puis combat : [3,4]").Any(d=>d.Role!="Rôle non précisé"))throw new Exception("Rôle ambigu inventé");
 if(DestinationRoute.Build("Aucune position","0,-2").Single().Role!="Rôle non précisé"||DestinationRoute.Build("[1000,2]").Count!=0||DestinationRoute.Build(null).Count!=0)throw new Exception("Position absente ou invalide");
 if(!DestinationRoute.Build("Prévoir 3 objets.\nPNJ : [1,2]\nParlez au gardien.\nObjectif : [2,3]")[0].Instruction.Contains("Parlez au gardien.")||DestinationRoute.Build("Ce n’est pas le départ de quête [1,2]")[0].Role!="Rôle non précisé")throw new Exception("Contexte perdu ou rôle narratif inventé");
 var store=new Store(Path.Combine(folder,"route-data"));store.Value.dofus.characters.Add(new DofusCharacter{id="route-a",name="Route A",guide="example"});store.Value.dofus.active="route-a";string key=DestinationRoute.CursorKey(store.Value,"step");string copied="";
 DestinationRoute.Select(store,key,route[1],true,s=>copied=s);if(copied!="")throw new Exception("Copie active par défaut");
 PersonalWorkflows.Change(store,()=>{store.Value.instructions.autoCopy=true;store.Value.instructions.compact=true;});DestinationRoute.Select(store,key,route[2],true,s=>copied=s);if(copied!="/travel 2,4")throw new Exception("Copie automatique");
 var restart=new Store(store.Root);if(!restart.Value.instructions.compact||!restart.Value.instructions.autoCopy||DestinationRoute.Selected(restart.Value.instructions,key,route)!=2||restart.Value.completedQuests.Count!=0||restart.Value.dofus.progress.Count!=0)throw new Exception("Préférences perdues ou validation accidentelle");
 store.Value.dofus.active="other";if(DestinationRoute.CursorKey(store.Value,"step")==key)throw new Exception("Itinéraire partagé entre personnages");
 bool failed=false;try{DestinationRoute.Select(restart,key,route[3],true,s=>{throw new System.Runtime.InteropServices.ExternalException();});}catch(System.Runtime.InteropServices.ExternalException){failed=true;}if(!failed||DestinationRoute.Selected(new Store(store.Root).Value.instructions,key,route)!=3)throw new Exception("Presse-papiers : destination perdue");
 Ui(folder);
 File.WriteAllText(Path.Combine(folder,"DESTINATION_ROUTE_PASS.txt"),"PASS: six explicit roles; ambiguous roles unknown; coordinates, repeated destinations and missing coordinates; opt-in copy; persisted compact mode and route cursor; clipboard failure; per-character isolation; navigation never completes quests.");
 }

 static void Ui(string folder){
 var store=new Store(Path.Combine(folder,"route-ui"));var c=new DofusCharacter{id="route-ui",name="Route",guide="ganymede-99"};store.Value.dofus.characters.Add(c);store.Value.dofus.active=c.id;store.Save();
 string raw=Store.Json.Serialize(new{ganymedeGuide=new{id=99,name="Route test",author="Fixture",description="Fixture",steps=new[]{new{id=1,name="Itinéraire",order=1,map="",pos_x=0,pos_y=0,text="<p>Départ de quête : [0,-1]</p><p>Objectif : [2,3]</p>"}}}});TougliReader.Cache(store,c.guide,raw);
 PersonalWorkflows.Change(store,()=>{store.Value.dofus.width=430;store.Value.dofus.height=640;store.Value.dofus.positionSaved=true;});store=new Store(store.Root);var window=new CompanionWindow(store,true);if(window.Width!=430||window.Height!=640)throw new Exception("Dimensions sauvegardées ignorées");window.Render(Path.Combine(folder,"overlay-route-detailed.png"));
 var next=StandaloneTests.Find<System.Windows.Controls.Button>(window.Content as System.Windows.DependencyObject).First(b=>Equals(b.Content,"Destination →"));next.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
 window.Render(Path.Combine(folder,"overlay-route-next.png"));if(!StandaloneTests.Find<System.Windows.Controls.TextBlock>(window.Content as System.Windows.DependencyObject).Any(t=>t.Text=="Objectif")||store.Value.completedQuests.Count!=0||store.Value.dofus.progress.Any(p=>p.done.Count>0))throw new Exception("Bouton destination valide une quête ou ne navigue pas");
 StandaloneTests.Find<System.Windows.Controls.Button>(window.Content as System.Windows.DependencyObject).First(b=>Equals(b.Content,"Compact")).RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));window.Render(Path.Combine(folder,"overlay-route-compact.png"));if(!new Store(store.Root).Value.instructions.compact||StandaloneTests.Find<System.Windows.Controls.Expander>(window.Content as System.Windows.DependencyObject).Any(e=>Equals(e.Header,"Toutes les destinations")))throw new Exception("Mode compact réel ou sauvegarde");window.Close();
 }
}
