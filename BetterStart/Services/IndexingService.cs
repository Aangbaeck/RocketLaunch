using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
using System.Text;
using IWshRuntimeLibrary;
using ProtoBuf;
using Newtonsoft.Json.Serialization;
using Serilog;
using Trie;
using TrieImplementation;
using File = System.IO.File;

namespace RocketLaunch.Services
{
    public class IndexingService : ViewModelBase
    {
        private int _nrOfPaths;
        private SettingsService _s;
        private int _progress;

        public IndexingService(SettingsService s)
        {
            if (!File.Exists(Common.Directory + "Matcher.trie"))
                RunIndexing();
            else
            {
                LoadTries();
                if (NrOfPaths == 0)
                {
                    Log.Debug("Something is wrong with the loading. Let's reindex...");
                    RunIndexing();
                }
            }
            
            S = s;
            var timer = new Timer(s.ReindexingTime);
            timer.Start();
            timer.Elapsed += (_, __) =>
            {
                RunIndexing();
            };
        }

        //General file matcher
        private Trie.Trie Matcher { get; set; } = new Trie.Trie();
        //This matcher is for things that has been run before. They are of course more prioritized.
        private Trie.Trie PrioMatcher { get; set; } = new Trie.Trie();


        public SettingsService S { get; set; }

        

        public int NrOfPaths
        {
            get { return _nrOfPaths; }
            set { _nrOfPaths = value; RaisePropertyChanged(); }
        }



        public void RunIndexing()
        {
            Task.Run(() =>
            {
                var unorderedFiles = new ConcurrentDictionary<string, RunItem>();

                //Spread out the search on all directories on different tasks
                Task.WaitAll(S.SearchDirectories.Select(d => ProcessDirectory(d.path, d.pattern, d.subFolders, unorderedFiles)).ToArray());

                RunItem[] tempList = unorderedFiles.Select(p => p.Value).ToArray();

                var temp = new Trie.Trie();
                //var sw = new Stopwatch();
                //sw.Start();
                for (int i = 0; i < tempList.Length; i++)
                {
                    Progress = i / tempList.Length * 100;
                    temp.Insert(tempList[i].Name.ToLower(), tempList[i]);
                    //if (!string.IsNullOrEmpty(tempList[i].Group))
                    //    TempMatcher.Insert(tempList[i].Group.ToLower(), tempList[i]);
                }
                //sw.Stop();
                //Add settings and other good stuff
                foreach (var setting in CommonControlPanel.Settings)
                {
                    if (!PrioMatcher.KeyValueObjects.ContainsKey(setting.Name))
                    {
                        PrioMatcher.Insert(setting.Name.ToLower(), setting);
                        PrioMatcher.Insert(setting.Group.ToLower(), setting);
                    }
                }

                NrOfPaths = temp.KeyValueObjects.Count;
                Matcher = temp;

                SaveTries();
            });
        }

        public void SavePrioTrie()
        {
            try
            {
                var json = JsonConvert.SerializeObject(PrioMatcher, Formatting.None, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });
                var zip = json.Zip();
                File.WriteAllBytes(Common.Directory + "PrioMatcher.trie", zip);

            }
            catch (Exception e)
            {
                Log.Error(e, "Could not save trie");

            }
        }

        public void SaveTries()
        {
            try
            {
                //var json = JsonConvert.SerializeObject(Matcher,Formatting.None, new JsonSerializerSettings{ PreserveReferencesHandling = PreserveReferencesHandling.Objects });
                var json = JsonConvert.SerializeObject(Matcher, Formatting.None, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });
                var zip = json.Zip();
                File.WriteAllBytes(Common.Directory + "Matcher.trie", zip);

                json = JsonConvert.SerializeObject(PrioMatcher, Formatting.None, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });
                zip = json.Zip();
                File.WriteAllBytes(Common.Directory + "PrioMatcher.trie", zip);

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
                var path = Common.Directory + "Matcher.trie";
                byte[] zip = File.ReadAllBytes(path);
                string json = zip.Unzip();
                Matcher = JsonConvert.DeserializeObject<Trie.Trie>(json);
                NrOfPaths = Matcher.KeyValueObjects.Count;

                path = Common.Directory + "PrioMatcher.trie";
                zip = File.ReadAllBytes(path);
                json = zip.Unzip();
                PrioMatcher = JsonConvert.DeserializeObject<Trie.Trie>(json);

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

        public List<RunItem> Search(string s, int nrOfHits = 10)
        {
            var fileList = new List<RunItem>();
            ICollection<RunItem> generalResult = Matcher.Search(s.ToLower(), SearchType.Substring, nrOfHits);
            ICollection<RunItem> prioResult = PrioMatcher.Search(s.ToLower(), SearchType.Substring, nrOfHits);
            prioResult = prioResult.GroupBy(x => x.URI).Select(x => x.First()).ToList();
            foreach (var genRes in generalResult)
            {
                if (prioResult.Count < nrOfHits && prioResult.All(p => p.URI != genRes.URI))
                    prioResult.Add(genRes);
            }

            var finalResults = prioResult.ToList().OrderByDescending(p => p.RunNrOfTimes).ToList();
            return finalResults;
        }

        public static Task ProcessDirectory(string targetDirectory, string searchPattern, bool searchsubDirectories, ConcurrentDictionary<string, RunItem> theBag)
        {
            try
            {
                var dir = new RunItem()
                {
                    Name = new DirectoryInfo(targetDirectory).Name,
                    URI = new StringBuilder(targetDirectory).Replace("/", "\\").Replace("//", "\\").ToString(),
                    Type = ItemType.Directory
                };
                theBag.TryAdd(dir.URI, dir);
                //var t = theBag.ContainsKey(dir.URI);
                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(targetDirectory, searchPattern);
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
                            if(!File.Exists(uri) && uri.Contains("Program Files"))
                                uri = new StringBuilder(uri).Replace("Program Files", "Program Files (x86)").ToString();
                        }
                        
                        if (System.IO.File.Exists(uri) && !uri.Contains("C:\\Windows\\Installer\\"))  //No reason to add a broken link.
                        {
                            item.Type = ItemType.File;
                            item.URI = new StringBuilder(uri).Replace("/", "\\").Replace("//", "\\").ToString();
                            item.Name = System.IO.Path.GetFileNameWithoutExtension(fileName);
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
                Log.Debug($"Could not search {targetDirectory}");
            }
            if (searchsubDirectories)
            {
                try
                {
                    // Recurse into subdirectories of this directory.
                    string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                    foreach (string subdirectory in subdirectoryEntries)
                        try
                        {
                            ProcessDirectory(subdirectory, searchPattern, true, theBag);
                        }
                        catch (Exception)
                        {
                            Log.Debug($"Could not search {subdirectory}");
                        }
                }
                catch (Exception)
                {
                    Log.Debug($"Could not get subdirectoryEntries for {targetDirectory}");
                }
            }
            return Task.CompletedTask;
        }


        public void AddExecutedItem(RunItem exItem)
        {
            PrioMatcher.Insert(exItem.Name, exItem);
        }


    }


}
