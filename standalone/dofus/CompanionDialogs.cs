using System.Windows;
using System.Windows.Controls;
internal sealed partial class CompanionWindow {
 internal Window UpdateConfirmationWindow(){StackPanel panel;var owner=options!=null&&options.IsVisible?options:this;var dialog=CompanionTheme.Dialog(owner,"Mise à jour du compagnon",500,300,out panel);dialog.Topmost=owner.Topmost;dialog.ShowInTaskbar=false;panel.Children.Add(CompanionTheme.Heading("La mise à jour est prête.",22));panel.Children.Add(Text("Le compagnon va se fermer, installer la mise à jour puis redémarrer. Vos personnages et votre progression sont conservés.",13,true));var later=Button("Plus tard",()=>dialog.DialogResult=false);later.IsCancel=true;var install=Button("Installer et redémarrer",()=>dialog.DialogResult=true,true);install.IsDefault=true;panel.Children.Add(CompanionTheme.Pair(later,install));return dialog;}
 bool ConfirmUpdate(){return UpdateConfirmationWindow().ShowDialog()==true;}
}
