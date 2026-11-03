using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
internal sealed partial class CompanionWindow {
 internal FrameworkElement OverlayHeader(DofusCharacter character,TougliDocument guide){
 if(character==null)return CompanionTheme.Header(this,"Le fil de votre aventure",testing);
 var preferences=store.Value.dofus.overlay;
 var header=new Grid{Tag="overlay-header",Margin=new Thickness(0,0,0,8),MinHeight=44};
 header.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});header.ColumnDefinitions.Add(new ColumnDefinition());header.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
 var brand=new StackPanel{Orientation=Orientation.Horizontal,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(0,0,8,0),Cursor=Cursors.SizeAll};
 brand.Children.Add(new TextBlock{Text="◈",FontSize=16,Foreground=Accent,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(0,0,6,0)});
 var title=new TextBlock{Text="COMPAGNON",Tag="overlay-brand",FontFamily=GuideAssets.TitleFont,FontSize=18,Foreground=Accent,VerticalAlignment=VerticalAlignment.Center};brand.Children.Add(title);header.Children.Add(brand);
 var identity=new Grid{Tag="overlay-identity",VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(0,0,6,0),Cursor=Cursors.SizeAll};identity.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});identity.ColumnDefinitions.Add(new ColumnDefinition());identity.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});Grid.SetColumn(identity,1);header.Children.Add(identity);
 if(preferences.showPortrait){int size=Math.Max(28,Math.Min(38,preferences.portrait/2));var skin=CharacterPortraits.FullView(store,character,size,(int)(size*1.25));skin.Tag="overlay-skin";skin.Margin=new Thickness(0,0,5,0);skin.ToolTip=character.name;identity.Children.Add(skin);}
 var labels=new StackPanel{VerticalAlignment=VerticalAlignment.Center};Grid.SetColumn(labels,1);identity.Children.Add(labels);
 labels.Children.Add(new TextBlock{Text=character.name,Tag="overlay-character-name",FontSize=12,FontWeight=FontWeights.SemiBold,Foreground=Ink,TextTrimming=TextTrimming.CharacterEllipsis,TextWrapping=TextWrapping.NoWrap,ToolTip=character.name+(String.IsNullOrWhiteSpace(character.server)?"":" · "+character.server)});
 if(guide!=null){bool dofus=GuideAssets.Icon(character.guide,guide.Title)!=null;labels.Children.Add(new TextBlock{Text=guide.Title,Tag="overlay-guide",FontSize=10,Foreground=Muted,Margin=new Thickness(0,3,0,0),TextTrimming=TextTrimming.CharacterEllipsis,TextWrapping=TextWrapping.NoWrap,ToolTip=(dofus?"Dofus en cours : ":"Parcours en cours : ")+guide.Title});if(preferences.showDofus){int size=Math.Max(26,Math.Min(36,preferences.dofus/2));var reward=(FrameworkElement)GuideAssets.Image(character.guide,guide.Title,size);reward.Tag="overlay-reward";reward.Margin=new Thickness(5,0,0,0);reward.VerticalAlignment=VerticalAlignment.Center;reward.ToolTip=(dofus?"Dofus en cours : ":"Parcours en cours : ")+guide.Title;Grid.SetColumn(reward,2);identity.Children.Add(reward);}}
 var controls=new StackPanel{Orientation=Orientation.Horizontal,VerticalAlignment=VerticalAlignment.Center};Grid.SetColumn(controls,2);header.Children.Add(controls);
 var minimize=Button("−",()=>WindowState=WindowState.Minimized);var close=Button("×",Close);foreach(var button in new[]{minimize,close}){button.Width=26;button.Height=28;button.MinHeight=28;button.Padding=new Thickness(0);button.Margin=new Thickness(3,0,0,0);controls.Children.Add(button);}minimize.ToolTip="Réduire";close.ToolTip="Fermer";System.Windows.Automation.AutomationProperties.SetName(minimize,"Réduire la fenêtre");System.Windows.Automation.AutomationProperties.SetName(close,"Fermer la fenêtre");
 MouseButtonEventHandler drag=delegate(object sender,MouseButtonEventArgs e){if(!testing&&e.LeftButton==MouseButtonState.Pressed)DragMove();};brand.MouseLeftButtonDown+=drag;identity.MouseLeftButtonDown+=drag;
 return header;
 }
}
