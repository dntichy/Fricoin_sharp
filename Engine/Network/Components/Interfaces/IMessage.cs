using System;
using Engine.Network.MessageParser;

namespace Engine.Network.Components

{
    public interface IMessage
    {
        int Type { get; }
        Byte[] GetMessagePacket();
        bool Parse(Byte[] data);
        IMessage Clone();
    }
}