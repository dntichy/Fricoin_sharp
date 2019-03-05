using CoreLib.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using ChainUtils;
using Org.BouncyCastle.Crypto.Tls;


namespace CoreLib
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
            var lastHash = Db.Get(StringToByteArray("lh"));
            //if exists lasthash retrieve it and set
            if (lastHash != null)
            {
                LastHash = lastHash;
            }
            else
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


        //public void Send(string from, string to, int amount)
        //{
        //    var tx = Transaction.NewTransaction(from, to, amount, this);
        //    if (tx != null)
        //    {
        //        AddBlock(new Block(DateTime.Now, LastHash, new List<Transaction>() {tx}));
        //    }
        //}

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

        public float GetBalance(string address)
        {
            throw new NotImplementedException();
        }

        //public float GetBalance(string address)
        //{
        //    var balance = 0;
        //    var UTXO = FindUTXO(address);

        //    foreach (var output in UTXO)
        //    {
        //        balance += output.Value;
        //    }

        //    Console.WriteLine("Balance of " + address + ": " + balance);
        //    return balance;
        //}


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

        //public List<Transaction> FindUnspentTransactions(string address)
        //{
        //    List<Transaction> unspentTx = new List<Transaction>();

        //    var spentTxOs = new Dictionary<string, List<int>>();

        //    foreach (var block in this)
        //    {
        //        foreach (var tx in block)
        //        {
        //            var tXId = tx.Id; //transaction ID
        //            if (!spentTxOs.ContainsKey(ByteArrayToString(tXId)))
        //                spentTxOs.Add(ByteArrayToString(tXId), new List<int>());

        //            var index = 0;
        //            foreach (var output in tx.Outputs)
        //            {
        //                if (spentTxOs.ContainsKey(ByteArrayToString(tXId)))
        //                {
        //                    foreach (var spentOut in spentTxOs[ByteArrayToString(tXId)])
        //                    {
        //                        if (spentOut.Equals(index))
        //                        {
        //                            goto endOfTheLoop;
        //                        }
        //                    }
        //                }

        //                if (output.CanBeUnlocked(address))
        //                {
        //                    unspentTx.Add(tx);
        //                }

        //                endOfTheLoop:
        //                {
        //                }
        //                index++;
        //            }


        //            if (tx.IsCoinBase() == false)
        //            {
        //                foreach (var input in tx.Inputs)
        //                {
        //                    if (input.CanUnlock(address))
        //                    {
        //                        if (!spentTxOs.ContainsKey(ByteArrayToString(input.Id)))
        //                            spentTxOs.Add(ByteArrayToString(input.Id), new List<int>());
        //                        spentTxOs[ByteArrayToString(input.Id)].Add(input.Out);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return unspentTx;
        //}

        //public List<TxOutput> FindUTXO(string address)
        //{
        //    var UTXOs = new List<TxOutput>();
        //    var unspentTransactions = FindUnspentTransactions(address);

        //    foreach (var tx in unspentTransactions)
        //    {
        //        foreach (var output in tx.Outputs)
        //        {
        //            if (output.CanBeUnlocked(address))
        //            {
        //                UTXOs.Add(output);
        //            }
        //        }
        //    }

        //    return UTXOs;
        //}

        //public (Dictionary<byte[], List<int>>, int) FindSpendableOutputs(string address, int amount)
        //{
        //    var unspentOuts = new Dictionary<byte[], List<int>>();
        //    var unspentTxs = FindUnspentTransactions(address);
        //    var accumulated = 0;

        //    foreach (var tx in unspentTxs)
        //    {
        //        var txid = tx.Id;
        //        if (!unspentOuts.ContainsKey(txid)) unspentOuts.Add(txid, new List<int>());

        //        var index = 0;
        //        foreach (var output in tx.Outputs)
        //        {
        //            if (output.CanBeUnlocked(address) && accumulated < amount)
        //            {
        //                accumulated += output.Value;
        //                unspentOuts[txid].Add(index);

        //                if (accumulated >= amount)
        //                {
        //                    goto endOfLoop;
        //                }
        //            }

        //            index++;
        //        }
        //    }

        //    endOfLoop:
        //    {
        //    }

        //    return (unspentOuts, accumulated);
        //}


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
        }

        public bool VerifyTransaction(Transaction tx)
        {
            return false;
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