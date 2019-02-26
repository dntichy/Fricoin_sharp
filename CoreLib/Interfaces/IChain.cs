using System.Collections.Generic;

namespace CoreLib.Interfaces
{
    public interface IChain
    {
        int Difficulty { set; get; }
        IList<Block> Chain { set; get; }

        Block GetLatestBlock();
        void AddBlock(Block block);
        void ProcessPendingTransactions(string minerAddress);
        void CreateTransaction(Transaction transaction);
        bool IsValid();
        float GetBalance(string address);
    }
}