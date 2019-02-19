using System;
using Engine.Network.Components.Interfaces;
using P2PLib.Network.Components.Enums;

namespace P2PLib.Network.MessageParser.Messages
{
    public class EmptyMessage : IMessage
    {
        public /*MessageType*/int Type
        {
            get { return (int)MessageType.EmptyMessage; }
        }

        public byte[] GetMessagePacket()
        {
            return null;
        }

        public bool Parse(Byte[] data)
        {
            //TODO -> ponder better on what to do with this
            return true;
        }

        public IMessage Clone()
        {
            return new EmptyMessage();
        }
    }
}