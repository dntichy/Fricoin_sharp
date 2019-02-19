using System;

namespace Engine.Network.Components.Interfaces
{
    public interface ICollaborativeClientDetails
    {
        String ClientName { get; set; }

        String ClientIPAddress { get; set; }

        int ClientListenPort { get; set; }
    }
}