using System;
using P2PLib.Network.Components.Enums;

namespace P2PLib.Network.Components.Interfaces
{
    public interface IClient
    {
        int ListenPort { get; set; }

        int ServerPort { get; set; }

        String Server { get; set; }

        InitState Initialize();

        void SendMessage(IMessage message);

        void SendMessageAsync(IMessage message);
    }
}