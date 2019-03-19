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
using System.Runtime.Serialization.Formatters.Binary;

namespace Wallet
{
    class LayerBlockchainNetwork
    {
        public BlockchainNetwork _blockchainNetwork;
        public Dictionary<string, Transaction> TransactionPool;
        private BlockChain chain;

        public LayerBlockchainNetwork(BlockChain bchain)
        {
            chain = bchain;
            TransactionPool = new Dictionary<string, Transaction>();

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
                            case CommandType.GetData:
                                HandleGetData(rxMsg);
                                break;
                            default:
                                Console.WriteLine("unknown command");
                                break;
                        }

                        break;
                    }
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

        private void HandleTransaction(CommandMessage message)
        {
            var tx = new Transaction().DeSerialize(message.Data);
            Console.WriteLine("Recieved new transaction");
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
            public byte[][] Items { get; set; }
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


        public void SendInv(string address, string kind, byte[][] items)
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
        public void SendVersion(BlockChain bchain)
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

            _blockchainNetwork.BroadcastMessageAsync(message);
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