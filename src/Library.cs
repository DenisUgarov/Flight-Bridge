using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

namespace FSMigrator {
 public sealed class TransferItem {
  public Plan Plan {get;set;}
  public string Device {get {return (string)Plan.Source.Device.Attribute("DeviceName");}}
  public string Category {get {return Plan.Target.Category;}}
  public string Summary {get {return Plan.Copied+" назначений · "+Plan.Skipped.Count+" пропущено";}}
 }
 public sealed class BackupItem {
  public string Manifest {get;set;} public string Name {get;set;} public string Created {get;set;}
  public override string ToString(){return Created+"   ·   "+Name;}
 }
 public static class Library {
  public static string DefaultRoot {get {return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"FlightBridge");}}
  public static string ExportBatch(IEnumerable<Plan> plans,string root,bool backup) {
   var items=plans.ToList();if(items.Count==0)throw new InvalidOperationException("Сначала добавьте профиль в список переноса.");
   foreach(var p in items) {if(p.Copied==0)throw new InvalidOperationException("В списке есть профиль без совместимых назначений.");if(!Engine.Unchanged(p.Source)||!Engine.Unchanged(p.Target))throw new IOException("Один из профилей изменился. Удалите его из списка и проверьте заново.");}
   Directory.CreateDirectory(root);string id=DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")+"_"+Guid.NewGuid().ToString("N").Substring(0,8);
   string staging=Path.Combine(root,".incomplete-"+id),final=Path.Combine(root,id);Directory.CreateDirectory(staging);
   // Each completed session is published only after all profiles and backups are written.
   var manifest=new XElement("Session",new XAttribute("Version","1"),new XAttribute("Created",DateTime.Now.ToString("o")),new XAttribute("Backup",backup));
   foreach(var p in items){string output=Engine.Export(p,staging,backup);manifest.Add(new XElement("Profile",new XAttribute("Directory",Path.GetFileName(output)),new XAttribute("Name",p.Source.Name),new XAttribute("Category",p.Target.Category),new XAttribute("Copied",p.Copied)));}
   new XDocument(manifest).Save(Path.Combine(staging,"session.xml"));
   File.WriteAllText(Path.Combine(staging,"НАЧНИТЕ ЗДЕСЬ.txt"),"Перенос подготовлен: "+items.Count+" профилей.\r\n\r\nВ каждой подпапке: MSFS2024-migrated.xml и report.txt.\r\nВ MSFS 2024: Настройки → Управление → устройство → шестерёнка нужной категории → Импорт.\r\nВыберите файл, затем профиль с именем, указанным в отчёте. Проверьте оси и кнопки.\r\n\r\nВозврат: выберите прежний профиль в игре. "+(backup?"В Flight Bridge раздел «Восстановление» выдаст исходный профиль для обратного импорта.":"Создание резервной копии было отключено.")+"\r\n");
   Directory.Move(staging,final);return final;
  }
  public static List<BackupItem> Backups(string root) {
   var result=new List<BackupItem>();if(!Directory.Exists(root))return result;
   foreach(var session in Directory.GetDirectories(root).Where(d=>!Path.GetFileName(d).StartsWith("."))) {
    if((File.GetAttributes(session)&FileAttributes.ReparsePoint)!=0)continue;
    foreach(var dir in new[]{session}.Concat(Directory.GetDirectories(session))) {
     if((File.GetAttributes(dir)&FileAttributes.ReparsePoint)!=0)continue;
     string m=Path.Combine(dir,"backup.xml");if(!File.Exists(m))continue;
     try {var p=Profile.Read(Path.Combine(dir,"original-2024.xml"));result.Add(new BackupItem{Manifest=m,Name=p.Name+" · "+p.Category,Created=File.GetCreationTime(m).ToString("dd.MM.yyyy HH:mm")});}catch {result.Add(new BackupItem{Manifest=m,Name="Копия недоступна или повреждена",Created=Path.GetFileName(session)});}
    }
   }
   return result.OrderByDescending(x=>x.Manifest,StringComparer.OrdinalIgnoreCase).ToList();
  }
 }
}
