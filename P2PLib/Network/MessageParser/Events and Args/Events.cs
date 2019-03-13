using P2PLib.Network.MessageParser;

namespace P2PLib.Network.Components
{
    public delegate void ServerRegisterEvent(object sender, ServerRegisterEventArgs e);
    public delegate void ServerUnRegisterEvent(object sender, ServerRegisterEventArgs e);
    public delegate void OnReceiveMessageEvent(object sender, ReceiveMessageEventArgs e);

}