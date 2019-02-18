using System;

namespace Engine.Network.Components
{
    public interface ICollaborativeClientDetails
    {
        String ClientName { get; set; }

        String ClientIPAddress { get; set; }

        int ClientListenPort { get; set; }
    }
}