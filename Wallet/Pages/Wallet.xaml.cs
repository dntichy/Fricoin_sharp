﻿using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CoreLib;
using DatabaseLib;
using Engine.Network.MessageParser;
using Newtonsoft.Json;
using P2PLib.Network.Components.Enums;
using P2PLib.Network.Components.Interfaces;
using P2PLib.Network.MessageParser;
using P2PLib.Network.MessageParser.Messages;
using QRCoder;

namespace Wallet.Pages
{
    /// <summary>
    /// Interaction logic for Wallet.xaml
    /// </summary>
    public partial class Wallet : Page
    {
        private static ApplicationInputState _mCurrentState;
        private readonly BlockchainNetwork _blockchainNetwork;

        public enum ApplicationInputState
        {
            EmptyInput,
            TextInput,
            ImageInput,
            StrokeInput
        }

        public Wallet(User user)
        {
            InitializeComponent();
            CreateQrCode(user.PublicKey);
            EmailValueBox.Content = user.Email;
            FullNameValueBox.Content = user.FirstName + user.LastName;


            //P2P stuff
            int listenPort = Convert.ToInt32(ConfigurationManager.AppSettings["ListenPort"]);
            String server = ConfigurationManager.AppSettings["Server"];
            int serverListenPort = Convert.ToInt32(ConfigurationManager.AppSettings["ServerListenPort"]);
            _blockchainNetwork = new BlockchainNetwork(listenPort, serverListenPort, server, "group11");
            _blockchainNetwork.OnReceiveMessage += new OnReceiveMessageDelegate(OnReceivePeerMessage);
            _mCurrentState = ApplicationInputState.EmptyInput;


            //reconstruct blockchain from b+ tree 
            //if not exists, inicialize blockchain
            //send request for current blockchain
            //if there are changes, obtain new blockchain

            BlockChain friCoin = new BlockChain();
            //Console.WriteLine(JsonConvert.SerializeObject(friCoin, Newtonsoft.Json.Formatting.Indented));
            
            
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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(_blockchainNetwork.GroupClients.Count);
        }
    }
}