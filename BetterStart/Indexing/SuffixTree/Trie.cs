using System;
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
        [ProtoMember(1)]
        public TrieBase TrieBase { get; set; } = new TrieBase();

        [ProtoMember(2)]
        public Dictionary<string, HashSet<T>> DataDictionary { get; set; } = new Dictionary<string, HashSet<T>>();

        public int Count => DataDictionary.Count;

        public void Insert(string key, T value)
        {
            this.InsertToDictionary(key.ToLower(), value);
            this.TrieBase.Insert(key.ToLower());
        }
        /// <summary>
        /// Inserts same value with multiple keys. Some things have similar keywords - lets add all of them.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="value"></param>
        public void Insert(IEnumerable<string> keys, T value)
        {
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    Insert(key.ToLower(), value);
                }
            }
        }


        private void InsertToDictionary(string key, T value)
        {
            if (!this.DataDictionary.ContainsKey(key))
            {
                this.DataDictionary[key] = new HashSet<T>();
            }
            this.DataDictionary[key].Add(value);

        }
        /// <summary>
        /// Returns true if we where able to remove item.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            if (this.DataDictionary.ContainsKey(key.ToLower()))
            {
                this.DataDictionary.Remove(key.ToLower());
                return true;
            }

            return false;
        }

        public void Replace(string key, T value)
        {
            Remove(key);
            Insert(key.ToLower(),value);
        }
        //public bool Remove(T item)
        //{
        //    return this.Remove(item.GetHashCode().ToString(), item);
        //}

        //public bool Remove(string key, T item)
        //{
        //    if (this.DataDictionary.ContainsKey(key))
        //    {
        //        return this.DataDictionary[key].Remove(item);
        //    }

        //    return false;
        //}
        
        public bool Contains(string key)
        {
            return this.TrieBase.Contains(key.ToLower());
        }

        public ICollection<T> Search(string filter, int nrOfHits = int.MaxValue)
        {
            var sw = new Stopwatch();
            sw.Start();
            ICollection<string> strResults = this.TrieBase.Search(filter, nrOfHits);
            var t = sw.ElapsedMilliseconds;
            ICollection<T> tResults = this.GetValuesFromKeys(strResults, nrOfHits);
            var t2 = sw.ElapsedMilliseconds;
            return tResults;
        }
        
        private ICollection<T> GetValuesFromKeys(ICollection<string> keys, int nrOfHits)
        {
            List<T> result = new List<T>();
            foreach (string key in keys)
            {
                if (this.DataDictionary.ContainsKey(key))
                {
                    foreach (T value in this.DataDictionary[key])
                    {
                        if (nrOfHits < result.Count) return result;
                        else
                            result.Add(value);
                    }
                }
            }

            return result;
        }

        
    }
}
