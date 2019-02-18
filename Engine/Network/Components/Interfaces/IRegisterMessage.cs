using System;
using Engine.Network.Components;

namespace Engine.Network.MessageParser
{
    public interface IRegisterMessage : IMessage
    {
        CollaborativeClientDetails Client { get; set; }

        String Group { get; set; }
    }
}