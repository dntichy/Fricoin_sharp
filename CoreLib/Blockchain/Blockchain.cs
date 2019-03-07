using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ChainUtils;
using CoreLib.Interfaces;
using Newtonsoft.Json;

namespace CoreLib.Blockchain
{
    public class BlockChain : IChain, IEnumerable<Block>
    {
        public int Difficulty { set; get; } = 2;
        public byte[] LastHash { get; set; }

        public Persistence Db = new Persistence();

        //private IList<Transaction>
        //    _transactionPool = new List<Transaction>(); //todo delete, persist


        public BlockChain()
        {
            //var lastHash = Db.Get(StringToByteArray("lh"));
            ////if exists lasthash retrieve it and set
            //if (lastHash != null)
            //{
            //    LastHash = lastHash;
            //}
            //else
            {
                //else create genesis block and set lasthash to the genesis
                var genesis = Block.GenesisBlock();
                Db.Put(StringToByteArray("lh"), genesis.Hash);
                Db.Put(genesis.Hash, genesis.Serialize());
                LastHash = genesis.Hash;
            }
        }


        public Block GetLatestBlock()
        {
            return (new Block()).DeSerialize(Db.Get(LastHash));
        }

        public void AddBlock(Block block)
        {
            Block latestBlock = GetLatestBlock();
            block.Index = latestBlock.Index + 1;
            block.Mine(this.Difficulty);

            Db.Put(block.Hash, block.Serialize());
            Db.Put(StringToByteArray("lh"), block.Hash);
            LastHash = block.Hash;
        }

        public void AddBlock(List<Transaction> transactions)
        {
            foreach (var tx in transactions)
            {
                if (VerifyTransaction(tx) != true)
                {
                    Console.WriteLine("problem addblock, invalid tx");
                    return;
                }
            }

            Block latestBlock = GetLatestBlock();

            var newBlock = new Block()
            {
                TimeStamp = DateTime.Now,
                PreviousHash = LastHash,
                Transactions = transactions
                
            };
            newBlock.Index = latestBlock.Index + 1;
            newBlock.Mine(Difficulty);

            Db.Put(newBlock.Hash, newBlock.Serialize());
            Db.Put(StringToByteArray("lh"), newBlock.Hash);
            LastHash = newBlock.Hash;
        }


        //todo delete, redo
        //public void ProcessPendingTransactions(string minerAddress)
        //{
        //    Block block = new Block(DateTime.Now, GetLatestBlock().Hash, _transactionPool);
        //    AddBlock(block); // pridaj block

        //    _transactionPool = new List<Transaction>(); // reset TransactionPool
        //    //CreateTransaction(new Transaction(null, minerAddress, 1)); //pridaj odmenovú transakciu
        //}

        //todo redo
        //public void CreateTransaction(Transaction transaction)
        //{
        //    _transactionPool.Add(transaction);
        //}


        public void Send(string from, string to, int amount)
        {
            var tx = Transaction.NewTransaction(from, to, amount, this);
            if (tx != null)
            {
                AddBlock(new List<Transaction>(){tx});
            }
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
                    var prevBlock = new Block().DeSerialize(Db.Get(block.PreviousHash));
                    if (block.PreviousHash != prevBlock.CalculateHash())
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        //public float GetBalance(byte[] pubKeyHash)
        //{
        //    var balance = 0;
        //    var UTXO = FindUTXO(pubKeyHash);

        //    foreach (var output in UTXO)
        //    {
        //        balance += output.Value;
        //    }

        //    Console.WriteLine("Balance of " + ByteHelper.GetStringFromBytes(pubKeyHash) + ": " + balance);
        //    return balance;
        //}

        public float GetBalance(string address)
        {
            var balance = 0;
            var pubKeyHash = Base58Encoding.Decode(address);
            pubKeyHash = ArrayHelpers.SubArray(pubKeyHash, 1, pubKeyHash.Length - 5);
            var utxo = FindUTXO(pubKeyHash);

            foreach (var output in utxo)
            {
                balance += output.Value;
            }

            Console.WriteLine("Balance of " + address + ": " + balance);
            return balance;
        }


        public void Print()
        {
            var currentHash = LastHash;
            while (currentHash != null)
            {
                var block = new Block().DeSerialize(Db.Get(currentHash));
                currentHash = block.PreviousHash;
                Console.WriteLine(JsonConvert.SerializeObject(block, Newtonsoft.Json.Formatting.Indented));
            }
        }

        public List<Transaction> FindUnspentTransactions(byte[] pubKeyHash)
        {
            List<Transaction> unspentTx = new List<Transaction>();

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

                        if (output.IsLockedWithKey(pubKeyHash))
                        {
                            unspentTx.Add(tx);
                        }

                        endOfTheLoop:
                        {
                        }
                        index++;
                    }


                    if (tx.IsCoinBase() == false)
                    {
                        foreach (var input in tx.Inputs)
                        {
                           
                            if (input.UsesKey(pubKeyHash))
                            {
                                if (!spentTxOs.ContainsKey(HexadecimalEncoding.ToHexString(input.Id)))
                                    spentTxOs.Add(HexadecimalEncoding.ToHexString(input.Id), new List<int>());
                                spentTxOs[HexadecimalEncoding.ToHexString(input.Id)].Add(input.Out);
                            }
                        }
                    }
                }
            }

            return unspentTx;
        }

        public List<TxOutput> FindUTXO(byte[] pubKeyHash)
        {
            var UTXOs = new List<TxOutput>();
            var unspentTransactions = FindUnspentTransactions(pubKeyHash);

            foreach (var tx in unspentTransactions)
            {
                foreach (var output in tx.Outputs)
                {
                    if (output.IsLockedWithKey(pubKeyHash))
                    {
                        UTXOs.Add(output);
                    }
                }
            }

            return UTXOs;
        }

        public (Dictionary<byte[], List<int>>, int) FindSpendableOutputs(byte[] pubKeyHash, int amount)
        {
            var unspentOuts = new Dictionary<byte[], List<int>>();
            var unspentTxs = FindUnspentTransactions(pubKeyHash);
            var accumulated = 0;

            foreach (var tx in unspentTxs)
            {
                var txid = tx.Id;
                if (!unspentOuts.ContainsKey(txid)) unspentOuts.Add(txid, new List<int>());

                var index = 0;
                foreach (var output in tx.Outputs)
                {
                    if (output.IsLockedWithKey(pubKeyHash) && accumulated < amount)
                    {
                        accumulated += output.Value;
                        unspentOuts[txid].Add(index);

                        if (accumulated >= amount)
                        {
                            goto endOfLoop;
                        }
                    }

                    index++;
                }
            }

            endOfLoop:
            {
            }

            return (unspentOuts, accumulated);
        }


        //todo move
        public byte[] StringToByteArray(string str)
        {
            return Encoding.ASCII.GetBytes(str);
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