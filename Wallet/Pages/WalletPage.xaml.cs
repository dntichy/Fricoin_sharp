using CoreLib.Blockchain;
using DatabaseLib;
using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CoreLib;
using Microsoft.Win32;

namespace Wallet.Pages
{
    /// <summary>
    /// Interaction logic for WalletPage.xaml
    /// </summary>
    public partial class WalletPage : Page
    {
        private readonly BlockChain _friChain;
        private User _loggedUser;
        private WalletCore _loggedUserWallet;

        public WalletPage(User user)
        {
            InitializeComponent();

            //INITIALIZE CHAIN, ETC
            _friChain = new BlockChain();
            _loggedUser = user;
            var bank = new WalletBank();
            _loggedUserWallet = bank.FindWallet(user.Address);

            //_friChain.Send("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", "19p2is8biiWDEBhbfQb4yQRv1zwKX1CR17", 50);
            //_friChain.Send("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", "19p2is8biiWDEBhbfQb4yQRv1zwKX1CR17", 20);
            //_friChain.Send("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", "19p2is8biiWDEBhbfQb4yQRv1zwKX1CR17", 20);
            //_friChain.Send("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", "19p2is8biiWDEBhbfQb4yQRv1zwKX1CR17", 20);

            _friChain.PrintWholeBlockChain();




            //SET GUI PROPERTIES 
            CreateQrCode(user.Address);
            Address.Content =  user.Address;
            Email.Content ="Email: "+ user.Email;
            FullName.Content = "Name: "+user.FirstName + user.LastName;
            Balance.Content = "Balance: "+_friChain.GetBalance(user.Address);

            //DISPLAY LIST OF USERS
            var context = new UserContext();
            var listUsers = context.Users.ToList();
            listBox1.ItemsSource = listUsers;

        }

        private void CreateQrCode(string serializedPublic2)
        {
            string payload = serializedPublic2;

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var qrCodeAsBitmap = qrCode.GetGraphic(20);

            //convert bitmap to datasource
            QrCodeAddressBox.Source = ConvertBitmapToBitmapImage(qrCodeAsBitmap);
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        private void Send(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ExportWalletCoreClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog file = new SaveFileDialog();
            file.ShowDialog();

            if (file.FileName != "")
            {
                File.WriteAllBytes(file.FileName, _loggedUserWallet.Serialize());
            }

        }
    }
}