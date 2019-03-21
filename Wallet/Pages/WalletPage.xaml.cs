using ChainUtils;
using CoreLib;
using CoreLib.Blockchain;
using DatabaseLib;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MS.WindowsAPICodePack.Internal;
using NLog;
using P2PLib.Network.Components.Interfaces;
using P2PLib.Network.MessageParser;
using P2PLib.Network.MessageParser.Messages;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Wallet.ShellHelpers;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

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
        private LayerBlockchainNetwork nettwork;
        private const string APP_ID = "Fricoin.Wallet";

        //-------------
        bool debug = false; //TODO remove, or set false
        ConsoleWindow DebugWindow;
        //------------


        public WalletPage(User user)
        {
            InitializeComponent();
            InitializeLogger();
            TryCreateShortcut(); // create shortcut, so i will be able to show toasts


            //INITIALIZE CHAIN, ETC
            _friChain = new BlockChain(LayerBlockchainNetwork.GetIpAddress());
            _loggedUser = user;
            var bank = new WalletBank();
            _loggedUserWallet = bank.FindWallet(user.Address);

            //INITIALIZE CONSOLE OUTPUT IF DEBUG
            if (debug)
            {
                DebugWindow = new ConsoleWindow();
                DebugWindow.Loaded += NetworkInitializeDelegate;
                DebugWindow.Show();
                DebugWindow.Title = user.Address;

            }

            //INITIALIZE NETTWORK if debug is false

            var thread = new Thread(new ThreadStart(InitializeNettwork));
            thread.Start();
            //if (!debug) InitializeNettwork();

            //CHAIN GAMES

            _friChain.PrintWholeBlockChain();
            InitializeListBox();

            //REGISTER CLOSING EVENET
            Application.Current.MainWindow.Closing += new CancelEventHandler(AppClosing);

            //SET GUI PROPERTIES 
            CreateQrCode(user.Address);
            Address.Content = user.Address;
            Email.Content = user.Email;
            var firstName = Regex.Replace(user.FirstName, @"\s+", "");
            var lastName = Regex.Replace(user.LastName, @"\s+", "");

            FullName.Content = firstName + " " + lastName;
            Balance.Content = _friChain.GetBalance(user.Address);

            //DISPLAY LIST OF USERS
            var context = new UserContext();
            var listUsers = context.Users.ToList();
            usersListBox.ItemsSource = listUsers;

        }

        private void InitializeLogger()
        {
            var logFile = LayerBlockchainNetwork.GetIpAddress().Replace(':','_') + ".txt";

            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = logFile };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
        }

        private void InitializeListBox()
        {
            //todo register this on Item added....
            var collectionOfHeaders = new Collection<BlockHeader>();
            foreach (var block in _friChain)
            {
                var bHash = Convert.ToBase64String(block.Hash);
                var MerkleRoot = Convert.ToBase64String(block.MerkleRoot);
                string PreviousHash = "-1";
                if (block.PreviousHash != null)
                {
                    PreviousHash = Convert.ToBase64String(block.PreviousHash);
                }

                var bH = new BlockHeader()
                {
                    Hash = bHash,
                    Index = block.Index,
                    MerkleRoot = MerkleRoot,
                    Nonce = block.Nonce,
                    PreviousHash = PreviousHash,
                    TimeStamp = block.TimeStamp
                };
                collectionOfHeaders.Add(bH);
            }
            SetListBlocksHeader(collectionOfHeaders);

        }

        private void NetworkInitializeDelegate(object sender, EventArgs e)
        {
            InitializeNettwork();
        }

        private void InitializeNettwork()
        {

            try
            {

                //NETTWORK STUFF
                nettwork = new LayerBlockchainNetwork(_friChain);
                nettwork._blockchainNetwork.OnRegisterClient += NewClientRegistered;
                nettwork._blockchainNetwork.OnUnRegisterClient += ClientUnregistered;
                nettwork._blockchainNetwork.OnRecieveListOfClients += PeersListObtained;
                nettwork.NewBlockAdded += NewBlockAdded;
                //kick of blockchain game here, must first register events, than kick that off
                nettwork._blockchainNetwork.Initialize();
                nettwork.Send(_loggedUser.Address, "1EkAmczL7REZVgTHfBC8Rk3fMLiVQnR3bi", 20, true);

                //set Console window title
                if (debug)
                {
                    DebugWindow.Title = _loggedUser.Address + " IP: " + nettwork._blockchainNetwork.ClientDetails().ToString();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void NewBlockAdded(object sender, EventArgs e)
        {
            InitializeListBox();
        }

        private void PeersListObtained(object sender, ReceiveListOfClientsEventArgs e)
        {
            Console.WriteLine("PEER List obtained");
            SetListOfPeers(e.ListOfClients);
        }

        private void ClientUnregistered(object sender, ServerRegisterEventArgs e)
        {
            Console.WriteLine("PEER UNREGISTERED");
            SetListOfPeers(nettwork._blockchainNetwork.GroupClients);//set list of peers
        }

        private void NewClientRegistered(object sender, ServerRegisterEventArgs e)
        {
            Console.WriteLine("PEER REGISTERED");
            SetListOfPeers(nettwork._blockchainNetwork.GroupClients); //set list of peers
        }

        private void SetListOfPeers(Collection<IClientDetails> groupClients)
        {
            Console.WriteLine("SET PEER list");
            Dispatcher.Invoke(() =>
            {
                PeersListBox.ItemsSource = groupClients;
                PeersListBox.Items.Refresh();
            });

        }

        private void SetListBlocksHeader(Collection<BlockHeader> blocks)
        {
            Dispatcher.Invoke(() =>
            {
                BlockHeaderListBox.ItemsSource = blocks;
                BlockHeaderListBox.Items.Refresh();
            });

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

        private void ExportWalletCoreClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog file = new SaveFileDialog();
            file.ShowDialog();

            if (file.FileName != "")
            {
                File.WriteAllBytes(file.FileName, _loggedUserWallet.Serialize());
            }

        }

        private void SendClick(object sender, RoutedEventArgs e)
        {

            //int.TryParse(AmountTextBox.Text, out int amount);
            //if (amount == 0)
            //{
            //    Console.WriteLine("Specify correct amount");
            //    return;
            //}
            //if (ToAddressTextBox.Text.Equals(nettwork._blockchainNetwork.ClientDetails().ToString()))
            //{
            //    Console.WriteLine("Can't send to yourself");
            //    return;
            //}
            //nettwork.Send(_loggedUser.Address, ToAddressTextBox.Text, amount, true);


            var message = new TextMessage()
            {
                Client = nettwork._blockchainNetwork.ClientDetails(),
                Text = "nazdaaaro"
            };
            var CmDmessage = new CommandMessage()
            {
                Data = ByteHelper.GetBytesFromString("Test message"),
                Client = nettwork._blockchainNetwork.ClientDetails(),
                Command = CommandType.Block
            };

            nettwork._blockchainNetwork.BroadcastMessage(message);
        }
        void AppClosing(object sender, CancelEventArgs e)
        {

            if (debug)
            {
                DebugWindow?.Close();
            }

            nettwork._blockchainNetwork.Close();
            e.Cancel = true;
            Application.Current.Shutdown();


        }

        private void LogoutClick(object sender, RoutedEventArgs e)
        {
            nettwork._blockchainNetwork.Close();
            NavigationService?.Navigate(new Loading());
        }

        private void BlockHeaderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var txList = new List<TransactionHeader>();
            var blockHeader = BlockHeaderListBox.SelectedItem as BlockHeader;

            var blockBytes = _friChain.ChainDb.Get(Convert.FromBase64String(blockHeader.Hash));
            var block = new Block().DeSerialize(blockBytes);

            foreach (var tx in block.Transactions)
            {
                txList.Add(new TransactionHeader()
                {
                    Id = Convert.ToBase64String(tx.Id)
                });
            }

            Dispatcher.Invoke(() =>
            {
                TxListBox.ItemsSource = txList;
                TxListBox.Items.Refresh();
            });

        }
        private void Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var user = usersListBox.SelectedItem as User;
            //copy to clipboard users address
            Clipboard.SetText(user.Address);

            //show notification about copied
            ShowToastMessageCopiedToCB();

        }

        private void ShowToastMessageCopiedToCB()
        {
            // Get a toast XML template
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText01);
            var audio = toastXml.CreateElement("audio");
            //audio.SetAttribute("src", "ms-winsoundevent:Notification.Alarm7");
            //audio.SetAttribute("loop", "false");
            audio.SetAttribute("silent", "true");

            // Add the audio element
            toastXml.DocumentElement.AppendChild(audio);


            //TEXT
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode("Copied to clipboard"));

            // PICTURE
            var imagePath = "file:///" + Path.GetFullPath(@"Pictures\copy.png");
            XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
            imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

            // Create the toast and attach event listeners
            ToastNotification toast = new ToastNotification(toastXml);

            //toast.Activated += ToastActivated;
            //toast.Dismissed += ToastDismissed;
            //toast.Failed += ToastFailed;


            // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!

            ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);

            //HIDE TOAST after 2500
            Task.Delay(2500).ContinueWith(t =>
            {
                ToastNotificationManager.CreateToastNotifier(APP_ID).Hide(toast);
            });

        }
        private bool TryCreateShortcut()
        {
            String shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\Fricoin.lnk";
            if (!File.Exists(shortcutPath))
            {
                InstallShortcut(shortcutPath);
                return true;
            }
            return false;
        }

        private void InstallShortcut(String shortcutPath)
        {
            // Find the path to the current executable
            String exePath = Process.GetCurrentProcess().MainModule.FileName;
            IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

            // Create a shortcut to the exe
            ShellHelpers.ErrorHelper.VerifySucceeded(newShortcut.SetPath(exePath));
            ShellHelpers.ErrorHelper.VerifySucceeded(newShortcut.SetArguments(""));

            // Open the shortcut property store, set the AppUserModelId property
            IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

            using (PropVariant appId = new PropVariant(APP_ID))
            {
                ShellHelpers.ErrorHelper.VerifySucceeded(newShortcutProperties.SetValue(SystemProperties.System.AppUserModel.ID, appId));
                ShellHelpers.ErrorHelper.VerifySucceeded(newShortcutProperties.Commit());
            }

            // Commit the shortcut to disk
            IPersistFile newShortcutSave = (IPersistFile)newShortcut;

            ShellHelpers.ErrorHelper.VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
        }

        private void QrCodeAddressBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetText(_loggedUser.Address);
            ShowToastMessageCopiedToCB();
        }





    }
}