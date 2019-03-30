using ChainUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace CoreLib.Blockchain
{
    [Serializable]
    public class Transaction
    {
        private Transaction(Transaction transaction)
        {
            Id = transaction.Id;
            Inputs = transaction.Inputs;
            Outputs = transaction.Outputs;
            TimeStamp = transaction.TimeStamp;
        }

        public byte[] Id { get; set; }
        public List<TxInput> Inputs { get; set; }
        public List<TxOutput> Outputs { get; set; }
        public DateTime TimeStamp { get; set; }

        public Transaction()
        {
        }

        private static Transaction TransactionTrimmed(Transaction transaction)
        {
            List<TxOutput> txOutputs = new List<TxOutput>();
            List<TxInput> txInputs = new List<TxInput>();

            foreach (var input in transaction.Inputs)
            {
                txInputs.Add(new TxInput() {Id = input.Id, Out = input.Out, Signature = null, PubKey = null});
            }

            foreach (var (output, index) in transaction.Outputs.Select((v, i) => (v, i)))
            {
                txOutputs.Add(new TxOutput() {PublicKeyHash = output.PublicKeyHash, Value = output.Value});
            }

            var txCopy = new Transaction()
            {
                Inputs = txInputs,
                Outputs = txOutputs,
                Id = transaction.Id,
                TimeStamp = transaction.TimeStamp
            };
            return txCopy;
        }


        public void Sign(byte[] privateKey, Dictionary<string, Transaction> prevTxs)
        {
            if (IsCoinBase()) return;

            foreach (var inp in Inputs)
            {
                var hex = HexadecimalEncoding.ToHexString(inp.Id);
                if (prevTxs[hex]?.Id == null)
                {
                    Console.WriteLine("prev transaction does not exist");
                }
            }

            var txCopy = TransactionTrimmed(this);

            var index = 0;
            foreach (var inp in Inputs)
            {
                var prevTx = prevTxs[HexadecimalEncoding.ToHexString(inp.Id)];
                txCopy.Inputs[index].Signature = null;
                txCopy.Inputs[index].PubKey = prevTx.Outputs[inp.Out].PublicKeyHash;
                txCopy.Id = txCopy.CalculateHash();
                txCopy.Inputs[index].PubKey = null;
                

                var signature = CryptoFinal.SignTransaction(txCopy.Id, privateKey);
                Inputs[index].Signature = signature;
                index++;
            }
        }

        public bool Verify(Dictionary<string, Transaction> prevTxs)
        {
            if (IsCoinBase()) return true;

            foreach (var inp in Inputs)
            {
                var hex = HexadecimalEncoding.ToHexString(inp.Id);
                if (prevTxs[hex]?.Id == null)
                {
                    Console.WriteLine("prev transaction does not exist");
                }
            }

            var txCopy = TransactionTrimmed(this);

            var index = 0;
            foreach (var inp in Inputs)
            {
                var prevTx = prevTxs[HexadecimalEncoding.ToHexString(inp.Id)];
                txCopy.Inputs[index].Signature = null;
                txCopy.Inputs[index].PubKey = prevTx.Outputs[inp.Out].PublicKeyHash;
                txCopy.Id = txCopy.CalculateHash();
                txCopy.Inputs[index].PubKey = null;

                var (r, s, v) = CryptoFinal.GetRSV(inp.Signature);
                var recoveredKey =
                    CryptoFinal.RecoverPublicKey(r.ToByteArray(), s.ToByteArray(), v.ToByteArray(), txCopy.Id);

                if (!CryptoFinal.VerifyHashed(inp.Signature, recoveredKey, txCopy.Id))
                {
                    Console.WriteLine("verify failed");
                    return false;
                }

                index++;
            }
            //Console.WriteLine("verify ok");
            return true;
        }


        public byte[] CalculateHash()
        {
            SHA256 sha256 = SHA256.Create();

            var copyOfTx = new Transaction(this);
            copyOfTx.Id = new byte[]{};
            byte[] inputBytes = copyOfTx.Serialize();
            byte[] outputBytes = sha256.ComputeHash(inputBytes);

            return outputBytes;
        }


        public bool IsCoinBase()
        {
            return Inputs.Count == 1 && Inputs[0].Id.Length == 0 && Inputs[0].Out == -1;
        }
        

        //ver1
        public static Transaction NewTransaction(string from, string to, int amount, UTXOSet set)
        {
            var inputs = new List<TxInput>();
            var outputs = new List<TxOutput>();

            var walletBank = new WalletBank();
            var wallet = walletBank.FindWallet(from);
            var pubKeyHash = wallet.PublicKeyHash;

            var spendableOutputs = set.FindSpendableOutputs(pubKeyHash, amount);
            var account = spendableOutputs.Item2;
            var validOutputs = spendableOutputs.Item1;

            if (account < amount)
            {
                Console.WriteLine("Insufficient funds");
                return null;
            }
            else
            {
                foreach (var output in validOutputs.Keys)
                {
                    var txId = (output);

                    if (validOutputs.ContainsKey(txId)) { 
                    foreach (var @out in validOutputs[txId])
                    {
                        var input = new TxInput() { Id = HexadecimalEncoding.FromHexStringToByte(txId), Out = @out, Signature = null, PubKey = wallet.PublicKey };
                        inputs.Add(input);
                    }
                    }
                }

                outputs.Add(TxOutput.NewTxOutput(amount, to));

                if (account > amount)
                {
                    outputs.Add(TxOutput.NewTxOutput(account - amount, from));
                }
            }

            var tx = new Transaction()
            {
                Id = null,
                Inputs = inputs,
                Outputs = outputs,
                TimeStamp = DateTime.Now
            };
            tx.Id = tx.CalculateHash();
            set.Chain.SignTransaction(tx, wallet.PrivateKey);
            return tx;
        }
        public static Transaction CoinBaseTx(string to, string data)
        {
            if (data == "") data = "Coins to " + to;

            var txIn = new TxInput()
            {
                Id = new byte[] { }, // no referencing any output
                Out = -1, // no referencing any output
                Signature = null,
                PubKey = ByteHelper.GetBytesFromString(data)
            };

            var txOut = TxOutput.NewTxOutput(100, to);

            var tx = new Transaction()
            {
                Id = null,
                Outputs = new List<TxOutput> {txOut},
                Inputs = new List<TxInput>() {txIn}
            };
            tx.Id = tx.CalculateHash();

            return tx;
        }


        public byte[] Serialize()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            return ms.ToArray();
        }

        public Transaction DeSerialize(byte[] fromBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(fromBytes, 0, fromBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            return (Transaction) binForm.Deserialize(memStream);
        }
    }
}