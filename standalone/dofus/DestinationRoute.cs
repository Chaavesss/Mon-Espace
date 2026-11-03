using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
internal sealed class RouteDestination {
 internal string Coordinates,Role,Instruction,Key;
 internal string Command {get{return "/travel "+Coordinates;}}
}
internal static class DestinationRoute {
 static readonly Regex Coordinates=new Regex(@"\[\s*([+\-−]?\d{1,3})\s*,\s*([+\-−]?\d{1,3})\s*\]",RegexOptions.CultureInvariant);
 // Only explicit labels are accepted. Narrative verbs, sequence and page metadata
 // are insufficient evidence of a quest role.
 internal static string Role(string instruction){
 var roles=new List<string>();
 foreach(var pair in new[]{new[]{@"départ\s+de\s+(?:la\s+)?quête","Départ de quête"},new[]{@"objectif\s*:","Objectif"},new[]{@"PNJ\s*:","PNJ"},new[]{@"combat\s*:","Combat"},new[]{@"destination\s+intermédiaire\s*:","Destination intermédiaire"},new[]{@"fin\s+de\s+(?:la\s+)?quête","Fin de quête"}})
 if(Regex.IsMatch(instruction,@"^\s*(?:[•–-]\s*)?"+pair[0],RegexOptions.IgnoreCase))roles.Add(pair[1]);
 return roles.Count==1?roles[0]:"Rôle non précisé";
 }
 internal static List<RouteDestination> Build(string source,string fallback=""){
 var result=new List<RouteDestination>();source=source??"";
 foreach(string block in source.Replace("\r","").Split('\n')){
 var matches=Coordinates.Matches(block);
 foreach(Match m in matches){string pos=Int32.Parse(m.Groups[1].Value.Replace('−','-'))+","+Int32.Parse(m.Groups[2].Value.Replace('−','-'));result.Add(new RouteDestination{Coordinates=pos,Role=matches.Count==1?Role(block):"Rôle non précisé",Instruction=block.Trim(),Key=WorkspaceNext.Hash(Encoding.UTF8.GetBytes(source+"/"+result.Count))});}
 }
 if(result.Count==0&&!String.IsNullOrWhiteSpace(fallback)){
 var match=Coordinates.Match("["+fallback+"]");if(match.Success&&match.Length==fallback.Length+2)result.Add(new RouteDestination{Coordinates=Int32.Parse(match.Groups[1].Value.Replace('−','-'))+","+Int32.Parse(match.Groups[2].Value.Replace('−','-')),Role="Rôle non précisé",Instruction="Coordonnée de la page source ; rôle non précisé.",Key=WorkspaceNext.Hash(Encoding.UTF8.GetBytes("fallback/"+fallback))});
 }return result;
 }
 internal static string CursorKey(Data data,string context){var c=DofusCompanion.Character(data);return WorkspaceNext.Hash(Encoding.UTF8.GetBytes((c==null?"":c.id+"/"+c.guide)+"/"+context));}
 internal static int Selected(InstructionData data,string key,List<RouteDestination> route){string saved;return data.destinations.TryGetValue(key,out saved)?Math.Max(0,route.FindIndex(d=>d.Key==saved)):0;}
 internal static void Select(Store store,string key,RouteDestination destination,bool copy,Action<string> clipboard){string previous;bool changed=!store.Value.instructions.destinations.TryGetValue(key,out previous)||previous!=destination.Key;PersonalWorkflows.Change(store,()=>{var values=store.Value.instructions.destinations;if(!values.ContainsKey(key)&&values.Count>=5000)values.Remove(values.Keys.First());values[key]=destination.Key;});if(changed&&copy&&store.Value.instructions.autoCopy)clipboard(destination.Command);}
}
internal sealed partial class CompanionWindow {
 internal void DestinationPanel(Panel parent,string source,string context,string fallback=""){
 var route=DestinationRoute.Build(source,fallback);if(route.Count==0)return;
 string cursor=DestinationRoute.CursorKey(store.Value,context);int selected=DestinationRoute.Selected(store.Value.instructions,cursor,route);var current=route[selected];
 var panel=new StackPanel();panel.Children.Add(CompanionTheme.Label("ITINÉRAIRE · DESTINATION "+(selected+1)+" / "+route.Count));panel.Children.Add(Text(current.Role,14));panel.Children.Add(PositionText("["+current.Coordinates+"]",24));panel.Children.Add(PositionText(current.Instruction,17));
 var navigation=new WrapPanel();Action<int> move=index=>{try{DestinationRoute.Select(store,cursor,route[index],!testing,value=>Clipboard.SetText(value));message="";}catch(System.Runtime.InteropServices.ExternalException){message="Destination enregistrée. Presse-papiers occupé : utilisez Copier /travel.";}catch(Exception ex){message="Destination non enregistrée : "+ex.Message;}RefreshView();};
 var previous=Button("← Destination",()=>move(selected-1));previous.IsEnabled=selected>0;navigation.Children.Add(previous);var next=Button("Destination →",()=>move(selected+1));next.IsEnabled=selected+1<route.Count;navigation.Children.Add(next);navigation.Children.Add(Button("Copier /travel",()=>Clipboard.SetText(current.Command)));panel.Children.Add(navigation);
 var auto=new CheckBox{Content="Copier /travel au changement de destination",IsChecked=autoCopyDestination,Margin=new Thickness(0,8,0,8),Foreground=Ink};auto.Click+=delegate{Safe(()=>PersonalWorkflows.Change(store,()=>store.Value.instructions.autoCopy=auto.IsChecked==true));};panel.Children.Add(auto);
 if(!compactInstructions){var list=new StackPanel();for(int i=0;i<route.Count;i++){int target=i;list.Children.Add(Button((i==selected?"● ":"")+(i+1)+" · "+route[i].Role+" ["+route[i].Coordinates+"]",()=>move(target)));}panel.Children.Add(CompanionTheme.Fold("Toutes les destinations",list));}
 panel.Children.Add(Text("Naviguer ici ne valide aucune quête.",11,true));parent.Children.Add(CompanionTheme.Card(panel,14));
 }
}
