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
            //BlockChain b = new BlockChain();
            //var TxDb = new PersistenceTransaction();
            //for (int i = 0; i < 1000; i++) TxDb.Put(ByteHelper.GetBytesFromString("kluc: " + i), ByteHelper.GetBytesFromString("hodnota: " + i * i));
            //TxDb.TestIterator();
            //Console.WriteLine("--------------------------------------------------------------------");
            //b.ChainDb.TestIterator();

        //b.ChainDb.TestIterator();
        //Console.WriteLine("--------------------------------------------------------------------");
        //var txDB = new PersistenceTransaction();
        //var startTime = DateTime.Now;
        //for (int i = 0; i<10000;i++) txDB.Put(ByteHelper.GetBytesFromString("kluc"+i), ByteHelper.GetBytesFromString("hodnota"+i*i));
        //var endTime = DateTime.Now;
        //Console.WriteLine($"Duration: {endTime - startTime}");
        //txDB.TestIterator();
        //Console.WriteLine("--------------------------------------------------------------------");
        //b.ChainDb.TestIterator();

        //     var wb = new WalletBank();

        //     var user1 = wb.FindWallet("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC");
        //     var user2 = wb.FindWallet("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG");

        //     Console.WriteLine(user1);
        //     Console.WriteLine(" should be equal prev " + Convert.ToBase64String(WalletCore.PublicKeyHashed(user1.PublicKey)));

        ////decode address
        // var address = user1.Address;;
        // var addressToPkHash = Base58Encoding.Decode(address);
        // addressToPkHash = ArrayHelpers.SubArray(addressToPkHash, 1, addressToPkHash.Length - 5);
        // Console.WriteLine("address to pkhash: "+Convert.ToBase64String(addressToPkHash));

        // //public key to publicHashKey
        // var pkHas = WalletCore.PublicKeyHashed(user1.PublicKey);
        // Console.WriteLine("PK hash "+Convert.ToBase64String(pkHas));


        //b.Send("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", "1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", 20);
        //     b.Send("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", "1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", 20);

        //     b.GetBalance("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC");
        //     b.GetBalance("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG");

        //     b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
        //     b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
        //     b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
        //     b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
        //     b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
        //     b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
        //     b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
        //     b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
        //     b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);
        //     b.Send("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", "1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", 20);




        //     b.GetBalance("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC");
        //     b.GetBalance("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG");




    }

        private void GoToRegistration(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Registration());
        }

        private void GoToLoginPage(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Login(null));
        }
    }
}