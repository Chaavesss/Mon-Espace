using System;
using System.Windows.Forms;
internal sealed partial class MainForm {
 internal void SetupWorkspace(){CancelCountdown();Stop();modern.ShowOnboarding();}
 internal bool FinishSetup(OnboardingDraft draft){try{if(!Onboarding.Commit(store,draft,Save))return false;Theme.Gold=System.Drawing.ColorTranslator.FromHtml(store.Value.settings.Accent);Text=store.Value.settings.AppTitle;UpdateCaptureHotkey();if(Program.Startup()!=draft.Startup)try{Program.SetStartup(draft.Startup);}catch(Exception ex){MessageBox.Show("Votre espace est enregistré. Le démarrage automatique n’a pas pu être modifié :\n\n"+ex.Message,"Démarrage Windows");}return true;}catch(Exception ex){MessageBox.Show(ex.Message,"Configuration de l’espace");return false;}}
}
