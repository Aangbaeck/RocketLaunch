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
using ProtoBuf;
using Newtonsoft.Json.Serialization;
using Serilog;
using Trie;
using TrieImplementation;

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
                LoadTrie();
            }

            if (NrOfPaths == 0)
            {
                Log.Debug("Something is wrong with the loading. Let's reindex...");
                RunIndexing();
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

        private List<(string path, string searchString, bool SearchSub)> SearchDirectories { get; set; } = new List<(string, string, bool)> { 
            (Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "*.*", true),
            (Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "*.*", true),
            (Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "*.*", true),
            (Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "*.*", true),
            //("C:/Program Files/", "*.exe", true),

        };

        public int NrOfPaths
        {
            get { return _nrOfPaths; }
            set { _nrOfPaths = value; RaisePropertyChanged(); }
        }



        public void RunIndexing()
        {
            Task.Run(() =>
            {
                var unorderedFiles = new ConcurrentBag<RunItem>();

                //This puts all files in from SearchDirectories in UnorderedFiles 
                Task.WaitAll(SearchDirectories.Select(d => ProcessDirectory(d.path, d.searchString, d.SearchSub, unorderedFiles)).ToArray());

                var tempList = unorderedFiles.ToArray();

                var temp = new Trie.Trie();
                //var sw = new Stopwatch();
                //sw.Start();
                for (int i = 0; i < tempList.Length; i++)
                {
                    Progress = i / tempList.Length * 100;
                    if (tempList[i].Type == RunItem.ItemType.File)
                        temp.Insert(tempList[i].Name.ToLower(), tempList[i]);
                    if (tempList[i].Type == RunItem.ItemType.Directory)
                        temp.Insert(tempList[i].Name.ToLower(), tempList[i]);
                    //if (!string.IsNullOrEmpty(tempList[i].Group))
                    //    TempMatcher.Insert(tempList[i].Group.ToLower(), tempList[i]);
                }
                //sw.Stop();
                //Add settings and other good stuff
                foreach (var setting in RunItem.CommonControlPanel.Settings)
                {
                    if (!PrioMatcher.KeyValueObjects.ContainsKey(setting.Name))
                    {
                        PrioMatcher.Insert(setting.Name, setting);
                        PrioMatcher.Insert(setting.Group, setting);
                    }
                }

                NrOfPaths = temp.KeyValueObjects.Count;
                Matcher = temp;

                SaveTrie();
                //var t = LoadTrie();


                //var list = new List<RunItem>();
                //foreach (var result in Matcher.Search("ru".ToLower(),SearchType.Substring))
                //{
                //    list.Add(result);
                //}
                //foreach (var result in Matcher.Search("w".ToLower(),SearchType.Substring))
                //{
                //    list.Add(result);
                //}
                //SaveTrie();
                //LoadTrie();

                //var list2 = new List<RunItem>();
                //foreach (var result in Matcher.Search("ru".ToLower(), SearchType.Substring))
                //{
                //    list2.Add(result);
                //}
                //foreach (var result in Matcher.Search("w".ToLower(), SearchType.Substring))
                //{
                //    list2.Add(result);
                //}

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

        public void SaveTrie()
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

        public void LoadTrie()
        {
            var trie = new Trie.Trie();
            try
            {
                var path = Common.Directory + "Matcher.trie";
                byte[] zip = File.ReadAllBytes(path);
                string json = zip.Unzip();
                Matcher = JsonConvert.DeserializeObject<Trie.Trie>(json);
                NrOfPaths = trie.KeyValueObjects.Count;

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
            prioResult = prioResult.GroupBy(x => x.Name).Select(x => x.First()).ToList();
            foreach (var genRes in generalResult)
            {
                if (prioResult.Count < nrOfHits && prioResult.All(p => p.Name != genRes.Name))
                    prioResult.Add(genRes);
            }

            var finalResults = prioResult.ToList().OrderByDescending(p => p.RunNrOfTimes).ToList();
            return finalResults;
        }

        public static Task ProcessDirectory(string targetDirectory, string searchPattern, bool searchsubDirectories, ConcurrentBag<RunItem> theBag)
        {
            try
            {
                var dir = new RunItem()
                {
                    Name = new DirectoryInfo(targetDirectory).Name,
                    URI = targetDirectory,
                    Type = RunItem.ItemType.Directory
                };
                theBag.Add(dir);

                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(targetDirectory, searchPattern);
                foreach (string fileName in fileEntries)
                {
                    var sb = new StringBuilder();
                    var item = new RunItem()
                    {
                        URI = new StringBuilder(fileName).Replace("\\", "/").Replace("//", "/").ToString(),
                        Type = RunItem.ItemType.File,
                    };
                    item.Name = item.FileName;
                    theBag.Add(item);
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
