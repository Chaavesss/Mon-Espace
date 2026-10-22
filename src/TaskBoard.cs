using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

internal static class TaskBoard {
 internal static bool Move(Store store,string id,string state){
  if(!new[]{"todo","doing","done"}.Contains(state))return false;
  var task=store.Value.tasks.FirstOrDefault(t=>t.id==id);
  if(task==null||task.status==state)return false;
  var json=new JavaScriptSerializer{MaxJsonLength=8*1024*1024};
  string before=json.Serialize(store.Value);
  try{if(state=="done")Schedule.Complete(store.Value,task);else task.status=state;store.Save();return true;}
  catch{store.Value=json.Deserialize<Data>(before);throw;}
 }
}
internal sealed partial class ModernWorkspace {
 const string BoardFormat="MonEspace.TaskBoard";
 bool boardList=false;readonly string boardSession=Guid.NewGuid().ToString("N");
 void Tasks(StackPanel root){
  var tools=new WrapPanel{Margin=new Thickness(0,0,0,12)};
  tools.Children.Add(Btn("Tableau",delegate{boardList=false;Refresh();},!boardList));
  tools.Children.Add(Btn("Liste",delegate{boardList=true;Refresh();},boardList));
  tools.Children.Add(Btn("+ Nouvelle tâche",delegate{owner.Edit(null);}));
  tools.Children.Add(Btn("Suppressions récentes",delegate{Go("Suppressions récentes");},false,true));
  root.Children.Add(tools);
  var filters=new WrapPanel{Margin=new Thickness(0,0,0,12)};
  var search=new TextBox{Text=query,Width=210,Margin=new Thickness(0,0,8,8),ToolTip="Rechercher dans les titres"};
  var project=Combo(new[]{"Tous les projets"}.Concat(owner.AllProjects()),projectFilter);project.Width=180;project.Margin=new Thickness(0,0,8,8);
  Action filter=delegate{query=search.Text;projectFilter=Convert.ToString(project.SelectedItem);Refresh();};
  search.KeyDown+=delegate(object sender,KeyEventArgs e){if(e.Key==Key.Enter){filter();e.Handled=true;}};
  filters.Children.Add(search);filters.Children.Add(project);filters.Children.Add(Btn("Filtrer",filter));root.Children.Add(filters);
  var tasks=store.Value.tasks.Where(t=>(projectFilter=="Tous les projets"||t.project==projectFilter)&&(String.IsNullOrWhiteSpace(query)||t.title.IndexOf(query.Trim(),StringComparison.CurrentCultureIgnoreCase)>=0)).ToList();
  root.Children.Add(T(tasks.Count+" tâche(s) • "+tasks.Count(t=>t.status=="doing")+" en cours",11,Muted,new Thickness(0,0,0,8)));
  root.Children.Add(T(boardList?"Modifiez une tâche en cliquant sur son titre.":"Glissez une carte vers une colonne, ou utilisez son bouton Déplacer.",11,Muted,new Thickness(0,0,0,18)));
  var grid=new Grid();for(int i=0;i<3;i++)grid.ColumnDefinitions.Add(new ColumnDefinition());
  string[] states={"todo","doing","done"},labels={"À faire","En cours","Terminé"};
  for(int i=0;i<3;i++){
   string state=states[i];var items=tasks.Where(t=>t.status==state).OrderBy(t=>t.due??"").ThenBy(t=>t.title).ToList();
   if(boardList){var list=Card(labels[i]+" · "+items.Count,TaskList(items,0));list.Margin=new Thickness(0,0,i==2?0:12,0);list.VerticalAlignment=VerticalAlignment.Top;Grid.SetColumn(list,i);grid.Children.Add(list);continue;}
   var body=new StackPanel();body.Children.Add(T(labels[i].ToUpperInvariant()+"  "+items.Count,11,Accent,new Thickness(0,0,0,16)));
   foreach(var task in items)body.Children.Add(BoardCard(task));
   if(items.Count==0)body.Children.Add(T("Déposez une tâche ici.",12,Muted,new Thickness(4,20,4,24)));
   var column=Box(body,new Thickness(14));column.MinHeight=300;column.Background=System.Windows.Media.Brushes.Transparent;column.VerticalAlignment=VerticalAlignment.Top;column.Margin=new Thickness(0,0,i==2?0:12,0);column.AllowDrop=true;column.Tag=state;
   column.DragOver+=delegate(object sender,DragEventArgs e){e.Effects=BoardDragId(e.Data)==null?DragDropEffects.None:DragDropEffects.Move;column.BorderBrush=e.Effects==DragDropEffects.Move?Accent:Line;e.Handled=true;};
   column.DragLeave+=delegate{column.BorderBrush=Line;};
   column.Drop+=delegate(object sender,DragEventArgs e){column.BorderBrush=Line;string id=BoardDragId(e.Data);e.Handled=true;if(id!=null)MoveBoardTask(id,state);};
   Grid.SetColumn(column,i);grid.Children.Add(column);
  }root.Children.Add(grid);
 }
 string BoardDragId(IDataObject data){if(!data.GetDataPresent(BoardFormat))return null;string value=data.GetData(BoardFormat) as string;string prefix=boardSession+"/";return value!=null&&value.StartsWith(prefix,StringComparison.Ordinal)?value.Substring(prefix.Length):null;}
 Border BoardCard(WorkTask task){
  var body=new StackPanel();var title=Btn(task.title,delegate{owner.Edit(task);},false,true);title.Padding=new Thickness(0,2,0,7);title.HorizontalContentAlignment=HorizontalAlignment.Left;title.Content=T(task.title,13,task.status=="done"?Muted:Ink);PinMenu(title,"task",task.id,task.title);body.Children.Add(title);
  body.Children.Add(T(task.project,10,Muted,new Thickness(0,0,0,8)));
  if(task.priority=="Haute")body.Children.Add(T("Priorité haute",10,Accent,new Thickness(0,0,0,6)));
  if(!String.IsNullOrEmpty(task.due)){var date=Schedule.Day(task.due);body.Children.Add(T((date<DateTime.Today&&task.status!="done"?"En retard · ":"Échéance · ")+date.ToString("dd/MM")+(String.IsNullOrEmpty(task.dueTime)?"":" à "+task.dueTime),10,Muted,new Thickness(0,0,0,6)));}
  if(task.repeat!="none")body.Children.Add(T("Récurrente",10,Muted,new Thickness(0,0,0,6)));
  var move=Btn("Déplacer…",delegate{},false,true);move.Padding=new Thickness(0,7,0,0);move.HorizontalAlignment=HorizontalAlignment.Left;
  var menu=new ContextMenu();string[] states={"todo","doing","done"},labels={"À faire","En cours","Terminé"};
  for(int i=0;i<3;i++){string state=states[i];var option=new MenuItem{Header=labels[i],IsEnabled=task.status!=state};option.Click+=delegate{MoveBoardTask(task.id,state);};menu.Items.Add(option);}
  move.Click+=delegate{menu.PlacementTarget=move;menu.IsOpen=true;};body.Children.Add(move);
  var card=Box(body,new Thickness(17));card.Background=CardBg;card.Margin=new Thickness(0,0,0,10);card.Cursor=Cursors.Hand;
  Point start=new Point();bool dragging=false,started=false;
  card.PreviewMouseLeftButtonDown+=delegate(object sender,MouseButtonEventArgs e){start=e.GetPosition(card);started=true;};
  card.PreviewMouseLeftButtonUp+=delegate{started=false;};
  card.PreviewMouseMove+=delegate(object sender,MouseEventArgs e){if(!started||dragging||e.LeftButton!=MouseButtonState.Pressed)return;var p=e.GetPosition(card);if(Math.Abs(p.X-start.X)<SystemParameters.MinimumHorizontalDragDistance&&Math.Abs(p.Y-start.Y)<SystemParameters.MinimumVerticalDragDistance)return;started=false;dragging=true;try{DragDrop.DoDragDrop(card,new DataObject(BoardFormat,boardSession+"/"+task.id),DragDropEffects.Move);e.Handled=true;}finally{dragging=false;}};
  return card;
 }
 void MoveBoardTask(string id,string state){try{TaskBoard.Move(store,id,state);}catch(Exception ex){MessageBox.Show("Déplacement non enregistré : "+ex.Message);}Refresh();}
 internal void VerifyBoard(string folder){
  store.Value.tasks=new List<WorkTask>{new WorkTask{id="board",title="Préparer la semaine",project="Personnel",priority="Haute",status="todo",due=DateTime.Today.ToString("yyyy-MM-dd"),notes="Notes conservées",repeat="daily"}};
  store.Save();if(!TaskBoard.Move(store,"board","doing"))throw new Exception("Déplacement en cours");
  if(!TaskBoard.Move(store,"board","done")||store.Value.tasks.Count!=2)throw new Exception("Occurrence suivante absente");
  TaskBoard.Move(store,"board","todo");TaskBoard.Move(store,"board","done");if(store.Value.tasks.Count!=2)throw new Exception("Occurrence dupliquée");
  if(new Store(store.Root).Value.tasks[0].notes!="Notes conservées")throw new Exception("Notes perdues");
  if(TaskBoard.Move(store,"absent","doing")||TaskBoard.Move(store,"board","bad"))throw new Exception("Déplacement invalide");
  string statePath=Path.Combine(store.Root,"state.json");File.Delete(statePath);Directory.CreateDirectory(statePath);
  bool failed=false;try{TaskBoard.Move(store,"board","doing");}catch{failed=true;}Directory.Delete(statePath);
  if(!failed||store.Value.tasks[0].status!="done")throw new Exception("Annulation après échec");
  store.Save();Go("Mes tâches");Preview(Path.Combine(folder,"Planning.png"));
  if(LogicalControls<Border>((DependencyObject)((ScrollViewer)content.Content).Content).Count(b=>b.AllowDrop&&new[]{"todo","doing","done"}.Contains(Convert.ToString(b.Tag)))!=3)throw new Exception("Colonnes de dépôt absentes");
  query="Préparer";projectFilter="Personnel";Refresh();if(LogicalControls<Button>((DependencyObject)((ScrollViewer)content.Content).Content).Count(b=>Convert.ToString(b.Content)=="Déplacer…")!=2)throw new Exception("Filtres tableau");
  Preview(Path.Combine(folder,"Planning.png"));
  projectFilter="Travail";Refresh();if(LogicalControls<Button>((DependencyObject)((ScrollViewer)content.Content).Content).Any(b=>Convert.ToString(b.Content)=="Déplacer…"))throw new Exception("Filtre projet non respecté");
  projectFilter="Tous les projets";query="Introuvable";Refresh();if(LogicalControls<Button>((DependencyObject)((ScrollViewer)content.Content).Content).Any(b=>Convert.ToString(b.Content)=="Déplacer…"))throw new Exception("Filtre recherche non respecté");
  query="";boardList=true;Refresh();if(LogicalControls<Border>((DependencyObject)((ScrollViewer)content.Content).Content).Any(b=>b.AllowDrop))throw new Exception("Vue liste");boardList=false;
 }
}
internal sealed partial class MainForm {
 internal void VerifyBoard(string folder){rendering=true;modern.VerifyBoard(folder);if(uiTicker!=null)uiTicker.Dispose();Stop();if(speech!=null)speech.Dispose();player.Close();}
}
internal static partial class Program {
 internal static int BoardTest(string folder){Root=Path.GetFullPath(folder);Directory.CreateDirectory(Root);System.Windows.Forms.Application.EnableVisualStyles();try{using(var form=new MainForm(false))form.VerifyBoard(Root);File.WriteAllText(Path.Combine(Root,"BOARD_PASS.txt"),"PASS: move, persistence, recurrence deduplication, notes preserved, invalid target, failed-save rollback, WPF columns, project/search filters, list view, rendered preview. Physical mouse drag requires desktop verification.");return 0;}catch(Exception ex){File.WriteAllText(Path.Combine(Root,"BOARD_FAIL.txt"),ex.ToString());return 1;}}
}
