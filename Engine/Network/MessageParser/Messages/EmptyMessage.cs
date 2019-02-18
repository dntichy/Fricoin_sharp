using System;
using Engine.Network.Components;

namespace Engine.Network.MessageParser
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