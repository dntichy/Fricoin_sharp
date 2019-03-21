using ChainUtils;
using CoreLib.Interfaces;
using Engine.Network.MessageParser;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CoreLib.Blockchain
{
    public class BlockChain : IChain, IEnumerable<Block>
    {
        public int Difficulty { set; get; } = 2;
        public byte[] LastHash { get; set; }

        //public PersistenceChain ChainDb = new PersistenceChain();
        //public PersistenceTransaction TransactionDB = new PersistenceTransaction();
        public PersistenceChain ChainDb;
        public PersistenceTransaction TransactionDB;

        public BlockChain(string address)
        {
            //create db environments if not already created
            ChainDb = new PersistenceChain(address);
            TransactionDB = new PersistenceTransaction(address);

            var lastHash = ChainDb.Get(ByteHelper.GetBytesFromString("lh"));  //if exists lasthash retrieve it and set
            if (lastHash != null)
            {
                LastHash = lastHash;
            }
            else
            {
                //else create genesis block and set lasthash to the genesis
                var genesis = Block.GenesisBlock();
                ChainDb.Put(ByteHelper.GetBytesFromString("lh"), genesis.Hash);
                ChainDb.Put(genesis.Hash, genesis.Serialize());
                LastHash = genesis.Hash;
                ReindexUTXO();
            }
        }


        private Block GetLatestBlock()
        {
            return (new Block()).DeSerialize(ChainDb.Get(LastHash));
        }


        public Block MineBlock(List<Transaction> transactions)
        {

            foreach (var tx in transactions)
            {
                if (VerifyTransaction(tx) != true)
                {
                    Console.WriteLine("problem addblock, invalid tx");
                    return null;
                }
            }

            Block latestBlock = GetLatestBlock();

            //CREATE NEW BLOCK
            var newBlock = new Block()
            {
                TimeStamp = DateTime.Now,
                PreviousHash = LastHash,
                Transactions = transactions
            };
            newBlock.Index = latestBlock.Index + 1; //increment index/height

            //set Merkle root
            newBlock.SetMerkleRoot();
            newBlock.Mine(Difficulty); //MINE IT

            ChainDb.Put(newBlock.Hash, newBlock.Serialize());
            ChainDb.Put(ByteHelper.GetBytesFromString("lh"), newBlock.Hash);
            LastHash = newBlock.Hash;

            return newBlock;
        }


        public void ReindexUTXO()
        {
            var utxoSet = new UTXOSet(this);
            utxoSet.ReIndex();

            var countTxs = utxoSet.CountTransactions();
            Console.WriteLine("DONE, there is: " + countTxs + " transactions");
        }

        public Block GetBlock(byte[] id)
        {
            var block = ChainDb.Get(id);
            if (block != null) return new Block().DeSerialize(block);
            else return null;
        }

        public List<byte[]> GetBlockHashes()
        {
            var listBlock = new List<byte[]>();

            foreach (var block in this)
            {
                
                listBlock.Add(block.Hash);
            }
            return listBlock;
        }




        public bool IsValid()
        {
            foreach (var block in this)
            {
                if (block.Hash != block.CalculateHash())
                {
                    return false;
                }

                if (block.PreviousHash != null)
                {
                    var prevBlock = new Block().DeSerialize(ChainDb.Get(block.PreviousHash));
                    if (block.PreviousHash != prevBlock.CalculateHash())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public float GetBalance(string address)
        {
            var balance = 0;
            var pubKeyHash = WalletCore.TransferAddressToPkHash(address);
            var utxoSet = new UTXOSet(this);
            var utxo = FindUnspentTransactionsOutputs(pubKeyHash);
            foreach (var output in utxo)
            {
                balance += output.Value;
            }

            Console.WriteLine("Balance of " + address + ": " + balance);
            return balance;
        }


        public void PrintWholeBlockChain()
        {
            var currentHash = LastHash;
            while (currentHash != null)
            {
                var block = new Block().DeSerialize(ChainDb.Get(currentHash));
                currentHash = block.PreviousHash;
                Console.WriteLine(JsonConvert.SerializeObject(block, Formatting.Indented));
            }
        }

        public int GetBestHeight()
        {
            var lastBlock = GetLatestBlock();
            return lastBlock.Index;

        }

        public void AddBlock(Block block)
        {
            //is already inside DB? return
            var getBlockFromDB = ChainDb.Get(block.Hash);
            if (getBlockFromDB != null) return;

            //persist new block
            var bData = block.Serialize();
            ChainDb.Put(block.Hash, bData);

            var latestBlock = GetLatestBlock();

            //if index of block to be added is biggest, persist lasthash
            if (block.Index > latestBlock.Index) SetLastHash(block);
        }

        private void SetLastHash(Block block)
        {
            ChainDb.Put(ByteHelper.GetBytesFromString("lh"), block.Hash);
            LastHash = block.Hash;
        }


        public List<TxOutput> FindUnspentTransactionsOutputs(byte[] pubKeyHash)
        {
            var UTXOs = new List<TxOutput>();

            var cursor = TransactionDB.Cursor();
            foreach (var current in cursor)
            {
                var outs = TxOutputs.DeSerialize(current.Value);
                foreach (var output in outs.Outputs)
                {
                    if (output.IsLockedWithKey(pubKeyHash))
                    {
                        UTXOs.Add(output);
                    }
                }
            }

            return UTXOs;
        }



        public Dictionary<string, TxOutputs> FindUtxo()
        {
            var UTXOs = new Dictionary<string, TxOutputs>();
            var spentTxOs = new Dictionary<string, List<int>>();

            foreach (var block in this)
            {
                foreach (var tx in block)
                {
                    var tXId = tx.Id; //transaction ID

                    if (!spentTxOs.ContainsKey(HexadecimalEncoding.ToHexString(tXId)))
                        spentTxOs.Add(HexadecimalEncoding.ToHexString(tXId), new List<int>());

                    var index = 0;
                    foreach (var output in tx.Outputs)
                    {
                        if (spentTxOs.ContainsKey(HexadecimalEncoding.ToHexString(tXId)))
                        {
                            foreach (var spentOut in spentTxOs[HexadecimalEncoding.ToHexString(tXId)])
                            {
                                if (spentOut.Equals(index))
                                {
                                    goto endOfTheLoop;
                                }
                            }
                        }

                        if (!UTXOs.ContainsKey(HexadecimalEncoding.ToHexString(tXId)))
                            UTXOs.Add(HexadecimalEncoding.ToHexString(tXId), new TxOutputs());

                        var outs = UTXOs[HexadecimalEncoding.ToHexString(tXId)];
                        outs.Outputs.Add(output);
                        UTXOs[HexadecimalEncoding.ToHexString(tXId)] = outs;

                        endOfTheLoop:
                        {
                        }
                        index++;
                    }


                    if (tx.IsCoinBase() == false)
                    {
                        foreach (var input in tx.Inputs)
                        {
                            if (!spentTxOs.ContainsKey(HexadecimalEncoding.ToHexString(input.Id)))
                                spentTxOs.Add(HexadecimalEncoding.ToHexString(input.Id), new List<int>());
                            spentTxOs[HexadecimalEncoding.ToHexString(input.Id)].Add(input.Out);

                        }
                    }
                }
            }




            return UTXOs;
        }


        public Transaction FindTransaction(byte[] id)
        {
            foreach (var block in this)
            {
                foreach (var tx in block.Transactions)
                {
                    if (ArrayHelpers.ByteArrayCompare(tx.Id, id))
                    {
                        return tx;
                    }
                }
            }

            return new Transaction();
        }

        public void SignTransaction(Transaction tx, byte[] privateKey)
        {
            var prevTxs = new Dictionary<string, Transaction>();
            foreach (var inp in tx.Inputs)
            {
                var prevTx = FindTransaction(inp.Id);
                prevTxs[HexadecimalEncoding.ToHexString(prevTx.Id)] = prevTx;
            }

            tx.Sign(privateKey, prevTxs);
        }

        public bool VerifyTransaction(Transaction tx)
        {
            if (tx.IsCoinBase()) return true; // new byte[]{} id, which is a problem as well as I dont need to verify cb txs

            var prevTxs = new Dictionary<string, Transaction>();
            foreach (var inp in tx.Inputs)
            {
                var prevTx = FindTransaction(inp.Id);
                prevTxs[HexadecimalEncoding.ToHexString(prevTx.Id)] = prevTx;
            }

            return tx.Verify(prevTxs);
        }

        public IEnumerator<Block> GetEnumerator()
        {
            var currentHash = LastHash;

            while (currentHash != null)
            {
                var block = new Block().DeSerialize(ChainDb.Get(currentHash));
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