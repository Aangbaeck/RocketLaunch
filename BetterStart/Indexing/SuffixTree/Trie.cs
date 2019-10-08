﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProtoBuf;
using RocketLaunch.Model;
using TrieImplementation;

namespace RocketLaunch.Indexing.SuffixTree
{
    [ProtoContract]
    public class Trie<T>
    {
        [ProtoMember(1, AsReference = true)]
        public TrieBase InnerTrie { get; set; } = new TrieBase();

        [ProtoMember(2)]
        public Dictionary<string, HashSet<T>> KeyValueObjects { get; set; } = new Dictionary<string, HashSet<T>>(true ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

        public void Insert(string key, T value)
        {
            this.InsertToDictionary(key, value);
            this.InnerTrie.Insert(key);
        }
        /// <summary>
        /// Inserts same value with multiple keys. Some things have similar keywords - lets add all of them.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="value"></param>
        public void Insert(IEnumerable<string> keys, T value)
        {
            foreach (var key in keys)
            {
                Insert(key,value);
            }
        }


        private void InsertToDictionary(string key, T value)
        {
            if (!this.KeyValueObjects.ContainsKey(key))
            {
                this.KeyValueObjects[key] = new HashSet<T>();
            }
            this.KeyValueObjects[key].Add(value);

        }

        public bool Remove(string key)
        {
            if (this.KeyValueObjects.ContainsKey(key))
            {
                this.KeyValueObjects.Remove(key);
                return true;
            }

            return false;
        }

        public bool Remove(T item)
        {
            return this.Remove(item.GetHashCode().ToString(), item);
        }

        public bool Remove(string key, T item)
        {
            if (this.KeyValueObjects.ContainsKey(key))
            {
                return this.KeyValueObjects[key].Remove(item);
            }

            return false;
        }
        

        public bool Contains(T item)
        {
            return this.Contains(item.GetHashCode().ToString());
        }

        public bool Contains(string key)
        {
            return this.InnerTrie.Contains(key);
        }

        public ICollection<T> Search(T item, int nrOfHits)
        {
            return this.Search(item.GetHashCode().ToString(), SearchType.Substring, nrOfHits);
        }

        public ICollection<T> Search(string filter, SearchType searchType, int nrOfHits)
        {
            var sw = new Stopwatch();
            sw.Start();
            ICollection<string> strResults = this.InnerTrie.Search(filter, searchType, nrOfHits);
            var t = sw.ElapsedMilliseconds;
            ICollection<T> tResults = this.GetValuesFromKeys(strResults, nrOfHits);
            var t2 = sw.ElapsedMilliseconds;
            return tResults;
        }

        //public ICollection<RunItem> FindAll()
        //{
        //    ICollection<string> allKeys = this.InnerTrie.FindAll();
        //    ICollection<RunItem> all = this.GetValuesFromKeys(allKeys);

        //    return all;
        //}
        private ICollection<T> GetValuesFromKeys(ICollection<string> keys, int nrOfHits)
        {
            List<T> result = new List<T>();
            foreach (string key in keys)
            {
                if (this.KeyValueObjects.ContainsKey(key))
                {
                    foreach (T value in this.KeyValueObjects[key])
                    {
                        if (nrOfHits < result.Count) return result;
                        else
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
