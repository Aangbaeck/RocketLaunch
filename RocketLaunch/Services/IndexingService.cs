using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using GalaSoft.MvvmLight;
using IWshRuntimeLibrary;
using ProtoBuf;
using RocketLaunch.Helper;
using RocketLaunch.Indexing.SuffixTree;
using RocketLaunch.Model;
using Serilog;
using Shell32;
using File = System.IO.File;
using Timer = System.Timers.Timer;

namespace RocketLaunch.Services
{

    public class IndexingService : ViewModelBase
    {
        private int _generalSearchTime;
        private bool _indexingIsRunning;
        private int _nrOfPaths;
        private int _prioSearchTime;
        private int _progress;
        private int _totalSearchTime;

        public IndexingService(SettingsService s)
        {
            S = s;
            if (File.Exists(Common.Directory + "MatcherGeneral.trie") &&
                File.Exists(Common.Directory + "MatcherPrio.trie"))
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
            timer.Elapsed += (_, __) => { RunIndexing(); };
        }

        public DateTime LastIndexed { get; set; } = DateTime.MinValue;

        //General file matcher
        private Trie<RunItem> MatcherGeneral { get; set; } = new Trie<RunItem>();

        //This matcher is for things that has been run before. They are of course more prioritized.
        private Trie<RunItem> MatcherPrio { get; set; } = new Trie<RunItem>();


        public SettingsService S { get; set; }


        public int NrOfPaths
        {
            get { return _nrOfPaths; }
            set
            {
                _nrOfPaths = value;
                RaisePropertyChanged();
            }
        }

