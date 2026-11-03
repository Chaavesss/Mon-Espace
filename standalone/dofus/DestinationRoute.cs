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
 foreach(var pair in new[]{new[]{@"départ\s+de\s+(?:la\s+)?quête\s*:","Départ de quête"},new[]{@"objectif\s*:","Objectif"},new[]{@"PNJ\s*:","PNJ"},new[]{@"combat\s*:","Combat"},new[]{@"destination\s+intermédiaire\s*:","Destination intermédiaire"},new[]{@"fin\s+de\s+(?:la\s+)?quête\s*:","Fin de quête"}})
 if(Regex.IsMatch(instruction,@"^\s*(?:[•–-]\s*)?"+pair[0],RegexOptions.IgnoreCase))roles.Add(pair[1]);
 return roles.Count==1?roles[0]:"Rôle non précisé";
 }
 internal static List<RouteDestination> Build(string source,string fallback=""){
 var result=new List<RouteDestination>();source=source??"";string sourceKey=WorkspaceNext.Hash(Encoding.UTF8.GetBytes(source));string prefix="";
 foreach(string block in source.Replace("\r","").Split('\n')){
 var matches=Coordinates.Matches(block);if(matches.Count==0){if(result.Count==0)prefix+=block+"\n";else result[result.Count-1].Instruction+="\n"+block;continue;}
 foreach(Match m in matches){string pos=Int32.Parse(m.Groups[1].Value.Replace('−','-'))+","+Int32.Parse(m.Groups[2].Value.Replace('−','-'));result.Add(new RouteDestination{Coordinates=pos,Role=matches.Count==1?Role(block):"Rôle non précisé",Instruction=(prefix+block).Trim(),Key=WorkspaceNext.Hash(Encoding.UTF8.GetBytes(sourceKey+"/"+result.Count))});}
 prefix="";
 }
 if(result.Count==0&&!String.IsNullOrWhiteSpace(fallback)){
 var match=Coordinates.Match("["+fallback+"]");if(match.Success&&match.Length==fallback.Length+2)result.Add(new RouteDestination{Coordinates=Int32.Parse(match.Groups[1].Value.Replace('−','-'))+","+Int32.Parse(match.Groups[2].Value.Replace('−','-')),Role="Rôle non précisé",Instruction=(String.IsNullOrWhiteSpace(source)?"":source+"\n")+"Coordonnée de la page source ; rôle non précisé.",Key=WorkspaceNext.Hash(Encoding.UTF8.GetBytes("fallback/"+fallback))});
 }return result;
 }
 internal static string CursorKey(Data data,string context){var c=DofusCompanion.Character(data);return WorkspaceNext.Hash(Encoding.UTF8.GetBytes((c==null?"":c.id+"/"+c.guide)+"/"+context));}
 internal static int Selected(InstructionData data,string key,List<RouteDestination> route){string saved;return data.destinations.TryGetValue(key,out saved)?Math.Max(0,route.FindIndex(d=>d.Key==saved)):0;}
 internal static void Select(Store store,string key,RouteDestination destination,bool copy,Action<string> clipboard){string previous;bool changed=!store.Value.instructions.destinations.TryGetValue(key,out previous)||previous!=destination.Key;PersonalWorkflows.Change(store,()=>{var values=store.Value.instructions.destinations;if(!values.ContainsKey(key)&&values.Count>=5000)values.Remove(values.Keys.First());values[key]=destination.Key;});if(changed&&copy&&store.Value.instructions.autoCopy)clipboard(destination.Command);}
}
internal sealed partial class CompanionWindow {
 StackPanel destinationNavigation;
 string cachedRouteSource,cachedRouteFallback;List<RouteDestination> cachedRoute;
 List<RouteDestination> ReadRoute(string source,string fallback){if(cachedRoute!=null&&cachedRouteSource==source&&cachedRouteFallback==fallback)return cachedRoute;cachedRouteSource=source;cachedRouteFallback=fallback;return cachedRoute=DestinationRoute.Build(source,fallback);}

