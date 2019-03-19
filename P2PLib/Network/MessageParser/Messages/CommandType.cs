namespace P2PLib.Network.MessageParser.Messages
{
    public enum CommandType
    {
       ClearTransactionPool,
       Transaction,
       Version,
       Inv,
       GetBlocks,
       Block,
       GetData
    }
}