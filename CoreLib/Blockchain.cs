using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using CoreLib.Interfaces;
using Newtonsoft.Json;


namespace CoreLib
{
    public class BlockChain : IChain, IEnumerable<Block>
    {
        public int Difficulty { set; get; } = 2;

        public IList<Block> Chain { set; get; }
        public byte[] LastHash { get; set; }

        public Persistence Db = new Persistence();

        private IList<TransactionNewVersion> _transactionPool = new List<TransactionNewVersion>();


        public BlockChain()
        {
            var lastHash = Db.Get(StringToByteArray("lh"));
            //if exists lasthash retrieve it and set
            if (lastHash != null)
            {
                LastHash = lastHash;
            }
            else
            {
                //else create genesis block and set lasthash to the genesis
                var genesis = new Block(DateTime.Parse("01.01.2019"), null, new List<TransactionNewVersion>());
                Db.Put(StringToByteArray("lh"), genesis.Hash);
                Db.Put(genesis.Hash, genesis.Serialize());
                LastHash = genesis.Hash;
            }

            //Chain = new List<Block>();
        }


        public Block GetLatestBlock()
        {
            return (new Block()).DeSerialize(Db.Get(LastHash));
            //return Chain[Chain.Count - 1];
        }

        public void AddBlock(Block block)
        {
            Block latestBlock = GetLatestBlock();
            block.Index = latestBlock.Index + 1;
            block.Mine(this.Difficulty);
            //Chain.Add(block);

            Db.Put(block.Hash, block.Serialize());
            Db.Put(StringToByteArray("lh"), block.Hash);
            LastHash = block.Hash;
        }


        public void ProcessPendingTransactions(string minerAddress)
        {
            Block block = new Block(DateTime.Now, GetLatestBlock().Hash, _transactionPool);
            AddBlock(block); // pridaj block

            _transactionPool = new List<TransactionNewVersion>(); // reset TransactionPool
            //CreateTransaction(new TransactionNewVersion(null, minerAddress, 1)); //pridaj odmenovú transakciu
        }


        public void CreateTransaction(Transaction transaction)
        {
            //_transactionPool.Add(transaction);
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


        public void Print()
        {
            var currentHash = LastHash;
            while (currentHash != null)
            {
                var block = new Block().DeSerialize(Db.Get(currentHash));
                currentHash = block.PreviousHash;
                Console.WriteLine(JsonConvert.SerializeObject(block, Formatting.Indented));
            }
        }


        public TransactionNewVersion[] FindUnspentTransactions(string address)
        {
            return null;
        }

        public byte[] StringToByteArray(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        public string ByteArrayToString(byte[] arr)
        {
            return Convert.ToBase64String(arr);
        }

        public IEnumerator<Block> GetEnumerator()
        {
            //throw new NotImplementedException();
            var currentHash = LastHash;
            while (currentHash != null)
            {
                var block = new Block().DeSerialize(Db.Get(currentHash));
                currentHash = block.PreviousHash;
                yield return block;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}