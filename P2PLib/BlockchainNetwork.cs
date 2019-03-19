using Engine.Network.MessageParser;
using P2PLib.Network.Client;
using P2PLib.Network.Components;
using P2PLib.Network.Components.Interfaces;
using P2PLib.Network.MessageParser;
using P2PLib.Network.MessageParser.Messages;
using P2PLib.Network.Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace P2PLib.Network
{
    public class BlockchainNetwork : IBlockchainNetwork
    {
        private event OnReceiveMessageEvent mOnReceiveMessage;
        public event OnReceiveMessageEvent OnReceiveMessage
        {
            add { mOnReceiveMessage += value; }
            remove { mOnReceiveMessage -= value; }
        }

        private event ServerRegisterEvent mOnRegisterClient;
        public event ServerRegisterEvent OnRegisterClient
        {
            add { mOnRegisterClient += value; }
            remove { mOnRegisterClient -= value; }
        }

        private event ServerUnRegisterEvent mOnUnRegisterClient;
        public event ServerUnRegisterEvent OnUnRegisterClient
        {
            add { mOnUnRegisterClient += value; }
            remove { mOnUnRegisterClient -= value; }
        }

        private event OnRecieveListOfClientsEvent mOnRecieveListOfClients;
        public event OnRecieveListOfClientsEvent OnRecieveListOfClients
        {
            add { mOnRecieveListOfClients += value; }
            remove { mOnRecieveListOfClients -= value; }
        }


        private int mListenPort;
        private int mServerListenPort;
        private String mServer;
        private String mGroup;
        private Collection<IMessage> mInboundMessages;
        private Collection<IClientDetails> mGroupClients;

        private IServer mListener;
        private P2PClient mClient;
        private Dictionary<IClientDetails, IClient> mClientConnections;

        public int ListenPort
        {
            get { return mListenPort; }
            set { mListenPort = value; }
        }

        public String Server
        {
            get { return mServer; }
            set { mServer = value; }
        }

        public ClientDetails ClientDetails()
        {
            return mClient.ClientDetails();
        }
        public int ServerListenPort
        {
            get { return mServerListenPort; }
            set { mServerListenPort = value; }
        }

        public String Group
        {
            get { return mGroup; }
            set { mGroup = value; }
        }

        public Collection<IMessage> InboundMessages
        {
            get { return mInboundMessages; }
        }

        public Collection<IClientDetails> GroupClients
        {
            get
            {
                if (mListener != null)
                    return ((P2PServer) mListener).GroupClientsDetails;
                return null;
            }
        }

        public IMessageParserEngine MessageParser
        {
            get
            {
                if (mListener != null)
                    return mListener.MessageParser;
                return null;
            }
        }

        public void Initialize()
        {
            mListener.Initialize();
            mClient.Initialize();
        }

        public BlockchainNetwork(int listenPort, int serverListenPort, string server, string group)
        {
            mListenPort = listenPort;
            mServerListenPort  = serverListenPort;
            mServer = server;
            mGroup = group;
            mInboundMessages = new Collection<IMessage>();
            mGroupClients = new Collection<IClientDetails>();
            //////////////////////////////////////////////////////////////////////////
            mListener = new P2PServer(listenPort, group);
            //mListener.ListenPort = listenPort;
            //((CollaborativeNotesServer)mListener).Group = group;
            ((P2PServer) mListener).IncomingMessageQueue = mInboundMessages;
            ((P2PServer) mListener).GroupClientsDetails = mGroupClients;
            ((P2PServer) mListener).OnReceiveMessage += new OnReceiveMessageEvent(OnReceivePeerMessage);
            ((P2PServer) mListener).OnRegisterClient += new ServerRegisterEvent(OnRegisterPeer);
            ((P2PServer) mListener).OnUnRegisterClient += new ServerUnRegisterEvent(OnUnRegisterPeer);
            ((P2PServer) mListener).OnRecieveListOfClients += new OnRecieveListOfClientsEvent(OnRecieveListPeers);


            //mListener.Initialize();
            //////////////////////////////////////////////////////////////////////////
            mClient = new P2PClient(listenPort, server, serverListenPort, group);
            //mClient.Initialize();
            //////////////////////////////////////////////////////////////////////////
            mClientConnections = new Dictionary<IClientDetails, IClient>();
        }

        private void OnRecieveListPeers(object sender, ReceiveListOfClientsEventArgs e)
        {
            if (mOnRecieveListOfClients != null) mOnRecieveListOfClients.Invoke(sender, e);
        }

        private void OnReceivePeerMessage(object sender, ReceiveMessageEventArgs e)
        {
            if (mOnReceiveMessage != null) mOnReceiveMessage.Invoke(sender, e);
        }

        private void OnRegisterPeer(object sender, ServerRegisterEventArgs e)
        {
            if (mOnRegisterClient != null) mOnRegisterClient.Invoke(sender, e);
        }
        private void OnUnRegisterPeer(object sender, ServerRegisterEventArgs e)
        {
            if (mOnUnRegisterClient != null) mOnUnRegisterClient.Invoke(sender, e);
        }


        public void Close()
        {
            UnregisterMessage msg = new UnregisterMessage();
            msg.Group = this.Group;
            msg.Client = new ClientDetails();
            msg.Client.ClientIPAddress = ((P2PServer) mListener).LocalIPAddress;
            msg.Client.ClientListenPort = mListenPort;
            mClient.SendMessageAsync(msg);
            BroadcastMessageAsync(msg);
            System.Threading.Thread.Sleep(100); //Sleep required due to the use of asynchronous methods
        }

        public void SendMessage(IMessage message, IClientDetails details)
        {
            if (message == null)
            {
                //throw a null reference exception
                throw new NullReferenceException("The supplied IMessage object is null!");
            }

            if (details == null)
            {
                //throw a null reference exception
                throw new NullReferenceException("The supplied IClientDetails object is null!");
            }

            IClient currClient = null;
            if (mClientConnections.ContainsKey(details))
            {
                currClient = mClientConnections[details];
            }
            else
            {
                currClient = new P2PClient(ListenPort, details.ClientIPAddress, details.ClientListenPort, Group);
                currClient.Initialize();
            }

            currClient.SendMessage(message);

            if (!mClientConnections.ContainsKey(details))
            {
                mClientConnections[details] = currClient;
            }
        }

        public void SendMessage(IMessage message, String name)
        {
            if (name == null)
            {
                //throw a null reference exception
                throw new NullReferenceException("The supplied name is null!");
            }

            if (name.Length == 0)
            {
                //throw an exception
                throw new Exception("The supplied name is invalid!");
            }

            IClientDetails details = null;

            if (this.GroupClients == null)
            {
                throw new NullReferenceException("The clients list is null!");
            }

            if (this.GroupClients.Count == 0)
            {
                throw new Exception("There are no clients available!");
            }

            for (int i = 0; i < GroupClients.Count; ++i)
            {
                if (GroupClients[i].ClientName == name)
                {
                    details = GroupClients[i];
                    break;
                }
            }

            SendMessage(message, details);
        }

        public void SendMessageToAddress(IMessage message, String address)
        {
            if (address == null)
            {
                //throw a null reference exception
                throw new NullReferenceException("The supplied address is null!");
            }

            if (address.Length == 0)
            {
                //throw an exception
                throw new Exception("The supplied address is invalid!");
            }

            IClientDetails details = null;

            if (GroupClients == null)
            {
                throw new NullReferenceException("The clients list is null!");
            }

            if (GroupClients.Count == 0)
            {
                throw new Exception("There are no clients available!");
            }

            for (int i = 0; i < GroupClients.Count; ++i)
            {
                if (GroupClients[i].ClientIPAddress == address)
                {
                    details = GroupClients[i];
                    break;
                }
            }

            SendMessage(message, details);
        }

        public void BroadcastMessage(IMessage message)
        {
            for (int i = 0; i < mGroupClients.Count; ++i)
            {
                IClientDetails currClientDetails = mGroupClients[i];
                IClient currClient = null;
                if (mClientConnections.ContainsKey(currClientDetails))
                {
                    currClient = mClientConnections[currClientDetails];
                }
                else
                {
                    currClient = new P2PClient(ListenPort, currClientDetails.ClientIPAddress,currClientDetails.ClientListenPort, Group);
                    currClient.Initialize();
                }

                currClient.SendMessage(message);

                if (!mClientConnections.ContainsKey(currClientDetails))
                {
                    mClientConnections[currClientDetails] = currClient;
                }
            }
        }

        public void SendMessageAsync(IMessage message, IClientDetails details)
        {
            if (message == null)
            {
                //throw a null reference exception
                throw new NullReferenceException("The supplied IMessage object is null!");
            }

            if (details == null)
            {
                //throw a null reference exception
                throw new NullReferenceException("The supplied IClientDetails object is null!");
            }

            IClient currClient = null;
            if (mClientConnections.ContainsKey(details))
            {
                currClient = mClientConnections[details];
            }
            else
            {
                currClient = new P2PClient(ListenPort, details.ClientIPAddress, details.ClientListenPort, Group);
                currClient.Initialize();
            }

            currClient.SendMessageAsync(message);

            if (!mClientConnections.ContainsKey(details))
            {
                mClientConnections[details] = currClient;
            }
        }

        public void SendMessageAsync(IMessage message, String name)
        {
            if (name == null)
            {
                //throw a null reference exception
                throw new NullReferenceException("The supplied name is null!");
            }

            if (name.Length == 0)
            {
                //throw an exception
                throw new Exception("The supplied name is invalid!");
            }

            IClientDetails details = null;

            if (this.GroupClients == null)
            {
                throw new NullReferenceException("The clients list is null!");
            }

            if (this.GroupClients.Count == 0)
            {
                throw new Exception("There are no clients available!");
            }

            for (int i = 0; i < GroupClients.Count; ++i)
            {
                if (GroupClients[i].ClientName == name)
                {
                    details = GroupClients[i];
                    break;
                }
            }

            SendMessageAsync(message, details);
        }

        public void SendMessageToAddressAsync(IMessage message, String address)
        {
            if (address == null)
            {
                //throw a null reference exception
                throw new NullReferenceException("The supplied address is null!");
            }

            if (address.Length == 0)
            {
                //throw an exception
                throw new Exception("The supplied address is invalid!");
            }

            IClientDetails details = null;

            if (this.GroupClients == null)
            {
                throw new NullReferenceException("The clients list is null!");
            }

            if (this.GroupClients.Count == 0)
            {
                throw new Exception("There are no clients available!");
            }

            for (int i = 0; i < GroupClients.Count; ++i)
            {
                if (GroupClients[i].ClientIPAddress == address)
                {
                    details = GroupClients[i];
                    break;
                }
            }

            SendMessageAsync(message, details);
        }

        public void BroadcastMessageAsync(IMessage message)
        {
            for (int i = 0; i < mGroupClients.Count; ++i)
            {
                IClientDetails currClientDetails = mGroupClients[i];
                IClient currClient = null;
                if (mClientConnections.ContainsKey(currClientDetails))
                {
                    currClient = mClientConnections[currClientDetails];
                }
                else
                {
                    currClient = new P2PClient(ListenPort, currClientDetails.ClientIPAddress, /*BaseSendPort+i*/
                        currClientDetails.ClientListenPort, Group);
                    currClient.Initialize();
                }

                currClient.SendMessageAsync(message);

                if (!mClientConnections.ContainsKey(currClientDetails))
                {
                    mClientConnections[currClientDetails] = currClient;
                }
            }
        }
    }
}