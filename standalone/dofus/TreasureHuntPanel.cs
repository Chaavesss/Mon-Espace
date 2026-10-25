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
 internal readonly ComboBox Results=new ComboBox{MinHeight=36,MaxDropDownHeight=220,DisplayMemberPath="Display"};
 internal readonly TextBlock Status=CompanionWindow.Text("Choisissez la position et la direction.",11,true),Destination=CompanionWindow.Text("Sélectionnez un indice.",18);
 internal readonly Button Copy;
 readonly Action<string> writeClipboard;
 internal readonly List<Button> arrows=new List<Button>();
 readonly Func<int,int,int,CancellationToken,Task<List<TreasureClue>>> fetch;
 List<TreasureClue> clues=new List<TreasureClue>();
 CancellationTokenSource pending;int generation,direction=-1;bool updating,filtering;
 internal TreasureHuntPanel(Func<int,int,int,CancellationToken,Task<List<TreasureClue>>> loader=null,Action<string> clipboard=null){
  fetch=loader??TreasureHunt.Fetch;writeClipboard=clipboard??new Action<string>(Clipboard.SetText);
  Children.Add(CompanionTheme.Heading("Chasse au trésor",22));
  var inputs=new StackPanel();inputs.Children.Add(CompanionTheme.Label("POSITION ACTUELLE"));
  var left=new StackPanel{Margin=new Thickness(0,0,6,0)};var right=new StackPanel{Margin=new Thickness(6,0,0,0)};
  CompanionWindow.Field(left,"X",X);CompanionWindow.Field(right,"Y",Y);
  var coordinates=CompanionTheme.Pair(left,right);coordinates.VerticalAlignment=VerticalAlignment.Center;
  var directions=new Grid{HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(8,0,0,0)};
  for(int i=0;i<3;i++){directions.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(38)});directions.RowDefinitions.Add(new RowDefinition{Height=new GridLength(34)});}
  string[] labels={"←","↑","↓","→"},names={"Gauche","Haut","Bas","Droite"};int[] values={4,6,2,0},rows={1,0,2,1},columns={0,1,1,2};
  for(int i=0;i<4;i++){int d=values[i];var b=CompanionWindow.Button(labels[i],async()=>await Load(d));b.Padding=new Thickness(0);b.Margin=new Thickness(2);b.Width=34;b.Height=30;b.MinHeight=30;b.FontSize=22;b.ToolTip=names[i];System.Windows.Automation.AutomationProperties.SetName(b,names[i]);Grid.SetRow(b,rows[i]);Grid.SetColumn(b,columns[i]);arrows.Add(b);directions.Children.Add(b);}
  var position=new Grid();position.ColumnDefinitions.Add(new ColumnDefinition());position.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});position.Children.Add(coordinates);Grid.SetColumn(directions,1);position.Children.Add(directions);inputs.Children.Add(position);
  inputs.Children.Add(Status);Children.Add(CompanionTheme.Card(inputs,12));
  var list=new StackPanel();CompanionWindow.Field(list,"Rechercher un indice",Search);list.Children.Add(Results);Children.Add(CompanionTheme.Card(list,12));
  var arrival=new StackPanel();arrival.Children.Add(CompanionTheme.Label("ARRIVÉE"));arrival.Children.Add(Destination);
  Copy=CompanionWindow.Button("Copier /travel",()=>{var clue=Results.SelectedItem as TreasureClue;if(clue==null)return;CopyClue(clue);},true);
  Copy.IsEnabled=false;
  arrival.Children.Add(Copy);Children.Add(CompanionTheme.Card(arrival,12));
  Children.Add(CompanionWindow.Text("Phorreur : repérez-le en jeu puis saisissez sa position pour continuer.",11,true));
  Children.Add(CompanionWindow.Button("Données : DofusDB ↗",()=>System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://dofusdb.fr/fr/tools/treasure-hunt"){UseShellExecute=true})));
  X.TextChanged+=delegate{if(!updating)Invalidate();};Y.TextChanged+=delegate{if(!updating)Invalidate();};
  Search.TextChanged+=delegate{Filter();};Results.SelectionChanged+=delegate{var clue=Results.SelectedItem as TreasureClue;Copy.IsEnabled=clue!=null;Destination.Text=clue==null?"Sélectionnez un indice.":"["+clue.X+", "+clue.Y+"] · "+clue.Distance+" carte(s)";if(clue!=null&&!filtering){updating=true;try{X.Text=clue.X.ToString();Y.Text=clue.Y.ToString();}finally{updating=false;}CopyClue(clue);}};
 }
 void CopyClue(TreasureClue clue){try{writeClipboard(clue.Travel);Status.Text="Commande copiée : "+clue.Travel;}catch(System.Runtime.InteropServices.ExternalException){Status.Text="Position actualisée · copie indisponible. Cliquez sur Copier /travel pour réessayer.";}}
 internal void Cancel(){generation++;if(pending!=null)pending.Cancel();}
 void Invalidate(){Cancel();clues.Clear();Filter();direction=-1;PaintArrows();Status.Text="Choisissez une direction pour cette position.";}
 void PaintArrows(){int[] values={4,6,2,0};for(int i=0;i<4;i++){arrows[i].BorderBrush=values[i]==direction?CompanionTheme.Gold:CompanionTheme.Line;arrows[i].Foreground=values[i]==direction?CompanionTheme.Gold:CompanionTheme.Ink;}}
 void Filter(){filtering=true;try{var selected=Results.SelectedItem as TreasureClue;Results.ItemsSource=clues.Where(c=>TreasureHunt.Matches(c.Name,Search.Text)).ToList();if(selected!=null&&((List<TreasureClue>)Results.ItemsSource).Contains(selected))Results.SelectedItem=selected;}finally{filtering=false;}}
 internal async Task Load(int d){
  Cancel();int request=generation;int x,y;
  clues.Clear();Search.Text="";Filter();
  if(!Int32.TryParse(X.Text,out x)||!Int32.TryParse(Y.Text,out y)||!TreasureHunt.Valid(x,y,d)){Status.Text="Saisissez X et Y entre -200 et 200.";return;}
  direction=d;PaintArrows();Status.Text="Recherche des indices…";
  var cts=new CancellationTokenSource();cts.CancelAfter(TimeSpan.FromSeconds(20));pending=cts;
  try{var found=await fetch(x,y,d,cts.Token);if(request!=generation)return;clues=found.ToList();Filter();Status.Text=clues.Count==0?"Aucun indice trouvé. Vérifiez la position et la direction.":clues.Count+" indices · choisissez celui indiqué en jeu.";}
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
