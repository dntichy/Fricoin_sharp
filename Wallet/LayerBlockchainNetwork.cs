using ChainUtils;
using CoreLib.Blockchain;
using P2PLib.Network;
using P2PLib.Network.Components;
using P2PLib.Network.Components.Enums;
using P2PLib.Network.Components.Interfaces;
using P2PLib.Network.MessageParser;
using P2PLib.Network.MessageParser.Messages;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Wallet
{
    class LayerBlockchainNetwork
    {
        public BlockchainNetwork _blockchainNetwork;
        public Dictionary<string, Transaction> TransactionPool;
        private BlockChain chain;
        private List<byte[]> blocksInTransit;


        public static string GetIpAddress() {

            int listenPort = Convert.ToInt32(ConfigurationManager.AppSettings["ListenPort"]);

            //SET local IP
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            var localAddress = "undefined";

            for (int i = 0; i < hostEntry.AddressList.Length; ++i)
            {
                if (hostEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    localAddress = hostEntry.AddressList[i].ToString() +":"+ listenPort;
            }

            return localAddress;
        }

        public LayerBlockchainNetwork(BlockChain bchain)
        {
            chain = bchain;
            TransactionPool = new Dictionary<string, Transaction>();
            blocksInTransit = new List<byte[]>();

            //P2P CONFIGURATIONS
            int listenPort = Convert.ToInt32(ConfigurationManager.AppSettings["ListenPort"]);
            String server = ConfigurationManager.AppSettings["Server"];
            int serverListenPort = Convert.ToInt32(ConfigurationManager.AppSettings["ServerListenPort"]);

            //CREATE BLOCKCHAIN OBJECT
            _blockchainNetwork = new BlockchainNetwork(listenPort, serverListenPort, server, "group11");

            //REGISTER recievers
            _blockchainNetwork.OnReceiveMessage += new OnReceiveMessageEvent(OnReceivePeerMessage);
            //TODO transfer events here from wallet.cs?

        }


        //MAIN PROCESSING MESSAGE LOGIC
        private void ProcessMessage(IMessage msg)
        {
            switch (msg.Type)
            {
                case ((int)MessageType.CommandMessage):
                    {
                        var rxMsg = msg as CommandMessage;

                        switch (rxMsg.Command)
                        {
                            case CommandType.ClearTransactionPool:
                                break;
                            case CommandType.Transaction:
                                HandleTransaction(rxMsg);
                                break;
                            case CommandType.Block:
                                HandleBlock(rxMsg);
                                break;
                            case CommandType.GetBlocks:
                                HandleGetBlocks(rxMsg);
                                break;
                            case CommandType.Version:
                                HandleVersion(rxMsg);
                                break;
                            case CommandType.GetData:
                                HandleGetData(rxMsg);
                                break;
                            case CommandType.Inv:
                                HandleInv(rxMsg);
                                break;
                            default:
                                Console.WriteLine("unknown command");
                                break;
                        }

                        break;
                    }
            }
        }
        public void Send(string from, string to, int amount, bool miningInProgress)
        {
            var utxoSet = new UTXOSet(chain);

            var tx = Transaction.NewTransaction(from, to, amount, utxoSet);
            if (tx == null)
            {
                return;
            }

            if (miningInProgress)
            {
                //var coinbaseTx = Transaction.CoinBaseTx(from, "");
                var block = chain.MineBlock(new List<Transaction>() {  tx });
                utxoSet.Update(block);
            }
            else
            {

                //send  to all except me and the one who send the tx
                var addressesToExclude = new string[] { _blockchainNetwork.ClientDetails().ToString() };
                var msg = new CommandMessage();
                msg.Command = CommandType.Transaction;
                msg.Client = _blockchainNetwork.ClientDetails();
                msg.Data = tx.Serialize();

                _blockchainNetwork.BroadcastMessageAsyncExceptAddress(addressesToExclude, msg);
                Console.WriteLine("Sending tx over nettwork");
            }


        }

        private void HandleGetData(CommandMessage message)
        {
            var data = new Data().DeSerialize(message.Data);

            if (data.Kind == "block")
            {
                var block = chain.GetBlock(data.Id);
                SendBlock(message.Client.ToString(), block);
            };
            if (data.Kind == "tx")
            {
                var txId = HexadecimalEncoding.ToHexString(data.Id);
                var transaction = TransactionPool[txId];
                SendTx(message.Client.ToString(), transaction);
            };

        }

        private void HandleGetBlocks(CommandMessage message)
        {
            var blocks = chain.GetBlockHashes();
            Console.WriteLine("GetBlocks recieved");
            SendInv(message.Client.ToString(), "block", blocks);
        }

        private void HandleBlock(CommandMessage message)
        {
            var blocksInTransmit = new List<Block>();

            var block = new Block().DeSerialize(message.Data);
            Console.WriteLine("Recieved new block");
            chain.AddBlock(block);
            Console.WriteLine("block added " + block.Hash);

            if (blocksInTransmit.Count > 0)
            {
                var blockHash = blocksInTransmit[0].Hash;
                SendGetData(message.Client.ToString(), "block", blockHash);
                blocksInTransmit.RemoveAt(0);
            }
            else
            {
                chain.ReindexUTXO();
            }


        }

        private void HandleVersion(CommandMessage message)
        {
            var version = new Version().DeSerialize(message.Data);
            Console.WriteLine("Version recieved");
            var incomeBestheight = version.BestHeigth;
            var myBestHeight = chain.GetBestHeight();
            if (myBestHeight < incomeBestheight)
            {
                SendGetBlocks(message.Client.ToString());

            }
            else if (myBestHeight > incomeBestheight)
            {
                SendVersion(message.Client.ToString(), chain);
            }

        }

        private void HandleInv(CommandMessage rxMsg)
        {
            var inventory = new Inv().DeSerialize(rxMsg.Data);
            Console.WriteLine("Inventory recieved: " + inventory.Kind);

            if (inventory.Kind == "block")
            {
                blocksInTransit = inventory.Items; //should be hashes of blocks
                var bHash = new Block().DeSerialize(blocksInTransit[0]).Hash;

                SendGetData(rxMsg.Client.ToString(), "block", bHash);

                var newInTransit = new List<byte[]>();


                foreach (var bl in blocksInTransit)
                {
                    if (!ArrayHelpers.ByteArrayCompare(bl, bHash))
                    {
                        newInTransit.Add(bl);
                    }
                }
                blocksInTransit = newInTransit;

            }
            else if (inventory.Kind == "tx")
            {
                var txId = inventory.Items[0];

                if (!TransactionPool.ContainsKey(HexadecimalEncoding.ToHexString(txId)))
                {
                    SendGetData(rxMsg.Client.ToString(), "tx", txId);
                }
            }


        }
        private void HandleTransaction(CommandMessage message)
        {
            var tx = new Transaction().DeSerialize(message.Data);
            Console.WriteLine("Recieved new transaction");
            TransactionPool.Add(HexadecimalEncoding.ToHexString(tx.Id), tx);

            //Mine transaction
            MineTransactions();

            //send inv to all except me and the one who send the tx
            var addressesToExclude = new string[] {
                message.Client.ToString(),
                _blockchainNetwork.ClientDetails().ToString()
            };

            var inv = new Inv() { Items = new List<byte[]>() { tx.Id }, Kind = "tx" };
            var msg = new CommandMessage();
            msg.Command = CommandType.Inv;
            msg.Client = _blockchainNetwork.ClientDetails();
            msg.Data = inv.Serialize();

            _blockchainNetwork.BroadcastMessageAsyncExceptAddress(addressesToExclude, msg);
        }

        private void MineTransactions()
        {
            Console.WriteLine("Minig started");
            var txList = new List<Transaction>();

            //fill txList from TransactionPool
            foreach (var txPair in TransactionPool)
            {
                var tx = txPair.Value;
                if (chain.VerifyTransaction(tx))
                {
                    txList.Add(tx);
                }
            }

            if (txList.Count == 0)
            {
                Console.WriteLine("All txs invalid");
                return;
            }

            var coinBaseTx = Transaction.CoinBaseTx(_blockchainNetwork.ClientDetails().ToString(), "");
            txList.Add(coinBaseTx);

            var newBlock = chain.MineBlock(txList);
            chain.ReindexUTXO();
            Console.WriteLine("new block mined");

            foreach (var tx in txList)
            {
                TransactionPool.Remove(HexadecimalEncoding.ToHexString(tx.Id));
            }

            var inv = new Inv()
            {
                Items = new List<byte[]> { newBlock.Hash },
                Kind = "block"
            };

            var msg = new CommandMessage();
            msg.Command = CommandType.Inv;
            msg.Client = _blockchainNetwork.ClientDetails();
            msg.Data = inv.Serialize();
            //send inv to all except me and the one who send the tx
            var addressesToExclude = new string[] {
                _blockchainNetwork.ClientDetails().ToString()
            };
            _blockchainNetwork.BroadcastMessageAsyncExceptAddress(addressesToExclude, msg);

            if (TransactionPool.Count > 0)
            {
                MineTransactions();
            }

        }



        //METHODS, SENDING MESSAGES

        public class Version
        {
            public int VersionCode { get; set; }
            public int BestHeigth { get; set; }

            public byte[] Serialize()
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, this);
                return ms.ToArray();
            }

            public Version DeSerialize(byte[] fromBytes)
            {
                MemoryStream memStream = new MemoryStream();
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(fromBytes, 0, fromBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return binForm.Deserialize(memStream) as Version;
            }
        }
        public class Inv
        {
            public List<byte[]> Items { get; set; }
            public string Kind { get; set; }

            public byte[] Serialize()
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, this);
                return ms.ToArray();
            }

            public Inv DeSerialize(byte[] fromBytes)
            {
                MemoryStream memStream = new MemoryStream();
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(fromBytes, 0, fromBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return binForm.Deserialize(memStream) as Inv;
            }
        }
        public class Data
        {
            public byte[] Id { get; set; }
            public string Kind { get; set; }

            public byte[] Serialize()
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, this);
                return ms.ToArray();
            }

            public Data DeSerialize(byte[] fromBytes)
            {
                MemoryStream memStream = new MemoryStream();
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(fromBytes, 0, fromBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return binForm.Deserialize(memStream) as Data;
            }
        }

        public void SendBlock(string address, Block block)
        {
            var message = new CommandMessage();
            message.Command = CommandType.Block;
            message.Client = _blockchainNetwork.ClientDetails();
            message.Data = block.Serialize();

            _blockchainNetwork.BroadcastMessageAsync(message);
        }


        public void SendInv(string address, string kind, List<byte[]> items)
        {
            var message = new CommandMessage();
            message.Command = CommandType.Inv;
            message.Client = _blockchainNetwork.ClientDetails();
            var inv = new Inv()
            {
                Items = items,
                Kind = kind
            };
            message.Data = inv.Serialize();
            _blockchainNetwork.BroadcastMessageAsync(message);
        }


        public void SendTx(string address, Transaction tx)
        {
            var message = new CommandMessage();
            message.Command = CommandType.Transaction;
            message.Client = _blockchainNetwork.ClientDetails();
            message.Data = tx.Serialize();

            _blockchainNetwork.BroadcastMessageAsync(message);
        }
        public void SendVersion(string address, BlockChain bchain)
        {
            var message = new CommandMessage();
            message.Command = CommandType.Version;
            message.Client = _blockchainNetwork.ClientDetails();
            var bestHeigth = bchain.GetBestHeight();
            message.Data = new Version()
            {
                BestHeigth = bestHeigth,
                VersionCode = 1
            }.Serialize();

            _blockchainNetwork.SendMessageToAddressAsync(message, address);
        }


        public void SendGetBlocks(string address)
        {
            var message = new CommandMessage();
            message.Command = CommandType.GetBlocks;
            message.Client = _blockchainNetwork.ClientDetails();

            _blockchainNetwork.SendMessageToAddressAsync(message, address);
        }


        public void SendGetData(string address, string kind, byte[] id)
        {
            var message = new CommandMessage();
            message.Command = CommandType.GetData;
            message.Client = _blockchainNetwork.ClientDetails();

            var dt = new Data()
            {
                Id = id,
                Kind = kind
            };
            message.Data = dt.Serialize();

            _blockchainNetwork.SendMessageToAddressAsync(message, address);
        }
        private void OnReceivePeerMessage(object sender, ReceiveMessageEventArgs e)
        {
            ProcessMessage(e.Message);
        }
    }
}