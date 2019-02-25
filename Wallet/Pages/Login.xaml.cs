using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ChainUtils;
using DatabaseLib;

namespace Wallet.Pages
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();

            var pubKey = File.ReadAllBytes("pub.dat");
            PublicKeyBox.Content = Convert.ToBase64String(pubKey);
        }


        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string pk = PublicKeyBox.Content as string;
            string hashPw = Sha.GenerateSha256String(PasswordBox1.Password);

            var context = new UserContext();
            var user = context.Users.FirstOrDefault(n => n.PublicKey == pk && n.Password == hashPw);

            if (user != null)
            {
                NavigationService?.Navigate(new Wallet());
            }
            else
            {
                Errormessage.Text = "Passwords doesnt match or internet connection lost";
            }
        }

        private void buttonRegister_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}