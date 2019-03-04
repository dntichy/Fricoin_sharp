using System;
using System.Collections.Generic;

namespace CoreLib.Interfaces
{
    interface IBlock
    {
        int Index { get; set; }
        int Nonce { get; set; }
        DateTime TimeStamp { get; set; }
        byte[] PreviousHash { get; set; }
        byte[] Hash { get; set; }
        IList<Transaction> Transactions { get; set; }

        byte[] CalculateHash();
        void Mine(int difficulty);

        byte[] Serialize();
        Block DeSerialize(byte[] fromBytes);
    }
}