using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using RocketLaunch.Helper;
using RocketLaunch.Model;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using IWshRuntimeLibrary;
using Newtonsoft.Json.Bson;
using ProtoBuf;
using Newtonsoft.Json.Serialization;
using RocketLaunch.Indexing.SuffixTree;
using Serilog;
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

        public void RunIndexing()
        {

            Task.Run(() =>
            {
                try
                {


                    if (IndexingIsRunning) return;
                    IndexingIsRunning = true;

                    foreach (var dir in S.Settings.SearchDirectories) { dir.NrOfFiles = 0; } //Resetting this so it looks nice when gui is propagating the new info..

                    var unorderedFiles = new ConcurrentDictionary<string, RunItem>();
                    LastIndexed = DateTime.Now;
                    //Spread out the search on all directories on different tasks
                    //Task.WaitAll(S.SearchDirectories.Select(d => ProcessDirectory(d.Path, d.SearchPattern, d.SearchSubFolders, unorderedFiles)).ToArray());

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
                    var temp = new Indexing.SuffixTree.Trie<RunItem>();
                    //var sw = new Stopwatch();
                    //sw.Start();
                    for (int i = 0; i < tempList.Length; i++)
                    {
                        try
                        {
                            Progress = i / tempList.Length * 100;
                            temp.Insert(tempList[i].Name.ToLower(), tempList[i]);
                            if (Path.GetExtension(tempList[i].FileName) == ".exe")  //adding the containing folder as well
                            {
                                temp.Insert(new DirectoryInfo(Path.GetDirectoryName(tempList[i].URI)).Name.ToLower(), tempList[i]);
                            }
                            if (tempList[i].KeyWords != null && tempList[i].KeyWords.Count > 0)
                                temp.Insert(tempList[i].KeyWords, tempList[i]);

                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "Could not add file to trie");
                        }

                    }

                    MatcherGeneral = temp;
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
            var fileList = new List<RunItem>();
            var sw = new Stopwatch();
            sw.Start();
            ICollection<RunItem> generalResult = MatcherGeneral.Search(s.ToLower(), nrOfHits);
            GeneralSearchTime = (int)sw.ElapsedTicks;
            sw.Restart();
            var count = nrOfHits;
            if (s == "") count = Math.Min(MatcherPrio.DataDictionary.Count, 50);
            ICollection<RunItem> prioResult = MatcherPrio.Search(s.ToLower(), count);
            prioResult = prioResult.Concat(generalResult).ToList();  //put the rest of the results on the stack
            prioResult = prioResult.GroupBy(x => x.Name + x.URI + x.Command).Select(x => x.First()).ToList();  //remove potential duplicates
            prioResult = prioResult.ToList().OrderByDescending(p => p.RunNrOfTimes).ToList();  //sort prioresults since they will always have at least one runtime

            PrioSearchTime = (int)sw.ElapsedTicks;
            TotalSearchTime = GeneralSearchTime + PrioSearchTime;
            return prioResult.Take(10).ToList();
        }

        public static void ProcessDirectory(FolderSearch dir, ConcurrentDictionary<string, RunItem> theBag)
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
                //var t = theBag.ContainsKey(dir.URI);
                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(dir.Path, dir.SearchPattern);
                foreach (string fileName in fileEntries)
                {
                    var sb = new StringBuilder();

                    if (Path.GetExtension(fileName) == ".lnk")
                    {
                        var item = new RunItem();

                        WshShell shell = new WshShell(); //Create a new WshShell Interface
                        IWshShortcut link = (IWshShortcut)shell.CreateShortcut(fileName); //Link the interface to our shortcut
                        var uri = link.TargetPath;

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
                            item.KeyWords = new List<string>() { Path.GetFileName(item.URI) };
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
            }
            catch (Exception)
            {
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


        public void AddExecutedItem(RunItem exItem)
        {
            MatcherGeneral.Remove(exItem);
            exItem.RunNrOfTimes++;
            MatcherPrio.Insert(exItem.Name, exItem);
        }
        public void ResetItemRunCounter(RunItem exItem)
        {
            exItem.RunNrOfTimes = 0;
            if (exItem.Type == ItemType.File || exItem.Type == ItemType.Directory)  //Move it back to general list. Settings and other stuff will always be in the priolist.
            {
                MatcherPrio.Remove(exItem);
                MatcherGeneral.Insert(exItem.Name, exItem);
            }
        }


    }


}
