using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ChainUtils;

namespace CoreLib.Blockchain
{
    [Serializable]
    public class TxOutputs
    {
      public List<TxOutput> Outputs { get; set; }

      public byte[] Serialize()
      {
          BinaryFormatter bf = new BinaryFormatter();
          MemoryStream ms = new MemoryStream();
          bf.Serialize(ms, this);
          return ms.ToArray();
      }

      public TxOutputs DeSerialize(byte[] fromBytes)
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
        private BlockChain _chain;


        public void DeleteByPrefix(byte[] prefix)
        {
            var collectSize = 100000;

            
        }


        private void DeleteKeys(byte[][] keysToDelete)
        {
            foreach (var key in keysToDelete) 
            {
                _chain.ChainDb.Delete(key);
            }
        }
    }
}