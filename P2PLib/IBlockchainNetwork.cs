using P2PLib.Network.Components;
using P2PLib.Network.Components.Interfaces;
using System;
using System.Collections.ObjectModel;

namespace P2PLib.Network
{
    public interface IBlockchainNetwork
    {
        int ListenPort
        {
            get;
            set;
        }

        int ServerListenPort
        {
            get;
            set;
        }

        String Server
        {
            get;
            set;
        }

        String Group
        {
            get;
            set;
        }


        IMessageParserEngine MessageParser
        {
            get;
        }

        event OnReceiveMessageEvent OnReceiveMessage;

        void BroadcastMessage(IMessage message);
        void SendMessage(IMessage message, IClientDetails details);

        void BroadcastMessageAsync(IMessage message);
        void SendMessageAsync(IMessage message, IClientDetails details);

        void Close();
    }
}