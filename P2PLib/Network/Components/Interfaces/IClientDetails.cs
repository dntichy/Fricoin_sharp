﻿using System;

namespace P2PLib.Network.Components.Interfaces
{
    public interface IClientDetails
    {
        String ClientName { get; set; }

        String ClientIPAddress { get; set; }

        int ClientListenPort { get; set; }
    }
}