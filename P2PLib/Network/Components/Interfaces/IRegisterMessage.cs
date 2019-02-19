using System;
using Engine.Network.MessageParser;

namespace Engine.Network.Components.Interfaces
{
    public interface IRegisterMessage : IMessage
    {
        CollaborativeClientDetails Client { get; set; }

        String Group { get; set; }
    }
}