using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterStart.Indexing
{
    public class IndexingService
    {
        public IndexingService()
        {
            RunSearch();
        }
        private List<string> SearchDirectories { get; set; } = new List<string>() { "C:/Temp/" };

        private ConcurrentBag<string> UnorderedFiles { get; set; } = new ConcurrentBag<string>();
        public void RunSearch()
        {
            Task.Run(() =>
            {
                UnorderedFiles = new ConcurrentBag<string>();
                foreach (var directory in SearchDirectories)
                {
                    ProcessDirectory(directory);
                }

            });
        }

        public void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        // Insert logic for processing found files here.
        public void ProcessFile(string path)
        {
            UnorderedFiles.Add(path);
        }

    }
}
