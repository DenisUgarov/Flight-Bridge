using System;
using FSMigrator;

public static class AppLocalizationTests {
 public static int Main(){
  if(AppLocalization.All.Count!=13)throw new Exception("Expected 13 app languages");
  string[] cultures={"en-US","ru-RU","de-DE","fr-FR","es-ES","it-IT","pt-BR","pl-PL","uk-UA","tr-TR","zh-CN","ja-JP","ko-KR"};
  foreach(string culture in cultures){var language=AppLocalization.Resolve(culture);if(string.IsNullOrWhiteSpace(language["Transfer"])||language["Transfer"]=="Transfer")throw new Exception("Missing app translation: "+culture);string.Format(language["Profiles"],3);string.Format(language["GameProfiles"],"MSFS 2020","Steam",51);string.Format(language["ChangeResult"],"A","B",12,3);string.Format(language["TransferMessage"],"C:\\Backup");Console.WriteLine("PASS app language: "+language.Code);}
  if(AppLocalization.Resolve("xx-XX").Code!="en")throw new Exception("Expected English fallback");
  Console.WriteLine("13 app languages and fallback passed.");return 0;
 }
}
