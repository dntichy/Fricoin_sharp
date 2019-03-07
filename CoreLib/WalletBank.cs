using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CoreLib
{
    public class WalletBank : IEnumerable<WalletCore>
    {
        private readonly List<WalletCore> _wallets;
        private const string FileName = "walletdb";

        public WalletBank()
        {
            _wallets = new List<WalletCore>();

            if (File.Exists(FileName))
            {
                var walletBank = File.ReadAllBytes(FileName);
                _wallets = Deserialize(walletBank);
            }
            else PersistWalletBank();
        }

        public WalletCore FindWallet(string address)
        {
            foreach (var wallet in _wallets)
            {
                if (wallet.Address == address) return wallet;
            }

            return null;
        }

        public WalletCore CreateWallet()
        {
            var wallet = new WalletCore();
            _wallets.Add(wallet);
            PersistWalletBank();
            return wallet;
        }

        private byte[] Serialize()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, _wallets);
            return ms.ToArray();
        }

        private void PersistWalletBank()
        {
            File.WriteAllBytes(FileName, Serialize());
        }

        public void DisplayAllAddresses()
        {
            foreach (var wallet in _wallets)
            {
                Console.WriteLine(wallet.Address);
            }
        }

        public void DisplayWalletInfo()
        {
            foreach (var wallet in _wallets)
            {
                Console.WriteLine(wallet);
            }
        }

        public List<string> GetAddresses()
        {
            var addressList = new List<string>();

            foreach (var wallet in _wallets)
            {
                addressList.Add(wallet.Address);
            }

            return addressList;
        }

        private List<WalletCore> Deserialize(byte[] fromBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(fromBytes, 0, fromBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            return (List<WalletCore>) binForm.Deserialize(memStream);
        }

        public IEnumerator<WalletCore> GetEnumerator()
        {
            foreach (var wallet in _wallets)
            {
                yield return wallet;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}