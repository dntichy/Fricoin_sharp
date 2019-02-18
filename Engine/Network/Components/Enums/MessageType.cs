namespace Engine.Network
{
    public enum MessageType : int
    {
        EmptyMessage,
        RegisterMessage,
        UnregisterMessage,
        ResgisteredClientsListMessage,
        TextDataMessage
    }
}