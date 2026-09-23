using System;
using System.Linq;
class DeviceCheckTests {
 static int count;
 static void Check(bool value,string label){if(!value)throw new Exception(label);count++;Console.WriteLine("PASS "+label);}
 static int Main(){try{
  Check(DeviceCheck.IsGameDevice("Generic HID",new[]{"HID_DEVICE_UP:0001_U:0004"}),"joystick HID usage recognized");
  Check(DeviceCheck.IsGameDevice("Generic HID",new[]{"HID_DEVICE_SYSTEM_GAME"}),"game HID recognized regardless of display language");
  Check(!DeviceCheck.IsGameDevice("Keyboard",new[]{"HID_DEVICE_UP:0001_U:0006"}),"keyboard not classified as joystick");
  Check(!DeviceCheck.IsGameDevice("Mouse",new[]{"HID_DEVICE_UP:0001_U:0002"}),"mouse not classified as joystick");
  Check(new DeviceInfo{Error=28}.State=="MissingDriver","missing driver code 28");
  Check(new DeviceInfo{Error=22}.State=="DisabledDevice","disabled device code 22");
  Check(new DeviceInfo{Error=10}.State=="DeviceProblem","cannot start code 10");
  Check(new DeviceInfo{Error=null}.State=="UnknownDevice","missing status is not healthy");
  Check(new DeviceInfo{Error=0,Driver="Microsoft",Version="old"}.State=="DeviceReady","inbox driver is not treated as a fault");
  Check(DeviceCheck.SupportUrl(new DeviceInfo{Name="Unknown"}).StartsWith("https://support.microsoft.com/"),"unknown vendor gets Windows guidance");
  Check(DeviceCheck.SupportUrl(new DeviceInfo{Name="Thrustmaster HOTAS"})=="https://support.thrustmaster.com/en/","known vendor uses official support");
  var report=new DeviceReport{Incomplete=true};report.Devices.Add(new DeviceInfo{Name="USB device",Id="USB_TEST",Error=28,IsController=false});
  foreach(var language in SetupLocalization.All){string text=report.Format(language);Check(text.Contains(language["ScanIncomplete"])&&text.Contains(language["NoDevices"])&&text.Contains(language["MissingDriver"])&&text.Contains(language["DriverAdvice"]),"actionable partial diagnosis in "+language.Code);}
  Console.WriteLine(count+" device diagnosis checks passed");return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
