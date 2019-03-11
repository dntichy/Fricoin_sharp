using System;
using P2PLib.Network.Components.Interfaces;

namespace Engine.Network.MessageParser
{
    public class ClientDetails : IClientDetails
    {
        private String mClientName;
        private String mClientIPAddress;
        private int mClientListenPort;

        public String ClientName
        {
            get { return mClientName; }
            set { mClientName = value; }
        }

        public String ClientIPAddress
        {
            get { return mClientIPAddress; }
            set { mClientIPAddress = value; }
        }

        public int ClientListenPort
        {
            get { return mClientListenPort; }
            set { mClientListenPort = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            return (mClientListenPort == ((ClientDetails) obj).ClientListenPort &&
                    mClientIPAddress == ((ClientDetails) obj).ClientIPAddress &&
                    mClientName == ((ClientDetails) obj).ClientName); //base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}