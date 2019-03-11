using ChainUtils;

namespace CoreLib.DataStructures.MerkleTree
{
    public class MerkleNode
    {
        public MerkleNode Left { get; set; }
        public MerkleNode Right { get; set; }
        public byte[] Data { get; set; }

        public MerkleNode(MerkleNode left, MerkleNode right, byte[] data)
        {
            Left = left;
            Right = right;
            Data = data;
        }

        public override string ToString()
        {
            return HexadecimalEncoding.ToHexString(Data).Substring(0,4);
        }
    }
}
