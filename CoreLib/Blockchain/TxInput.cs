using System;
using ChainUtils;

namespace CoreLib.Blockchain
{
    [Serializable]
    public class TxInput
    {
        public byte[] Id { get; set; }
        public int Out { get; set; }
        public byte[] Signature { get; set; }
        public byte[] PubKey { get; set; }

        //public bool CanUnlock(string address)
        //{
        //    return address == Signature;
        //}
        public bool UsesKey(byte[] pubKeyHash)
        {
            var lockingHash = WalletCore.PublicKeyHashed(PubKey);
            return ArrayHelpers.ByteArrayCompare(lockingHash, pubKeyHash);
        }
    }
}