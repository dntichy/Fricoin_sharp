using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using ChainUtils;
using CoreLib.Blockchain;

namespace CoreLib.DataStructures.MerkleTree
{
    public class MerkleTree
    {
        public MerkleNode RootNode { get; set; }

        public MerkleTree(IList<Transaction> transactionList)
        {
            var merkleNodeList = TransformList(transactionList);
            BuildTree(merkleNodeList);        
        }

        private IList<MerkleNode> RepairList(IList<MerkleNode> transactionList)
        {
             transactionList.Add(transactionList[transactionList.Count - 1]);
             return transactionList;
        }

        private List<MerkleNode> TransformList(IList<Transaction> transactionList)
        {
            var firstLevel = new List<MerkleNode>();
            //create leaves hashes
            foreach(var tx in transactionList)
            {
                firstLevel.Add(new MerkleNode(null, null, CalculateHash(tx.Serialize())));
            }

            return firstLevel;
        }

        private void BuildTree(IList<MerkleNode> level)
        {
            var newLevel = new List<MerkleNode>();

            if(level.Count ==1)
            {
                RootNode = newLevel[0];
                return;
            }

            while (newLevel.Count != 1)
            {
                if (level.Count == 1) break;
                if (level.Count % 2 != 0) level = RepairList(level);
                newLevel.Clear();

                for (var i = 0; i < level.Count - 1; i += 2)
                { 
                    var leftHash = CalculateHash(level[i].Data);
                    var rightHash = CalculateHash(level[i + 1].Data);
                    var parent = new MerkleNode(level[i], level[i+1], CalculateHash(ArrayHelpers.ConcatArrays(leftHash, rightHash)));
                    newLevel.Add(parent);    
                }
                
                level =  new List<MerkleNode>(newLevel);
            }

            RootNode = newLevel[0];
        }

        public byte[] CalculateHash(byte[] bytes)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] outputBytes = sha256.ComputeHash(bytes);
            return outputBytes;
        }

        public void LevelOrder()
        {
            
            var nextlevel = new List<MerkleNode>();
            nextlevel.Add(RootNode);

            do
            {
                for (var i = 0; i < nextlevel.Count; i++)
                {
                    var node = nextlevel[i];
                    Console.Write(node + " ");
                    if(node.Left!= null) { 
                    nextlevel.Add(node.Left);
                    nextlevel.Add(node.Right);
                    }

                    nextlevel.RemoveAt(i);
                }

              
               

            } while ((nextlevel.Count != 0));



        }
        
        
    }
}
