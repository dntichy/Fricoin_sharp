using System;
using P2PLib.Network.Components.Interfaces;

namespace P2PLib.Network.MessageParser
{
    public class ReceiveMessageEventArgs : EventArgs
    {
        private IMessage mMessage;

        public IMessage Message
        {
            get { return mMessage; }
        }

        public ReceiveMessageEventArgs(IMessage newMessage)
        {
            mMessage = newMessage;
        }
    }
}