using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
internal sealed class GuideChoice {public string Title{get;set;}public string Slug{get;set;}public string Detail{get;set;}public string Source{get{return GanymedeReader.IsGuide(Slug)?"Ganymède":"Tougli";}}public ImageSource Icon{get;set;}}
internal static class GuideAssets {
 internal static readonly FontFamily TitleFont=LoadFont();
 static FontFamily LoadFont(){System.IO.Packaging.PackUriHelper.Create(new Uri("application:///"));return new FontFamily(new Uri("pack://application:,,,/"),"./assets/fonts/#Bleach");}
 static readonly Dictionary<string,BitmapSource> icons=new Dictionary<string,BitmapSource>();
 static readonly string[] names={"dofus-argente-scintillant","dofus-des-veilleurs","dofus-du-cauchemar","dofus-des-glaces","dofus-forgelave","dofus-sylvestre","dofus-turquoise","dofus-emeraude","dofus-nebuleux","dofus-abyssal","dofus-tachete","dofus-kaliptus","dofus-pourpre","dofus-cawotte","dofus-argente","dofus-ivoire","dofus-cacao","dofus-vulbis","dofus-ebene","dofus-ocre","dom-de-pin","dolmanax","domakuro","dorigami","dofoozbz","dofawa","dokille","dokoko","dotruche","jyfus"};
 static string Key(string value){return new string((value??"").Normalize(NormalizationForm.FormD).Where(c=>CharUnicodeInfo.GetUnicodeCategory(c)!=UnicodeCategory.NonSpacingMark&&Char.IsLetterOrDigit(c)).ToArray()).ToLowerInvariant();}
 internal static BitmapSource Icon(string slug,string title){string text=Key(slug+" "+title);string name=names.FirstOrDefault(n=>text.Contains(Key(n.Replace("dofus-",""))));return name==null?null:Load(name);}
 internal static BitmapSource Load(string name){BitmapSource value;if(icons.TryGetValue(name,out value))return value;var uri=new Uri("pack://application:,,,/assets/dofus/"+name+".png",UriKind.Absolute);var bitmap=new BitmapImage();bitmap.BeginInit();bitmap.CacheOption=BitmapCacheOption.OnLoad;bitmap.UriSource=uri;bitmap.EndInit();bitmap.Freeze();icons[name]=bitmap;return bitmap;}
 internal static UIElement Image(string slug,string title,int size=52){var icon=Icon(slug,title);if(icon==null)return new TextBlock{Text="◈",Foreground=CompanionTheme.Gold,FontSize=size*.6,Width=size,TextAlignment=TextAlignment.Center,VerticalAlignment=VerticalAlignment.Center};return new Image{Source=icon,Width=size,Height=size,Stretch=Stretch.Uniform,Margin=new Thickness(0,0,12,0)};}
 internal static DataTemplate GuideTemplate(){return (DataTemplate)XamlReader.Parse(@"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'><DockPanel LastChildFill='True'><Image Width='40' Height='40' Source='{Binding Icon}' Margin='0,0,12,0'/><StackPanel VerticalAlignment='Center'><TextBlock Text='{Binding Title}' Foreground='#F4EDDE' FontSize='13' TextWrapping='Wrap'/><TextBlock Text='{Binding Detail}' Foreground='#B3AC9C' FontSize='10' Margin='0,3,0,0'/></StackPanel></DockPanel></DataTemplate>");}
 internal static ComboBox Picker(IEnumerable<TougliGuideInfo> guides,int selected){var combo=CompanionWindow.Combo(new string[0]);combo.ItemTemplate=(DataTemplate)XamlReader.Parse(@"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'><DockPanel LastChildFill='True'><Image Width='28' Height='28' Source='{Binding Icon}' Margin='0,0,9,0'/><TextBlock Text='{Binding Title}' Foreground='#F4EDDE' VerticalAlignment='Center' TextTrimming='CharacterEllipsis'/></DockPanel></DataTemplate>");SetItems(combo,guides,selected);return combo;}
 internal static void SetItems(ComboBox combo,IEnumerable<TougliGuideInfo> guides,int selected){combo.ItemsSource=guides.Select(g=>new GuideChoice{Title=g.Title,Slug=g.Slug,Icon=Icon(g.Slug,g.Title)}).ToArray();combo.SelectedIndex=selected;}
}
