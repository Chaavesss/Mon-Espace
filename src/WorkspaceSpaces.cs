using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
public sealed class SpaceProfile {public string id="",name="";}
public sealed class SpaceCatalog {public string active="perso";public List<SpaceProfile> spaces=new List<SpaceProfile>{new SpaceProfile{id="perso",name="Perso"},new SpaceProfile{id="travail",name="Travail"},new SpaceProfile{id="gaming",name="Gaming"}};}
internal static class WorkspaceSpaces {
 static readonly JavaScriptSerializer Json=new JavaScriptSerializer{MaxJsonLength=65536};
 internal static void Validate(SpaceCatalog c){if(c==null||c.spaces==null||c.spaces.Count<1||c.spaces.Count>20||!c.spaces.Any(s=>s!=null&&s.id=="perso")||c.spaces.Any(s=>s==null||!ValidId(s.id)||String.IsNullOrWhiteSpace(s.name)||s.name.Length>40)||c.spaces.Select(s=>s.id).Distinct().Count()!=c.spaces.Count||!c.spaces.Any(s=>s.id==c.active))throw new InvalidDataException("Liste des espaces invalide.");}
 static bool ValidId(string id){Guid value;return id=="perso"||id=="travail"||id=="gaming"||Guid.TryParseExact(id,"N",out value);}
 internal static SpaceCatalog Read(string root){string file=Path.Combine(root,"spaces.json");if(File.Exists(file)&&new FileInfo(file).Length>65536)throw new InvalidDataException("Liste des espaces trop volumineuse.");var c=File.Exists(file)?Json.Deserialize<SpaceCatalog>(File.ReadAllText(file)):new SpaceCatalog();Validate(c);return c;}
 internal static void Save(string root,SpaceCatalog c){Validate(c);Directory.CreateDirectory(root);string file=Path.Combine(root,"spaces.json"),temp=file+".tmp";File.WriteAllText(temp,Json.Serialize(c));if(File.Exists(file))File.Replace(temp,file,null);else File.Move(temp,file);}
 internal static string DataRoot(string root,string id){if(!ValidId(id))throw new InvalidDataException("Espace invalide.");return id=="perso"?Path.Combine(root,"Data"):Path.Combine(root,"Spaces",id,"Data");}
 internal static string ActiveRoot(string root){return DataRoot(root,Read(root).active);}
 internal static void Activate(string root,string id){var c=Read(root);if(!c.spaces.Any(s=>s.id==id))throw new InvalidDataException("Cet espace n’existe plus.");new Store(DataRoot(root,id));c.active=id;Save(root,c);}
 internal static bool FinishSwitch(string[] args){if(args.Length!=3||args[0]!="--switch-space")return false;int pid;if(!Int32.TryParse(args[2],out pid))throw new InvalidDataException("Relance d’espace invalide.");UpdateInstaller.WaitForParent(pid);Activate(Program.Root,args[1]);return true;}
}
internal sealed partial class ModernWorkspace {
 Button spacesButton;
 void InstallSpacesButton(StackPanel chrome){spacesButton=Btn("Espaces ⌄",()=>Go("Mes espaces"),false,true);chrome.Children.Insert(0,spacesButton);}
 void UpdateSpaceLabel(){if(spacesButton==null)return;var catalog=WorkspaceSpaces.Read(Program.Root);spacesButton.Content=store.Value.settings.PrivacyMode?"Espaces":catalog.spaces.First(s=>s.id==catalog.active).name+" ⌄";spacesButton.Visibility=store.Value.settings.PrivacyMode?Visibility.Collapsed:Visibility.Visible;}
 void SpacesPage(StackPanel body){var catalog=WorkspaceSpaces.Read(Program.Root);body.Children.Add(T("Chaque espace possède ses tâches, projets, notes, budget, raccourcis, widgets et connexions. Vos données actuelles restent dans Perso. Le changement enregistre puis relance Mon espace.",12,Muted,new Thickness(0,0,0,18)));foreach(var item in catalog.spaces){var space=item;bool active=space.id==catalog.active;var panel=new StackPanel();var tools=new WrapPanel();tools.Children.Add(Btn(active?"Espace actuel":"Ouvrir cet espace",()=>SwitchSpace(space.id),!active));tools.Children.Add(Btn("Renommer",()=>RenameSpace(space.id),false,true));panel.Children.Add(tools);var card=Card(space.name,panel);card.Margin=new Thickness(0,0,0,14);body.Children.Add(card);}body.Children.Add(Btn("+ Créer un espace",()=>RenameSpace(null),true));body.Children.Add(T("Changer d’espace ne ferme pas les applications externes déjà lancées. Les rappels et sauvegardes automatiques concernent l’espace ouvert.",11,Muted,new Thickness(0,15,0,0)));}
 void RenameSpace(string id){StackPanel body,actions;var w=Dialog(id==null?"Créer un espace":"Renommer cet espace",out body,out actions,450);var c=WorkspaceSpaces.Read(Program.Root);var existing=c.spaces.FirstOrDefault(s=>s.id==id);var name=new TextBox{Text=existing==null?"":existing.name,MaxLength=40};Field(body,"Nom",name);var error=T("",12,Accent);body.Children.Add(error);actions.Children.Add(Btn("Enregistrer",delegate{try{if(String.IsNullOrWhiteSpace(name.Text))throw new InvalidDataException("Donnez un nom à cet espace.");if(existing==null)c.spaces.Add(new SpaceProfile{id=Guid.NewGuid().ToString("N"),name=name.Text.Trim()});else existing.name=name.Text.Trim();WorkspaceSpaces.Save(Program.Root,c);FinishFinanceDialog(w);}catch(Exception ex){error.Text=ex.Message;}},true));ShowWindow(w);Refresh();}
 void SwitchSpace(string id){if(owner.IsPreview)return;if(dofusBusy||updateBusy||autoBackupBusy||IsAttachmentBusy||FocusClock.Active(store.Value.focus)){MessageBox.Show("Attendez la fin de la mise à jour, de la sauvegarde, de la copie ou de la session de concentration avant de changer d’espace.");return;}if(!FlushDeskNotes()||!ConfirmDocumentDrafts()||!owner.Save())return;owner.RestartForSpace(id);}
}
internal sealed partial class MainForm {
 internal bool RestartForSpace(string id){var catalog=WorkspaceSpaces.Read(Program.Root);if(!catalog.spaces.Any(s=>s.id==id))return false;System.Windows.Forms.FormClosedEventHandler restart=delegate{try{Process.Start(new ProcessStartInfo(Assembly.GetExecutingAssembly().Location,"--switch-space "+id+" "+Process.GetCurrentProcess().Id){UseShellExecute=true});}catch(Exception ex){System.Windows.Forms.MessageBox.Show("La relance de l’espace a échoué : "+ex.Message);}};FormClosed+=restart;RequestQuit();if(!IsDisposed){FormClosed-=restart;return false;}return true;}
}
