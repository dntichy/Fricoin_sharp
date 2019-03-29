using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLib;
using CoreLib.Blockchain;
using CoreLib.DataStructures.MerkleTree;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var startTime = DateTime.Now;

            //var wBank = new WalletBank();

            //for (var i = 0; i < 100; i++) wBank.CreateWallet();


            //foreach (var wallet in wBank)
            //{
            //    var isOk = wallet.VerifyAddress(wallet.Address);
            //    Console.WriteLine(isOk);
            //}


            //var blockChain = new BlockChain();
            //blockChain.Print();


            //var txList = new List<Transaction>();
            //for(var i = 0; i<10000; i++) { 
            //Transaction tx1 = new Transaction()
            //{
            //   Id = null,
            //   Inputs = new List<TxInput>()
            //   {
            //       new TxInput() {
            //           Id = new byte[] {},
            //           PubKey = new byte[]{0x01},
            //           Out = i
            //       }
            //   } ,
            //   Outputs = new List<TxOutput>() { new TxOutput()
            //   {
            //       Value = i,
            //   } }
            //};
            //tx1.CalculateHash();

            //    txList.Add(tx1);
            //}

            //var  merkleTree = new MerkleTree(txList);
            //merkleTree.LevelOrder();

            var list = new List<Transaction>();

            var bank = new WalletBank();
            var _loggedUserWallet = bank.FindWallet("112H2TcYAvxWGPSWXz4bzGvm5RXEdFDCms");
            var _friChain = new BlockChain("x");
            var utxoSet = new UTXOSet(_friChain);
            for (int i = 0; i < 20; i++)
            {
                var tx = Transaction.NewTransaction("112H2TcYAvxWGPSWXz4bzGvm5RXEdFDCms", "1fp9JwtnMMnYVLaABMEQuKGtpXUnJm7Cz", 2, utxoSet);
                if (tx == null) return;
                list.Add(tx);
            }

            var block = _friChain.MineBlock(list);
            utxoSet.Update(block);
            _friChain.GetBalance("112H2TcYAvxWGPSWXz4bzGvm5RXEdFDCms");

            var endTime = DateTime.Now;
            Console.WriteLine($"Duration: {endTime - startTime}");
            Console.Read();
        }
    }
}