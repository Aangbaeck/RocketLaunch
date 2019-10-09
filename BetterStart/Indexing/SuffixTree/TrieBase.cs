using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Navigation;
using ProtoBuf;
using RocketLaunch.Indexing.SuffixTree;
using Trie;

namespace TrieImplementation
{
    /// <summary>
    /// Trie is an ordered tree data structure that is used to store a dynamic set or associative array where the keys are usually strings(chars in this case)
    /// http://en.wikipedia.org/wiki/Trie
    /// This is a modified version of the Trie which supports searching for a Substring.
    /// </summary>
    [ProtoContract]
    public class TrieBase
    {
        
        [ProtoMember(1)]
        public Dictionary<char, List<TrieNode>> Nodes { get; set; } = new Dictionary<char, List<TrieNode>>(new CharComparer());
        /// <summary>
        /// The first Node in the Trie
        /// </summary>
        [ProtoMember(2)]
        public TrieNode Root { get; set; }=new TrieNode(Char.MinValue, null, false);

        #region Insertion
        
        /// <summary>
        /// Inserts a word in the Trie
        /// </summary>
        /// <param name="word"></param>
        public void Insert(string word)
        {
            this.InsertCore(this.Root, 0, word);
        }
        
        /// <summary>
        /// Inserts all words from a collection
        /// </summary>
        /// <param name="words"></param>
        public void InsertRange(IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                this.Insert(word);
            }
        }
        
        /// <summary>
        /// Adds a node to the master collection of all nodes
        /// </summary>
        protected virtual void AddNodeToCollection(char currentChar, TrieNode node)
        {
            if (!this.Nodes.ContainsKey(currentChar))
            {
                this.Nodes[currentChar] = new List<TrieNode>();
            }
            
            this.Nodes[currentChar].Add(node);
        }
        
        /// <summary>
        /// The main method which Inserts a a word in the Trie
        /// </summary>
        protected virtual void InsertCore(TrieNode currentNode, int positionInWord, string word)
        {
            if (positionInWord >= word.Length)
            {
                return;
            }
            
            char currentChar = word[positionInWord];
            TrieNode node = null;
            if (!currentNode.Children.ContainsKey(currentChar))
            {
                bool isWord = positionInWord == word.Length - 1 ? true : false;
                node = new TrieNode(currentChar, currentNode, isWord);
                this.AddNodeToCollection(currentChar, node);
                currentNode.AddChild(node);
            }
            else
            {
                node = currentNode.Children[currentChar];
            }
            
            this.InsertCore(node, ++positionInWord, word);
        }
        
        #endregion
        
        #region Contains
        
        /// <summary>
        /// Checks if a word is inside the Trie
        /// </summary>
        /// <param name="word">Word to check</param>
        /// <returns>Whether the word was found</returns>
        public bool Contains(string word)
        {
            return this.ContainsCore(this.Root, 0, word);
        }
        
        /// <summary>
        /// The main method which checks whether a word is contained inside the Trie
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="currentPositionInWord"></param>
        /// <param name="word"></param>
        /// <returns></returns>
        protected virtual bool ContainsCore(TrieNode currentNode, int currentPositionInWord, string word)
        {
            if (currentPositionInWord >= word.Length)
            {
                return true;
            }
            
            char currentChar = word[currentPositionInWord];
            bool containsKey = currentNode.Children.ContainsKey(currentChar);
            if (containsKey)
            {
                return this.ContainsCore(currentNode.Children[currentChar], ++currentPositionInWord, word);
            }
            
            return false;
        }
        
        #endregion
        
        #region Search
        
        /// <summary>
        /// Searches for word inside the Trie with a StartsWith SearchType
        /// </summary>
        /// <returns>All found words starting with Word</returns>
        public ICollection<string> Search(string word, int nrOfHits)
        {
            return this.Search(word, SearchType.Prefix, nrOfHits);
        }
        
        /// <summary>
        /// Searches for word inside the Trie with the specified SearchType
        /// </summary>
        /// <param name="word"></param>
        /// <param name="searchType"></param>
        /// <returns>All words meeting the criteria</returns>
        public ICollection<string> Search(string word, SearchType searchType, int nrOfHits)
        {
            if (word == null)
            {
                throw new ArgumentException("The word must not be null");
            }
            else if (word == string.Empty)
            {
                return this.FindAll(nrOfHits);
            }
            
            ICollection<string> results = new HashSet<string>();
            if (searchType == SearchType.Prefix)
            {
                this.PrefixSearchCore(this.Root, 0, new StringBuilder(word), results, nrOfHits);
            }
            else if (searchType == SearchType.Substring)
            {
                this.SubstringSearchCore(word, results, nrOfHits);
            }
            
            return results;
        }

        public ICollection<string> FindAll(int nrOfHits)
        {
            List<string> words = new List<string>();
            this.DfsForAllWords(this.Root, new StringBuilder(string.Empty), words, nrOfHits);
            return words;
        }
        
