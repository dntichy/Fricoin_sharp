using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using Engine.Network;
using Engine.Network.Client;
using Engine.Network.Components;
using Engine.Network.Components.Interfaces;
using Engine.Network.MessageParser;
using P2PLib.Network.Components.Enums;
using P2PLib.Network.MessageParser;
using P2PLib.Network.MessageParser.Messages;

namespace P2PLib.Network.Server
{
    public class P2PServer : IServer
    {
        private Collection<ICollaborativeClientDetails> _mGroupClientsDetails;

        public Collection<ICollaborativeClientDetails> GroupClientsDetails
        {
            get { return _mGroupClientsDetails; }
            set { _mGroupClientsDetails = value; }
        }


        private Socket mListenerSocket;

        private String mGroup;

        public String Group
        {
            get { return mGroup; }
            set { mGroup = value; }
        }

        private String mLocalIPAddress;

        public String LocalIPAddress
        {
            get { return mLocalIPAddress; }
        }

        private Collection<IMessage> mIncomingMessageQueue;

        public Collection<IMessage> IncomingMessageQueue
        {
            get { return mIncomingMessageQueue; }
            set { mIncomingMessageQueue = value; }
        }

        private int mListenPort;
        public int ListenPort
        {
            get { return mListenPort; }
            set { mListenPort = value; }
        }

        private event ServerRegisterEvent mOnRegisterClient;

        public event ServerRegisterEvent OnRegisterClient
        {
            add { mOnRegisterClient += value; }
            remove { mOnRegisterClient -= value; }
        }

        private IMessageParserEngine mMessageParser;

        public IMessageParserEngine MessageParser
        {
            get { return mMessageParser; }
        }

        private event OnReceiveMessageDelegate mOnReceiveMessage;

        public event OnReceiveMessageDelegate OnReceiveMessage
        {
            add { mOnReceiveMessage += value; }
            remove { mOnReceiveMessage -= value; }
        }


