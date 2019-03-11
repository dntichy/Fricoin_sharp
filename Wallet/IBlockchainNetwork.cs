using System;
using System.Collections.ObjectModel;
using P2PLib.Network.Components.Interfaces;
using P2PLib.Network.MessageParser;

namespace Wallet
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

        Collection<IMessage> InboundMessages
        {
            get;
        }

        IMessageParserEngine MessageParser
        {
            get;
        }

        event OnReceiveMessageDelegate OnReceiveMessage;

        void BroadcastMessage(IMessage message);
        void SendMessage(IMessage message, IClientDetails details);
        void SendMessage(IMessage message, String name);

        void BroadcastMessageAsync(IMessage message);
        void SendMessageAsync(IMessage message, IClientDetails details);
        void SendMessageAsync(IMessage message, String name);

        void Close();
    }
}