using System;
using System.Collections.Generic;

namespace Engine.Core
{
    public class BlockChain : IChain
    {
        public int Difficulty { set; get; } = 2;
        public IList<Block> Chain { set; get; }

        private IList<Transaction> _transactionPool = new List<Transaction>();


        public BlockChain()
        {
            Chain = new List<Block>();
            AddGenesisBlock();
        }

        private void AddGenesisBlock()
        {
            Chain.Add(new Block(DateTime.Now, null, new List<Transaction>()));
        }

        public Block GetLatestBlock()
        {
            return Chain[Chain.Count - 1];
        }

        public void AddBlock(Block block)
        {
            Block latestBlock = GetLatestBlock();
            block.Index = latestBlock.Index + 1;
            block.Mine(this.Difficulty);
            Chain.Add(block);
        }


        public void ProcessPendingTransactions(string minerAddress)
        {
            Block block = new Block(DateTime.Now, GetLatestBlock().Hash, _transactionPool);
            AddBlock(block); // pridaj block

            _transactionPool = new List<Transaction>(); // reset TransactionPool
            CreateTransaction(new Transaction(null, minerAddress, 1)); //pridaj odmenovú transakciu
        }


        public void CreateTransaction(Transaction transaction)
        {
            _transactionPool.Add(transaction);
        }


        public bool IsValid()
        {
            for (var i = 1; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];
                var previousBlock = Chain[i - 1];

                if (currentBlock.Hash != currentBlock.CalculateHash())
                {
                    return false;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return false;
                }
            }

            return true;
        }

        public float GetBalance(string address)
        {
            return 0;
        }
    }
}