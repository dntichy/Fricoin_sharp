using ChainUtils;
using CoreLib;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Org.BouncyCastle.Asn1;


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