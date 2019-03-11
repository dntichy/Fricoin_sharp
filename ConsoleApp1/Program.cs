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


            var endTime = DateTime.Now;
            Console.WriteLine($"Duration: {endTime - startTime}");
            Console.Read();
        }
    }
}