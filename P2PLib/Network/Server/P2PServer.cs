using Engine.Network.MessageParser;
using P2PLib.Network.Client;
using P2PLib.Network.Components;
using P2PLib.Network.Components.Enums;
using P2PLib.Network.Components.Interfaces;
using P2PLib.Network.MessageParser;
using P2PLib.Network.MessageParser.Messages;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;

namespace P2PLib.Network.Server
{
    public class P2PServer : IServer
    {

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Collection<IClientDetails> groupClientsDetails;
        public Collection<IClientDetails> GroupClientsDetails
        {
            get { return groupClientsDetails; }
            set { groupClientsDetails = value; }
        }

        private Socket mListenerSocket;

        private String group;
        public String Group
        {
            get { return group; }
            set { group = value; }
        }

        private String mLocalIPAddress;
        public String LocalIPAddress
        {
            get { return mLocalIPAddress; }
        }

        private Collection<IMessage> incomingMessageQueue;
        public Collection<IMessage> IncomingMessageQueue
        {
            get { return incomingMessageQueue; }
            set { incomingMessageQueue = value; }
        }

        private int mListenPort;
        public int ListenPort
        {
            get { return mListenPort; }
            set { mListenPort = value; }
        }



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



        private IMessageParserEngine mMessageParser;
        public IMessageParserEngine MessageParser
        {
            get { return mMessageParser; }
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
                logger.Error("Unable to create the socket : " + ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                logger.Error("The socket was forcefully closed : " + ex.Message);
                System.Diagnostics.Debug.WriteLine("The socket was forcefully closed : " + ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                logger.Error(ex.Message);

            }

            return InitState.InitOK;
        }


        public P2PServer(int listenPort,  string group)
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
                TxRxPacket dataStatus = (TxRxPacket)asyncResult.AsyncState;


                dataStatus.mCurrentSocket.EndReceive(asyncResult);
                dataStatus.StoreCurrentData();


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
                    case ((int)MessageType.ResgisteredClientsListMessage):
                        {
                            Socket workerSocket = dataStatus.mCurrentSocket;
                            //respond with the current group in the message

                            RegisteredClientsListMessage rxClientList = (RegisteredClientsListMessage)rxMessage;
                            if (rxClientList.Clients != null)
                            {
                                for (int i = 0; i < rxClientList.Clients.Count; ++i)
                                {
                                    groupClientsDetails.Add(rxClientList.Clients[i]);
                                    //register on each of them
                                    IClient client = new P2PClient(mListenPort, rxClientList.Clients[i].ClientIPAddress,
                                        rxClientList.Clients[i].ClientListenPort, group);
                                    client.Initialize();
                                    client = null;
                                }

                                if (mOnRecieveListOfClients != null && group == ((RegisteredClientsListMessage)rxMessage).Group)
                                    mOnRecieveListOfClients.Invoke(this, new ReceiveListOfClientsEventArgs(((RegisteredClientsListMessage)rxMessage).Clients));
                            }

                            break;
                        }
                    case ((int)MessageType.RegisterMessage):
                        {
                            if (!(groupClientsDetails.IndexOf(((RegisterMessage)rxMessage).Client) >= 0))
                            {
                                groupClientsDetails.Add(((RegisterMessage)rxMessage).Client);

                                if (mOnRegisterClient != null && group == ((RegisterMessage)rxMessage).Group) mOnRegisterClient.Invoke(this, new ServerRegisterEventArgs(((RegisterMessage)rxMessage).Client));
                                if (mOnReceiveMessage != null) mOnReceiveMessage.Invoke(this, new ReceiveMessageEventArgs(rxMessage));
                            }

                            break;
                        }
                    case ((int)MessageType.UnregisterMessage):
                        {
                            if ((groupClientsDetails.IndexOf(((UnregisterMessage)rxMessage).Client) >= 0))
                            {
                                groupClientsDetails.Remove(((UnregisterMessage)rxMessage).Client);

                                if (mOnUnRegisterClient != null && group == ((UnregisterMessage)rxMessage).Group)
                                    mOnUnRegisterClient.Invoke(this,
                                    new ServerRegisterEventArgs(((UnregisterMessage)rxMessage).Client));
                            }

                            break;
                        }
                    case ((int)MessageType.CommandMessage):
                        {
                            if (mOnReceiveMessage != null) mOnReceiveMessage.Invoke(this, new ReceiveMessageEventArgs(rxMessage));
                            break;
                        }
                    default:
                        {
                            if (rxMessage.Type != ((int)MessageType.EmptyMessage))
                            {
                                if (mOnReceiveMessage != null)
                                    mOnReceiveMessage.Invoke(this,
                                        new ReceiveMessageEventArgs(rxMessage));
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