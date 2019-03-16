using P2PLib.Network;
using P2PLib.Network.Components;
using P2PLib.Network.Components.Enums;
using P2PLib.Network.Components.Interfaces;
using P2PLib.Network.MessageParser;
using P2PLib.Network.MessageParser.Messages;
using System;
using System.Configuration;

namespace Wallet
{
    class LayerBlockchainNetwork
    {
        private static ApplicationInputState _mCurrentState;
        public  BlockchainNetwork _blockchainNetwork;

        public enum ApplicationInputState
        {
            EmptyInput,
            TextInput,
            ImageInput,
            StrokeInput
        }

        public LayerBlockchainNetwork()
        {
            //P2P CONFIGURATIONS
            int listenPort = Convert.ToInt32(ConfigurationManager.AppSettings["ListenPort"]);
            String server = ConfigurationManager.AppSettings["Server"];
            int serverListenPort = Convert.ToInt32(ConfigurationManager.AppSettings["ServerListenPort"]);

            //CREATE BLOCKCHAIN OBJECT
            _blockchainNetwork = new BlockchainNetwork(listenPort, serverListenPort, server, "group11");

            //REGISTER recievers
            _blockchainNetwork.OnReceiveMessage += new OnReceiveMessageEvent(OnReceivePeerMessage);
            
            _mCurrentState = ApplicationInputState.EmptyInput;
        }


        //MAIN PROCESSING MESSAGE LOGIC
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
        //METHODS, SENDING MESSAGES
        public void CheckForUpdates()
        {   
            var message = new CommandMessage();
            message.Command = CommandType.Chain;
            message.Client = _blockchainNetwork.ClientDetails();
            if (_blockchainNetwork.GroupClients.Count != 0) _blockchainNetwork.SendMessageAsync(message, _blockchainNetwork.GroupClients[0]);
            
        }
        public void SendTransactionPool()
        {
            var message = new CommandMessage();
            message.Command = CommandType.ClearTransactionPool;
            message.Client = _blockchainNetwork.ClientDetails();
            _blockchainNetwork.BroadcastMessageAsync(message);
        }
        public void PublishMinedBlock()
        {
            var message = new CommandMessage();
            message.Command = CommandType.Block;
            message.Client = _blockchainNetwork.ClientDetails();
            _blockchainNetwork.BroadcastMessageAsync(message);
        }

        private static void OnReceivePeerMessage(object sender, ReceiveMessageEventArgs e)
        {
            ProcessMessage(e.Message);
        }
    }
}