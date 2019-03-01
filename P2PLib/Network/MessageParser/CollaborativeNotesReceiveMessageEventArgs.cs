using System;
using P2PLib.Network.Components.Interfaces;

namespace P2PLib.Network.MessageParser
{
    public class CollaborativeNotesReceiveMessageEventArgs : EventArgs
    {
        private IMessage mMessage;

        public IMessage Message
        {
            get { return mMessage; }
        }

        public CollaborativeNotesReceiveMessageEventArgs(IMessage newMessage)
        {
            mMessage = newMessage;
        }
    }
}