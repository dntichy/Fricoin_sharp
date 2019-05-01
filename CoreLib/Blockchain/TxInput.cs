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
        public int MagicValue { get; set; }

        public bool UsesKey(byte[] pubKeyHash)
        {
            var lockingHash = WalletCore.PublicKeyHashed(PubKey);
            //Console.WriteLine("lockin hash "+ Convert.ToBase64String(lockingHash));
            //Console.WriteLine("pkHash " + Convert.ToBase64String(pubKeyHash));
            return ArrayHelpers.ByteArrayCompare(lockingHash, pubKeyHash);
        }
    }
}