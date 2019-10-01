using System;
using System.Collections.Generic;
using System.Text;
using BetterStart.Model;
using ProtoBuf;
using TrieImplementation;

namespace Trie
{
    [ProtoContract]
    public class Trie
    {
        [ProtoMember(1, AsReference = true)] 
        public TrieBase InnerTrie { get; set; } = new TrieBase();

        [ProtoMember(2)]
        public Dictionary<string, HashSet<RunItem>> KeyValueObjects { get; set; } =new Dictionary<string, HashSet<RunItem>>(true ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

        public void Insert(string key, RunItem value)
        {
            this.InsertToDictionary(key, value);
            this.InnerTrie.Insert(key);
        }

        private void InsertToDictionary(string key, RunItem value)
        {
            if (!this.KeyValueObjects.ContainsKey(key))
            {
                this.KeyValueObjects[key] = new HashSet<RunItem>();
            }

            this.KeyValueObjects[key].Add(value);
        }

        //public bool Remove(string key)
        //{
        //    if (this.KeyValueObjects.ContainsKey(key))
        //    {
        //        this.KeyValueObjects.Remove(key);
        //        return true;
        //    }

        //    return false;
        //}

        //public bool Remove(RunItem item)
        //{
        //    return this.Remove(item.GetHashCode().ToString(), item);
        //}

        //public bool Remove(string key, RunItem item)
        //{
        //    if (this.KeyValueObjects.ContainsKey(key))
        //    {
        //        return this.KeyValueObjects[key].Remove(item);
        //    }

        //    return false;
        //}

        //public void InsertRange(IEnumerable<RunItem> items)
        //{
        //    foreach (var item in items)
        //    {
        //        this.Insert(item);
        //    }
        //}

        //public bool Contains(RunItem item)
        //{
        //    return this.Contains(item.GetHashCode().ToString());
        //}

        //public bool Contains(string key)
        //{
        //    return this.InnerTrie.Contains(key);
        //}

        public ICollection<RunItem> Search(RunItem item)
        {
            return this.Search(item.GetHashCode().ToString(), SearchType.Substring);
        }

        public ICollection<RunItem> Search(string filter, SearchType searchType)
        {
            ICollection<string> strResults = this.InnerTrie.Search(filter, searchType);
            ICollection<RunItem> tResults = this.GetValuesFromKeys(strResults);

            return tResults;
        }

        //public ICollection<RunItem> FindAll()
        //{
        //    ICollection<string> allKeys = this.InnerTrie.FindAll();
        //    ICollection<RunItem> all = this.GetValuesFromKeys(allKeys);

        //    return all;
        //}
        private ICollection<RunItem> GetValuesFromKeys(ICollection<string> keys)
        {
            List<RunItem> result = new List<RunItem>();
            foreach (string key in keys)
            {
                if (this.KeyValueObjects.ContainsKey(key))
                {
                    foreach (RunItem value in this.KeyValueObjects[key])
                    {
                        result.Add(value);
                    }
                }
            }

            return result;
        }

        

        //#region IEnumerable<T> Members

        //public IEnumerator<RunItem> GetEnumerator()
        //{
        //    ICollection<RunItem> all = this.FindAll();

        //    foreach (RunItem item in all)
        //    {
        //        yield return item;
        //    }
        //}

        //#endregion

        //#region IEnumerable Members

        ////System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        ////{
        ////    return this.GetEnumerator();
        ////}

        //#endregion
    }
}
