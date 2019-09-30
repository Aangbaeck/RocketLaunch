using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using BetterStart.Model;
using GalaSoft.MvvmLight;
using Gma.DataStructures.StringSearch;
using NReco.Text;
using Serilog;

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
        }

        public PatriciaSuffixTrie<string> Matcher { get; set; } = new PatriciaSuffixTrie<string>(2);
        private PatriciaSuffixTrie<string> TempMatcher { get; set; } = new PatriciaSuffixTrie<string>(2);

        public SettingsService S { get; set; }

        private List<string> SearchDirectories { get; set; } = new List<string>() { "C:/Program Files/" };

        public int NrOfPaths
        {
            get { return _nrOfPaths; }
            set { _nrOfPaths = value; RaisePropertyChanged(); }
        }

        private ConcurrentBag<string> UnorderedFiles { get; set; } = new ConcurrentBag<string>();
        public void RunIndexing()
        {
            Task.Run(() =>
            {
                UnorderedFiles = new ConcurrentBag<string>();
                foreach (var directory in SearchDirectories)
                {
                    ProcessDirectory(directory);
                }

                NrOfPaths = UnorderedFiles.Count();


                var tempList = UnorderedFiles.ToArray();
                var count = tempList.Length;
                for (int i = 0; i < 2000; i++)
                {
                    Progress = i / count * 100;
                    TempMatcher.Add(tempList[i].ToLower(), tempList[i]);
                }

                Matcher = TempMatcher;

                //var list = new List<string>();
                //foreach (var result in Matcher.Retrieve("Trans".ToLower()))
                //{
                //    list.Add(result);
                //}


                //List<AhoCorasickDoubleArrayTrie<int>.Hit> res = Matcher.ParseText("1");


            });
        }

        public int Progress
        {
            get { return _progress; }
            set { _progress = value; RaisePropertyChanged(); }
        }

        public List<string> Search(string s)
        {
            var list = new List<string>();
            list.AddRange(Matcher.Retrieve(s.ToLower()));
            return list;
        }

        //<div>Icons made by<a href="https://www.flaticon.com/authors/flat-icons" title="Flat Icons"> Flat Icons</a> from<a href= "https://www.flaticon.com/"             title= "Flaticon" > www.flaticon.com </ a ></ div >
        public void ProcessDirectory(string targetDirectory)
        {

            try
            {


                try
                {
                    // Process the list of files found in the directory.
                    string[] fileEntries = Directory.GetFiles(targetDirectory);
                    foreach (string fileName in fileEntries)
                        ProcessFile(fileName);
                }
                catch (Exception)
                {
                    Log.Debug($"Could not search {targetDirectory}");
                }
                // Recurse into subdirectories of this directory.
                string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                foreach (string subdirectory in subdirectoryEntries)
                    try
                    {
                        ProcessDirectory(subdirectory);
                    }
                    catch (Exception)
                    {
                        Log.Debug($"Could not search {subdirectory}");
                    }

            }
            catch (Exception)
            {

                throw;
            }

        }

        // Insert logic for processing found files here.
        public void ProcessFile(string path)
        {
            UnorderedFiles.Add(path);
        }

    }
}
