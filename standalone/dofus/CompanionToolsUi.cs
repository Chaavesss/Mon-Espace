using System;
using System.Windows;
using System.Windows.Controls;
internal sealed partial class CompanionWindow {
 void OverlayOptions(){StackPanel body;var dialog=CompanionTheme.Dialog(this,"Personnaliser l’overlay",520,720,out body);var actions=new WrapPanel();DofusToolsUi.Options(body,actions,store,dialog.Close,RefreshView,Ink,Muted);body.Children.Add(actions);dialog.ShowDialog();}
 void QuestNotes(TougliStep step){if(step==null)return;StackPanel body;var dialog=CompanionTheme.Dialog(this,"Notes et préparation",560,740,out body);var actions=new WrapPanel();DofusToolsUi.Notes(body,actions,store,step,dialog.Close,RefreshView,Ink,Muted);body.Children.Add(actions);dialog.ShowDialog();}
 void ResumeDofus(){Safe(()=>{DofusTools.Resume(store);documentKey="";});if(Current()!=null)EnterOverlay();}
}
