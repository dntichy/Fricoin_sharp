using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using ChainUtils;

namespace CoreLib
{
    [Serializable]
    public class Transaction
    {
        private Transaction(Transaction transaction)
        {
            Id = transaction.Id;
            Inputs = transaction.Inputs;
            Outputs = transaction.Outputs;
        }

        public byte[] Id { get; set; }
        public List<TxInput> Inputs { get; set; }
        public List<TxOutput> Outputs { get; set; }

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
            foreach (var inp in txCopy.Inputs)
            {
                var prevTx = prevTxs[HexadecimalEncoding.ToHexString(inp.Id)];
                txCopy.Inputs[index].Signature = null;
                txCopy.Inputs[index].PubKey = prevTx.Outputs[inp.Out].PublicKeyHash;
                txCopy.Id = CalculateHash();
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
            foreach (var inp in txCopy.Inputs)
            {
                var prevTx = prevTxs[HexadecimalEncoding.ToHexString(inp.Id)];
                txCopy.Inputs[index].Signature = null;
                txCopy.Inputs[index].PubKey = prevTx.Outputs[inp.Out].PublicKeyHash;
                txCopy.Id = CalculateHash();
                txCopy.Inputs[index].PubKey = null;

                var (r, s, v) = CryptoFinal.GetRSV(inp.Signature);
                var recoveredKey =
                    CryptoFinal.RecoverPublicKey(r.ToByteArray(), s.ToByteArray(), v.ToByteArray(), txCopy.Id);

                if (!CryptoFinal.VerifyHashed(inp.Signature, recoveredKey, txCopy.Id))
                {
                    return false;
                }

                ;
                index++;
            }

            return true;
        }

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
                Id = transaction.Id
            };
            return txCopy;
        }

        public byte[] CalculateHash()
        {
            SHA256 sha256 = SHA256.Create();

            var copyOfTx = new Transaction(this);
            copyOfTx.Id = null;
            byte[] inputBytes = copyOfTx.Serialize();
            byte[] outputBytes = sha256.ComputeHash(inputBytes);

            return outputBytes;
        }


        public bool IsCoinBase()
        {
            return Inputs.Count == 1 && Inputs[0].Id.Length == 0 && Inputs[0].Out == -1;
        }

        public void SetId()
        {
            SHA256 sha256 = SHA256.Create();
            byte[] inputBytes =
                Encoding.ASCII.GetBytes(
                    $"{Id} - {JsonConvert.SerializeObject(Inputs)} - {JsonConvert.SerializeObject(Outputs)}");
            byte[] outputBytes = sha256.ComputeHash(inputBytes);

            this.Id = outputBytes;
        }

        public static Transaction NewTransaction(string from, string to, int amount, BlockChain chain)
        {
            var inputs = new List<TxInput>();
            var outputs = new List<TxOutput>();

            //todo getWallet...
            var wallet = new WalletCore();
            var pubKeyHash = wallet.PublicKeyHash;

            var spendableOutputs = chain.FindSpendableOutputs(pubKeyHash, amount);
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
                    var txId = output;

                    foreach (var @out in validOutputs[txId])
                    {
                        var input = new TxInput() {Id = txId, Out = @out, Signature = null, PubKey = wallet.PublicKey};
                        inputs.Add(input);
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
                Outputs = outputs
            };
            chain.SignTransaction(tx, wallet.PrivateKey);
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
            tx.SetId();

            return tx;
        }
    }

    [Serializable]
    public class TxOutput
    {
        public int Value { get; set; }
        public byte[] PublicKeyHash { get; set; }

        public static TxOutput NewTxOutput(int value, string address)
        {
            var txO = new TxOutput()
            {
                Value = value
            };

            txO.Lock(ByteHelper.GetBytesFromString(address));
            return txO;
        }

        //public bool CanBeUnlocked(string address)
        //{
        //    return address == PublicKey;
        //}
        public void Lock(byte[] address)
        {
            var pubKeyHashed = Base58Encoding.Decode(ByteHelper.GetStringFromBytes(address));
            PublicKeyHash =
                ArrayHelpers.SubArray(
                    Base58Encoding.VerifyAndRemoveCheckSum(pubKeyHashed), 1); // remove version // remove checksum
        }

        public bool IsLockedWithKey(byte[] pubKeyHash)
        {
            return ArrayHelpers.ByteArrayCompare(PublicKeyHash, pubKeyHash);
        }
    }

    [Serializable]
    public class TxInput
    {
        public byte[] Id { get; set; }
        public int Out { get; set; }
        public byte[] Signature { get; set; }
        public byte[] PubKey { get; set; }

        //public bool CanUnlock(string address)
        //{
        //    return address == Signature;
        //}
        public bool UsesKey(byte[] pubKeyHash)
        {
            var lockingHash = WalletCore.PublicKeyHashed(PubKey);
            return ArrayHelpers.ByteArrayCompare(lockingHash, pubKeyHash);
        }
    }
}