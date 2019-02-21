using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ECPoint = Org.BouncyCastle.Math.EC.ECPoint;


namespace ChainUtils
{


    public class Crypto
    {

        
        private static String RANDOM_NUMBER_ALGORITHM = "SHA1PRNG"; 
        private static bool RANDOM_NUMBER_ALGORITHM_PROVIDER = true; 

        //public string PublicKeyBeforeSha256 { get; set; }
        //private string _privateKey;


        //public AsymmetricCipherKeyPair GenerateKeyPair()
        //{
        //    ECKeyPairGenerator keyPair = new ECKeyPairGenerator("EC");
        //    keyPair.Init(new KeyGenerationParameters(new SecureRandom(), 384));
        //    var pair = keyPair.GenerateKeyPair();

        //    //PublicKeyBeforeSha256 = GetPublicKeyStringRepresentation(pair.Public);
        //    //_privateKey = GetPrivateKeyStringRepresentation(pair.Private);

        //    return pair;
        //}




        /**
* Generate a random private key that can be used with Secp256k1. 
*/
        public static byte[] GeneratePrivateKey()
        {
            SecureRandom secureRandom = null;
            try
            {
                secureRandom =
                    SecureRandom.GetInstance(RANDOM_NUMBER_ALGORITHM, RANDOM_NUMBER_ALGORITHM_PROVIDER);
            }
            catch (Exception e)
            {
                Console.WriteLine("err");
            }

            BigInteger privateKeyCheck = BigInteger.Zero;
            // Bit of magic, move this maybe. This is the max key range. 
            BigInteger maxKey =
                new BigInteger("00FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEBAAEDCE6AF48A03BBFD25E8CD0364140", 16);

            // Generate the key, skipping as many as desired. 
            byte[] privateKeyAttempt = new byte[32];
            secureRandom?.NextBytes(privateKeyAttempt);
            privateKeyCheck = new BigInteger(1, privateKeyAttempt);
            while (privateKeyCheck.CompareTo(BigInteger.Zero) == 0
                   || privateKeyCheck.CompareTo(maxKey) == 1)
            {
                secureRandom?.NextBytes(privateKeyAttempt);
                privateKeyCheck = new BigInteger(1, privateKeyAttempt);
            }

            return privateKeyAttempt;
        }






        //public static string SignData(string msg, AsymmetricKeyParameter privKey)
        public static byte[][] SignTransaction(byte[] data, byte[] privateKey)
        {
            try
            {
                //byte[] msgBytes = Encoding.UTF8.GetBytes(msg);

                //ISigner signer = SignerUtilities.GetSigner("SHA384withECDSA");
                //signer.Init(true, privKey);
                //signer.BlockUpdate(msgBytes, 0, msgBytes.Length);
                //byte[] sigBytes = signer.GenerateSignature();

                //return Convert.ToBase64String(sigBytes);

                X9ECParameters spec = ECNamedCurveTable.GetByName("secp256k1");

                ECDsaSigner ecdsaSigner = new ECDsaSigner();
                ECDomainParameters domain = new ECDomainParameters(spec.Curve, spec.G, spec.N);
                ECPrivateKeyParameters privateKeyParms =
                    new ECPrivateKeyParameters(new BigInteger(1, privateKey), domain);
                ParametersWithRandom paramxs = new ParametersWithRandom(privateKeyParms);
                ecdsaSigner.Init(true, paramxs);
                BigInteger[] sig = ecdsaSigner.GenerateSignature(data);
                LinkedList<byte[]> sigData = new LinkedList<byte[]>();
                byte[] publicKey = GetPublicKey(privateKey);
                byte recoveryId = GetRecoveryId(sig[0].ToByteArray(), sig[1].ToByteArray(), data, publicKey);
                foreach (var sigChunk in sig)
                {
                    sigData.AddLast(sigChunk.ToByteArray());
                }

                sigData.AddLast(new byte[] {recoveryId});
                return sigData.ToArray();
            }
            catch (Exception exc)
            {
                Console.WriteLine("Signing Failed: " + exc);
                return null;
            }
        }

        public static byte GetRecoveryId(byte[] sigR, byte[] sigS, byte[] message, byte[] publicKey)
        {
            //ECNamedCurveParameterSpec spec = ECNamedCurveTable.getParameterSpec("secp256k1");
            X9ECParameters spec = ECNamedCurveTable.GetByName("secp256k1");

            BigInteger pointN = spec.N;
            for (int recoveryId = 0; recoveryId < 2; recoveryId++)
            {
                try
                {
                    BigInteger pointX = new BigInteger(1, sigR);

                    byte[] compEnc =
                        X9IntegerConverter.IntegerToBytes(pointX, 1 + X9IntegerConverter.GetByteLength(spec.Curve));
                    compEnc[0] = (byte) ((recoveryId & 1) == 1 ? 0x03 : 0x02);
                    ECPoint pointR = spec.Curve.DecodePoint(compEnc);
                    if (!pointR.Multiply(pointN).IsInfinity)
                    {
                        continue;
                    }

                    BigInteger pointE = new BigInteger(1, message);
                    BigInteger pointEInv = BigInteger.Zero.Subtract(pointE).Mod(pointN);
                    BigInteger pointRInv = new BigInteger(1, sigR).ModInverse(pointN);
                    BigInteger srInv = pointRInv.Multiply(new BigInteger(1, sigS)).Mod(pointN);
                    BigInteger pointEInvRInv = pointRInv.Multiply(pointEInv).Mod(pointN);
                    ECPoint pointQ = ECAlgorithms.SumOfTwoMultiplies(spec.G, pointEInvRInv, pointR, srInv);
                    byte[] pointQBytes = pointQ.GetEncoded(false);
                    bool matchedKeys = true;
                    for (int j = 0; j < publicKey.Length; j++)
                    {
                        if (pointQBytes[j] != publicKey[j])
                        {
                            matchedKeys = false;
                            break;
                        }
                    }

                    if (!matchedKeys)
                    {
                        continue;
                    }

                    return (byte) (0xFF & recoveryId);
                }
                catch (Exception e)
                {
                    continue;
                    Console.WriteLine(" Failed: GET recoveryID" );

                }
            }

            return (byte) 0xFF;
        }

