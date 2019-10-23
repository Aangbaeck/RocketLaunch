using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using RocketLaunch.Helper;
using RocketLaunch.Model;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Management;
using System.Management.Automation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using IWshRuntimeLibrary;
using Newtonsoft.Json.Bson;
using ProtoBuf;
using Newtonsoft.Json.Serialization;
using RocketLaunch.Indexing.SuffixTree;
using Serilog;
using Shell32;
using TrieImplementation;
using File = System.IO.File;

namespace RocketLaunch.Services
{
    public class IndexingService : ViewModelBase
    {
        private int _nrOfPaths;
        private SettingsService _s;
        private int _progress;
        private int _generalSearchTime;
        public DateTime LastIndexed { get; set; } = DateTime.MinValue;

        public IndexingService(SettingsService s)
        {
            S = s;
            if (File.Exists(Common.Directory + "MatcherGeneral.trie") && File.Exists(Common.Directory + "MatcherPrio.trie"))
            {
                LoadTries();
                if (NrOfPaths == 0)
                {
                    Log.Debug("Something is wrong with the loading. Let's reindex...");
                    RunIndexingFirstTime();
                }
            }
            else
                RunIndexingFirstTime();

            S.Settings.SearchDirectories.ListChanged += ListChanged;



            var timer = new Timer(s.Settings.ReindexingTime * 1000 * 60);
            timer.Start();
            timer.Elapsed += (_, __) =>
            {
                RunIndexing();
            };
        }

        private void ListChanged(object sender, ListChangedEventArgs e)
        {
            try
            {
                if (e.ListChangedType == ListChangedType.ItemAdded)
                {
                    RunIndexingOnSingleFolder(S.Settings.SearchDirectories[e.NewIndex]);
                }
                else if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor.Name != nameof(FolderSearch.NrOfFiles)) //We don't care if the count has changed
                {
                    RunIndexingOnSingleFolder(S.Settings.SearchDirectories[e.NewIndex]);
                }
                else if (e.ListChangedType == ListChangedType.ItemDeleted)
                {
                    RunIndexing();
                }
                else
                {

                }
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Could not update indexes when search paths list changed");
            }


        }

        private void RunIndexingFirstTime()
        {
            RunIndexing();
            //Add settings and other good stuff
            AddGoodStuffToPrioMatcher();
        }

        public void CleanIndexes()
        {
            if (File.Exists(Common.Directory + "MatcherGeneral.trie"))
                File.Delete(Common.Directory + "MatcherGeneral.trie");
            if (File.Exists(Common.Directory + "MatcherPrio.trie"))
                File.Delete(Common.Directory + "MatcherPrio.trie");
            MatcherGeneral = new Indexing.SuffixTree.Trie<RunItem>();
            MatcherPrio = new Indexing.SuffixTree.Trie<RunItem>();
            RunIndexingFirstTime();
        }

        //General file matcher
        private Indexing.SuffixTree.Trie<RunItem> MatcherGeneral { get; set; } = new Indexing.SuffixTree.Trie<RunItem>();
        //This matcher is for things that has been run before. They are of course more prioritized.
        private Indexing.SuffixTree.Trie<RunItem> MatcherPrio { get; set; } = new Indexing.SuffixTree.Trie<RunItem>();


        public SettingsService S { get; set; }



        public int NrOfPaths
        {
            get { return _nrOfPaths; }
            set { _nrOfPaths = value; RaisePropertyChanged(); }
        }

        public void RunIndexingOnSingleFolder(FolderSearch dir)
        {
            Task.Run(() =>
            {
                try
                {
                    if (IndexingIsRunning) return;
                    IndexingIsRunning = true;

                    var unorderedFiles = new ConcurrentDictionary<string, RunItem>();
                    LastIndexed = DateTime.Now;

                    var tempBag = new ConcurrentDictionary<string, RunItem>();
                    ProcessDirectory(dir, tempBag);
                    dir.NrOfFiles = tempBag.Count;
                    foreach (var item in tempBag)
                    {
                        unorderedFiles.TryAdd(item.Key, item.Value);
                    }

                    RunItem[] tempList = unorderedFiles.Select(p => p.Value).ToArray();
                    tempList = tempList.GroupBy(p => p.URI).Select(p => p.First()).ToArray();

                    for (int i = 0; i < tempList.Length; i++)
                    {
                        Progress = i / tempList.Length * 100;
                        MatcherGeneral.Insert(tempList[i].Name.ToLower(), tempList[i]);
                        //if (!string.IsNullOrEmpty(tempList[i].Group))
                        //    TempMatcher.Insert(tempList[i].Group.ToLower(), tempList[i]);
                    }

                    NrOfPaths = MatcherGeneral.DataDictionary.Count + MatcherPrio.DataDictionary.Count;
                    SaveTries();

                    IndexingIsRunning = false;
                }
                catch (Exception e)
                {
                    Log.Error(e, $"Could not search folder {dir.Path}");
                }


            });
        }

