using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using BetterStart.Helper;
using BetterStart.Model;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;
using ProtoBuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Trie;
using TrieImplementation;

namespace BetterStart.Services
{
    public class IndexingService : ViewModelBase
    {
        private int _nrOfPaths;
        private SettingsService _s;
        private int _progress;

        public IndexingService(SettingsService s)
        {

            RunIndexing();
            S = s;
            var timer = new Timer(s.ReindexingTime);
            timer.Start();
            timer.Elapsed += (_, __) =>
            {
                RunIndexing();
            };
        }

        public Trie.Trie Matcher { get; set; } = new Trie.Trie();
        public Trie.Trie Matcher2 { get; set; } = new Trie.Trie();
        private Trie.Trie TempMatcher { get; set; } = new Trie.Trie();

        public SettingsService S { get; set; }

        private List<(string path, string searchString, bool SearchSub)> SearchDirectories { get; set; } = new List<(string, string, bool)> { ("C:/Program Files/", "*.*", true) };

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
                NrOfPaths = unorderedFiles.Count();

                var tempList = unorderedFiles.ToArray();

                NrOfPaths = tempList.Length;
                if (NrOfPaths == 0) return;

                var count = tempList.Length;
                for (int i = 0; i < 2000; i++)
                {
                    Progress = i / count * 100;
                    TempMatcher.Insert(tempList[i].Name.ToLower(), tempList[i]);
                }

                //Add settings and other good stuff
                foreach (var setting in CommonControlPanel.Settings)
                {
                    TempMatcher.Insert(setting.Name, setting);
                    TempMatcher.Insert(setting.Group, setting);
                }

                Matcher = TempMatcher;

                //SaveTrie(TempMatcher);
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

        public void SaveTrie(Trie.Trie trie)
        {
            try
            {
                //var json = JsonConvert.SerializeObject(Matcher,Formatting.None, new JsonSerializerSettings{ PreserveReferencesHandling = PreserveReferencesHandling.Objects });
                var json = JsonConvert.SerializeObject(trie, Formatting.None, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });
                var zip = json.Zip();
                File.WriteAllBytes(Common.Directory + "Matcher.trie", zip);
                
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not save trie");

            }
        }

        public Trie.Trie LoadTrie()
        {
            var trie = new Trie.Trie();
            try
            {
                var path = Common.Directory + "Matcher.trie";
                byte[] zip = File.ReadAllBytes(path);
                string json = zip.Unzip();
                trie = JsonConvert.DeserializeObject<Trie.Trie>(json);
                
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not load Trie");
            }
            return trie;
        }

        

        public int Progress
        {
            get { return _progress; }
            set { _progress = value; RaisePropertyChanged(); }
        }

        public List<RunItem> Search(string s, int nrOfResults = 10)
        {
            var fileList = new List<RunItem>();
            var counter = nrOfResults;
            foreach (var result in Matcher.Search(s.ToLower(), SearchType.Substring))
            {
                counter--;
                fileList.Add(result);
                if (counter <= 0)
                    break;
            }
            return fileList;
        }

        public static Task ProcessDirectory(string targetDirectory, string searchPattern, bool searchsubDirectories, ConcurrentBag<RunItem> theBag)
        {
            try
            {
                theBag.Add(new RunItem()
                {
                    Name = new DirectoryInfo(targetDirectory).Name,
                    Group = targetDirectory,
                    URI = targetDirectory,
                    Type = ItemType.Directory
                });
                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(targetDirectory, searchPattern);
                foreach (string fileName in fileEntries)
                {
                    var item = new RunItem()
                    {
                        URI = fileName,
                        Type = ItemType.File,
                    };
                    item.Name = item.FileNameWithoutExtension;
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




    }

}
