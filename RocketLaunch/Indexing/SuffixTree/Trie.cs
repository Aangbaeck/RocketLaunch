using System.Collections.Generic;
using System.Diagnostics;
using ProtoBuf;
using TrieImplementation;

namespace RocketLaunch.Indexing.SuffixTree
{
    [ProtoContract]
    public class Trie<T>
    {
        [ProtoMember(1)] public TrieBase TrieBase { get; set; } = new TrieBase();

        [ProtoMember(2)]
        public Dictionary<string, HashSet<T>> DataDictionary { get; set; } = new Dictionary<string, HashSet<T>>();

        public int Count => DataDictionary.Count;

        public void Insert(string key, T value)
        {
            InsertToDictionary(key.ToLower(), value);
            TrieBase.Insert(key.ToLower());
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
            if (!DataDictionary.ContainsKey(key))
            {
                DataDictionary[key] = new HashSet<T>();
            }

            DataDictionary[key].Add(value);
        }

        /// <summary>
        /// Returns true if we where able to remove item.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            if (DataDictionary.ContainsKey(key.ToLower()))
            {
                DataDictionary.Remove(key.ToLower());
                return true;
            }

            return false;
        }

        public void Replace(string key, T value)
        {
            Remove(key);
            Insert(key.ToLower(), value);
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
            return TrieBase.Contains(key.ToLower());
        }

        public ICollection<T> Search(string filter, int nrOfHits = int.MaxValue)
        {
            ICollection<string> strResults = TrieBase.Search(filter, nrOfHits);
            ICollection<T> tResults = GetValuesFromKeys(strResults, nrOfHits);
            return tResults;
        }

        private ICollection<T> GetValuesFromKeys(ICollection<string> keys, int nrOfHits)
        {
            List<T> result = new List<T>();
            foreach (string key in keys)
            {
                if (DataDictionary.ContainsKey(key))
                {
                    foreach (T value in DataDictionary[key])
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