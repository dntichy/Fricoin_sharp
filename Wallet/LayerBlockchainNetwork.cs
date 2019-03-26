using ChainUtils;
using CoreLib.Blockchain;
using Engine.Network.MessageParser;
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
        public event EventHandler NewBlockAdded; //todo change this stupid name
        public event EventHandler<ProgressBarEventArgs> NewBlockArrived;
        private bool isBusy = false;
        private bool reindexing = false;
        private int highestIndex = 0;
        Queue<IMessage> IncomingMessages;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public static string GetIpAddress()
        {

            int listenPort = Convert.ToInt32(ConfigurationManager.AppSettings["ListenPort"]);

            //SET local IP
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            var localAddress = "undefined";

            for (int i = 0; i < hostEntry.AddressList.Length; ++i)
            {
                if (hostEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    localAddress = hostEntry.AddressList[i].ToString() + ":" + listenPort;
            }

            return localAddress;
        }

        public LayerBlockchainNetwork(BlockChain bchain)
        {
            logger.Debug("Initialized nettwork");
            chain = bchain;
            TransactionPool = new Dictionary<string, Transaction>();
            blocksInTransit = new List<byte[]>();
            IncomingMessages = new Queue<IMessage>();

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
                                Console.WriteLine("recieved block from " + rxMsg.Client.ToString());
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
                                logger.Debug("unknown command");
                                break;
                        }

                        break;
                    }

                case ((int)MessageType.RegisterMessage):
                    {
                        logger.Debug("Register message came, is bussy? " + isBusy);
                        if (isBusy)
                        {
                            IncomingMessages.Enqueue(msg);
                            return;
                        }

                        var rxMsg = msg as RegisterMessage;
                        SendVersion(rxMsg.Client.ToString(), chain);
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
                var block = chain.MineBlock(new List<Transaction>() { tx });
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
                logger.Debug("Sending tx over nettwork");

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
            logger.Debug("GetBlocks recieved");
            SendInv(message.Client.ToString(), "block", blocks);
        }

        private void HandleBlock(CommandMessage message)
        {
            var block = new Block().DeSerialize(message.Data);
            logger.Debug("Recieved new block from: " + message.Client.ToString());
            chain.AddBlock(block);
            logger.Debug("block added ID:" + Convert.ToBase64String(block.Hash) + "index: " + block.Index);

            if (highestIndex < block.Index) highestIndex = block.Index; //check best index
            NewBlockArrived?.Invoke(this, new ProgressBarEventArgs(highestIndex, block.Index)); //invoke this, mainly for progress bar now.



            if (blocksInTransit.Count > 0)
            {
                var blockHash = blocksInTransit[0];
                SendGetData(message.Client.ToString(), "block", blockHash);
                blocksInTransit.RemoveAt(0);
            }
            else
            {
                if (reindexing) return;
                reindexing = true;
                chain.ReindexUTXO(); // asynch
                NewBlockAdded?.Invoke(this, EventArgs.Empty); // invoke this after all is downloaded, cause downloading from lastest to oldest, could cause problems after displaying after each block 
                isBusy = false;
                reindexing = false;
                ProcessNextMessage();

            }
        }

        private void ProcessNextMessage()
        {
            if (IncomingMessages.Count > 0)
            {
                var nextMessage = IncomingMessages.Dequeue();
                ProcessMessage(nextMessage);
            }
        }

        private void HandleVersion(CommandMessage message)
        {
            var version = new Version().DeSerialize(message.Data);
            logger.Debug("Version recieved");
            var incomeBestheight = version.BestHeigth;
            var myBestHeight = chain.GetBestHeight();
            if (myBestHeight < incomeBestheight)
            {
                SendGetBlocks(message.Client.ToString());
                isBusy = true;

            }
            else if (myBestHeight > incomeBestheight)
            {
                SendVersion(message.Client.ToString(), chain);
            }

        }

        private void HandleInv(CommandMessage rxMsg)
        {
            var inventory = new Inv().DeSerialize(rxMsg.Data);

            logger.Debug("Inventory recieved: " + inventory.Kind);
            if (inventory.Kind == "block")
            {
                if (blocksInTransit.Count == 0)
                {
                    //blocksInTransit = ReduceBlocksInTransit(inventory.Items); //reduce blocksInTransit, don't wanna download whole chain again
                    blocksInTransit = (inventory.Items); //dont reduce blocksInTransit
                }
                if (blocksInTransit.Count == 0) return; //this means, blockInTransit was reduced totaly and nothing is needed

                var latestHashToTake = blocksInTransit[0];
                SendGetData(rxMsg.Client.ToString(), "block", latestHashToTake);
                blocksInTransit.RemoveAt(0);

            }
            else if (inventory.Kind == "tx")
            {
                var txId = inventory.Items[0];
                //request it only if I dont have it yet
                if (!TransactionPool.ContainsKey(HexadecimalEncoding.ToHexString(txId)))
                {
                    SendGetData(rxMsg.Client.ToString(), "tx", txId);
                }
            }


        }

        private List<byte[]> ReduceBlocksInTransit(List<byte[]> items)
        {
            var lastIndex = -1;
            for (var i = 0; i < items.Count; i++)
            {
                //check existence in chain for hashes obtained from nettwork
                var block = chain.GetBlock(items[i]);
                if (block != null)
                {
                    lastIndex = i;
                    break;
                }
            }
            items.RemoveRange(lastIndex, items.Count - lastIndex);
            return items;
        }

        private void HandleTransaction(CommandMessage message)
        {
            var tx = new Transaction().DeSerialize(message.Data);
            logger.Debug("Recieved new transaction");
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
            logger.Debug("Minig started");
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
                logger.Debug("All txs invalid");
                return;
            }

            var coinBaseTx = Transaction.CoinBaseTx(_blockchainNetwork.ClientDetails().ToString(), "");
            txList.Add(coinBaseTx);

            var newBlock = chain.MineBlock(txList);
            chain.ReindexUTXO();
            logger.Debug("new block mined");

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
        [Serializable]
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

        [Serializable]
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
        [Serializable]
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


        public void SendBlock(string address, Block block)
        {
            var message = new CommandMessage();
            message.Command = CommandType.Block;
            message.Client = _blockchainNetwork.ClientDetails();
            message.Data = block.Serialize();
            logger.Debug("send block index: " + block.Index);
            _blockchainNetwork.SendMessageToAddressAsync(message, address);
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
            _blockchainNetwork.SendMessageToAddressAsync(message, address);
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