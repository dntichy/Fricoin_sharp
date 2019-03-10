using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using ChainUtils;

namespace CoreLib.Blockchain
{
    [Serializable]
    public class TxOutputs
    {
        
      public List<TxOutput> Outputs { get; set; }

      public TxOutputs()
      {
          Outputs = new List<TxOutput>();
      }

        public byte[] Serialize()
      {
          BinaryFormatter bf = new BinaryFormatter();
          MemoryStream ms = new MemoryStream();
          bf.Serialize(ms, this);
          return ms.ToArray();
      }

      public static TxOutputs DeSerialize(byte[] fromBytes)
      {
          MemoryStream memStream = new MemoryStream();
          BinaryFormatter binForm = new BinaryFormatter();
          memStream.Write(fromBytes, 0, fromBytes.Length);
          memStream.Seek(0, SeekOrigin.Begin);
          return (TxOutputs)binForm.Deserialize(memStream);
      }
    }

    public class UTXOSet
    {
        public BlockChain Chain;

        public UTXOSet(BlockChain bchain)
        {
            Chain = bchain;
        }

        private void DeleteKeys()
        {
            var collectSize= 100000;
            var keysForDelete =new List<byte[]>();
            var collected = 0;
            
            var cursor =Chain.TransactionDB.Cursor();

           foreach (var current in cursor)
           {
               keysForDelete.Add(current.Key);
               collected++;

               if (collected == collectSize)
               {
                   Chain.TransactionDB.DeleteKeys(keysForDelete);
                   keysForDelete = new List<byte[]>();
                   collected = 0;
               }
           }

           if (collected > 0)
           {
               Chain.TransactionDB.DeleteKeys(keysForDelete);
           }
        }

        public void ReIndex()
        {

            DeleteKeys();
            var UTXO = Chain.FindUtxo();

            var index = 0;
            foreach (var outputs in UTXO)
            {
                var key = HexadecimalEncoding.FromHexStringToByte(outputs.Key);
                Chain.TransactionDB.Put(key, outputs.Value.Serialize());

                index++;
            }
        }

        public void Update(Block b)
        {
            foreach (var tx in b.Transactions)
            {
                if (!tx.IsCoinBase())
                {
                    foreach (var input in tx.Inputs)
                    {
                        var updatedOutputs = new TxOutputs();
                        var inId = input.Id;
                        var item = Chain.TransactionDB.Get(inId);

                        var outs = TxOutputs.DeSerialize(item);

                        foreach (var (@out, index) in outs.Outputs.Select((v, i) => (v, i)))
                        {
                            if (input.Out != index)
                            {
                                updatedOutputs.Outputs.Add(@out);
                            }
                        }

                        if (updatedOutputs.Outputs.Count == 0)
                        {
                            Chain.TransactionDB.Delete(inId);
                           
                        }
                        else
                        {
                            Chain.TransactionDB.Put(inId, updatedOutputs.Serialize());
                        }
                }
                }

                var newOutputs = new TxOutputs();
                foreach (var @out in tx.Outputs)
                {
                    newOutputs.Outputs.Add(@out);
                }

                var txId = tx.Id;
                Chain.TransactionDB.Put(txId, newOutputs.Serialize());
            }
        }

        public int CountTransactions()
        {
            var counter = 0;

            var cursor = Chain.TransactionDB.Cursor();
            foreach (var _ in cursor)
            {
                counter++;
            }
            
            return counter;
        }


        public (Dictionary<string, List<int>>, int) FindSpendableOutputs(byte[] pubKeyHash, int amount)
        {
            var unspentOuts = new Dictionary<string, List<int>>();
            var accumulated = 0;

            var cursor = Chain.TransactionDB.Cursor();
            foreach (var current in cursor)
            {
                var key = current.Key;
                var value = current.Value;

                var txId = HexadecimalEncoding.ToHexString(key);
                var outs = TxOutputs.DeSerialize(value);


                if (!unspentOuts.ContainsKey(txId)) unspentOuts.Add(txId, new List<int>());

            foreach (var (output, index) in outs.Outputs.Select((v, i) => (v, i)))
                {

                    if (output.IsLockedWithKey(pubKeyHash) && accumulated < amount)
                    {
                        accumulated += output.Value;
                        unspentOuts[txId].Add(index);

                    }
                }
            }

            return (unspentOuts, accumulated);
        }
    }
}