using System;
using ChainUtils;

namespace CoreLib
{
    [Serializable]
    public class TxOutput 
    {
        public int Value { get; set; }
        public byte[] PublicKeyHash { get; set; }

        public static TxOutput NewTxOutput(int value, string address)
        {
            var txO = new TxOutput()
            {
                Value = value
            };

            txO.Lock(ByteHelper.GetBytesFromString(address));
            return txO;
        }


        public void Lock(byte[] address)
        {
            var pubKeyHashed = Base58Encoding.Decode(ByteHelper.GetStringFromBytes(address));
            pubKeyHashed = ArrayHelpers.SubArray(pubKeyHashed, 1, pubKeyHashed.Length - 5);
            PublicKeyHash = pubKeyHashed;
        }

        public bool IsLockedWithKey(byte[] pubKeyHash)
        {
            return ArrayHelpers.ByteArrayCompare(PublicKeyHash, pubKeyHash);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            return (ArrayHelpers.ByteArrayCompare(((TxOutput)obj).PublicKeyHash, PublicKeyHash)  &&
                    Value.Equals(((TxOutput)obj).Value));
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}