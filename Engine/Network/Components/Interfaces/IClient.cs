using System;

namespace Engine.Network.Components
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