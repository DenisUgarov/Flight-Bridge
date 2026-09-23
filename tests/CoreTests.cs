using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FSMigrator;
class CoreTests {
 static int count;static void Check(bool b,string n){if(!b)throw new Exception(n);Console.WriteLine("PASS "+n);count++;}
 static void Throws(Action a,string n){bool failed=false;try{a();}catch{failed=true;}Check(failed,n);}
 static int Main(){try{
 string dir=Path.Combine(Path.GetTempPath(),"FlightBridge-tests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
 string s=Path.Combine(dir,"2020.xml"),t=Path.Combine(dir,"2024.xml");
 File.WriteAllText(s,"<DefaultInput><Version Num='1'/><FriendlyName>Stick</FriendlyName><Device DeviceName='Joystick' ProductID='123'><Axes><Axis AxisName='X' AxisDeadZone='12'/></Axes><Context ContextName='PLANE'><Action ActionName='KEY_A' Flag='132'><Primary><KEY Information='X'>512</KEY></Primary></Action><Action ActionName='KEY_UNKNOWN' Flag='2'><Primary><KEY>1</KEY></Primary></Action></Context></Device></DefaultInput>");
 File.WriteAllText(t,"<DefaultInput ProfileName='2024'><Version Num='20'/><Device DeviceName='Joystick' ProductID='0x7b' GUID='target-guid'><Axes><Axis AxisName='X' AxisDeadZone='0'/></Axes><Context ContextName='PLANE'><Action ActionName='KEY_A' Flag='4'/><Action ActionName='KEY_B' Flag='2'><Primary><KEY>7</KEY></Primary></Action></Context></Device></DefaultInput>");
 var p=Engine.Analyze(Profile.Read(s),Profile.Read(t));Check(p.Copied==1 && p.Skipped.Count==1,"matching and unknown actions");Check((string)p.Output.Root.Element("Device").Attribute("GUID")=="target-guid","target identity preserved");Check((string)p.Output.Root.Element("Version").Attribute("Num")=="20","version preserved");Check((string)p.Output.Descendants("Action").First().Attribute("Flag")=="4","native MSFS 2024 action flag preserved");Check((string)p.Output.Descendants("Axis").First().Attribute("AxisDeadZone")=="12","axis tuning");Check(p.Output.Descendants("Action").Last().Descendants("KEY").Single().Value=="7","unrelated target bindings unchanged");
 string sh=Engine.Hash(s),th=Engine.Hash(t);string result=Engine.Export(p,dir,true);Check(sh==Engine.Hash(s)&&th==Engine.Hash(t),"source files unchanged");Check(File.Exists(Path.Combine(result,"backup.xml")),"backup created by default workflow");
 string restored=Path.Combine(dir,"restored.xml");Engine.Restore(Path.Combine(result,"backup.xml"),restored);Check(Engine.Hash(restored)==th,"byte exact restoration");Throws(()=>Engine.Restore(Path.Combine(result,"backup.xml"),restored),"restore cannot overwrite");File.AppendAllText(Path.Combine(result,"original-2024.xml")," ");Throws(()=>Engine.Restore(Path.Combine(result,"backup.xml"),Path.Combine(dir,"bad.xml")),"tamper detected");
 var bad=Profile.Read(t);bad.Device.SetAttributeValue("ProductID","999");Throws(()=>Engine.Analyze(Profile.Read(s),bad),"wrong device blocked");
 var other=Profile.Read(t);other.Device.Element("Context").SetAttributeValue("ContextName","OTHER");Check(Engine.Analyze(Profile.Read(s),other).Copied==0,"cross context matching blocked");
 var dup=Profile.Read(t);dup.Device.Element("Context").Add(new XElement(dup.Device.Descendants("Action").First()));Check(Engine.Analyze(Profile.Read(s),dup).Copied==0,"ambiguous matches skipped");
 File.AppendAllText(t,"<!-- changed -->");Throws(()=>Engine.Export(p,dir,true),"stale preview blocked");
 string xxe=Path.Combine(dir,"xxe.xml");File.WriteAllText(xxe,"<!DOCTYPE x [<!ENTITY e SYSTEM 'file:///C:/Windows/win.ini'>]><x>&e;</x>");Throws(()=>Profile.Read(xxe),"DTD rejected");
 var fs=Profile.Read("tests/fixtures/2020-fragment.xml");var ft=Profile.Read("tests/fixtures/2024-fragment.xml");
 Check(fs.IsFragment && ft.IsFragment,"MSFS multi-root fragments read");
 var fp=Engine.Analyze(fs,ft,true);Check(fp.Copied==2 && fp.Relocated==2,"unique event relocation from PLANE to 2024 contexts");
 Check(fp.Warnings.Any(x=>x.Contains("KEY_BRAKES")),"shared input conflict reported");
 Check(!fp.Output.Descendants("Action").First().Elements("Axis").Any(),"stale target per-action sensitivity removed");
 Check((string)fp.Output.Descendants("Action").First().Attribute("Delay")=="2","native MSFS 2024 action delay preserved");
 Check(fp.OutputName!=ft.Name && fp.Output.Root.Element("FriendlyName").Attribute("PlatformAvailability").Value=="1","new name and platform preserved");
 string batch=Library.ExportBatch(new[]{fp,Engine.Analyze(fs,ft,true)},Path.Combine(dir,"library"),true);
 Check(File.Exists(Path.Combine(batch,"session.xml")) && !Directory.GetDirectories(Path.Combine(dir,"library")).Any(x=>Path.GetFileName(x).StartsWith(".")),"batch published after completion");
 var backups=Library.Backups(Path.Combine(dir,"library"));Check(backups.Count==2,"backup library lists all batch profiles");
 string migrated=Path.Combine(Path.GetDirectoryName(backups[0].Manifest),"MSFS2024-migrated.xml");Check(Profile.Read(migrated).IsFragment && !File.ReadAllText(migrated).Contains("ProfileFragment"),"export preserves native fragment structure");
 string restoredFragment=Path.Combine(dir,"fragment-restored.xml");Engine.Restore(backups[0].Manifest,restoredFragment);Check(Engine.Hash(restoredFragment)==Engine.Hash(ft.Path),"fragment backup restores byte for byte");
 var canceled=new System.Threading.CancellationToken(true);Throws(()=>Engine.Scan(dir,0,canceled).ToList(),"scan cancellation");
 string before=Path.Combine(dir,"invalid-batch");Throws(()=>Library.ExportBatch(new[]{Engine.Analyze(fs,ft)},before,true),"zero-match batch blocked");Check(!Directory.Exists(before),"failed preflight creates no partial library");
 string noBackup=Library.ExportBatch(new[]{fp},Path.Combine(dir,"no-backup"),false);Check(Library.Backups(Path.Combine(dir,"no-backup")).Count==0 && !Directory.GetFiles(noBackup,"backup.xml",SearchOption.AllDirectories).Any(),"backup opt-out respected");
 foreach(string external in new[]{"tests/external/2024-aircraft.xml","tests/external/2024-general.xml"})if(File.Exists(external)) {var real=Profile.Read(external);Check(real.IsFragment && real.Device.Descendants("Action").Count()>100,"public MSFS export parsed: "+external);string roundtrip=Path.Combine(dir,Path.GetFileName(external));real.Save(real.Xml,roundtrip);Check(XNode.DeepEquals(real.Xml,Profile.Read(roundtrip).Xml),"public profile roundtrip: "+external);if(external.Contains("aircraft"))Check(Engine.Analyze(fs,real,true).Copied==2,"synthetic 2020 bindings match public 2024 commands");}
 Console.WriteLine(count+" tests passed. Fixtures: "+dir);return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