        /// <summary>
        /// The main method which Searches for substring inside the Trie
        /// </summary>
        /// <param name="word">Word to search</param>
        /// <param name="results"></param>
        protected virtual void SubstringSearchCore(string word, ICollection<string> results, int nrOfHits)
        {
            int currentPositionInWord = 0;
            if (this.Nodes.ContainsKey(word[currentPositionInWord]))
            {
                List<TrieNode> startNodes = this.Nodes[word[currentPositionInWord++]];
                
                for (int i = 0; i < startNodes.Count; i++)
                {
                    if (results.Count > nrOfHits)
                        break;
                    TrieNode currentStartNode = startNodes[i];
                    bool contains = false;
                    while (currentPositionInWord < word.Length)
                    {
                        if (results.Count > nrOfHits)
                            return;
                        if (currentStartNode.Children.ContainsKey(word[currentPositionInWord]))
                        {
                            currentStartNode = currentStartNode.Children[word[currentPositionInWord]];
                            contains = true;
                        }
                        else
                        {
                            contains = false;
                            break;
                        }
                    
                        currentPositionInWord++;
                    }
                
                    if (contains || word.Length == 1)
                    {
                        this.BuildResultsFromSubstring(currentStartNode, results, nrOfHits);
                        
                    }
                    if (results.Count > nrOfHits)
                        return;
                    currentPositionInWord = 1;
                }
            }
        }
        
        /// <summary>
        /// Builds the resulted words from a one word. Searches for all descendants which match that word.
        /// The word is being built by the node itself. As it iterates its parents until the end of a word or the root are met.
        /// </summary>
        /// <param name="nodeValue">The node to build results from</param>
        /// <param name="results">Output</param>
        protected virtual void BuildResultsFromSubstring(TrieNode nodeValue, ICollection<string> results, int nrOfHits)
        {
            if (results.Count > nrOfHits)
                return;
            string builtWord = nodeValue.BuildUpToWord();
            if (nodeValue.Children.Count == 0)
            {
                //if we at the bottom of the tree just add this word as no descendants will be found
                results.Add(builtWord);
            }
            else
            {
                if (results.Count > nrOfHits)
                    return;
                //Using a DepthFirstApproach (http://en.wikipedia.org/wiki/Depth-first_search) finds all descendants of the current node and adds their
                //values to the results.
                this.DfsForAllWords(nodeValue, new StringBuilder(builtWord), results, nrOfHits);
            }
        }
        
        /// <summary>
        /// Searches the Trie for words with a StartsWith SearchType. Uses recursion - http://en.wikipedia.org/wiki/Recursion
        /// </summary>
        protected virtual void PrefixSearchCore(TrieNode currentNode, int currentPositionInWord, StringBuilder word, ICollection<string> results, int nrOfHits)
        {
            if (currentPositionInWord >= word.Length)
            {
                if (results.Count > nrOfHits)
                    return;
                this.DfsForAllWords(currentNode, word, results, nrOfHits);
                return;
            }
            
            char currentChar = word[currentPositionInWord];
            
            bool containsKey = currentNode.Children.ContainsKey(currentChar);
            if (containsKey)
            {
                if (results.Count > nrOfHits)
                    return;
                if (currentPositionInWord == word.Length - 1)
                {
                    results.Add(word.ToString());
                }
                
                TrieNode child = currentNode.Children[currentChar];
                this.PrefixSearchCore(child, ++currentPositionInWord, word, results, nrOfHits);
            }
        }
        
        /// <summary>
        /// Using a DepthFirstApproach (http://en.wikipedia.org/wiki/Depth-first_search) finds all descendants of the current node and adds their
        /// values to the results.
        /// </summary>
        protected virtual void DfsForAllWords(TrieNode currentNode, StringBuilder word, ICollection<string> results, int nrOfHits)
        {
            if (results.Count > nrOfHits)
                return;
            TrieNode[] nodesArray = new TrieNode[currentNode.Children.Count];
            currentNode.Children.Values.CopyTo(nodesArray, 0);
            foreach (TrieNode node in nodesArray)
            {
                if (results.Count > nrOfHits)
                    return;
                word.Append(node.Value);
                if (node.IsWord)
                {
                    
                    results.Add(word.ToString());
                }
                
                this.DfsForAllWords(node, word, results, nrOfHits);
                word.Length--;
            }
        }

        
        #endregion

        //#region IEnumerable<string> Members

        //public IEnumerator<string> GetEnumerator()
        //{
        //    ICollection<string> all = this.FindAll();
        //    foreach (string item in all)
        //    {
        //        yield return item;
        //    }
        //}

        //#endregion

        //#region IEnumerable Members

        //System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //{
        //    return this.GetEnumerator();
        //}

        //#endregion
    }
}