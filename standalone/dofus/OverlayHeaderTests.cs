using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
internal static class OverlayHeaderTests {
 static FrameworkElement Tagged(Window window,string tag){return StandaloneTests.Find<FrameworkElement>(window.Content as DependencyObject).Single(x=>Equals(x.Tag,tag));}
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 internal static void Run(string folder){
 var store=new Store(Path.Combine(folder,"header-identity"));var c=new DofusCharacter{id="header",name="Personnage-au-nom-vraiment-très-long-pour-le-bandeau",server="Serveur test",guide="parcours-exemple",classCode="Sacri"};store.Value.dofus.characters.Add(c);store.Value.dofus.active=c.id;TougliReader.Cache(store,c.guide,StandaloneTests.Sample.Replace("Parcours fictif de validation","Dofus Turquoise"));store.Value.portraits[c.id]=CharacterPortraits.Save(store,CharacterPortraits.Prepare(Path.Combine(folder,"portraits/sample.webp")));store.Value.dofus.positionSaved=true;store.Value.dofus.width=430;store.Value.dofus.height=640;store.Save();
 var window=new CompanionWindow(store,true);foreach(int width in new[]{360,430,640}){window.Width=width;window.Render(Path.Combine(folder,"overlay-header-"+width+".png"));var header=Tagged(window,"overlay-header");var brand=Tagged(window,"overlay-brand");var identity=Tagged(window,"overlay-identity");var name=(TextBlock)Tagged(window,"overlay-character-name");var reward=Tagged(window,"overlay-reward");var skin=Tagged(window,"overlay-skin");
 var brandEnd=brand.TransformToAncestor(header).Transform(new Point(brand.ActualWidth,0));var identityStart=identity.TransformToAncestor(header).Transform(new Point(0,0));Check(identityStart.X>=brandEnd.X,"Identity overlaps Compagnon");foreach(var element in new[]{identity,reward,skin}){var bounds=element.TransformToAncestor(header).TransformBounds(new Rect(new Size(element.ActualWidth,element.ActualHeight)));Check(bounds.Left>=0&&bounds.Right<=header.ActualWidth+1&&bounds.Top>=0&&bounds.Bottom<=header.ActualHeight+1,"Header element outside window at "+width);}
 Check(name.TextTrimming==TextTrimming.CharacterEllipsis&&Convert.ToString(name.ToolTip).Contains(c.name),"Long name loses tooltip");Check(StandaloneTests.Find<Image>(skin).Any(x=>x.Source!=null&&x.Stretch==System.Windows.Media.Stretch.Uniform),"Real full portrait missing or distorted");Check(StandaloneTests.Find<TextBlock>(window.Content as DependencyObject).Count(x=>x.Text==c.name)==1,"Character name duplicated below header");}
 PersonalWorkflows.Change(store,()=>{store.Value.dofus.overlay.showPortrait=false;store.Value.dofus.overlay.showDofus=false;});window.RefreshView();window.Render(Path.Combine(folder,"overlay-header-hidden.png"));Check(!StandaloneTests.Find<FrameworkElement>(window.Content as DependencyObject).Any(x=>Equals(x.Tag,"overlay-skin")||Equals(x.Tag,"overlay-reward")),"Hidden portrait/reward settings ignored");window.Close();
 File.WriteAllText(Path.Combine(folder,"OVERLAY_HEADER_PASS.txt"),"PASS: identity right of Compagnon; 360/430/640 px bounds, real undistorted portrait, long-name ellipsis and tooltip, single character name, portrait/reward visibility preferences, existing progress and navigation tests retained.");
 }
}
