namespace P2PLib.Network.MessageParser.Messages
{
    public enum CommandType
    {
       ClearTransactionPool,
       Chain,
       Transaction,
       Version,
       Inv,
       GetBlocks,
       Block,
       GetData
    }
}