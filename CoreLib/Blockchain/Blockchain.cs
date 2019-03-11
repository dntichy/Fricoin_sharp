using ChainUtils;
using CoreLib.Interfaces;
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

        public PersistenceChain ChainDb = new PersistenceChain();
        public PersistenceTransaction TransactionDB = new PersistenceTransaction();

        public BlockChain()
        {
            //var lastHash = ChainDb.Get(StringToByteArray("lh"));
            ////if exists lasthash retrieve it and set
            //if (lastHash != null)
            //{
            //    LastHash = lastHash;
            //}
            //else
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

        //public void AddBlock(Block block)
        //{
        //    Block latestBlock = GetLatestBlock();
        //    block.Index = latestBlock.Index + 1;
        //    block.Mine(this.Difficulty);

        //    ChainDb.Put(block.Hash, block.Serialize());
        //    ChainDb.Put(ByteHelper.GetBytesFromString("lh"), block.Hash);
        //    LastHash = block.Hash;
        //}

        private Block AddBlock(List<Transaction> transactions)
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

            var newBlock = new Block()
            {
                TimeStamp = DateTime.Now,
                PreviousHash = LastHash,
                Transactions = transactions
                
            };
            newBlock.Index = latestBlock.Index + 1;

            //set Merkle root
            newBlock.SetMerkleRoot();
            //MINE IT
            newBlock.Mine(Difficulty);

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
            Console.WriteLine("DONE, there is: "+ countTxs +" transactions");
        }

        public void Send(string from, string to, int amount)
        {
            var utxoSet = new UTXOSet(this); 

            var tx = Transaction.NewTransaction(from, to, amount, utxoSet);
            if (tx == null)
            {
                return;
            }
            
              var block = AddBlock(new List<Transaction>(){tx});
              utxoSet.Update(block);
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

        //ver0
        //public List<Transaction> FindUnspentTransactionsOutputs(byte[] pubKeyHash)
        //{
        //    List<Transaction> unspentTx = new List<Transaction>();

        //    var spentTxOs = new Dictionary<string, List<int>>();

        //    foreach (var block in this)
        //    {
        //        foreach (var tx in block)
        //        {
        //            var tXId = tx.Id; //transaction ID
        //            if (!spentTxOs.ContainsKey(HexadecimalEncoding.ToHexString(tXId)))
        //                spentTxOs.Add(HexadecimalEncoding.ToHexString(tXId), new List<int>());

        //            var index = 0;
        //            foreach (var output in tx.Outputs)
        //            {
        //                if (spentTxOs.ContainsKey(HexadecimalEncoding.ToHexString(tXId)))
        //                {
        //                    foreach (var spentOut in spentTxOs[HexadecimalEncoding.ToHexString(tXId)])
        //                    {
        //                        if (spentOut.Equals(index))
        //                        {
        //                            goto endOfTheLoop;
        //                        }
        //                    }
        //                }

        //                if (output.IsLockedWithKey(pubKeyHash))
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

        //                    if (input.UsesKey(pubKeyHash))
        //                    {
        //                        if (!spentTxOs.ContainsKey(HexadecimalEncoding.ToHexString(input.Id)))
        //                            spentTxOs.Add(HexadecimalEncoding.ToHexString(input.Id), new List<int>());
        //                        spentTxOs[HexadecimalEncoding.ToHexString(input.Id)].Add(input.Out);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return unspentTx;
        //}

        //ver1
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



        //ver0
        //public List<TxOutput> FindUtxo(byte[] pubKeyHash)
        //{
        //    var UTXOs = new List<TxOutput>();
        //    var unspentTransactions = FindUnspentTransactionsOutputs(pubKeyHash);

        //    foreach (var tx in unspentTransactions)
        //    {
        //        foreach (var output in tx.Outputs)
        //        {
        //            if (output.IsLockedWithKey(pubKeyHash))
        //            {
        //                UTXOs.Add(output);
        //            }
        //        }
        //    }

        //    return UTXOs;
        //}

        //ver1
        public Dictionary<string, TxOutputs> FindUtxo()
        {
            var UTXOs = new Dictionary<string, TxOutputs>();
            
            var spentTxOs = new Dictionary<string, List<int>>();


            foreach (var block in this)
            {
                foreach (var tx in block)
                {
                    var tXId = tx.Id; //transaction ID

                    //pokus
                    var hex = HexadecimalEncoding.ToHexString(tXId);
                    var backToBytes = HexadecimalEncoding.FromHexStringToByte(hex);
                    

                    //endpokus


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


            //var unspentTransactions = FindUnspentTransactionsOutputs(pubKeyHash);
            //foreach (var tx in unspentTransactions)
            //{
            //    foreach (var output in tx.Outputs)
            //    {
            //        if (output.IsLockedWithKey(pubKeyHash))
            //        {
            //            UTXOs.Add(output);
            //        }
            //    }
            //}

            return UTXOs;
        }

        //ver0
        //public (Dictionary<byte[], List<int>>, int) FindSpendableOutputs(byte[] pubKeyHash, int amount)
        //{
        //    var unspentOuts = new Dictionary<byte[], List<int>>();
        //    var unspentTxs = FindUnspentTransactionsOutputs(pubKeyHash);
        //    var accumulated = 0;

        //    foreach (var tx in unspentTxs)
        //    {
        //        var txid = tx.Id;
        //        if (!unspentOuts.ContainsKey(txid)) unspentOuts.Add(txid, new List<int>());

        //        var index = 0;
        //        foreach (var output in tx.Outputs)
        //        {
        //            if (output.IsLockedWithKey(pubKeyHash) && accumulated < amount)
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

        //ver1
        //public (Dictionary<string, List<int>>, int) FindSpendableOutputs(byte[] pubKeyHash, int amount)
        //{
        //    var unspentOuts = new Dictionary<string, List<int>>();
        //    var accumulated = 0;

        //    var cursor = TransactionDB.Cursor();
        //    while (cursor.MoveNext())
        //    {
        //        var key = cursor.Current.Key;
        //        var value = cursor.Current.Value;

        //        var txId = HexadecimalEncoding.ToHexString(key);
        //        var outs = TxOutputs.DeSerialize(value);

        //        foreach (var (output, index) in outs.Outputs.Select((v, i) => (v, i)))
        //        {
                    
                
        //        if (output.IsLockedWithKey(pubKeyHash) && accumulated < amount)
        //        {
        //            accumulated += output.Value;
        //            unspentOuts[txId].Add(index);

        //        }
        //        }
        //    }

        //    return (unspentOuts, accumulated);
        //}



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