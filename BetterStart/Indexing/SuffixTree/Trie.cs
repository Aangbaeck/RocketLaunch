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
        public Dictionary<string, HashSet<T>> DataDictionary { get; set; } = new Dictionary<string, HashSet<T>>(true ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

        public void Insert(string key, T value)
        {
            this.InsertToDictionary(key, value);
            this.TrieBase.Insert(key);
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

        public bool Remove(string key)
        {
            if (this.DataDictionary.ContainsKey(key))
            {
                this.DataDictionary.Remove(key);
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
            if (this.DataDictionary.ContainsKey(key))
            {
                return this.DataDictionary[key].Remove(item);
            }

            return false;
        }

        public bool Contains(T item)
        {
            return this.Contains(item.GetHashCode().ToString());
        }

        public bool Contains(string key)
        {
            return this.TrieBase.Contains(key);
        }

        public ICollection<T> Search(T item, int nrOfHits)
        {
            return this.Search(item.GetHashCode().ToString(), nrOfHits);
        }

        public ICollection<T> Search(string filter, int nrOfHits)
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
