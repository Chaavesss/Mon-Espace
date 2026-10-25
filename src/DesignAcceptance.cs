using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
internal sealed partial class ModernWorkspace {
 internal void PreviewSized(string path,int width,int height){
  Width=width;Height=height;Measure(new Size(width,height));Arrange(new Rect(0,0,width,height));UpdateLayout();
  var bitmap=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);bitmap.Render(this);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using(var file=File.Create(path))png.Save(file);
 }
 internal void VerifyDesignV2(string folder){
  Go("Aujourd’hui");Go("Agenda");Go("Mes projets");OpenProject("Travail");Go("Mon budget");
  var projectTab=openTabs.FirstOrDefault(t=>t.Page=="Mes projets"&&t.Project=="Travail");if(projectTab==null)throw new Exception("Contexte de projet non retenu dans les onglets");
  var projectButton=LogicalControls<Button>(workspaceTabs).First(b=>Convert.ToString(b.Content)=="Travail");projectButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));if(openProject!="Travail"||selected!="Mes projets")throw new Exception("Retour au projet depuis son onglet");
  var close=LogicalControls<Button>(workspaceTabs).First(b=>Convert.ToString(b.ToolTip)=="Fermer cet onglet");close.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));if(openTabs.Count>5)throw new Exception("Limite des onglets");
  Go("Ma musique");if(!LogicalControls<TextBlock>(this).Any(t=>t.Text=="Ma musique"))throw new Exception("Titre lecteur absent après réorganisation");
  if(!LogicalControls<Button>(themeMusic).Any(b=>Convert.ToString(b.Content)=="Widget"))throw new Exception("Raccourci widget absent de la barre du bas");
  var prior=store.Value.settings.PrivacyMode;store.Value.settings.PrivacyMode=true;Refresh();if(workspaceTabs.Visibility!=Visibility.Collapsed||miniSong.Text!="Mode discret")throw new Exception("Contenu d'onglet ou de lecteur visible en mode discret");store.Value.settings.PrivacyMode=prior;
  foreach(var page in new[]{"Aujourd’hui","Mes tâches","Mes projets","Agenda","Mon budget","Mes notes","Ma musique","Réglages"}){
   Go(page);PreviewSized(Path.Combine(folder,page.Replace("’","").Replace(" ","-")+".png"),1040,720);
  }
  var original=Appearance.Clone(store.Value.settings.Appearance);try{store.Value.settings.Appearance=Appearance.Presets().First(t=>t.name=="Sable");Go("Aujourd’hui");Preview(Path.Combine(folder,"Accueil-clair.png"));}finally{store.Value.settings.Appearance=original;Refresh();}
  VerifyDayDashboard(folder);VerifyProductivity(folder);VerifyCompleteWorkspace(folder);
 }
}
internal sealed partial class MainForm {
 internal void VerifyDesignV2(string folder){
  rendering=true;store.Value.settings.Configured=true;store.Value.settings.DisplayName="Victor";store.Value.tasks.Add(new WorkTask{id="v2-demo",title="Préparer la prochaine étape",project="Travail",priority="Haute",status="doing",due=DateTime.Today.ToString("yyyy-MM-dd"),notes=""});store.Save();
  modern.VerifyDesignV2(folder);if(uiTicker!=null)uiTicker.Dispose();modern.CancelMusic();modern.CancelModeLaunch();Stop();if(speech!=null)speech.Dispose();player.Close();
 }
}
internal static partial class Program {
 internal static int DesignTest(string folder){
  Root=Path.GetFullPath(folder);Directory.CreateDirectory(Root);System.Windows.Forms.Application.EnableVisualStyles();
  try{using(var form=new MainForm(false))form.VerifyDesignV2(Root);File.WriteAllText(Path.Combine(Root,"DESIGN_PASS.txt"),"PASS: project tab context and navigation, close-tab interaction, music heading retained, footer widget button, privacy masks tab strip and mini-player, eight pages rendered at 1040x720, light theme rendered. Real Google/Spotify connection and physical resize remain target-PC checks.");return 0;}catch(Exception ex){File.WriteAllText(Path.Combine(Root,"DESIGN_FAIL.txt"),ex.ToString());return 1;}
 }
}

