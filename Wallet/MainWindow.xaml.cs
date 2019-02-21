using ChainUtils;
using Engine.Network.Components.Interfaces;
using Engine.Network.MessageParser;
using P2PLib.Network.Components.Enums;
using P2PLib.Network.MessageParser.Messages;
using QRCoder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Org.BouncyCastle.Utilities;

namespace Wallet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static ApplicationInputState _mCurrentState;
        private readonly CollaborativeNotesClass _mCollaborativeNotes;

        public enum ApplicationInputState
        {
            EmptyInput,
            TextInput,
            ImageInput,
            StrokeInput
        }

        public MainWindow()
        {
            InitializeComponent();

            //int listenPort = Convert.ToInt32(ConfigurationManager.AppSettings["ListenPort"]);
            //String server = ConfigurationManager.AppSettings["Server"];
            //int serverListenPort = Convert.ToInt32(ConfigurationManager.AppSettings["ServerListenPort"]);

            //_mCollaborativeNotes = new CollaborativeNotesClass(listenPort, serverListenPort, server, "group11");
            //_mCollaborativeNotes.OnReceiveMessage += new OnReceiveMessageDelegate(OnReceivePeerMessage);
            //_mCurrentState = ApplicationInputState.EmptyInput;

            //Console.Read();


            var privK = Crypto.GeneratePrivateKey();
            var pubK = Crypto.GetPublicKey(privK);

            Console.WriteLine("Private: " + privK);
            Console.WriteLine("Public: " + pubK);

            var str = "ahoj";
            byte[] bytes = Encoding.ASCII.GetBytes(str);

            var hash = Crypto.SignTransaction(bytes, privK);
            Console.WriteLine("Signature: " + hash);


            var list = new List<byte[]>();
            var arr = hash.ToArray();

            foreach (var sigVal in arr)
            {
                list.Add(sigVal);
            }


            var recoveredKey = Crypto.RecoverPublicKey(list[0], list[1], list[2], bytes);
            if (StructuralComparisons.StructuralEqualityComparer.Equals(recoveredKey, pubK))
            {
                Console.WriteLine("GOOD JOB");
            }


            CreateQrCode("");


            //other
            //Console.WriteLine("Public: " + serializedPublic2);
            //Console.WriteLine("Private: " + serializedPrivate2);
            //var signature = Crypto.SignData("Traktor", priK);
            //Console.WriteLine("Signature: " + signature);

            //var verificationOk = Crypto.VerifySignature(pubK, signature, "Traktr");

            Console.WriteLine("Is verified? ");
        }

        private void CreateQrCode(string serializedPublic2)
        {
            string payload = serializedPublic2;

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var qrCodeAsBitmap = qrCode.GetGraphic(20);

            //convert bitmap to datasource
            QrCodeAddress.Source = ConvertBitmapToBitmapImage(qrCodeAsBitmap);
        }

        private static void UpdateCurrentView(IMessage msg)
        {
            switch (msg.Type)
            {
                case ((int) MessageType.TextDataMessage):
                {
                    TextMessage rxMsg = (TextMessage) msg;
                    break;
                }
            }
        }

        private static void OnReceivePeerMessage(object sender, CollaborativeNotesReceiveMessageEventArgs e)
        {
            UpdateCurrentView(e.Message);
        }

        private void SendMessage(object sender, RoutedEventArgs e)
        {
            TextMessage message = new TextMessage()
            {
                Text = "Hello"
            };
            _mCollaborativeNotes.BroadcastMessage(message);
        }


        public BitmapImage ConvertBitmapToBitmapImage(Bitmap src)
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
    }
}