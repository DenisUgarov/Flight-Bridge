using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public sealed class DeviceInfo {
 public string Name, Id, Driver, Version; public int? Error; public bool IsController;
 public string State {get {return Error==28?"MissingDriver":Error==22?"DisabledDevice":Error.HasValue&&Error.Value!=0?"DeviceProblem":Error==0?"DeviceReady":"UnknownDevice";}}
}
public sealed class DeviceReport {
 public List<DeviceInfo> Devices=new List<DeviceInfo>(); public bool Incomplete;
 public bool Data2020,Data2024;
 public string Format(SetupLanguage language){
  var lines=new List<string>{language["CheckIntro"],"", "MSFS 2020: "+language[Data2020?"DataFound":"DataNotFound"],"MSFS 2024: "+language[Data2024?"DataFound":"DataNotFound"],""};
  if(Incomplete)lines.Add(language["ScanIncomplete"]);
  if(!Devices.Any(d=>d.IsController))lines.Add(language["NoDevices"]);
  foreach(var d in Devices){lines.Add(d.Name+" — "+language[d.State]+(d.Error.HasValue&&d.Error.Value!=0?" ("+d.Error+")":""));lines.Add((d.IsController?language["Controller"]:language["OtherDevice"])+" | "+d.Id);lines.Add(language["Driver"]+": "+(string.IsNullOrEmpty(d.Driver)?language["UnknownDevice"]:d.Driver+" "+d.Version));if(d.State=="MissingDriver")lines.Add(language["DriverAdvice"]);else if(d.State=="DisabledDevice"||d.State=="DeviceProblem")lines.Add(language["ProblemAdvice"]);lines.Add("");}
  lines.Add(language["InboxDriver"]);return string.Join("\r\n",lines);
 }
}
public static class DeviceCheck {
 public static bool IsGameDevice(string name,IEnumerable<string> compatible){return (compatible??new string[0]).Any(x=>x.IndexOf("HID_DEVICE_SYSTEM_GAME",StringComparison.OrdinalIgnoreCase)>=0||Regex.IsMatch(x,@"HID_DEVICE_UP:0001_U:000[458]",RegexOptions.IgnoreCase)) || Regex.IsMatch(name??"",@"joystick|game controller|hotas|throttle|rudder|pedal|yoke|flight stick|flightstick|vkb|virpil|winwing|t\.16000|tca sidestick",RegexOptions.IgnoreCase);}
 public static string SupportUrl(DeviceInfo device){
  string name=(device.Name??"").ToLowerInvariant();
  if(name.Contains("thrustmaster")||name.Contains("t.16000")||name.Contains("tca "))return "https://support.thrustmaster.com/en/";
  if(name.Contains("logitech")||name.Contains("saitek"))return "https://support.logi.com/";
  if(name.Contains("vkb"))return "https://vkbsimcontrollers.com/pages/downloads";
  return "https://support.microsoft.com/en-us/windows/hardware/drivers/automatically-get-recommended-and-updated-hardware-drivers";
 }
 static string S(ManagementBaseObject o,string key){return Convert.ToString(o[key]);}
 public static DeviceReport Run(){
  var result=new DeviceReport();string local=Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
  result.Data2020=Directory.Exists(Path.Combine(local,@"Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\SystemAppData"));result.Data2024=Directory.Exists(Path.Combine(local,@"Packages\Microsoft.Limitless_8wekyb3d8bbwe\SystemAppData"));
  try{string steam=Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam","SteamPath",null) as string;if(steam!=null&&Directory.Exists(Path.Combine(steam,"userdata")))foreach(string user in Directory.GetDirectories(Path.Combine(steam,"userdata"))){result.Data2020|=Directory.Exists(Path.Combine(user,"1250410"));result.Data2024|=Directory.Exists(Path.Combine(user,"2537590"));}}catch{result.Incomplete=true;}
  try{
   var options=new EnumerationOptions{Timeout=TimeSpan.FromSeconds(4),ReturnImmediately=false};
   using(var search=new ManagementObjectSearcher("root\\CIMV2","SELECT Name,PNPDeviceID,CompatibleID,ConfigManagerErrorCode,Present FROM Win32_PnPEntity WHERE PNPClass='HIDClass' OR PNPClass='USB' OR PNPClass IS NULL",options))using(var collection=search.Get())foreach(ManagementObject o in collection)using(o){
    if(o["Present"]!=null && !(bool)o["Present"])continue;
    string name=S(o,"Name"),id=S(o,"PNPDeviceID");int? code=o["ConfigManagerErrorCode"]==null?(int?)null:Convert.ToInt32(o["ConfigManagerErrorCode"]);
    bool game=IsGameDevice(name,o["CompatibleID"] as string[]);
    if(!game&&!(id.StartsWith("USB\\",StringComparison.OrdinalIgnoreCase)&&code.HasValue&&code!=0))continue;
    result.Devices.Add(new DeviceInfo{Name=string.IsNullOrWhiteSpace(name)?id:name,Id=id,Error=code,IsController=game});
   }
   if(result.Devices.Count>0)using(var search=new ManagementObjectSearcher("root\\CIMV2","SELECT DeviceID,DriverProviderName,DriverVersion FROM Win32_PnPSignedDriver",options))using(var collection=search.Get())foreach(ManagementObject o in collection)using(o){var d=result.Devices.FirstOrDefault(x=>string.Equals(x.Id,S(o,"DeviceID"),StringComparison.OrdinalIgnoreCase));if(d!=null){d.Driver=S(o,"DriverProviderName");d.Version=S(o,"DriverVersion");}}
  }catch{result.Incomplete=true;}
  return result;
 }
}