        public bool IndexingIsRunning
        {
            get { return _indexingIsRunning; }
            set
            {
                _indexingIsRunning = value;
                RaisePropertyChanged();
            }
        }

        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                RaisePropertyChanged();
            }
        }

        public int GeneralSearchTime
        {
            get { return _generalSearchTime; }
            set
            {
                _generalSearchTime = value;
                RaisePropertyChanged();
            }
        }

        public int PrioSearchTime
        {
            get { return _prioSearchTime; }
            set
            {
                _prioSearchTime = value;
                RaisePropertyChanged();
            }
        }

        public int TotalSearchTime
        {
            get { return _totalSearchTime; }
            set
            {
                _totalSearchTime = value;
                RaisePropertyChanged();
            }
        }

        private void ListChanged(object sender, ListChangedEventArgs e)
        {
            try
            {
                if (e.ListChangedType == ListChangedType.ItemAdded)
                {
                    RunIndexingOnSingleFolder(S.Settings.SearchDirectories[e.NewIndex]);
                }
                else if (e.ListChangedType == ListChangedType.ItemChanged &&
                         e.PropertyDescriptor.Name != nameof(FolderSearch.NrOfFiles)
                ) //We don't care if the count has changed
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
            MatcherGeneral = new Trie<RunItem>();
            MatcherPrio = new Trie<RunItem>();
            RunIndexingFirstTime();
        }

        public void RunIndexingOnSingleFolder(FolderSearch dir)
        {
            Task.Run(() =>
            {
                try
                {
                    if (IndexingIsRunning) return;
                    IndexingIsRunning = true;

                    var unorderedFiles = new HashSet<RunItem>();
                    LastIndexed = DateTime.Now;

                    var tempBag = new HashSet<RunItem>();
                    ProcessDirectory(dir, tempBag);
                    dir.NrOfFiles = tempBag.Count;
                    foreach (var item in tempBag)
                    {
                        unorderedFiles.Add(item);
                    }

                    RunItem[] tempList = unorderedFiles.ToArray();
                    tempList = tempList.GroupBy(p => p.URI + p.Arguments + p.Command).Select(p => p.First()).ToArray();

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



        public void RunIndexing()
        {
            Task.Run(() =>
            {
                try
                {
                    if (IndexingIsRunning) return;
                    IndexingIsRunning = true;

                    var tempTrie = new Trie<RunItem>();



                    foreach (var dir in S.Settings.SearchDirectories)
                    {
                        dir.NrOfFiles = 0;
                    } //Resetting this so it looks nice when gui is propagating the new info..


                    LastIndexed = DateTime.Now;

                    var unorderedFiles = new HashSet<RunItem>();


                    foreach (FolderSearch dir in S.Settings.SearchDirectories)
                    {
                        var tempBag = new HashSet<RunItem>();
                        ProcessDirectory(dir, tempBag);
                        dir.NrOfFiles = tempBag.Count;
                        foreach (var item in tempBag)
                        {
                            unorderedFiles.Add(item);
                        }
                    }

                    RunItem[] tempList = unorderedFiles.ToArray();
                    tempList = tempList.GroupBy(p => p.URI + p.Arguments + p.Command).Select(p => p.First()).ToArray();

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

                    try
                    {
                        var win10List = Win10AppsSearcher.AddWindows10AppsBetterWay();
                        foreach (var item in win10List)
                        {
                            if (tempTrie.DataDictionary.ContainsKey(item.Name.ToLower()))
                            {
                                if (item.IconName != null)
                                {

                                }
                                
                            }
                            else
                            {
                                tempTrie.Insert(item.Name.ToLower(), item);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Could not add Windows10 store apps.");
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
                using var fs = new FileStream(Path.GetFullPath(Common.Directory + "MatcherPrio.trie"),
                    FileMode.Create);
                Serializer.Serialize(fs, MatcherPrio);
                fs.Flush();
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
                using (var fs = new FileStream(Path.GetFullPath(Common.Directory + "MatcherGeneral.trie"),
                    FileMode.Create))
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

            ICollection<RunItem> prioResult = MatcherPrio.Search(s.ToLower()); //Get all results from indexing

            prioResult = prioResult.Concat(generalResult).ToList(); //put the rest of the results on the stack
            //foreach (var runItem in prioResult)
            //{
            //    Console.WriteLine($"{runItem.Name}, {runItem.RunNrOfTimes}");

            //}
            prioResult = prioResult.GroupBy(x => x.Name + x.URI + x.Command)
                .Select(x => x.OrderByDescending(p => p.RunNrOfTimes).First()).ToList(); //remove potential duplicates
            prioResult =
                prioResult.ToList().OrderByDescending(p => p.RunNrOfTimes)
                    .ToList(); //sort prioresults since they will always have at least one runtime

            PrioSearchTime = (int)sw.ElapsedTicks;
            TotalSearchTime = GeneralSearchTime + PrioSearchTime;
            return prioResult.Take(10).ToList();
        }
        [STAThread]
        public void ProcessDirectory(FolderSearch dir, HashSet<RunItem> theBag)
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
                        theBag.Add(folder);
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
                            Directory.CreateDirectory(Common.LinksPath);
                            File.Copy(fileName, Common.LinksPath + Path.GetFileName(fileName), true);

                            var item = new RunItem();
                            item.Type = ItemType.Link;
                            item.Command = Common.LinksPath + Path.GetFileName(fileName);
                            WshShell shell = new WshShell(); //Create a new WshShell Interface
                            IWshShortcut link = (IWshShortcut)shell.CreateShortcut(item.Command); //Link the interface to our shortcut
                            var uri = link.TargetPath;
                            item.URI = uri;
                            item.Arguments = link.Arguments;

                            //var t = link.FullName;
                            //var t2 = link.Description;
                            //var t3 = link.WorkingDirectory;
                            //var t4 = link.FullName;



                            var split = link.IconLocation.Split(',');
                            var iconName = split[0];
                            item.IconNr = Convert.ToInt32(split[1]);

                            if (File.Exists(iconName))
                            {
                                item.IconName = iconName;
                            }


                            item.Name = Path.GetFileNameWithoutExtension(item.Command).Replace(" - Shortcut", "");  //get the name of the icon and removing the shortcut ending that sometimes are there, because nice-ness
                            if(uri != "")
                                item.KeyWords.Add(Path.GetFileName(uri));
                            theBag.Add(item);
                        }
                        else
                        {
                            var item = new RunItem
                            {
                                Type = ItemType.File,
                                URI = new StringBuilder(fileName).Replace("/", "\\").Replace("//", "\\").ToString()
                            };
                            item.Name = item.FileName;
                            theBag.Add(item);
                        }
                    }
                    catch (Exception e)
                    {
                        if (S.Settings.DebugMode)
                            Log.Error(e, $"Could not index {fileName}");
                    }
                }
            }
            catch (Exception)
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
                            ProcessDirectory(
                                new FolderSearch()
                                {
                                    Path = subdirectory,
                                    IncludeFoldersInSearch = dir.IncludeFoldersInSearch,
                                    SearchPattern = dir.SearchPattern,
                                    SearchSubFolders = dir.SearchSubFolders
                                }, theBag);
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


        // Get information about this link.
        // Return an error message if there's a problem.
        
        [STAThread]
        private static string GetShortcutInfo(string full_name,out string name, out string path, out string descr,out string working_dir, out string args)
        {
            name = "";
            path = "";
            descr = "";
            working_dir = "";
            args = "";
            try
            {
                // Make a Shell object.
                Shell32.Shell shell = new Shell32.Shell();

                // Get the shortcut's folder and name.
                string shortcut_path =
                    full_name.Substring(0, full_name.LastIndexOf("\\"));
                string shortcut_name =
                    full_name.Substring(full_name.LastIndexOf("\\") + 1);
                if (!shortcut_name.EndsWith(".lnk"))
                    shortcut_name += ".lnk";

                // Get the shortcut's folder.
                Shell32.Folder shortcut_folder =
                    shell.NameSpace(shortcut_path);

                // Get the shortcut's file.
                Shell32.FolderItem folder_item =
                    shortcut_folder.Items().Item(shortcut_name);

                if (folder_item == null)
                    return "Cannot find shortcut file '" + full_name + "'";
                if (!folder_item.IsLink)
                    return "File '" + full_name + "' isn't a shortcut.";

                // Display the shortcut's information.
                Shell32.ShellLinkObject lnk =
                    (Shell32.ShellLinkObject)folder_item.GetLink;
                name = folder_item.Name;
                descr = lnk.Description;
                path = lnk.Path;
                working_dir = lnk.WorkingDirectory;
                args = lnk.Arguments;
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public string IconLink { get; set; }

        [STAThread]
        public static string GetShortcutTargetFile(string shortcutFilename)
        {
            string pathOnly = Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = Path.GetFileName(shortcutFilename);

            Shell shell = new Shell();
            Shell32.Folder folder = shell.NameSpace(pathOnly);
            FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                ShellLinkObject link = (ShellLinkObject)folderItem.GetLink;
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
            if (exItem.Type == ItemType.File || exItem.Type == ItemType.Directory
            ) //Move it back to general list. Settings and other stuff will always be in the priolist.
            {
                MatcherPrio.Remove(exItem.Name.ToLower());
                MatcherGeneral.Insert(exItem.Name.ToLower(), exItem);
            }

            SaveTries();
        }
    }
}