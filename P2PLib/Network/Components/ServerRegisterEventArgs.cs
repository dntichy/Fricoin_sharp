using System;
using P2PLib.Network.Components.Interfaces;

namespace P2PLib.Network.Components
{
    public class ServerRegisterEventArgs : EventArgs
    {
        private IClientDetails mNewClient;

        public IClientDetails NewClient
        {
            get { return mNewClient; }
        }

        public ServerRegisterEventArgs(IClientDetails newClient)
        {
            mNewClient = newClient;
        }
    };
}