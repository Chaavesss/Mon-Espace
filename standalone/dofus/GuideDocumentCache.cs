using System;
using System.IO;
using System.Collections.Generic;
internal static class GuideDocumentCache {
 sealed class Entry {internal long Stamp,Length;internal TougliDocument Document;}
 static readonly Dictionary<string,Entry> entries=new Dictionary<string,Entry>();
 internal static int Parses;
 internal static TougliDocument Read(Store store,DofusCharacter c,string slug){var file=new FileInfo(TougliReader.CachePath(store,slug));string key=file.FullName+"|"+c.classCode+"|"+c.alignment+"|"+c.order;if(!file.Exists){entries.Remove(key);return null;}Entry hit;if(entries.TryGetValue(key,out hit)&&hit.Stamp==file.LastWriteTimeUtc.Ticks&&hit.Length==file.Length)return hit.Document;var raw=TougliReader.Cached(store,slug);if(raw==null)return null;var doc=GanymedeReader.IsGuide(slug)?GanymedeReader.Parse(raw,slug):TougliReader.Parse(raw,c);if(doc.Slug!=slug)throw new InvalidDataException("Cache de guide incorrect.");Parses++;if(entries.Count>=256)entries.Clear();entries[key]=new Entry{Stamp=file.LastWriteTimeUtc.Ticks,Length=file.Length,Document=doc};return doc;}
}
