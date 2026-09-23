using System;
using FSMigrator;
class Diagnostic {
 static int Main(){try{var a=AutoMigration.Build(AutoMigration.Discover());Console.WriteLine(AutoMigration.RedactedReport(a));foreach(var s in a.Stores)Console.WriteLine(s.Year+" installation: "+s.InstallPath+"; storage: "+s.Root);foreach(var p in a.Changes)Console.WriteLine("Plan: "+p.Source.Name+" -> "+p.Target.Name+"; copied="+p.Copied+"; skipped="+p.Skipped.Count+"; relocated="+p.Relocated);foreach(var issue in a.Issues)Console.WriteLine("BLOCK: "+issue);return 0;}catch(Exception ex){Console.Error.WriteLine("Диагностика остановлена без записи в игры: "+ex.GetType().Name+": "+ex.Message);return 1;}}
}
