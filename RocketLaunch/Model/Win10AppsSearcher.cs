using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RocketLaunch.Helper;
using Serilog;

namespace RocketLaunch.Model
{
    public class Win10AppsSearcher
    {
        private static List<RunItem> AddWindows10Apps()
        {
            var list = new List<RunItem>();
            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                // use "AddScript" to add the contents of a script file to the end of the execution pipeline.
                // use "AddCommand" to add individual commands/cmdlets to the end of the execution pipeline.
                PowerShellInstance.AddScript("get-appxpackage");

                // invoke execution on the pipeline (collecting output)
                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();
                // use "AddParameter" to add a single parameter to the last command/script on the pipeline.
                //PowerShellInstance.AddParameter("param1", "parameter 1 value!");
                foreach (PSObject outputItem in PSOutput)
                {
                    //TODO: handle/process the output items if required
                    var item = outputItem.BaseObject;


                    var propertyInfo = item.GetType().GetProperty("NonRemovable");
                    var NonRemovable = (bool)propertyInfo.GetValue(item, null);
                    propertyInfo = item.GetType().GetProperty("IsFramework");
                    var IsFramework = (bool)propertyInfo.GetValue(item, null);
                    //var cast = (Microsoft.Windows.Appx.PackageManager.Commands.AppxPackage)

                    if (!NonRemovable && !IsFramework)
                    {
                        var runItem = new RunItem() { Type = ItemType.Win10App };

                        propertyInfo = item.GetType().GetProperty("PackageFamilyName");
                        var packageFamily = (string)propertyInfo.GetValue(item, null);
                        runItem.Command = packageFamily + "!App";

                        propertyInfo = item.GetType().GetProperty("Name");
                        var names = ((string)propertyInfo.GetValue(item, null)).Split('.').ToList();
                        runItem.Name = names.Last();

                        names.Remove(names.Last());
                        runItem.URI = string.Join(".", names.ToArray());

                        propertyInfo = item.GetType().GetProperty("InstallLocation");
                        var installLocation = (string)propertyInfo.GetValue(item, null);
                        var icon = GetWin10Icon(installLocation + "\\AppxManifest.xml");
                        runItem.IconName = icon.icon;
                        runItem.IconBackGround = icon.background;
                        list.Add(runItem);
                        //Console.WriteLine(packageFamily);
                    }
                }
            }

            return list;
        }
        
        public static List<RunItem> AddWindows10AppsBetterWay()
        {
            var win10AppList = AddWindows10Apps();

            var list = new List<RunItem>();
            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                // use "AddScript" to add the contents of a script file to the end of the execution pipeline.
                // use "AddCommand" to add individual commands/cmdlets to the end of the execution pipeline.
                PowerShellInstance.AddScript("Get-StartApps");

                // invoke execution on the pipeline (collecting output)
                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();
                // use "AddParameter" to add a single parameter to the last command/script on the pipeline.
                //PowerShellInstance.AddParameter("param1", "parameter 1 value!");

                var dirs = Directory.GetDirectories("C:\\Program Files\\WindowsApps");
                foreach (PSObject outputItem in PSOutput)
                {
                    //TODO: handle/process the output items if required
                    var item = outputItem.ToString();
                    try
                    {
                        var runItem = new RunItem() { Type = ItemType.Win10App };

                        var split = item.Split('=');
                        var name = split[1].Split(';')[0];
                        var appID = split[2].Substring(0, split[2].Length - 1);
                        runItem.Command = appID;
                        runItem.Name = name;
                        var uri = appID;
                        int count = uri.Count(f => f == '{');

                        if (count > 0)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var startIndex = uri.IndexOf("{", StringComparison.Ordinal);
                                var endIndex = uri.IndexOf("}", StringComparison.Ordinal);
                                string guids = uri.Substring(startIndex + 1, endIndex - startIndex - 1);
                                var guid = new Guid(guids);
                                var knownFolder = KnownFolderFinder.GetFolderFromKnownFolderGUID(guid);
                                if (knownFolder != null)
                                    uri = uri.Replace($"{{{guids}}}", knownFolder);
                            }
                        }

                        runItem.URI = uri;
                        if (!File.Exists(runItem.URI))
                            runItem.URI = "Win 10 app";


                        //propertyInfo = item.GetType().GetProperty("InstallLocation");
                        //var installLocation = (string)propertyInfo.GetValue(item, null);
                        //var icon = GetWin10Icon(installLocation + "\\AppxManifest.xml");


                        try
                        {
                            var hit = win10AppList.FirstOrDefault(p => p.Command.Contains(uri.Split('!')[0]));
                            if (hit != null)
                            {
                                runItem.IconName = hit.IconName;
                                runItem.IconBackGround = hit.IconBackGround;
                                win10AppList.Remove(hit);
                            }
                        }
                        catch (Exception)
                        {
                            Log.Error("Could not att win 10 icon.");
                        }


                        list.Add(runItem);
                    }
                    catch (Exception)
                    {
                        Log.Error($"Could not add win 10 app {item}");
                    }
                }
            }
            list.AddRange(win10AppList);

            return list;
        }


        private static (string icon, string background) GetWin10Icon(string installLocation)
        {
            XmlDocument doc = new XmlDocument();
            if (File.Exists(installLocation))
            {
                try
                {
                    var data = File.ReadAllLines(installLocation).ToList();
                    data.RemoveAt(0);

                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(string.Join("", data.ToArray()));

                    XmlNodeList node = xml.GetElementsByTagName("uap:VisualElements");
                    if (node.Count == 0)
                        node = xml.GetElementsByTagName("VisualElements");
                    if (node.Count > 0)
                    {
                        var logoNode = node.Item(0).Attributes["Square44x44Logo"];
                        if (logoNode == null)
                        {
                            logoNode = node.Item(0).Attributes["Logo"];
                        }

                        if (logoNode != null)
                        {
                            var assetDirectory =
                                Path.GetDirectoryName(installLocation) + "\\" + Path.GetDirectoryName(logoNode.Value);
                            var extension = Path.GetExtension(logoNode.Value);
                            var files = new List<string>();
                            foreach (var file in assetDirectory.GetFiles($"*{extension}"))
                            {
                                files.Add(file);
                            }


                            var fileName = Path.GetFileNameWithoutExtension(logoNode.Value);


                            var path = files.Where(s => s.ToLower().Contains(fileName.ToLower()));

                            if (path.Any(p => p.Contains("44")))
                                path = path.Where(p => p.Contains("44"));
                            if (path.Any(p => p.Contains("light")))
                                path = path.Where(p => p.Contains("light"));

                            string logoPath = path.First();

                            string background = node.Item(0).Attributes["BackgroundColor"].Value;
                            return (logoPath, background);
                        }
                        else
                        {
                            Log.Debug($"Could not find Icon name in {installLocation}");
                        }
                    }
                    else
                    {
                        Log.Debug($"Could not find VisualElements for icon in {installLocation}");
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Broken xml file");
                }
            }

            return ("", "");
        }
    }
}
