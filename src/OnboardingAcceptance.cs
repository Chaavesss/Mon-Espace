using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Web.Script.Serialization;
internal sealed partial class ModernWorkspace {
 Func<Window,bool?> dialogProbe;
 static IEnumerable<T> LogicalControls<T>(DependencyObject node)where T:DependencyObject{var match=node as T;if(match!=null)yield return match;foreach(object child in LogicalTreeHelper.GetChildren(node)){var dependency=child as DependencyObject;if(dependency!=null)foreach(var descendant in LogicalControls<T>(dependency))yield return descendant;}}
 static Button WizardButton(Window window,string label){var button=LogicalControls<Button>(window).FirstOrDefault(b=>Convert.ToString(b.Content)==label);if(button==null)throw new Exception("Bouton absent : "+label);return button;}
 static void ClickWizard(Window window,string label){WizardButton(window,label).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));}
 static void CheckWizard(Window window,string label,bool value){var check=LogicalControls<CheckBox>(window).FirstOrDefault(c=>Convert.ToString(c.Content).Contains(label));if(check==null)throw new Exception("Option absente : "+label);check.IsChecked=value;check.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));}
 internal void RunOnboardingAcceptance(){string before=new JavaScriptSerializer().Serialize(store.Value);bool reviewed=false;dialogProbe=delegate(Window window){var fields=LogicalControls<TextBox>(window).ToList();fields[0].Text="Utilisateur test";fields[1].Text="Bureau test";ClickWizard(window,"Suivant");LogicalControls<ComboBox>(window).First().SelectedIndex=4;ClickWizard(window,"Suivant");CheckWizard(window,"Notes récentes",true);ClickWizard(window,"Suivant");LogicalControls<TextBox>(window).First().Text="Discord personnel";ClickWizard(window,"Précédent");var notes=LogicalControls<CheckBox>(window).First(c=>Convert.ToString(c.Content)=="Notes récentes");if(notes.IsChecked!=true)throw new Exception("Choix d’accueil perdu en revenant");ClickWizard(window,"Suivant");if(LogicalControls<TextBox>(window).First().Text!="Discord personnel")throw new Exception("Nom du raccourci perdu");ClickWizard(window,"Suivant");LogicalControls<TextBox>(window).First().Text="Mon mode";CheckWizard(window,"Spotify (application)",true);ClickWizard(window,"Suivant");CheckWizard(window,"Ctrl + Alt + N",false);ClickWizard(window,"Suivant");string summary=String.Join("\n",LogicalControls<TextBlock>(window).Select(t=>t.Text));if(!summary.Contains("Utilisateur test")||!summary.Contains("Bureau test")||!summary.Contains("1 mode(s) personnalisés")||!summary.Contains("Sable"))throw new Exception("Récapitulatif incohérent");WizardButton(window,"Terminer");ClickWizard(window,"Précédent");var capture=LogicalControls<CheckBox>(window).First(c=>Convert.ToString(c.Content).Contains("Ctrl + Alt + N"));if(capture.IsChecked!=false)throw new Exception("Préférence perdue");reviewed=true;return false;};try{ShowOnboarding();}finally{dialogProbe=null;}if(!reviewed||before!=new JavaScriptSerializer().Serialize(store.Value))throw new Exception("L’abandon de l’assistant a modifié les données");}
}
internal sealed partial class MainForm {
 internal void VerifyOnboardingUi(){rendering=true;modern.RunOnboardingAcceptance();if(uiTicker!=null)uiTicker.Dispose();Stop();if(speech!=null)speech.Dispose();player.Close();}
}
internal static partial class Program {
 internal static int OnboardingUiTest(string folder){Root=Path.GetFullPath(folder);Directory.CreateDirectory(Root);System.Windows.Forms.Application.EnableVisualStyles();try{using(var form=new MainForm(false))form.VerifyOnboardingUi();File.WriteAllText(Path.Combine(Root,"UI_PASS.txt"),"PASS: actual WPF wizard controls, seven steps, theme selection, shortcut rename, mode creation, back/next retention, review content, cancellation without data mutation. No install, registry write, Google login or application launch performed.");return 0;}catch(Exception ex){File.WriteAllText(Path.Combine(Root,"UI_FAIL.txt"),ex.ToString());return 1;}}
}
