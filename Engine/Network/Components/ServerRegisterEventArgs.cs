﻿using System;

namespace Engine.Network.Components
{
    public class ServerRegisterEventArgs : EventArgs
    {
        private ICollaborativeClientDetails mNewClient;

        public ICollaborativeClientDetails NewClient
        {
            get { return mNewClient; }
        }

        public ServerRegisterEventArgs(ICollaborativeClientDetails newClient)
        {
            mNewClient = newClient;
        }
    };
}