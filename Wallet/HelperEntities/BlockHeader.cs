using System;

namespace Wallet.Pages
{
    internal class BlockHeader
    {
        public int Index { get; set; }
        public int Nonce { get; set; } = 0;
        public DateTime TimeStamp { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public string MerkleRoot { get; set; }
    }
}