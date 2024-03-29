using System.Collections.Generic;
using ProtoBuf;

namespace RocketLaunch.Indexing.SuffixTree
{
    [ProtoContract(AsReferenceDefault = true)]
    public class TrieNode
    {
        /// <param name="value">char</param>
        /// <param name="parent">If parent is null then this is the Root</param>
        /// <param name="isWord">Defines whether this node marks the ending of a word</param>
        public TrieNode(char value, TrieNode parent, bool isWord)
        {
            Value = value;
            ParentNode = parent;
            IsWord = isWord;
        }

        public TrieNode()
        {
        }

        [ProtoMember(1)] public char Value { get; set; }

        /// <summary>
        /// Marks if this is a ending of a word.
        /// </summary>
        [ProtoMember(2)]
        public bool IsWord { get; set; }

        /// <summary>
        /// If parent is null then this is the Root
        /// </summary>
        [ProtoMember(3)]
        public TrieNode ParentNode { get; set; }

        [ProtoMember(4)] public Dictionary<char, TrieNode> Children { get; } = new Dictionary<char, TrieNode>();

        public int ChildrenCount
        {
            get { return Children.Keys.Count; }
        }

        public void AddChild(TrieNode node)
        {
            if (!Children.ContainsKey(node.Value))
            {
                Children.Add(node.Value, node);
            }
        }

        /// <summary>
        /// Builds the whole word from this node all the way up. Adds char by char to the beginning of the word
        /// </summary>
        public string BuildUpToWord()
        {
            string word = "";
            TrieNode currentNode = this;
            while (currentNode != null || currentNode.IsWord)
            {
                if (currentNode.ParentNode == null)
                    break;
                word = currentNode.Value + word;
                currentNode = currentNode.ParentNode;
            }

            return word;
        }
    }
}