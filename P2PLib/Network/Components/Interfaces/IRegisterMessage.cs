using System;
using Engine.Network.MessageParser;

namespace P2PLib.Network.Components.Interfaces
{
    public interface IRegisterMessage : IMessage
    {
        ClientDetails Client { get; set; }

        String Group { get; set; }
    }
}