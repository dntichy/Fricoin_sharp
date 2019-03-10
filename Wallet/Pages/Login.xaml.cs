using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ChainUtils;
using CoreLib;
using DatabaseLib;
using Microsoft.Win32;

namespace Wallet.Pages
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {

        private readonly WalletBank _walletBank;
        public Login(User newUser)
        {
            InitializeComponent();
             _walletBank = new WalletBank();
            AddressComboBox.ItemsSource = _walletBank.GetAddresses();

            if (newUser != null)
            {
                AddressComboBox.Text = newUser.Address;
            }
        }


        private void LoginButtonClicked(object sender, RoutedEventArgs e)
        {
            string address = AddressComboBox.Text; ;
            string hashPw = Sha.GenerateSha256String(PasswordBox1.Password);

            //check Private key and public Key
            var walletBank = new WalletBank();
            var wallet = walletBank.FindWallet(address);

            if (wallet == null)
            {
                Errormessage.Text = "First import your wallet";
                return;
            }

            if (!wallet.IsValid())
            {
                Errormessage.Text = "Wallet isn't valid";
                return;
            }

            //check inside DB
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

        private void ImportWalletCoreClick(object sender, RoutedEventArgs e)
        {
            var result =new  OpenFileDialog();
            result.ShowDialog();

            if (result.FileName != "")
            {
                var bytes = File.ReadAllBytes(result.FileName);
                try
                {
                    var wallet = WalletCore.DeSerialize(bytes);
                    var isAdded = _walletBank.AddWallet(wallet);

                    if (isAdded)
                    {
                        Console.WriteLine("Wallet imported sucessfully");
                        AddressComboBox.Text = wallet.Address;
                    }
                    else
                    {
                        Console.WriteLine("Wallet import not sucessfull, already there");
                        AddressComboBox.Text = wallet.Address;
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception+" Bad file :(");
                    
                }
            }
        }
    }
} 