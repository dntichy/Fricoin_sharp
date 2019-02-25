using System;

namespace P2PLib.Network.Components.Interfaces
{
    public interface ICollaborativeClientDetails
    {
        String ClientName { get; set; }

        String ClientIPAddress { get; set; }

        int ClientListenPort { get; set; }
    }
}