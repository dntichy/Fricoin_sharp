using P2PLib.Network.Components.Enums;

namespace Engine.Network.Components.Interfaces
{
    public interface IServer
    {
        int ListenPort { get; set; }

        IMessageParserEngine MessageParser { get; }

        event ServerRegisterEvent OnRegisterClient;

        InitState Initialize();
    }
}