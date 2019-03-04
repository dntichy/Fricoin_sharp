using ChainUtils;
using System;
using System.Security.Cryptography;
using System.Text;

namespace CoreLib
{
    public class WalletCore
    {
        public byte[] PrivateKey { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] PublicKeyHash { get; set; }

        public string Address { get; set; }


        public WalletCore()
        {
            PrivateKey = Crypto.GeneratePrivateKey();
            PublicKey = Crypto.GetPublicKey(PrivateKey);
            PublicKeyHash = PublicKeyHashed(GetStringFromBytes(PublicKey));

            var version = new byte[] {0x00};
            var versionHashed = ArrayHelpers.ConcatArrays(version, PublicKeyHash);

            var address = Base58Encoding.EncodeWithCheckSum(versionHashed);
            Address = address;
        }

        public byte[] PublicKeyHashed(string pubK)
        {
            var pubHash = Sha.GenerateSha256String(pubK);
            var ripemd160 = new RIPEMD160Managed();
            var ripemd160Hash = ripemd160.ComputeHash(Encoding.UTF8.GetBytes(pubHash));
            return ripemd160Hash;
        }


        public override string ToString()
        {
            return "PK: " + GetStringFromBytes(PublicKey) + "\n" +
                   "PKHashed: " + GetStringFromBytes(PublicKeyHash) + "\n" +
                   "Address: " + Address;
        }

        private static string GetStringFromBytes(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("x"));
            }

            return result.ToString();
        }

        public bool VerifyAddress(string address)
        {
            var pubKeyHash = Base58Encoding.Decode(address);
            var verify = Base58Encoding.VerifyAndRemoveCheckSum(pubKeyHash);
            if (verify != null)
            {
                Console.WriteLine("PK hash " + GetStringFromBytes(verify));
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}