        private List<RunItem> AddWindows10Apps()
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
                        runItem.Command = packageFamily;

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

        private List<RunItem> AddWindows10AppsBetterWay()
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
                            }
                        }
                        catch (Exception e)
                        {


                        }


                        list.Add(runItem);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Could not add win 10 app {item}");
                    }
                }
            }

            return list;

        }


        private (string icon, string background) GetWin10Icon(string installLocation)
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
                            foreach (var file in assetDirectory.GetFiles($"*{extension}")) { files.Add(file); }


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

        public void RunIndexing()
        {

            Task.Run(() =>
            {
                try
                {
                    if (IndexingIsRunning) return;
                    IndexingIsRunning = true;

                    var tempTrie = new Indexing.SuffixTree.Trie<RunItem>();

                    foreach (var dir in S.Settings.SearchDirectories) { dir.NrOfFiles = 0; } //Resetting this so it looks nice when gui is propagating the new info..


                    LastIndexed = DateTime.Now;
                    //Spread out the search on all directories on different tasks

                    //try
                    //{
                    //    var win10List = AddWindows10AppsBetterWay();
                    //    foreach (var item in win10List)
                    //    {
                    //        tempTrie.Insert(item.Name, item);
                    //        tempTrie.Insert(item.URI, item);
                    //    }
                    //}
                    //catch (Exception e)
                    //{
                    //    Log.Error(e, "Could not add Windows10 store apps.");
                    //}

                    var unorderedFiles = new ConcurrentDictionary<string, RunItem>();


                    foreach (FolderSearch dir in S.Settings.SearchDirectories)
                    {
                        var tempBag = new ConcurrentDictionary<string, RunItem>();
                        ProcessDirectory(dir, tempBag);
                        dir.NrOfFiles = tempBag.Count;
                        foreach (var item in tempBag)
                        {
                            unorderedFiles.TryAdd(item.Key, item.Value);
                        }
                    }

                    RunItem[] tempList = unorderedFiles.Select(p => p.Value).ToArray();
                    tempList = tempList.GroupBy(p => p.URI).Select(p => p.First()).ToArray();

                    //var sw = new Stopwatch();
                    //sw.Start();
                    for (int i = 0; i < tempList.Length; i++)
                    {
                        try
                        {
                            //Progress = i / tempList.Length * 100;

                            tempTrie.Insert(tempList[i].Name.ToLower(), tempList[i]);

                            if (tempList[i].Type == ItemType.File)
                                tempTrie.Insert(tempList[i].FileName.ToLower(), tempList[i]);
                            if (tempList[i].Type == ItemType.Win10App)
                                tempTrie.Insert(tempList[i].URI.ToLower(), tempList[i]);
                            if (tempList[i].KeyWords != null && tempList[i].KeyWords.Count > 0)
                                tempTrie.Insert(tempList[i].KeyWords, tempList[i]);

                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "Could not add file to trie");
                        }

                    }

                    MatcherGeneral = tempTrie;
                    NrOfPaths = MatcherGeneral.DataDictionary.Count + MatcherPrio.DataDictionary.Count;

                    SaveTries();

                }
                catch (Exception e)
                {
                    Log.Error(e, "Werid error in indexing service");
                }
                IndexingIsRunning = false;
            });

        }

        public bool IndexingIsRunning
        {
            get { return _indexingIsRunning; }
            set { _indexingIsRunning = value; RaisePropertyChanged(); }
        }

        private void AddGoodStuffToPrioMatcher()
        {
            Task.Run(() =>
            {
                if (S.Settings.IncludeWindowsSettings)
                {
                    foreach (var setting in RunItemFactory.Settings)
                    {
                        AddItem(setting);
                    }
                    AddItem(RunItemFactory.RunDialog());
                    AddItem(RunItemFactory.TurnOffComputer());
                    AddItem(RunItemFactory.HibernateWindows());
                    AddItem(RunItemFactory.LockWindows());
                    AddItem(RunItemFactory.LogOffWindows());
                    AddItem(RunItemFactory.RestartComputer());
                    AddItem(RunItemFactory.SleepWindows());
                    AddItem(RunItemFactory.SleepWindows());
                }
                try
                {


                    var win10List = AddWindows10AppsBetterWay();
                    foreach (var item in win10List)
                    {
                        if (!MatcherPrio.DataDictionary.ContainsKey(item.Name))
                        {
                            MatcherPrio.Insert(item.Name.ToLower(), item);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Could not add Windows10 store apps.");
                }
            });

        }

        public void AddItem(RunItem item)
        {
            if (!MatcherPrio.DataDictionary.ContainsKey(item.Name))
            {
                MatcherPrio.Insert(item.Name.ToLower(), item);
                MatcherPrio.Insert(item.KeyWords, item);
            }
        }
        public void SavePrioTrie()
        {
            try
            {
                if (S.Settings.DebugMode)
                    Log.Debug("Saving prio trie");
                using (var fs = new FileStream(Path.GetFullPath(Common.Directory + "MatcherPrio.trie"), FileMode.Create))
                {
                    Serializer.Serialize(fs, MatcherPrio);
                    fs.Flush();
                }

            }
            catch (Exception e)
            {
                Log.Error(e, "Could not save prio trie");

            }
        }

        public void SaveTries()
        {
            try
            {
                if (S.Settings.DebugMode)
                    Log.Debug("Saving general trie");
                using (var fs = new FileStream(Path.GetFullPath(Common.Directory + "MatcherGeneral.trie"), FileMode.Create))
                {
                    Serializer.Serialize(fs, MatcherGeneral);
                    fs.Flush();
                }
                SavePrioTrie();
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not save trie");

            }
        }

        public void LoadTries()
        {
            try
            {
                Log.Debug("Loading general trie");
                using (var file = File.OpenRead(Common.Directory + "MatcherGeneral.trie"))
                {
                    MatcherGeneral = Serializer.Deserialize<Trie<RunItem>>(file);
                }

                NrOfPaths = MatcherGeneral.DataDictionary.Count;

                Log.Debug("Loading prio trie");
                using (var file = File.OpenRead(Common.Directory + "MatcherPrio.trie"))
                {
                    MatcherPrio = Serializer.Deserialize<Trie<RunItem>>(file);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not load Trie");
            }
        }

        public int Progress
        {
            get { return _progress; }
            set { _progress = value; RaisePropertyChanged(); }
        }

        public int GeneralSearchTime
        {
            get { return _generalSearchTime; }
            set { _generalSearchTime = value; RaisePropertyChanged(); }
        }
        private int _prioSearchTime;

        public int PrioSearchTime
        {
            get { return _prioSearchTime; }
            set { _prioSearchTime = value; RaisePropertyChanged(); }
        }
        private int _totalSearchTime;
        private bool _indexingIsRunning;

        public int TotalSearchTime
        {
            get { return _totalSearchTime; }
            set { _totalSearchTime = value; RaisePropertyChanged(); }
        }

        public List<RunItem> Search(string s, int nrOfHits = 10)
        {
            if (s == null) s = "";
            s = s.Trim();
            var fileList = new List<RunItem>();
            var sw = new Stopwatch();
            sw.Start();
            ICollection<RunItem> generalResult = MatcherGeneral.Search(s.ToLower(), nrOfHits);
            GeneralSearchTime = (int)sw.ElapsedTicks;
            sw.Restart();
            //Console.WriteLine($"    ");

            ICollection<RunItem> prioResult = MatcherPrio.Search(s.ToLower());  //Get all results from indexing

            prioResult = prioResult.Concat(generalResult).ToList();  //put the rest of the results on the stack
            //foreach (var runItem in prioResult)
            //{
            //    Console.WriteLine($"{runItem.Name}, {runItem.RunNrOfTimes}");

            //}
            prioResult = prioResult.GroupBy(x => x.Name + x.URI + x.Command).Select(x => x.OrderByDescending(p => p.RunNrOfTimes).First()).ToList();  //remove potential duplicates
            prioResult = prioResult.ToList().OrderByDescending(p => p.RunNrOfTimes).ToList();  //sort prioresults since they will always have at least one runtime

            PrioSearchTime = (int)sw.ElapsedTicks;
            TotalSearchTime = GeneralSearchTime + PrioSearchTime;
            return prioResult.Take(10).ToList();
        }

        public void ProcessDirectory(FolderSearch dir, ConcurrentDictionary<string, RunItem> theBag)
        {
            try
            {

                if (Directory.Exists(dir.Path))
                {
                    if (dir.IncludeFoldersInSearch)
                    {
                        var folder = new RunItem()
                        {
                            Name = new DirectoryInfo(dir.Path).Name,
                            URI = new StringBuilder(dir.Path).Replace("/", "\\").Replace("//", "\\").ToString(),
                            Type = ItemType.Directory
                        };
                        theBag.TryAdd(folder.URI, folder);
                    }
                }
                else
                {
                    Log.Error($"{dir.Path} does not exist...");
                    return;
                }


                //ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Product");
                //foreach (ManagementObject mgmtObjectin in searcher.Get())
                //{
                //    Console.WriteLine(mgmtObjectin["Name"]);
                //    Console.WriteLine(mgmtObjectin.Path);
                //}




                //var t = theBag.ContainsKey(dir.URI);
                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(dir.Path, dir.SearchPattern);
                foreach (string fileName in fileEntries)
                {



                    try
                    {




                        if (Path.GetExtension(fileName) == ".lnk")
                        {
                            var item = new RunItem();
                            WshShell shell = new WshShell(); //Create a new WshShell Interface
                            IWshShortcut link = (IWshShortcut)shell.CreateShortcut(fileName); //Link the interface to our shortcut
                            var uri = link.TargetPath;
                            item.Arguments = link.Arguments;
                            var t = link.FullName;

                            if (!System.IO.File.Exists(uri))  //It sometimes mixes up the program files folder. Lets check both.
                            {
                                if (uri.Contains("Program Files (x86)"))
                                {
                                    uri = new StringBuilder(uri).Replace("Program Files (x86)", "Program Files").ToString();
                                }
                                if (!File.Exists(uri) && uri.Contains("Program Files"))
                                    uri = new StringBuilder(uri).Replace("Program Files", "Program Files (x86)").ToString();
                            }

                            if (System.IO.File.Exists(uri) && !uri.Contains("C:\\Windows\\Installer\\"))  //No reason to add a broken link.
                            {
                                item.Type = ItemType.File;
                                item.URI = new StringBuilder(uri).Replace("/", "\\").Replace("//", "\\").ToString();
                                item.Name = System.IO.Path.GetFileNameWithoutExtension(fileName);
                                item.KeyWords = new List<string>() { Path.GetFileName(uri) };
                                theBag.TryAdd(item.URI, item);
                            }
                        }
                        else
                        {
                            var item = new RunItem();
                            item.Type = ItemType.File;
                            item.URI = new StringBuilder(fileName).Replace("/", "\\").Replace("//", "\\").ToString();
                            item.Name = item.FileName;
                            if (!theBag.ContainsKey(item.URI))
                                theBag.TryAdd(item.URI, item);
                        }
                    }
                    catch (Exception e)
                    {
                        if (S.Settings.DebugMode)
                            Log.Error(e, $"Could not index {fileName}");
                    }


                }
            }
            catch (Exception e)
            {
                if (S.Settings.DebugMode)
                    Log.Debug($"Could not search {dir.Path}");
            }
            if (dir.SearchSubFolders)
            {
                try
                {
                    // Recurse into subdirectories of this directory.
                    string[] subdirectoryEntries = Directory.GetDirectories(dir.Path);
                    foreach (string subdirectory in subdirectoryEntries)
                        try
                        {
                            ProcessDirectory(new FolderSearch() { Path = subdirectory, IncludeFoldersInSearch = dir.IncludeFoldersInSearch, SearchPattern = dir.SearchPattern, SearchSubFolders = dir.SearchSubFolders }, theBag);
                        }
                        catch (Exception e)
                        {
                            Log.Debug(e.Message);
                        }
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
            }
        }
        [STAThread]
        public static string GetShortcutTargetFile(string shortcutFilename)
        {
            string pathOnly = System.IO.Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = System.IO.Path.GetFileName(shortcutFilename);

            Shell shell = new Shell();
            Shell32.Folder folder = shell.NameSpace(pathOnly);
            FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
                return link.Path;
            }

            return string.Empty;
        }

        public void AddExecutedItem(RunItem exItem)
        {
            MatcherGeneral.Remove(exItem.Name.ToLower());
            exItem.RunNrOfTimes++;
            MatcherPrio.Replace(exItem.Name, exItem);
            SaveTries();
        }
        public void ResetItemRunCounter(RunItem exItem)
        {
            exItem.RunNrOfTimes = 0;
            if (exItem.Type == ItemType.File || exItem.Type == ItemType.Directory)  //Move it back to general list. Settings and other stuff will always be in the priolist.
            {
                MatcherPrio.Remove(exItem.Name.ToLower());
                MatcherGeneral.Insert(exItem.Name.ToLower(), exItem);
            }
            SaveTries();
        }


    }


}
