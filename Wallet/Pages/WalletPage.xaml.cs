using CoreLib.Blockchain;
using DatabaseLib;
using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Wallet.Pages
{
    /// <summary>
    /// Interaction logic for WalletPage.xaml
    /// </summary>
    public partial class WalletPage : Page
    {
        public WalletPage(User user)
        {
            InitializeComponent();
            BlockChain friCoin = new BlockChain();
            friCoin.Send("1Gd8WnpnfH4oaCjva6JfgGRJRRQ271KpHC", "19p2is8biiWDEBhbfQb4yQRv1zwKX1CR17", 50);
            CreateQrCode(user.Address);
            Address.Content =  user.Address;
            Email.Content ="Email: "+ user.Email;
           FullName.Content = "Name: "+user.FirstName + user.LastName;
           Balance.Content = "Balance: "+friCoin.GetBalance(user.Address);


            //var layer = new LayerBlockchainNetwork();
            //reconstruct blockchain from b+ tree 
            //if not exists, inicialize blockchain
            //send request for current blockchain
            //if there are changes, obtain new blockchain




            Console.Read();
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
    }
}