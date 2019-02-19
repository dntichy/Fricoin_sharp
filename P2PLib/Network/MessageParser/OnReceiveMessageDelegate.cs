using Engine.Network.MessageParser;

namespace P2PLib.Network.MessageParser
{
    public delegate void OnReceiveMessageDelegate(object sender, CollaborativeNotesReceiveMessageEventArgs e);
}