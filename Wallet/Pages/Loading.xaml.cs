using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ChainUtils;
using CoreLib;
using LightningDB;


namespace Wallet.Pages
{
    /// <summary>
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : Page
    {
        public Loading()
        {
            InitializeComponent();
            Loaded += Loading_OnLoaded;
        }

        private void Loading_OnLoaded(object sender, RoutedEventArgs e)
        {
            //BlockChain b = new BlockChain();


            //for (int i = 0; i < 6; i++)
            //{
            //    b.Send("dusan", "veronika", 20);
            //    b.GetBalance("dusan");
            //    b.GetBalance("veronika");
            //}


            //Console.WriteLine(a);
            //Console.WriteLine(a.VerifyAddress(a.Address));


            var wallet = new WalletCore();
            var privK = wallet.PrivateKey;
            var pubK = wallet.PublicKey;

            var str = "ahoj";

            var hash = wallet.SignMessage(str);
            Console.WriteLine("Signature: " + hash);

            Console.WriteLine(wallet.VerifyHashedMessage(hash, str));


            //var list = new List<byte[]>();
            //var arr = hash.ToArray();

            //foreach (var sigVal in arr)
            //{
            //    list.Add(sigVal);
            //}


            //var recoveredKey = Crypto.RecoverPublicKey(list[0], list[1], list[2], bytes);
            //if (StructuralComparisons.StructuralEqualityComparer.Equals(recoveredKey, pubK))
            //{
            //    Console.WriteLine("GOOD JOB");
            //}

            //var isOk = Crypto.VerifyHashed(list[0], list[1], recoveredKey, bytes);
            //Console.WriteLine(isOk);


            //Console.WriteLine();
            //var genesis = b.GetLatestBlock();
            //var bytes = genesis.Serialize();
            //var deser = new Block();
            //deser = deser.DeSerialize(bytes);
            //Console.WriteLine(deser);


            //    if (File.Exists("pub.dat") && File.Exists("pri.dat"))
            //    {
            //        var pub = File.ReadAllBytes("pub.dat");
            //        var pri = File.ReadAllBytes("pri.dat");

            //        var hash = Crypto.SignTransaction(Encoding.UTF8.GetBytes("test"), pri);
            //        var isOk = Crypto.VerifyHashed(hash, pub, Encoding.UTF8.GetBytes("test"));
            //        if (isOk)
            //        {
            //            Console.WriteLine("Registered");
            //            // go to Login 
            //            NavigationService?.Navigate(new Login());
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine("Not registered");
            //        // continue presenting register form
            //        NavigationService?.Navigate(new Registration());
            //    }
        }
    }
}