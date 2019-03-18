using ChainUtils;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
        public string PasswordHash { get; set; }


        public WalletCore(string hashedPassword)
        {
            PrivateKey = CryptoFinal.GeneratePrivateKey();
            PublicKey = CryptoFinal.GetPublicKey(PrivateKey);
            PublicKeyHash = PublicKeyHashed(PublicKey);
            PasswordHash = hashedPassword;

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

        public static byte[] TransferAddressToPkHash(string address)
        {
            var pubKeyHash = Base58Encoding.Decode(address);
            pubKeyHash = ArrayHelpers.SubArray(pubKeyHash, 1, pubKeyHash.Length - 5);
            return pubKeyHash;
        }

        public byte[] Serialize()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            return ms.ToArray();
        }

        public static WalletCore DeSerialize(byte[] fromBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(fromBytes, 0, fromBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            return (WalletCore)binForm.Deserialize(memStream);
        }


        //makes sure, that wallet is valid, its public and private keys are ok 
        public bool IsValid()
        {
            var testMessage = "this is my test message to be signed";
            var signature = CryptoFinal.SignTransaction(ByteHelper.GetBytesFromString(testMessage), PrivateKey);
            var isOk = CryptoFinal.VerifyHashed(signature, PublicKey, ByteHelper.GetBytesFromString(testMessage));
            return isOk;
        }
    }
}