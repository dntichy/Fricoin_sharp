using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Network.MessageParser;
using P2PLib.Network.Components.Enums;
using P2PLib.Network.Components.Interfaces;
using P2PLib.Network.MessageParser;
using P2PLib.Network.MessageParser.Messages;

namespace Wallet
{
    class LayerBlockchainNetwork
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

        public LayerBlockchainNetwork()
        {
            //P2P stuff
            int listenPort = Convert.ToInt32(ConfigurationManager.AppSettings["ListenPort"]);
            String server = ConfigurationManager.AppSettings["Server"];
            int serverListenPort = Convert.ToInt32(ConfigurationManager.AppSettings["ServerListenPort"]);
            _blockchainNetwork = new BlockchainNetwork(listenPort, serverListenPort, server, "group11");
            _blockchainNetwork.OnReceiveMessage += new OnReceiveMessageDelegate(OnReceivePeerMessage);
            _mCurrentState = ApplicationInputState.EmptyInput;
        }

        private static void ProcessMessage(IMessage msg)
        {
            switch (msg.Type)
            {
                case ((int) MessageType.TextDataMessage):
                {
                    TextMessage rxMsg = (TextMessage) msg;
                    Console.WriteLine(rxMsg.Text);
                    break;
                }
                case ((int) MessageType.CommandMessage):
                {
                    CommandMessage rxMsg = (CommandMessage) msg;

                    switch (rxMsg.Command)
                    {
                        case CommandType.Chain:
                            break;
                        case CommandType.ClearTransactionPool:
                            break;
                        case CommandType.Transaction:
                            break;
                    }

                    break;
                }
            }
        }

        public void CheckForUpdates()
        {   
            var message = new CommandMessage();
            message.Command = CommandType.Chain;
            _blockchainNetwork.SendMessageAsync(message, _blockchainNetwork.GroupClients[0]);
        }
        public void SendTransactionPool()
        {
            var message = new CommandMessage();
            message.Command = CommandType.ClearTransactionPool;
            _blockchainNetwork.BroadcastMessageAsync(message);
        }
        public void PublishMinedBlock()
        {
            var message = new CommandMessage();
            message.Command = CommandType.NewBlock;
            _blockchainNetwork.BroadcastMessageAsync(message);
        }

        private static void OnReceivePeerMessage(object sender, ReceiveMessageEventArgs e)
        {
            //todo add from whom the message has come for reply
            ProcessMessage(e.Message);
        }
    }
}