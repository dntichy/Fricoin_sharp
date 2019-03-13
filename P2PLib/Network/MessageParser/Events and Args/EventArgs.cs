using System;
using System.Collections.ObjectModel;
using P2PLib.Network.Components.Interfaces;

namespace P2PLib.Network.MessageParser
{
    public class ReceiveMessageEventArgs : EventArgs
    {
        private IMessage mMessage;

        public IMessage Message
        {
            get { return mMessage; }
        }


        public ReceiveMessageEventArgs(IMessage newMessage)
        {
            mMessage = newMessage;

        }
    }

    public class ServerRegisterEventArgs : EventArgs
    {
        private IClientDetails mNewClient;
        public IClientDetails NewClient
        {
            get { return mNewClient; }
        }

        public ServerRegisterEventArgs(IClientDetails newClient)
        {
            mNewClient = newClient;
        }
    };

    public class ReceiveListOfClientsEventArgs 
    {
        private Collection<IClientDetails> mListOfClients;
        public Collection<IClientDetails> ListOfClients
        {
            get { return mListOfClients; }
        }

        public ReceiveListOfClientsEventArgs (Collection<IClientDetails> listOfClients)
        {
            mListOfClients = listOfClients;
        }
    }
}