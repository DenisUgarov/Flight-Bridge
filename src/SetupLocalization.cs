using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Reflection;

public sealed class SetupLanguage {
 public string Code {get;set;} public string Name {get;set;}
 public Dictionary<string,string> Strings;
 public string this[string key] {get{return Strings[key];}}
 public override string ToString(){return Name;}
}
public static class SetupLocalization {
 public static readonly List<SetupLanguage> All=Load();
 static List<SetupLanguage> Load(){using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("SetupLanguages.xml"))return XDocument.Load(stream).Root.Elements("Language").Select(e=>new SetupLanguage{Code=(string)e.Attribute("Code"),Name=(string)e.Attribute("Name"),Strings=e.Elements().ToDictionary(s=>s.Name.LocalName,s=>s.Value)}).ToList();}
 public static SetupLanguage Resolve(string culture){string code=(culture ?? "en").Split('-')[0].ToLowerInvariant();return All.FirstOrDefault(l=>l.Code==code) ?? All.First(l=>l.Code=="en");}
}
