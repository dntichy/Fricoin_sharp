using ChainUtils;
using CoreLib;
using CoreLib.Blockchain;
using System;
using System.Windows;
using System.Windows.Controls;


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
            BlockChain b = new BlockChain();

            
            var wb = new WalletBank();

            var user1 = wb.FindWallet("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC");
            var user2 = wb.FindWallet("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG");

            Console.WriteLine(user1);
            Console.WriteLine(" should be equal prev " + Convert.ToBase64String(WalletCore.PublicKeyHashed(user1.PublicKey)));

       //decode address
        var address = user1.Address;;
        var addressToPkHash = Base58Encoding.Decode(address);
        addressToPkHash = ArrayHelpers.SubArray(addressToPkHash, 1, addressToPkHash.Length - 5);
        Console.WriteLine("address to pkhash: "+Convert.ToBase64String(addressToPkHash));
            
        //public key to publicHashKey
        var pkHas = WalletCore.PublicKeyHashed(user1.PublicKey);
        Console.WriteLine("PK hash "+Convert.ToBase64String(pkHas));
            
        
            b.Send("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", "1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", 20);
            b.Send("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", "1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", 20);

            b.GetBalance("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC");
            b.GetBalance("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG");

            b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
            b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
            b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
            b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
            b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
            b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
            b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
            b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
            b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
            b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);


         

            b.GetBalance("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC");
            b.GetBalance("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG");





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