using DatabaseLib;
using P2PLib.Network.MessageParser.Messages;
using System.ComponentModel;
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
            //var user = new User { FirstName = "D"};
            //NavigationService?.Navigate(new WalletPage(user));


            //BlockChain friCoin = new BlockChain();
            //friCoin.Send("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", "19p2is8biiWDEBhbfQb4yQRv1zwKX1CR17", 50);
            //friCoin.Send("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", "19p2is8biiWDEBhbfQb4yQRv1zwKX1CR17", 20);
            //friCoin.Send("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", "19p2is8biiWDEBhbfQb4yQRv1zwKX1CR17", 20);

            //var pTransaction = new PersistenceTransaction();
            //var cursor = pTransaction.Cursor();

            //foreach (var pair in cursor)
            //{
            //    Console.WriteLine(pair.Key);
            //}

            //BlockChain b = new BlockChain();
            //var wb = new WalletBank();
            //var user1 = wb.FindWallet("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC");
            //var user2 = wb.FindWallet("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG");
            //b.Send("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", "1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", 20);
            //b.Send("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", "1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG", 20);
            //b.GetBalance("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC");
            //b.GetBalance("1KEXhE2mFTtn5HYeLxfXhxykb95GMfZSqG");

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