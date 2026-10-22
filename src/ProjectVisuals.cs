using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
public sealed class ProjectBlock {public string id="";public bool visible=true;}
internal static class ProjectVisuals {
 internal static readonly string[] Ids={"tasks","notes","links","files"};
 internal static List<ProjectBlock> Defaults(){return Ids.Select(id=>new ProjectBlock{id=id}).ToList();}
 internal static string Name(string id){return id=="tasks"?"Tâches":id=="notes"?"Pages et notes":id=="links"?"Liens utiles":"Fichiers joints";}
 internal static void Validate(Data data){
  foreach(var project in data.projectData){
   project.icon=project.icon??"";if(project.icon.Length>8)throw new InvalidDataException("Icône de projet trop longue.");
   if(project.cover!=null){ProjectData.CheckFile(project.cover);if(!new[]{".png",".jpg",".jpeg"}.Contains(project.cover.extension.ToLowerInvariant()))throw new InvalidDataException("Couverture de projet invalide.");}
   if(project.blocks==null)project.blocks=Defaults();var ids=new HashSet<string>();project.blocks=project.blocks.Where(b=>b!=null&&Ids.Contains(b.id)&&ids.Add(b.id)).ToList();foreach(var id in Ids)if(!ids.Contains(id))project.blocks.Add(new ProjectBlock{id=id});
  }
 }
 internal static BitmapImage Read(string path){
  if(new FileInfo(path).Length>10*1024*1024)throw new InvalidDataException("Choisissez une image de moins de 10 Mo.");
  if(!new[]{".png",".jpg",".jpeg"}.Contains(Path.GetExtension(path).ToLowerInvariant()))throw new InvalidDataException("Choisissez une image PNG ou JPEG.");
  var image=new BitmapImage();using(var file=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read)){image.BeginInit();image.CacheOption=BitmapCacheOption.OnLoad;image.DecodePixelWidth=1200;image.StreamSource=file;image.EndInit();}image.Freeze();return image;
 }
}
internal sealed partial class ModernWorkspace {
 string ProjectHeading(string name){var project=store.Value.projectData.FirstOrDefault(p=>p.name==name);return project==null||String.IsNullOrWhiteSpace(project.icon)?name:project.icon+"  "+name;}
 Border CoverBorder(ImageSource image,double height){return new Border{Height=height,CornerRadius=new CornerRadius(12),Background=new ImageBrush(image){Stretch=Stretch.UniformToFill},Margin=new Thickness(0,0,0,16)};}
 void RenderProjectVisual(StackPanel root,ProjectSpace project){if(project.cover==null)return;try{string path=ProjectData.PathFor(store,project.cover);if(File.Exists(path))root.Children.Add(CoverBorder(ProjectVisuals.Read(path),150));else root.Children.Add(T("Couverture absente de ce PC.",11,Muted,new Thickness(0,0,0,12)));}catch{root.Children.Add(T("Couverture indisponible.",11,Muted,new Thickness(0,0,0,12)));}}
 void AddProjectVisualPreview(StackPanel body,string name){var project=store.Value.projectData.FirstOrDefault(p=>p.name==name);if(project==null||project.cover==null)return;try{string path=ProjectData.PathFor(store,project.cover);if(File.Exists(path))body.Children.Add(CoverBorder(ProjectVisuals.Read(path),90));}catch{}}
 void EditProjectVisual(string name){
  var source=ProjectData.Get(store.Value,name);var blocks=source.blocks.Select(b=>new ProjectBlock{id=b.id,visible=b.visible}).ToList();var cover=source.cover;string pending="";bool saved=false;
  StackPanel body,actions;var w=Dialog("Apparence du projet",out body,out actions,770);var icon=new TextBox{Text=source.icon,MaxLength=8};Field(body,"Icône : emoji ou symbole (facultatif)",icon);
  var preview=new StackPanel();body.Children.Add(preview);var error=T("",12,Accent);Action drawCover=delegate{preview.Children.Clear();try{if(!String.IsNullOrEmpty(pending))preview.Children.Add(CoverBorder(ProjectVisuals.Read(pending),110));else if(cover!=null&&File.Exists(ProjectData.PathFor(store,cover)))preview.Children.Add(CoverBorder(ProjectVisuals.Read(ProjectData.PathFor(store,cover)),110));}catch(Exception ex){error.Text=ex.Message;}};drawCover();
  var coverTools=new WrapPanel{Margin=new Thickness(0,0,0,16)};coverTools.Children.Add(Btn("Choisir une couverture",delegate{using(var picker=new System.Windows.Forms.OpenFileDialog{Title="Couverture du projet",Filter="Images PNG et JPEG|*.png;*.jpg;*.jpeg"})if(picker.ShowDialog()==System.Windows.Forms.DialogResult.OK)try{ProjectVisuals.Read(picker.FileName);pending=picker.FileName;drawCover();}catch(Exception ex){error.Text=ex.Message;}},false,true));coverTools.Children.Add(Btn("Retirer la couverture",delegate{pending="";cover=null;drawCover();},false,true));body.Children.Add(coverTools);
  body.Children.Add(T("Blocs du projet : visibilité et ordre",13,Ink,new Thickness(0,0,0,10)));var list=new StackPanel();body.Children.Add(list);Action draw=null;draw=delegate{list.Children.Clear();for(int i=0;i<blocks.Count;i++){int index=i;var block=blocks[i];var row=new DockPanel{Margin=new Thickness(0,0,0,9)};var arrows=new WrapPanel();arrows.Children.Add(Btn("↑",delegate{if(index>0){blocks.RemoveAt(index);blocks.Insert(index-1,block);draw();}},false,true));arrows.Children.Add(Btn("↓",delegate{if(index<blocks.Count-1){blocks.RemoveAt(index);blocks.Insert(index+1,block);draw();}},false,true));DockPanel.SetDock(arrows,Dock.Right);row.Children.Add(arrows);var show=new CheckBox{Content=ProjectVisuals.Name(block.id),IsChecked=block.visible};show.Click+=delegate{block.visible=show.IsChecked==true;};row.Children.Add(show);list.Children.Add(row);}};draw();
  body.Children.Add(T("Une copie de la couverture est conservée avec vos fichiers. Elle est incluse dans les sauvegardes complètes.",11,Muted,new Thickness(0,12,0,12)));body.Children.Add(error);
  actions.Children.Add(Btn("Annuler",delegate{w.DialogResult=false;},false,true));actions.Children.Add(Btn("Enregistrer l'apparence",delegate{if(saved)return;ProjectFile attached=null;try{
   if(!String.IsNullOrEmpty(pending)){ProjectVisuals.Read(pending);attached=ProjectData.Attach(store,pending);}
   PersonalWorkflows.Change(store,delegate{var project=ProjectData.Get(store.Value,name);project.icon=icon.Text.Trim();project.cover=attached??cover;project.blocks=blocks;});saved=true;FinishFinanceDialog(w);
  }catch(Exception ex){if(attached!=null)try{File.Delete(ProjectData.PathFor(store,attached));}catch{}error.Text=ex.Message;}},true));ShowWindow(w);Refresh();
 }
}
