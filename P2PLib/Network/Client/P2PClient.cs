using Engine.Network.MessageParser;
using P2PLib.Network.Components;
using P2PLib.Network.Components.Enums;
using P2PLib.Network.Components.Interfaces;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security;

namespace P2PLib.Network.Client
{
    public class P2PClient : IClient
    {
        private Socket mClientSocket;

        private int mListenPort;
        private int mServerPort;
        private string mServer;
        private string mGroup;
        private IPAddress localAddress;

        public int ListenPort
        {
            get { return mListenPort; }
            set { mListenPort = value; }
        }

        public int ServerPort
        {
            get { return mServerPort; }
            set { mServerPort = value; }
        }

        public string Server
        {
            get { return mServer; }
            set { mServer = value; }
        }

        public string Group
        {
            get { return mGroup; }
            set { mGroup = value; }
        }

        public P2PClient(int listenPort, string server, int serverPort, string group)
        {
            ListenPort = listenPort;
            ServerPort = serverPort;
            Server = server;
            Group = group;
        }

        ~P2PClient()
        {
            if (mClientSocket != null)
                mClientSocket.Close();
        }


        public ClientDetails ClientDetails() {
            var details = new ClientDetails();
            details.ClientIPAddress = localAddress.ToString();
            details.ClientListenPort = mListenPort;
            return details;

        }

        public InitState Initialize()
        {
            if (mListenPort <= 0 || mListenPort >= 65536)
                return InitState.InvalidListenPort;

            if (mServerPort <= 0 || mServerPort >= 65536)
                return InitState.InvalidServerPort;

            if (string.IsNullOrEmpty(mServer))
                return InitState.InvalidServer;

            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName()); 

            if (hostEntry.AddressList.Length <= 0)
                return InitState.ErrorNoAvailableIPAddress;

             localAddress = null;
            for (int i = 0; i < hostEntry.AddressList.Length; ++i)
            {
                if (hostEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    localAddress = hostEntry.AddressList[i];
            }

            if (localAddress == null)
                return InitState.ErrorNoAvailableIPAddress;

            try
            {
                //register on the server
                RegisterMessage regMessage = new RegisterMessage();
                regMessage.Group = mGroup;
                regMessage.Client = new ClientDetails();
                regMessage.Client.ClientIPAddress = localAddress.ToString();
                regMessage.Client.ClientListenPort = mListenPort;

                SendMessage(regMessage);

            }
            catch (ArgumentNullException ex)
            {
                System.Diagnostics.Debug.WriteLine("Invalid Endpoint supplied : " + ex.Message);
            }
            catch (SocketException ex)
            {
                System.Diagnostics.Debug.WriteLine("Error attempting to access the socket : " + ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debug.WriteLine("The socket was closed : " + ex.Message);
            }
            catch (SecurityException ex)
            {
                System.Diagnostics.Debug.WriteLine("No permission for the requested operation : " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine("Invalid operation, the Socket is listening: " + ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("There was an unknown error : " + ex.Message);
            }

            return InitState.InitOK;
        }

        public void SendMessage(IMessage message)
        {
            mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress remoteMachine = IPAddress.Parse(Server);
            IPEndPoint remoteEndpoint = new IPEndPoint(remoteMachine, ServerPort);
            try
            {
                mClientSocket.Connect(remoteEndpoint);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("An error occurred while attempting to connnect : " + ex.Message);
                return;
            }

            if (mClientSocket.Connected)
                mClientSocket.Send(message.GetMessagePacket());
            mClientSocket.Close();
            mClientSocket = null;
        }


        public void SendMessageAsync(IMessage message)
        {
            mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress remoteMachine = IPAddress.Parse(Server);
            IPEndPoint remoteEndpoint = new IPEndPoint(remoteMachine, ServerPort);
            try
            {
                mClientSocket.BeginConnect(remoteEndpoint, new AsyncCallback(OnHandleConnection), message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("An error occurred while attempting to connnect : " + ex.Message);
                return;
            }
        }

        protected void OnHandleConnection(IAsyncResult asyncResult)
        {
            try
            {
                //mClientSocket.BeginConnect(remoteEndpoint, new AsyncCallback(OnHandleConnection), mClientSocket);
                IMessage message = (IMessage) asyncResult.AsyncState;
                mClientSocket.EndConnect(asyncResult);
                Byte[] dataBuffer = message.GetMessagePacket();
                TxRxPacket msgPacket = new TxRxPacket(mClientSocket, dataBuffer.Length);
                msgPacket.mDataBuffer = dataBuffer;
                if (mClientSocket.Connected)
                    mClientSocket.BeginSend(msgPacket.mDataBuffer, 0, msgPacket.mDataBuffer.Length, SocketFlags.None,
                        new AsyncCallback(OnHandleSend), msgPacket);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("An error occurred while attempting to connnect : " + ex.Message);
                return;
            }
        }

        protected void OnHandleSend(IAsyncResult asyncResult)
        {
            try
            {
                mClientSocket.EndSend(asyncResult);
                //TxRxPacket msgPacket = (TxRxPacket)asyncResult.AsyncState;

                mClientSocket.Close();
                mClientSocket = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("An error occurred while attempting to connnect : " + ex.Message);
                return;
            }
        }
    }
}