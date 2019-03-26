﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using CoreLib.DataStructures.MerkleTree;
using CoreLib.Interfaces;
using Newtonsoft.Json;

namespace CoreLib.Blockchain
{
    [Serializable]
    [JsonObject(MemberSerialization.OptOut)]

    public class Block : IBlock, IEnumerable<Transaction>
    {
        public int Index { get; set; }
        public int Nonce { get; set; } = 0;
        public DateTime TimeStamp { get; set; }
        public byte[] PreviousHash { get; set; }
        public byte[] Hash { get; set; }
        public byte[] MerkleRoot { get; set; }
        public IList<Transaction> Transactions { get; set; }

        public Block(DateTime dateTime, byte[] previousHash, IList<Transaction> transactions)
        {
            Index = 0;
            TimeStamp = dateTime;
            PreviousHash = previousHash;

            Hash = CalculateHash();
            Transactions = transactions;
        }

        public Block()
        {
        }

        public byte[] CalculateHash()
        {
            SHA256 sha256 = SHA256.Create();
            byte[] inputBytes =
                Encoding.ASCII.GetBytes($"{TimeStamp} - {PreviousHash} - {JsonConvert.SerializeObject(Transactions)} - {Nonce}");
            byte[] outputBytes = sha256.ComputeHash(inputBytes);

            return outputBytes;
        }

        public void Mine(int difficulty)
        {
            var leadingZeros = new string('0', difficulty);
            while (Hash == null ||
                   Convert.ToBase64String(Hash).Substring(0, difficulty) != leadingZeros)
            {
                Nonce++;
                Hash = CalculateHash();
            }
        }

        internal void SetMerkleRoot()
        {
            var merkle = new MerkleTree(Transactions);
            MerkleRoot = merkle.RootNode.Data;
        }


        public byte[] Serialize()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            return ms.ToArray();
        }

        public Block DeSerialize(byte[] fromBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(fromBytes, 0, fromBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            return (Block) binForm.Deserialize(memStream);
        }

        public IEnumerator<Transaction> GetEnumerator()
        {
            foreach (var t in Transactions)
            {
                yield return t;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static Block GenesisBlock()
        {
            var transactions = new List<Transaction>()
            {
                Transaction.CoinBaseTx("112H2TcYAvxWGPSWXz4bzGvm5RXEdFDCms", ""),
          
            };
            var genesis = new Block(DateTime.Parse("1.1.2019"), null, transactions);
            genesis.SetMerkleRoot();

            return genesis;
        }
    }
}