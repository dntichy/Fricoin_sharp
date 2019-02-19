using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Engine;
using Engine.Network;
using Engine.Network.Components.Interfaces;
using Engine.Network.MessageParser;
using Engine.Network.MessageParser.Messages;
using P2PLib.Network.MessageParser;
using P2PLib.Network.MessageParser.Messages;

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

            int listenPort = Convert.ToInt32(ConfigurationManager.AppSettings["ListenPort"]);
            String server = ConfigurationManager.AppSettings["Server"];
            int serverListenPort = Convert.ToInt32(ConfigurationManager.AppSettings["ServerListenPort"]);

            _mCollaborativeNotes = new CollaborativeNotesClass(listenPort, serverListenPort, server, "group11");
            _mCollaborativeNotes.OnReceiveMessage += new OnReceiveMessageDelegate(OnReceivePeerMessage);
            _mCurrentState = ApplicationInputState.EmptyInput;

            Console.Read();
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
    }
}