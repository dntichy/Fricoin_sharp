using P2PLib.Network.Components.Interfaces;
using System;

namespace Engine.Network.MessageParser
{
    public class ClientDetails : IClientDetails
    {
        private String clientName;
        private String clientIPAddress;
        private int clientListenPort;

        public String ClientName
        {
            get { return clientName; }
            set { clientName = value; }
        }

        public String ClientIPAddress
        {
            get { return clientIPAddress; }
            set { clientIPAddress = value; }
        }

        public int ClientListenPort
        {
            get { return clientListenPort; }
            set { clientListenPort = value; }
        }

        public int CompareTo(IClientDetails other)
        {
            if (Equals(other)) return 0;
            else return 1;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            return (clientListenPort == ((IClientDetails)obj).ClientListenPort &&
                    clientIPAddress.Equals(((IClientDetails)obj).ClientIPAddress));
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return ClientIPAddress + ":" + ClientListenPort;
        }
    }
}