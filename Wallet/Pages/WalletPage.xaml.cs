using CoreLib;
using CoreLib.Blockchain;
using DatabaseLib;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MS.WindowsAPICodePack.Internal;
using NLog;
using P2PLib.Network.Components.Interfaces;
using P2PLib.Network.MessageParser;
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
        public BlockChain _friChain { get; set; }
        private User _loggedUser;
        private WalletCore _loggedUserWallet;
        private LayerBlockchainNetwork nettwork;
        private const string APP_ID = "Fricoin.Wallet";
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public WalletPage(User user)
        {
            InitializeComponent();
            InitializeLogger();
            TryCreateShortcut(); // create shortcut, so i will be able to show toasts

            //INITIALIZE CHAIN, ETC
            _loggedUser = user;
            var bank = new WalletBank();
            _loggedUserWallet = bank.FindWallet(user.Address);
            _friChain = new BlockChain(LayerBlockchainNetwork.GetIpAddress());

            //REGISTER CLOSING EVENET
            Application.Current.MainWindow.Closing += new CancelEventHandler(AppClosing);

            //SET GUI PROPERTIES 
            InitializeProfile();
            UpdateBalance();
            UpdateRawChain();
            UpdatePeerList();
        }



        private void NewBlockArrived(object sender, ProgressBarEventArgs e)
        {
            double fraction = 1.0 * (e.HighestIndex - e.CountCurrentBlocksInTranzit - e.ReducedBlocksCount) / (e.HighestIndex - e.ReducedBlocksCount);

            Dispatcher.Invoke(() =>
            {
                progBgLabel.Width = progBorder.Width * fraction;

                if (fraction != 1) progLabel.Content = "downloading... " + Math.Ceiling(100 * fraction) + "%";

                else progLabel.Content = "synchronized";
            });
        }


        private void BlockChainSynchronizing(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                progBgLabel.Width = 0;
                progLabel.Content = "downloading...";
            });
        }

        private void BlockChainSynchronized(object sender ,  EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                progBgLabel.Width = progBorder.Width;
                progLabel.Content = "synchronized";
            });
        }



        private void InitializeLogger()
        {
            var logFile = LayerBlockchainNetwork.GetIpAddress().Replace(':', '_') + ".txt";

            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = logFile };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
        }

        private void InitializeListBox()
        {
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

        private void InitializeNettwork()
        {

            try
            {
                //NETTWORK STUFF
                nettwork = new LayerBlockchainNetwork(_friChain, _loggedUser);
                nettwork._blockchainNetwork.OnRegisterClient += NewClientRegistered;
                nettwork._blockchainNetwork.OnUnRegisterClient += ClientUnregistered;
                nettwork._blockchainNetwork.OnRecieveListOfClients += PeersListObtained;
                nettwork.WholeChainDownloaded += WholeChainDownloaded;
                nettwork.NewBlockArrived += NewBlockArrived;
                nettwork.TransactionPoolChanged += TransactionPoolChanged;
                nettwork.BlockChainSynchronized += BlockChainSynchronized;
                nettwork.BlockChainSynchronizing += BlockChainSynchronizing;

                //kick of blockchain game here, must first register events, than kick that off
                nettwork._blockchainNetwork.Initialize();
                //for (int i = 0; i < 5; i++)
                //{
                //    nettwork.Send(_loggedUser.Address, "1EkAmczL7REZVgTHfBC8Rk3fMLiVQnR3bi", 2);
                //}


                //CHAIN GAMES
                InitializeListBox();

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }




        private void MinedHashUpdate(object sender, MinedHashUpdateEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                HashDisplayLabel.Content = e.Hash;
            });
        }

        private void TransactionPoolChanged(object sender, TransactionPoolEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TransactionPoolListBox.ItemsSource = e.TransactionPoolList;
                TransactionPoolListBox.Items.Refresh();
            });
        }

        private void WholeChainDownloaded(object sender, EventArgs e)
        {
            InitializeListBox();
            UpdateBalance();
            UpdateRawChain();
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

            int.TryParse(AmountTextBox.Text, out int amount);
            if (amount == 0)
            {
                Console.WriteLine("Specify correct amount");
                return;
            }
            if (ToAddressTextBox.Text.Equals(nettwork._blockchainNetwork.ClientDetails().ToString()))
            {
                Console.WriteLine("Can't send to yourself");
                return;
            }
            nettwork.Send(_loggedUser.Address, ToAddressTextBox.Text, amount);
        }
        void AppClosing(object sender, CancelEventArgs e)
        {

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
            if (blockHeader == null) return;

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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var thread = new Thread(new ThreadStart(InitializeNettwork));
            thread.Start();
        }

        private void SliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var speed = (int)MiningSpeedSlider.Value;
            if (_friChain.ActuallBlockInMining != null)
            {
                _friChain.ActuallBlockInMining.Speed = speed;
            }

        }

        private void DisplayMiningCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MiningSpeedSlider.IsEnabled = true;
            nettwork.MinedHashUpdate += MinedHashUpdate;
        }

        private void DisplayMiningCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            MiningSpeedSlider.IsEnabled = false;
            nettwork.MinedHashUpdate -= MinedHashUpdate;
            _friChain.ActuallBlockInMining.Speed = 0;
            HashDisplayLabel.Content = "";

        }





        //refers to p2p obtaining peers events
        private void PeersListObtained(object sender, ReceiveListOfClientsEventArgs e)
        {
            logger.Debug("PEER List obtained");
            SetListOfPeers(e.ListOfClients);
        }

        private void ClientUnregistered(object sender, ServerRegisterEventArgs e)
        {
            logger.Debug("PEER UNREGISTERED");
            SetListOfPeers(nettwork._blockchainNetwork.GroupClients);//set list of peers
        }

        private void NewClientRegistered(object sender, ServerRegisterEventArgs e)
        {
            logger.Debug("PEER REGISTERED");
            SetListOfPeers(nettwork._blockchainNetwork.GroupClients); //set list of peers
        }

        private void SetListOfPeers(Collection<IClientDetails> groupClients)
        {
            logger.Debug("SET PEER list");
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



        public void InitializeProfile()
        {
            CreateQrCode(_loggedUser.Address);
            Address.Content = _loggedUser.Address;
            var firstName = Regex.Replace(_loggedUser.FirstName, @"\s+", "");
            var lastName = Regex.Replace(_loggedUser.LastName, @"\s+", "");
            progLabel.Content = "unknown";  // set progressbar values 
            progBgLabel.Width = 0;  // set progressbar values 
            FullName.Content = "Full name : " + firstName + " " + lastName;
            Email.Content = "Email :     " + _loggedUser.Email;
        }
        public void UpdateBalance()
        {
            logger.Debug("Balande updated");
            Dispatcher.Invoke(() =>
            {
                Balance.Content = "Balance :   " + _friChain.GetBalance(_loggedUser.Address);
            });

        }
        public void UpdateRawChain()
        {
            logger.Debug("RawChain updated");
            Dispatcher.Invoke(() =>
            {
                var rawchain = _friChain.PrintWholeBlockChain();
                RawChainTextBlock.Text = rawchain;
                Console.WriteLine(rawchain);
            });

        }
        public void UpdatePeerList()
        {
            var context = new UserContext();
            var listUsers = context.Users.ToList();
            usersListBox.ItemsSource = listUsers;
        }

    }
}