        public InitState Initialize()
        {
            if (mListenPort <= 0 || mListenPort >= 65536)
                return InitState.InvalidListenPort;

            IPHostEntry
                hostEntry = Dns.GetHostEntry( /*"localhost"*/
                    Dns.GetHostName()); //Dns.Resolve("localhost").AddressList[0];

            if (hostEntry.AddressList.Length <= 0)
                return InitState.ErrorNoAvailableIPAddress;

            IPAddress localAddress = null;
            for (int i = 0; i < hostEntry.AddressList.Length; ++i)
            {
                if (hostEntry.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    localAddress = hostEntry.AddressList[i];
            }

            if (localAddress == null)
                return InitState.ErrorNoAvailableIPAddress;

            mLocalIPAddress = localAddress.ToString();

            try
            {
                //create a listening socket
                if (mListenerSocket == null)
                    mListenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint localIP = new IPEndPoint(localAddress /*IPAddress.Any*/, mListenPort);
                mListenerSocket.Bind(localIP);
                mListenerSocket.Listen(50); //TODO -> ponder which is the best value to use here
                mListenerSocket.BeginAccept(new AsyncCallback(OnHandleClientConnection), null);
            }
            catch (SocketException ex)
            {
                System.Diagnostics.Debug.WriteLine("Unable to create the socket : " + ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debug.WriteLine("The socket was forcefully closed : " + ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return InitState.InitOK;
        }


        public P2PServer(int listenPort, /*int sendPort,*/ string group)
        {
            mMessageParser = new MessageParserEngineClass();
            ListenPort = listenPort;
            Group = group;
        }

        public void OnHandleClientConnection(IAsyncResult asyncResult)
        {
            try
            {
                Socket workerSocket = mListenerSocket.EndAccept(asyncResult);

                try
                {
                    TxRxPacket dataStatus = new TxRxPacket(workerSocket);

                    workerSocket.BeginReceive(dataStatus.mDataBuffer, 0, dataStatus.mDataBuffer.Length,
                        SocketFlags.None, new AsyncCallback(OnHandleClientData), dataStatus);
                }
                catch (SocketException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                mListenerSocket.BeginAccept(new AsyncCallback(OnHandleClientConnection), null);
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debug.WriteLine("Socket has been closed : " + ex.Message);
            }
            catch (SocketException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public void OnHandleClientData(IAsyncResult asyncResult)
        {
            try
            {
                TxRxPacket dataStatus = (TxRxPacket) asyncResult.AsyncState;

                /*int countRx =*/
                dataStatus.mCurrentSocket.EndReceive(asyncResult);
                dataStatus.StoreCurrentData();
                /*
				String rxMsgText = Encoding.UTF8.GetString(dataStatus.mStoredBuffer / *, 0, countRx* /);

				//parse the message
				IMessage rxMessage = null;

				rxMessage = ParseMessage(rxMsgText, dataStatus);
				*/

                IMessage rxMessage = mMessageParser.ParseMessage(dataStatus.mStoredBuffer);

                if (rxMessage == null)
                {
                    //receive the rest of the message
                    dataStatus.mCurrentSocket.BeginReceive(dataStatus.mDataBuffer, 0, dataStatus.mDataBuffer.Length,
                        SocketFlags.None, new AsyncCallback(OnHandleClientData), dataStatus);
                    return;
                }

                //handle the message (which can either be register or unregister)
                //send response message if needed
                switch (rxMessage.Type)
                {
                    case ((int) MessageType.ResgisteredClientsListMessage):
                    {
                        Socket workerSocket = (Socket) dataStatus.mCurrentSocket;
                        //respond with the current group in the message

                        RegisteredClientsListMessage rxClientList = (RegisteredClientsListMessage) rxMessage;
                        if (rxClientList.Clients != null)
                        {
                            for (int i = 0; i < rxClientList.Clients.Count; ++i)
                            {
                                _mGroupClientsDetails.Add(rxClientList.Clients[i]);
                                //register on each of them
                                IClient client = new P2PClient(mListenPort, rxClientList.Clients[i].ClientIPAddress,
                                    rxClientList.Clients[i].ClientListenPort, mGroup);
                                client.Initialize();
                                client = null;
                            }
                        }

                        break;
                    }
                    case ((int) MessageType.RegisterMessage):
                    {
                        if (!(_mGroupClientsDetails.IndexOf(((RegisterMessage) rxMessage).Client) >= 0))
                        {
                            _mGroupClientsDetails.Add(((RegisterMessage) rxMessage).Client);
                            if (mOnRegisterClient != null && mGroup == ((RegisterMessage) rxMessage).Group)
                                mOnRegisterClient.Invoke(this,
                                    new ServerRegisterEventArgs(((RegisterMessage) rxMessage).Client));
                        }

                        break;
                    }
                    case ((int) MessageType.UnregisterMessage):
                    {
                        if ((_mGroupClientsDetails.IndexOf(((UnregisterMessage) rxMessage).Client) >= 0))
                        {
                            _mGroupClientsDetails.Remove(((UnregisterMessage) rxMessage).Client);
                            if (mOnRegisterClient != null && mGroup == ((UnregisterMessage) rxMessage).Group)
                                mOnRegisterClient.Invoke(this,
                                    new ServerRegisterEventArgs(((UnregisterMessage) rxMessage).Client));
                        }

                        break;
                    }

                    default:
                    {
                        if (rxMessage.Type != ((int) MessageType.EmptyMessage))
                        {
                            mIncomingMessageQueue.Add(rxMessage);
                            if (mOnReceiveMessage != null)
                                mOnReceiveMessage.Invoke(this,
                                    new CollaborativeNotesReceiveMessageEventArgs(rxMessage));
                        }

                        break;
                    }
                }
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debug.WriteLine("Socket has been closed : " + ex.Message);
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == 10054) // Error code for Connection reset by peer
                {
                    System.Diagnostics.Debug.WriteLine("Connection reset by peer : " + ex.Message);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}