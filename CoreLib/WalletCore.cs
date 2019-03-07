using ChainUtils;
using System;
using System.Security.Cryptography;
using System.Text;

namespace CoreLib
{
    [Serializable]
    public class WalletCore
    {
        public byte[] PrivateKey { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] PublicKeyHash { get; set; }
        public string Address { get; set; }


        public WalletCore()
        {
            PrivateKey = CryptoFinal.GeneratePrivateKey();
            PublicKey = CryptoFinal.GetPublicKey(PrivateKey);
            PublicKeyHash = PublicKeyHashed(PublicKey);

            var version = new byte[] {0x00};
            var versionHashed = ArrayHelpers.ConcatArrays(version, PublicKeyHash);

            var address = Base58Encoding.EncodeWithCheckSum(versionHashed);
            Address = address;
        }

        public static byte[] PublicKeyHashed(byte[] pubK)
        {
            var pubHash = Sha.GenerateSha256String(ByteHelper.GetStringFromBytes(pubK));
            var ripemd160 = new RIPEMD160Managed();
            var ripemd160Hash = ripemd160.ComputeHash(Encoding.UTF8.GetBytes(pubHash));
            return ripemd160Hash;
        }

        public bool VerifyAddress(string address)
        {
            var pubKeyHash = Base58Encoding.Decode(address);
            var verify = Base58Encoding.VerifyAndRemoveCheckSum(pubKeyHash);
            if (verify != null)
            {
                Console.WriteLine("PK hash " + ByteHelper.GetStringFromBytes(verify));
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return 
                "---WALLET INFO----"+"\nPK: " + Convert.ToBase64String(PublicKey) + "\n" +
                   "PKHashed: " + Convert.ToBase64String(PublicKeyHash) + "\n" +
                   "Address: " + Address+ "\n-- - WALLET INFO END----";
        }
    }
}