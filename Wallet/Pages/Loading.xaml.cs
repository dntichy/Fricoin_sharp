using ChainUtils;
using CoreLib;
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