        //public static bool VerifySignature(AsymmetricKeyParameter pubKey, string signature, string msg)
        //{
        //    try
        //    {
        //        byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
        //        byte[] sigBytes = Convert.FromBase64String(signature);

        //        ISigner signer = SignerUtilities.GetSigner("SHA384withECDSA");
        //        signer.Init(false, pubKey);
        //        signer.BlockUpdate(msgBytes, 0, msgBytes.Length);

        //        return signer.VerifySignature(sigBytes);
        //    }
        //    catch (Exception exc)
        //    {
        //        Console.WriteLine("Verification failed with the error: " + exc.ToString());
        //        return false;
        //    }
        //}

        /// <summary>
        /// NOT WORKING cause of SHA 256
        /// </summary>
        /// <param name="pubK"></param>
        /// <returns></returns>
        //public static AsymmetricKeyParameter GetPublicKeyObjectRepresentation(string pubK)
        //{
        //    //byte[] recoveredPublic = Convert.FromBase64String(pubK);
        //    byte[] recoveredPublic = Base58Encoding.Decode(pubK);
        //    var publicKey = PublicKeyFactory.CreateKey(recoveredPublic);

        //    return publicKey;
        //}
        //public static string GetPublicKeyStringRepresentation(AsymmetricKeyParameter pubK)
        //{
        //    SubjectPublicKeyInfo subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pubK);
        //    byte[] serializedPublicBytes = subjectPublicKeyInfo.ToAsn1Object().GetDerEncoded();

        //    var base64 = Convert.ToBase64String(serializedPublicBytes);
        //    var sha256 = ComputeSha256HashB(base64);
        //    var result = Base58Encoding.Encode(sha256);

        //    return result;
        //}


        /// <summary>
        /// Not gonna be used
        /// </summary>
        /// <param name="priK"></param>
        /// <returns></returns>
        //public static string GetPrivateKeyStringRepresentation(AsymmetricKeyParameter priK)
        //{
        //    PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(priK);
        //    byte[] serializedPrivateBytes = privateKeyInfo.ToAsn1Object().GetDerEncoded();

        //    //var result = Convert.ToBase64String(serializedPrivateBytes);
        //    var result = Base58Encoding.Encode(serializedPrivateBytes);

        //    return result;
        //}


        //private static string ComputeSha256Hash(string rawData)
        //{
        //    // Create a SHA256   
        //    using (SHA256 sha256Hash = SHA256.Create())
        //    {
        //        // ComputeHash - returns byte array  
        //        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

        //        // Convert byte array to a string   
        //        StringBuilder builder = new StringBuilder();
        //        for (int i = 0; i < bytes.Length; i++)
        //        {
        //            builder.Append(bytes[i].ToString("x2"));
        //        }

        //        return builder.ToString();
        //    }
        //}

        private static byte[] ComputeSha256HashB(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                return bytes;
            }
        }

        /**
         * Converts a private key into its corresponding public key. 
         */
        public static byte[] GetPublicKey(byte[] privateKey)
        {
            try
            {
                //ECNamedCurveParameterSpec spec = ECNamedCurveTable.getParameterSpec("secp256k1");
                X9ECParameters spec = ECNamedCurveTable.GetByName("secp256k1");
                ECPoint pointQ = spec.G.Multiply(new BigInteger(1, privateKey));

                return pointQ.GetEncoded(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(" Failed: GET public key");

                return new byte[0];
            }
        }


        /**
 * Recover the public key that corresponds to the private key, which signed this message. 
 */
        public static byte[] RecoverPublicKey(byte[] sigR, byte[] sigS, byte[] sigV, byte[] message)
        {
            //ECNamedCurveParameterSpec spec = ECNamedCurveTable.getParameterSpec("secp256k1");
            X9ECParameters spec = ECNamedCurveTable.GetByName("secp256k1");

            BigInteger pointN = spec.N;

            try
            {
                BigInteger pointX = new BigInteger(1, sigR);

                
                byte[] compEnc = X9IntegerConverter.IntegerToBytes(pointX, 1 + X9IntegerConverter.GetByteLength(spec.Curve));
                compEnc[0] = (byte)((sigV[0] & 1) == 1 ? 0x03 : 0x02);
                ECPoint pointR = spec.Curve.DecodePoint(compEnc);
                if (!pointR.Multiply(pointN).IsInfinity)
                {
                    return new byte[0];
                }

                BigInteger pointE = new BigInteger(1, message);
                BigInteger pointEInv = BigInteger.Zero.Subtract(pointE).Mod(pointN);
                BigInteger pointRInv = new BigInteger(1, sigR).ModInverse(pointN);
                BigInteger srInv = pointRInv.Multiply(new BigInteger(1, sigS)).Mod(pointN);
                BigInteger pointEInvRInv = pointRInv.Multiply(pointEInv).Mod(pointN);
                ECPoint pointQ = ECAlgorithms.SumOfTwoMultiplies(spec.G, pointEInvRInv, pointR, srInv);
                byte[] pointQBytes = pointQ.GetEncoded(false);
                return pointQBytes;
            }
            catch (Exception e)
            {
              
            }

            return new byte[0];
        }
    }
}