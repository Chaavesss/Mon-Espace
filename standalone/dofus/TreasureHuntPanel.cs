using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

internal sealed class TreasureHuntPanel:StackPanel {
 internal readonly TextBox X=new TextBox{Text="0",MaxLength=4},Y=new TextBox{Text="0",MaxLength=4},Search=new TextBox{MaxLength=100};
 internal readonly ListBox Results=new ListBox{Height=165,DisplayMemberPath="Display"};
 internal readonly TextBlock Status=CompanionWindow.Text("Choisissez la position et la direction.",11,true),Destination=CompanionWindow.Text("Sélectionnez un indice.",18);
 internal readonly Button Copy,Next;
 readonly List<Button> arrows=new List<Button>();
 readonly Func<int,int,int,CancellationToken,Task<List<TreasureClue>>> fetch;
 List<TreasureClue> clues=new List<TreasureClue>();
 CancellationTokenSource pending;int generation,direction=-1;bool updating;
 internal TreasureHuntPanel(Func<int,int,int,CancellationToken,Task<List<TreasureClue>>> loader=null){
  fetch=loader??TreasureHunt.Fetch;
  Children.Add(CompanionTheme.Heading("Chasse au trésor",25));
  var inputs=new StackPanel();inputs.Children.Add(CompanionTheme.Label("POSITION DE DÉPART"));
  var left=new StackPanel{Margin=new Thickness(0,0,6,0)};var right=new StackPanel{Margin=new Thickness(6,0,0,0)};
  CompanionWindow.Field(left,"X",X);CompanionWindow.Field(right,"Y",Y);inputs.Children.Add(CompanionTheme.Pair(left,right));
  var directions=new System.Windows.Controls.Primitives.UniformGrid{Columns=4};
  string[] labels={"← Ouest","↑ Nord","↓ Sud","→ Est"};int[] values={4,6,2,0};
  for(int i=0;i<4;i++){int d=values[i];var b=CompanionWindow.Button(labels[i],async()=>await Load(d));b.Padding=new Thickness(4,8,4,8);arrows.Add(b);directions.Children.Add(b);}
  inputs.Children.Add(directions);inputs.Children.Add(Status);Children.Add(CompanionTheme.Card(inputs));
  var list=new StackPanel();CompanionWindow.Field(list,"Rechercher un indice",Search);list.Children.Add(Results);Children.Add(CompanionTheme.Card(list));
  var arrival=new StackPanel();arrival.Children.Add(CompanionTheme.Label("ARRIVÉE"));arrival.Children.Add(Destination);
  Copy=CompanionWindow.Button("Copier /travel",()=>{var clue=Results.SelectedItem as TreasureClue;if(clue==null)return;try{Clipboard.SetText(clue.Travel);Status.Text="Commande copiée : "+clue.Travel;}catch(System.Runtime.InteropServices.ExternalException){Status.Text="Copie indisponible. Réessayez.";}},true);
  Next=CompanionWindow.Button("Repartir d’ici →",Advance);Copy.IsEnabled=Next.IsEnabled=false;
  arrival.Children.Add(Copy);arrival.Children.Add(Next);Children.Add(CompanionTheme.Card(arrival));
  Children.Add(CompanionWindow.Text("Phorreur : repérez-le en jeu puis saisissez sa position pour continuer.",11,true));
  Children.Add(CompanionWindow.Button("Données : DofusDB ↗",()=>System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://dofusdb.fr/fr/tools/treasure-hunt"){UseShellExecute=true})));
  X.TextChanged+=delegate{if(!updating)Invalidate();};Y.TextChanged+=delegate{if(!updating)Invalidate();};
  Search.TextChanged+=delegate{Filter();};Results.SelectionChanged+=delegate{var clue=Results.SelectedItem as TreasureClue;Copy.IsEnabled=Next.IsEnabled=clue!=null;Destination.Text=clue==null?"Sélectionnez un indice.":"["+clue.X+", "+clue.Y+"] · "+clue.Distance+" carte(s)";};
 }
 internal void Cancel(){generation++;if(pending!=null)pending.Cancel();}
 void Invalidate(){Cancel();clues.Clear();Filter();direction=-1;PaintArrows();Status.Text="Choisissez une direction pour cette position.";}
 void PaintArrows(){int[] values={4,6,2,0};for(int i=0;i<4;i++){arrows[i].BorderBrush=values[i]==direction?CompanionTheme.Gold:CompanionTheme.Line;arrows[i].Foreground=values[i]==direction?CompanionTheme.Gold:CompanionTheme.Ink;}}
 void Filter(){var selected=Results.SelectedItem as TreasureClue;Results.ItemsSource=clues.Where(c=>TreasureHunt.Matches(c.Name,Search.Text)).ToList();if(selected!=null&&((List<TreasureClue>)Results.ItemsSource).Contains(selected))Results.SelectedItem=selected;}
 internal void Advance(){var clue=Results.SelectedItem as TreasureClue;if(clue==null)return;updating=true;X.Text=clue.X.ToString();Y.Text=clue.Y.ToString();updating=false;Search.Text="";Invalidate();}
 internal async Task Load(int d){
  Cancel();int request=generation;int x,y;
  clues.Clear();Filter();
  if(!Int32.TryParse(X.Text,out x)||!Int32.TryParse(Y.Text,out y)||!TreasureHunt.Valid(x,y,d)){Status.Text="Saisissez X et Y entre -200 et 200.";return;}
  direction=d;PaintArrows();Status.Text="Recherche des indices…";
  var cts=new CancellationTokenSource();cts.CancelAfter(TimeSpan.FromSeconds(20));pending=cts;
  try{var found=await fetch(x,y,d,cts.Token);if(request!=generation)return;clues=found;Filter();Status.Text=clues.Count==0?"Aucun indice trouvé. Vérifiez la position et la direction.":clues.Count+" indices · choisissez celui indiqué en jeu.";}
  catch(OperationCanceledException){if(request==generation)Status.Text="Recherche trop longue. Réessayez.";}
  catch(Exception ex){if(request==generation)Status.Text=ex is IOException?ex.Message:"DofusDB est inaccessible. Réessayez ou ouvrez le site.";}
  finally{if(pending==cts)pending=null;cts.Dispose();}
 }
}
internal sealed partial class CompanionWindow {
 bool treasureTab;internal TreasureHuntPanel treasurePanel;
 internal void ShowTreasure(){treasureTab=true;RefreshView();}
 void AddTreasure(Panel body){if(treasurePanel==null)treasurePanel=new TreasureHuntPanel();var parent=treasurePanel.Parent as Panel;if(parent!=null)parent.Children.Remove(treasurePanel);body.Children.Add(treasurePanel);}
}
