using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ChainUtils;
using CoreLib;
using DatabaseLib;

namespace Wallet.Pages
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public Login(User newUser)
        {
            InitializeComponent();
            var walletBank = new WalletBank();
            AddressComboBox.ItemsSource = walletBank.GetAddresses();
            if (newUser != null)
            {
                AddressComboBox.Text = newUser.Address;
            }
        }


        private void LoginButtonClicked(object sender, RoutedEventArgs e)
        {
            string address = AddressComboBox.Text; ;
            string hashPw = Sha.GenerateSha256String(PasswordBox1.Password);

            var context = new UserContext();
            var user = context.Users.FirstOrDefault(n => n.Address == address && n.Password == hashPw);

            if (user != null)
            {
                NavigationService?.Navigate(new WalletPage(user));
            }
            else
            {
                Errormessage.Text = "Passwords doesn't match or connection is lost";
            }
        }

    }
}