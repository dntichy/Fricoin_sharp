using System;
using Engine.Network.Components;

namespace Engine.Network.MessageParser
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