using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ChainUtils;

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
            if (File.Exists("pub.dat") && File.Exists("pri.dat"))
            {
                var pub = File.ReadAllBytes("pub.dat");
                var pri = File.ReadAllBytes("pri.dat");

                var hash = Crypto.SignTransaction(Encoding.UTF8.GetBytes("test"), pri);
                var isOk = Crypto.VerifyHashed(hash, pub, Encoding.UTF8.GetBytes("test"));
                if (isOk)
                {
                    Console.WriteLine("Registered");
                    // go to Login 
                    NavigationService?.Navigate(new Login());
                }
            }
            else
            {
                Console.WriteLine("Not registered");
                // continue presenting register form
                NavigationService?.Navigate(new Registration());
            }
        }
    }
}