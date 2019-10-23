using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Navigation;
using ProtoBuf;
using RocketLaunch.Indexing.SuffixTree;

namespace TrieImplementation
{
    /// <summary>
    /// Modified version of the Trie that supports searching for substrings.
    /// http://en.wikipedia.org/wiki/Trie
    /// </summary>
    [ProtoContract]
    public class TrieBase
    {

        [ProtoMember(1)]
        public Dictionary<char, List<TrieNode>> Nodes { get; set; } = new Dictionary<char, List<TrieNode>>();
        /// <summary>
        /// The first Node in the Trie
        /// </summary>
        [ProtoMember(2)]
        public TrieNode Root { get; set; } = new TrieNode(Char.MinValue, null, false);

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

        /// <summary>
        /// Searches for word inside the Trie. Will only return nrOfHits. This goes much faster than returning all words
        /// </summary>
        /// <param name="word">The actual word</param>
        /// <param name="nrOfHits">The nr of hits we want returned</param>
        /// <returns>All words meeting the criteria</returns>
        public ICollection<string> Search(string word, int nrOfHits)
        {
            if (word == string.Empty)
            {
                return this.FindAll(nrOfHits);
            }
            ICollection<string> results = new HashSet<string>();
            this.SubstringSearchCore(word, results, nrOfHits);
            return results;
        }

        public ICollection<string> FindAll(int nrOfHits)
        {
            List<string> words = new List<string>();
            this.DepthFirstSearchAllWords(this.Root, new StringBuilder(string.Empty), words, nrOfHits);
            return words;
        }

        /// <summary>
        /// The method that Searches for substring inside the Trie
        /// </summary>
        /// <param name="word">Word to search</param>
        /// <param name="results"></param>
        private void SubstringSearchCore(string word, ICollection<string> results, int nrOfHits)
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
                        if(currentStartNode.IsWord) results.Add(word);
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

                this.DepthFirstSearchAllWords(nodeValue, new StringBuilder(builtWord), results, nrOfHits);
            }
        }

        ///// <summary>
        ///// Searches the Trie for words with a StartsWith SearchType. Uses recursion - http://en.wikipedia.org/wiki/Recursion
        ///// </summary>
        //protected virtual void PrefixSearchCore(TrieNode currentNode, int currentPositionInWord, StringBuilder word, ICollection<string> results, int nrOfHits)
        //{
        //    if (currentPositionInWord >= word.Length)
        //    {
        //        if (results.Count > nrOfHits)
        //            return;
        //        this.DfsForAllWords(currentNode, word, results, nrOfHits);
        //        return;
        //    }

        //    char currentChar = word[currentPositionInWord];

        //    bool containsKey = currentNode.Children.ContainsKey(currentChar);
        //    if (containsKey)
        //    {
        //        if (results.Count > nrOfHits)
        //            return;
        //        if (currentPositionInWord == word.Length - 1)
        //        {
        //            results.Add(word.ToString());
        //        }

        //        TrieNode child = currentNode.Children[currentChar];
        //        this.PrefixSearchCore(child, ++currentPositionInWord, word, results, nrOfHits);
        //    }
        //}

        /// <summary>
        /// Using a Depth first search (http://en.wikipedia.org/wiki/Depth-first_search) to find all words under the current node.
        /// </summary>
        protected virtual void DepthFirstSearchAllWords(TrieNode currentNode, StringBuilder word, ICollection<string> results, int nrOfHits)
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

                this.DepthFirstSearchAllWords(node, word, results, nrOfHits);
                word.Length--;
            }
        }
    }
}