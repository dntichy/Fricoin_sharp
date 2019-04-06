using ChainUtils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CoreLib.Blockchain
{
    public class InMemoryBlockChain : IEnumerable<Block>
    {
        public List<Block> InMemBlockchain { get; set; }

        public InMemoryBlockChain()
        {
            InMemBlockchain = new List<Block>();
        }

        public Block GetLastBlock()
        {
            if (InMemBlockchain.Count == 0) return null;
            return InMemBlockchain[InMemBlockchain.Count - 1];
        }

        public int GetLastIndex()
        {
            if (InMemBlockchain.Count == 0) return -1;
            return InMemBlockchain[InMemBlockchain.Count - 1].Index;
        }

        public bool AddBlock(Block b)
        {
            InMemBlockchain.Add(b);
            return true;
        }

        public bool BelongsToThisChain(Block b)
        {
            if (ArrayHelpers.ByteArrayCompare(GetLastBlock().Hash, b.PreviousHash)) return true;
            return false;
        }

        public override string ToString()
        {
            string str = "[ ";
            foreach (var block in InMemBlockchain)
            {
                str += Convert.ToBase64String(block.Hash) + ", ";
            }
            str += " ]";
            return str;
        }

        public IEnumerator<Block> GetEnumerator()
        {
            foreach (var block in InMemBlockchain)
            {
                yield return block;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
