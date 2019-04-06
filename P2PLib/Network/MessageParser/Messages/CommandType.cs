namespace P2PLib.Network.MessageParser.Messages
{
    public enum CommandType
    {
       Transaction,
       Version,
       Inv,
       GetBlocks,
       Block,
       GetData,
       NewBlockMined
    }
}