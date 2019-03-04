using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CoreLib
{
    [Serializable]
    public class Transaction
    {
        public byte[] Id { get; set; }
        public List<TxInput> Inputs { get; set; }
        public List<TxOutput> Outputs { get; set; }


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

            var spendableOutputs = chain.FindSpendableOutputs(from, amount);
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

                    foreach (var s in validOutputs[txId])
                    {
                        var input = new TxInput() {Id = txId, Out = s, Signature = from};
                        inputs.Add(input);
                    }
                }

                outputs.Add(new TxOutput() {PublicKey = to, Value = amount});

                if (account > amount)
                {
                    outputs.Add(new TxOutput()
                    {
                        PublicKey = from,
                        Value = account - amount
                    });
                }
            }

            var tx = new Transaction()
            {
                Id = null,
                Inputs = inputs,
                Outputs = outputs
            };
            tx.SetId();
            return tx;
        }


        public static Transaction CoinBaseTx(string to, string data)
        {
            if (data == "") data = "Coins to " + to;

            var txIn = new TxInput()
            {
                Id = new byte[] { }, // no referencing any output
                Out = -1, // no referencing any output
                Signature = data
            };

            var txOut = new TxOutput()
            {
                PublicKey = to,
                Value = 100
            };

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
        public string PublicKey { get; set; }

        public bool CanBeUnlocked(string address)
        {
            return address == PublicKey;
        }
    }

    [Serializable]
    public class TxInput
    {
        public byte[] Id { get; set; }
        public int Out { get; set; }
        public string Signature { get; set; }

        public bool CanUnlock(string address)
        {
            return address == Signature;
        }
    }
}