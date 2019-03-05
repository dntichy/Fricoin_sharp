using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Utilities;
using ECPoint = Org.BouncyCastle.Math.EC.ECPoint;


namespace ChainUtils
{
    public class CryptoFinal
    {
        private static String RANDOM_NUMBER_ALGORITHM = "SHA1PRNG";
        private static bool RANDOM_NUMBER_ALGORITHM_PROVIDER = true;

        public static byte[] SignTransaction(byte[] data, byte[] privateKey)
        {
            ECDsaSigner signer = new ECDsaSigner();
            X9ECParameters spec = SecNamedCurves.GetByName("secp256k1");
            ECDomainParameters domain = new ECDomainParameters(spec.Curve, spec.G, spec.N);

            ECPrivateKeyParameters privateKeyParms =
                new ECPrivateKeyParameters(new BigInteger(1, privateKey), domain);
            ParametersWithRandom paramxs = new ParametersWithRandom(privateKeyParms);
            signer.Init(true, paramxs);

            var signature = signer.GenerateSignature(data); //sign and get R and S

            //return as DER format
            using (MemoryStream outStream = new MemoryStream(80))
            {
                DerSequenceGenerator seq = new DerSequenceGenerator(outStream);
                seq.AddObject(new DerInteger(signature[0])); //r
                seq.AddObject(new DerInteger(signature[1])); //s
                seq.AddObject(new DerInteger(GetRecoveryId(signature[0].ToByteArray(),
                    signature[1].ToByteArray(), data, GetPublicKey(privateKey)))); //v
                seq.Close();
                return outStream.ToArray();
            }
        }

        public static bool VerifyHashed(byte[] signature, byte[] publicKey, byte[] data)
        {
            var (sigR, sigS, _) = GetRSV(signature);
            try
            {
                X9ECParameters spec = ECNamedCurveTable.GetByName("secp256k1");
                ECDomainParameters domain = new ECDomainParameters(spec.Curve, spec.G, spec.N);
                ECPublicKeyParameters publicKeyParams =
                    new ECPublicKeyParameters(spec.Curve.DecodePoint(publicKey), domain);

                ECDsaSigner signer = new ECDsaSigner();
                signer.Init(false, publicKeyParams);
                return signer.VerifySignature(data, new BigInteger(1, sigR.ToByteArray()),
                    new BigInteger(1, sigS.ToByteArray()));
            }
            catch
            {
                return false;
            }
        }

        public static (BigInteger r, BigInteger s, BigInteger v) GetRSV(byte[] signature)
        {
            using (var stream = new Asn1InputStream(signature))
            {
                var sequence = (DerSequence) stream.ReadObject();
                var r = ((DerInteger) sequence[0]).Value;
                var s = ((DerInteger) sequence[1]).Value;
                var v = ((DerInteger) sequence[2]).Value;

                return (r, s, v);
            }
        }

        public static byte[] RecoverPublicKey(byte[] sigR, byte[] sigS, byte[] sigV, byte[] message)
        {
            //ECNamedCurveParameterSpec spec = ECNamedCurveTable.getParameterSpec("secp256k1");
            X9ECParameters spec = ECNamedCurveTable.GetByName("secp256k1");

            BigInteger pointN = spec.N;

            try
            {
                BigInteger pointX = new BigInteger(1, sigR);


                byte[] compEnc =
                    X9IntegerConverter.IntegerToBytes(pointX, 1 + X9IntegerConverter.GetByteLength(spec.Curve));
                compEnc[0] = (byte) ((sigV[0] & 1) == 1 ? 0x03 : 0x02);
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

        private static byte GetRecoveryId(byte[] sigR, byte[] sigS, byte[] message, byte[] publicKey)
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
                    Console.WriteLine(" Failed: GET recoveryID");
                }
            }

            return (byte) 0xFF;
        }

        //public static BigInteger ExtractR(byte[] signature)
        //{
        //    int startR = (signature[1] & 0x80) != 0 ? 3 : 2;
        //    int lengthR = signature[startR + 1];
        //    return new BigInteger(Arrays.CopyOfRange(signature, startR + 2, startR + 2 + lengthR));
        //}

        //public static BigInteger ExtractS(byte[] signature)
        //{
        //    int startR = (signature[1] & 0x80) != 0 ? 3 : 2;
        //    int lengthR = signature[startR + 1];
        //    int startS = startR + 2 + lengthR;
        //    int lengthS = signature[startS + 1];
        //    return new BigInteger(Arrays.CopyOfRange(signature, startS + 2, startS + 2 + lengthS));
        //}
    }
}