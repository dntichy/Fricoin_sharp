using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Engine.Interfaces;
using Newtonsoft.Json;

namespace Engine.Core
{
    public class Block : IBlock
    {
        public int Index { get; set; }
        public int Nonce { get; set; } = 0;
        public DateTime TimeStamp { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public IList<Transaction> Transactions { get; set; }

        public Block(DateTime timeStamp, string previousHash, IList<Transaction> transactions)
        {
            Index = 0;
            TimeStamp = timeStamp;
            PreviousHash = previousHash;

            Hash = CalculateHash();
            Transactions = transactions;
        }

        public string CalculateHash()
        {
            SHA256 sha256 = SHA256.Create();
            byte[] inputBytes =
                Encoding.ASCII.GetBytes(
                    $"{TimeStamp} - {PreviousHash ?? ""} - {JsonConvert.SerializeObject(Transactions)} - {Nonce}");
            byte[] outputBytes = sha256.ComputeHash(inputBytes);

            return Convert.ToBase64String(outputBytes);
        }

        public void Mine(int difficulty)
        {
            var leadingZeros = new string('0', difficulty);
            while (this.Hash == null || this.Hash.Substring(0, difficulty) != leadingZeros)
            {
                this.Nonce++;
                this.Hash = this.CalculateHash();
            }
        }
    }
}