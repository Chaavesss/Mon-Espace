using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
internal sealed partial class ModernWorkspace {
 bool ActivateModeHome(LaunchMode mode){try{PersonalWorkflows.Change(store,delegate{DayDashboard.Activate(store.Value,mode);});return true;}catch(Exception ex){MessageBox.Show(ex.Message,"Accueil du mode");return false;}}
 void RoutineExamples(WrapPanel tools){foreach(string preset in new[]{"Travail","Dofus","Perso"}){string name=preset;tools.Children.Add(Btn("+ "+name,delegate{var mode=new LaunchMode{name=name,homePreset=name,description=name=="Dofus"?"Choisissez Ankama Launcher et ajoutez votre page Tougli. Spotify est proposé ci-dessous.":name=="Travail"?"Vos outils de travail et un accueil centré sur vos priorités.":"Votre planning, vos idées, vos habitudes et votre budget."};if(name=="Dofus")mode.actions.Add(new ModeAction{kind="spotify",label="Spotify"});EditMode(mode);},false,true));}}
 Action SaveRoutineFields(StackPanel body,LaunchMode draft){
  string original=draft.homePreset??"";var presets=new[]{"Ne pas modifier mon accueil","Travail","Dofus","Perso","Mon accueil actuel"};int initial=original=="Travail"?1:original=="Dofus"?2:original=="Perso"?3:original=="Personnalisé"?4:0;var preset=Combo(presets,presets[initial]);Field(body,"Accueil à afficher avec ce mode",preset);bool capture=false;preset.SelectionChanged+=delegate{capture=preset.SelectedIndex==4;};
  var projects=new List<string>{"Tous mes projets"};projects.AddRange(owner.AllProjects());if(!String.IsNullOrEmpty(draft.homeProject)&&!projects.Contains(draft.homeProject))projects.Add(draft.homeProject);var project=Combo(projects,String.IsNullOrEmpty(draft.homeProject)?projects[0]:draft.homeProject);Field(body,"Priorités et tâches de cet accueil",project);body.Children.Add(T("Le mode conserve sa propre disposition. Vous pourrez déplacer ses blocs puis revenir à votre accueil habituel. Les applications ne s’ouvrent que lorsque vous cliquez sur Lancer.",11,Muted,new Thickness(0,0,0,16)));
  return delegate{string selectedPreset=new[]{"","Travail","Dofus","Perso","Personnalisé"}[preset.SelectedIndex];draft.homeProject=project.SelectedIndex==0?"":Convert.ToString(project.SelectedItem);if(selectedPreset=="Personnalisé"&&(capture||draft.homeLayout==null))draft.homeLayout=DayDashboard.CloneLayout(store.Value.settings.HomeBlocks);else if(selectedPreset!=original)draft.homeLayout=null;draft.homePreset=selectedPreset;};
 }
}
