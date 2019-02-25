using System;

namespace P2PLib.Network.Components.Interfaces

{
    public interface IMessage
    {
        int Type { get; }
        Byte[] GetMessagePacket();
        bool Parse(Byte[] data);
        IMessage Clone();
    }
}