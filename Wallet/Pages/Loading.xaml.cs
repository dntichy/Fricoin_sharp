using CoreLib;
using CoreLib.Blockchain;
using System.Collections.Generic;
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

            //var list = new List<Transaction>();

            //var bank = new WalletBank();
            //var _loggedUserWallet = bank.FindWallet("112H2TcYAvxWGPSWXz4bzGvm5RXEdFDCms");
            //var _friChain = new BlockChain("x");
            //var utxoSet = new UTXOSet(_friChain);
            //for (int i = 0; i <1; i++)
            //{
                
            //    var tx = Transaction.NewTransaction("112H2TcYAvxWGPSWXz4bzGvm5RXEdFDCms", "1fp9JwtnMMnYVLaABMEQuKGtpXUnJm7Cz", 5, utxoSet);
            //    var tx2 = Transaction.NewTransaction("1fp9JwtnMMnYVLaABMEQuKGtpXUnJm7Cz", "1LKeGb2LNZwzTBGkuwW4PaP6EuvWhb3vuM", 5, utxoSet);
            //    var tx3 = Transaction.NewTransaction("1LEB1mKYDEMfpNZtSkwqNDQMGAr3mBscsk", "112H2TcYAvxWGPSWXz4bzGvm5RXEdFDCms", 5, utxoSet);
            //    var tx4 = Transaction.NewTransaction("1LKeGb2LNZwzTBGkuwW4PaP6EuvWhb3vuM", "112H2TcYAvxWGPSWXz4bzGvm5RXEdFDCms", 5, utxoSet);
            //    if (tx == null) return;
            //    if (tx2 == null) return;
            //    if (tx3 == null) return;
            //    if (tx4 == null) return;
            //    list.Add(tx);
            //    list.Add(tx2);
            //    list.Add(tx3);
            //    list.Add(tx4);
            //    var block = _friChain.MineBlock(list);
            //    utxoSet.Update(block);
            //    list = new List<Transaction>();

            //}


            //_friChain.GetBalance("112H2TcYAvxWGPSWXz4bzGvm5RXEdFDCms");
            //_friChain.GetBalance("1fp9JwtnMMnYVLaABMEQuKGtpXUnJm7Cz");
            //_friChain.GetBalance("1LEB1mKYDEMfpNZtSkwqNDQMGAr3mBscsk");
            //_friChain.GetBalance("1LKeGb2LNZwzTBGkuwW4PaP6EuvWhb3vuM");

            //_friChain.PrintWholeBlockChain();



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