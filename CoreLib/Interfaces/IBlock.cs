using System;
using System.Collections.Generic;
using CoreLib;

namespace Engine.Interfaces
{
    interface IBlock
    {
        int Index { get; set; }
        int Nonce { get; set; }
        DateTime TimeStamp { get; set; }
        string PreviousHash { get; set; }
        string Hash { get; set; }
        IList<Transaction> Transactions { get; set; }

        string CalculateHash();
        void Mine(int difficulty);
    }
}