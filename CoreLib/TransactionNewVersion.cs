using System;

namespace CoreLib
{
    [Serializable]
    public class TransactionNewVersion
    {
        public byte[] Id { get; set; }
        public TxInput[] Inputs { get; set; }
        public TxOutput[] Outputs { get; set; }
    }

    public class TxOutput
    {
        public int Value { get; set; }
        public string PublicKey { get; set; }
    }

    public class TxInput
    {
        public byte[] Id { get; set; }
        public int Out { get; set; }
        public string Signature { get; set; }
    }
}