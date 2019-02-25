using P2PLib.Network.Components.Interfaces;
using P2PLib.Network.MessageParser;
using System;
using System.Collections.ObjectModel;

namespace Engine
{
    public interface ICollaborativeNotes
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
        void SendMessage(IMessage message, ICollaborativeClientDetails details);
        void SendMessage(IMessage message, String name);

        void BroadcastMessageAsync(IMessage message);
        void SendMessageAsync(IMessage message, ICollaborativeClientDetails details);
        void SendMessageAsync(IMessage message, String name);

        void Close();
    }
}