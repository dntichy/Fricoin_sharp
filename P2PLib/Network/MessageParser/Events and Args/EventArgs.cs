using System;
using System.Collections.Generic;
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

        public ReceiveListOfClientsEventArgs(Collection<IClientDetails> listOfClients)
        {
            mListOfClients = listOfClients;
        }
    }


    public class ProgressBarEventArgs : EventArgs
    {
        public int HighestIndex { get; set; }
        public int CountCurrentBlocksInTranzit { get; set; }
        public int ReducedBlocksCount { get; set; }

        public ProgressBarEventArgs(int heighestIndex, int countCurrentBlocksInTranzit, int reducedBlockCount)
        {
            HighestIndex = heighestIndex;
            CountCurrentBlocksInTranzit = countCurrentBlocksInTranzit;
            ReducedBlocksCount = reducedBlockCount;
        }
    }

    public class TransactionPoolEventArgs : EventArgs
    {
        public List<string> TransactionPoolList { get; set; }

        public TransactionPoolEventArgs(List<string> txPoolList)
        {
            this.TransactionPoolList = txPoolList;
        }
    }
    public class MinedHashUpdateEventArgs : EventArgs
    {
        public string Hash { get; set; }

        public MinedHashUpdateEventArgs(string hash)
        {
            this.Hash = hash;
        }
    }
    

}