 double InstructionFontSize {get{return compactInstructions?13:14;}}
 TextBlock InstructionText(string value){var text=PositionText(value,InstructionFontSize);text.LineHeight=InstructionFontSize*1.35;text.Margin=new Thickness(0,2,0,3);return text;}
 RouteDestination displayedDestination;
 void CopyChangedDestination(RouteDestination before){if(testing||!autoCopyDestination||displayedDestination==null||before!=null&&before.Key==displayedDestination.Key)return;try{Clipboard.SetText(displayedDestination.Command);}catch(System.Runtime.InteropServices.ExternalException){message="Destination enregistrée. Presse-papiers occupé : utilisez Copier /travel.";RefreshView();}}
 internal void DestinationPanel(Panel parent,string source,string context,string fallback=""){
 var route=ReadRoute(source,fallback);if(route.Count==0)return;
 string cursor=DestinationRoute.CursorKey(store.Value,context);int selected=DestinationRoute.Selected(store.Value.instructions,cursor,route);bool expanded=false;string feedback="";
 var frame=CompanionTheme.Card(new StackPanel(),9);parent.Children.Add(frame);
 Action render=null;render=()=>{
 var current=route[selected];displayedDestination=current;var panel=new StackPanel();frame.Child=panel;
 panel.Children.Add(CompanionTheme.Label("ITINÉRAIRE · DESTINATION "+(selected+1)+" / "+route.Count));panel.Children.Add(CompanionTheme.Pair(Text(current.Role,12),PositionText("["+current.Coordinates+"]",compactInstructions?16:18)));panel.Children.Add(InstructionText(store.Value.instructions.companion?CompanionInstructions.Rewrite(current.Instruction):current.Instruction));
 Action<int> move=index=>{try{DestinationRoute.Select(store,cursor,route[index],!testing,value=>Clipboard.SetText(value));selected=index;feedback="";}catch(System.Runtime.InteropServices.ExternalException){selected=DestinationRoute.Selected(store.Value.instructions,cursor,route);feedback="Destination enregistrée. Presse-papiers occupé : utilisez Copier /travel.";}catch(Exception ex){feedback="Destination non enregistrée : "+ex.Message;}render();};
 var navigation=new WrapPanel();var previous=Button("← Destination",()=>move(selected-1));previous.IsEnabled=selected>0;navigation.Children.Add(previous);var next=Button("Destination →",()=>move(selected+1),true);next.IsEnabled=selected+1<route.Count;navigation.Children.Add(next);navigation.Children.Add(Button("Copier /travel",()=>Clipboard.SetText(current.Command)));
 if(destinationNavigation!=null){destinationNavigation.Children.Clear();destinationNavigation.Children.Add(Text("ITINÉRAIRE · SANS VALIDATION",10,true));destinationNavigation.Children.Add(navigation);}else panel.Children.Add(navigation);
 var auto=new CheckBox{Content="Copier /travel au changement de destination",IsChecked=autoCopyDestination,Margin=new Thickness(0,4,0,4),Foreground=Ink};auto.Click+=delegate{Safe(()=>PersonalWorkflows.Change(store,()=>store.Value.instructions.autoCopy=auto.IsChecked==true));};panel.Children.Add(auto);
 if(!compactInstructions){var list=new StackPanel();Action fill=()=>{if(list.Children.Count>0)return;for(int i=0;i<route.Count;i++){int target=i;list.Children.Add(Button((i==selected?"● ":"")+(i+1)+" · "+route[i].Role+" ["+route[i].Coordinates+"]",()=>move(target)));}};var fold=CompanionTheme.Fold("Toutes les destinations",list,expanded);fold.Expanded+=delegate{expanded=true;fill();};fold.Collapsed+=delegate{expanded=false;};if(expanded)fill();panel.Children.Add(fold);}
 if(feedback!="")panel.Children.Add(Text(feedback,11,true));
 };render();
 }
}
