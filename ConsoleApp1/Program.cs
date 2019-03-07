using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLib;
using CoreLib.Blockchain;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var startTime = DateTime.Now;

            var wBank = new WalletBank();

            for (var i = 0; i < 100; i++) wBank.CreateWallet();

            



            //foreach (var wallet in wBank)
            //{
            //    var isOk = wallet.VerifyAddress(wallet.Address);
            //    Console.WriteLine(isOk);
            //}


            //var blockChain = new BlockChain();
            //blockChain.Print();

            var endTime = DateTime.Now;
            Console.WriteLine($"Duration: {endTime - startTime}");
            Console.Read();
        }
    }
}