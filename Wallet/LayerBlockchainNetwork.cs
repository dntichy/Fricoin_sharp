using ChainUtils;
using CoreLib.Blockchain;
using DatabaseLib;
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
using System.Threading;

namespace Wallet
{
    class LayerBlockchainNetwork
    {
        public BlockchainNetwork _blockchainNetwork;
        public Dictionary<string, Transaction> TransactionPool;
        private BlockChain chain;
        private List<byte[]> blocksInTransit;
        public event EventHandler WholeChainDownloaded;
        public event EventHandler BlockChainSynchronized; // remove, is unused?
        public event EventHandler BlockChainSynchronizing;
        public event EventHandler<TransactionPoolEventArgs> TransactionPoolChanged;
        public event EventHandler<ProgressBarEventArgs> NewBlockArrived;
        public event EventHandler<MinedHashUpdateEventArgs> MinedHashUpdate;
        private bool isBusy = false; //is currently downloading chain from other peers
        private bool reindexing = false;
        readonly User _loggedUser;
        private int highestIndex = 0;
        private int reducedBlockCount = 0;
        Queue<IMessage> IncomingMessages;
        List<string> BlockOnTheFly = new List<string>();
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private bool miningInProgress;
        private int numberOfTransactionsToStartMining = 1;
        List<InMemoryBlockChain> InMemoryBlockChains = new List<InMemoryBlockChain>(); //local fork chains
        Queue<IMessage> IncomingMinedNewBlocksMessages;
        private bool handlingNewBlock = false; //is currently processing mined block

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

        public LayerBlockchainNetwork(BlockChain bchain, User _loggedUser)
        {
            logger.Debug("Initialized nettwork");
            chain = bchain;
            TransactionPool = new Dictionary<string, Transaction>();
            blocksInTransit = new List<byte[]>();
            IncomingMessages = new Queue<IMessage>();
            IncomingMinedNewBlocksMessages = new Queue<IMessage>();
            this._loggedUser = _loggedUser;

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

                            case CommandType.Transaction:
                                HandleTransaction(rxMsg);
                                break;
                            case CommandType.Block:
                                if (reindexing)
                                {
                                    //if reorganizing database, enquee, and return. When Reindexing is done, The message will be processed
                                    IncomingMessages.Enqueue(msg);
                                    logger.Debug("Message Block ENQUEED");
                                    return;
                                }
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
                            case CommandType.NewBlockMined:
                                if (handlingNewBlock)
                                {
                                    IncomingMinedNewBlocksMessages.Enqueue(rxMsg);
                                    logger.Debug("NewBlockMined message enqueed");
                                    return;
                                }
                                HandleNewBlockMined(rxMsg);
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
                            logger.Debug("Message RegisterMessage ENQUEED");
                            return;
                        }

                        var rxMsg = msg as RegisterMessage;
                        SendVersion(rxMsg.Client.ToString(), chain);
                        break;
                    }
            }
        }

        private void HandleNewBlockMined(CommandMessage rxMsg)
        {

            try
            {
                handlingNewBlock = true;
                PauseMining(); //pause mining if mining

                Block minedBlock = new Block().DeSerialize(rxMsg.Data);
                var transactions = minedBlock.Transactions;
                logger.Debug("Recieved block, somebody mined: " + Convert.ToBase64String(minedBlock.Hash));

                var isValid = IsPreviousHashValid(minedBlock.PreviousHash);

                if (!isValid)
                {
                    logger.Debug("Previous hash invalid, try add to fork subchain");
                    if (!VerifyTransactions(transactions))
                    {
                        logger.Debug("Transactions  not valid");
                        ResumeMining();
                        handlingNewBlock = false;
                        return;
                    }

                    //try to add to fork chains
                    var added = false;
                    foreach (var memChain in InMemoryBlockChains)
                    {
                        if (memChain.BelongsToThisChain(minedBlock))
                        {
                            added = memChain.AddBlock(minedBlock);
                            logger.Debug("Added to subchain");
                        }
                    }

                    if (!added)
                    {
                        var newForkChain = new InMemoryBlockChain();
                        newForkChain.AddBlock(minedBlock);
                        InMemoryBlockChains.Add(newForkChain);
                        logger.Debug("Created new subchain");
                    }
                    PrintCurrentForkChains(InMemoryBlockChains);
                }
                else
                {
                    logger.Debug("Previous hash VALID, add to local chain");

                    if (!VerifyTransactions(transactions))
                    {
                        ResumeMining();
                        handlingNewBlock = false;
                        return;
                    }

                    RemoveTransactionsFromPool(minedBlock.Transactions); //remove new block transactions from pool
                    chain.AddBlock(minedBlock);
                    BreakMining();
                    chain.ReindexUTXO(); // asynch
                    
                }

                if (InMemoryBlockChains.Count > 0)
                {

                    logger.Debug("Find best chain");
                    InMemoryBlockChain bestChain = null;
                    var count = 1;
                    var bestIndex = chain.GetBestHeight();

                    foreach (var chain in InMemoryBlockChains)
                    {
                        var currentIndex = chain.GetLastIndex();

                        if (currentIndex > bestIndex)
                        {
                            bestIndex = currentIndex;
                            count = 1;
                            bestChain = chain;
                        }
                        else if (currentIndex == bestIndex)
                        {
                            count++;
                        }
                    }
                    if (count == 1)
                    {
                        logger.Debug("Best chain found");
                        BreakMining();

                        if (bestChain != null)
                        {
                            logger.Debug("Best chain found in subchains, restructualize chain");
                            //if not restruct to new one
                            chain.RestructualizeSubchain(bestChain);

                        }
                        else
                        {
                            //if bestChain is null, chain stays local
                            logger.Debug("Best chain found in localchain, do nothing");
                        }
                        InMemoryBlockChains.Clear(); //clear InMemoryChains
                    }
                    else
                    {
                        logger.Debug("Best chain not found");
                    }
                }

                WholeChainDownloaded?.Invoke(this, EventArgs.Empty);
                handlingNewBlock = false;
                if (IncomingMinedNewBlocksMessages.Count > 0) ProcessMessage(IncomingMinedNewBlocksMessages.Dequeue());

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                logger.Error(e.StackTrace);
            }
        }

        private void RemoveTransactionsFromPool(IList<Transaction> transactions)
        {
            foreach (var tx in transactions)
            {
                TransactionPool.Remove(HexadecimalEncoding.ToHexString(tx.Id));
            }
        }

        private void PrintCurrentForkChains(List<InMemoryBlockChain> inMemoryBlockChains)
        {
            foreach (var chain in inMemoryBlockChains)
            {
                logger.Debug(chain);
            }
        }
        private void ResumeMining()
        {
            if (miningInProgress) chain.ActuallBlockInMining.Mining = true;
        }

        private void PauseMining()
        {
            if (miningInProgress) chain.ActuallBlockInMining.Mining = false;
        }
        private void BreakMining()
        {
            if (miningInProgress) chain.ActuallBlockInMining.BreakMining = true; //break mining
        }

        private bool VerifyTransactions(IList<Transaction> transactions)
        {
            var isValid = true;
            //verification
            foreach (var tx in transactions)
            {
                if (!chain.VerifyTransaction(tx))
                {
                    isValid = false;
                };
            }
            return isValid;
        }

        private bool IsPreviousHashValid(byte[] previousHash)
        {
            return ArrayHelpers.ByteArrayCompare(chain.GetLatestBlock().Hash, previousHash);
        }

        public void SendNewBlockMined(Block newBlock)
        {
            var addressesToExclude = new string[] { _blockchainNetwork.ClientDetails().ToString() };
            var msg = new CommandMessage();
            msg.Command = CommandType.NewBlockMined;
            msg.Client = _blockchainNetwork.ClientDetails();
            msg.Data = newBlock.Serialize();

            _blockchainNetwork.BroadcastMessageAsyncExceptAddress(addressesToExclude, msg);

        }

        public void Send(string from, string to, int amount)
        {
            var utxoSet = new UTXOSet(chain);

            var tx = Transaction.NewTransaction(from, to, amount, utxoSet);
            if (tx == null)
            {
                logger.Debug("Couldn't create new TX to send");
                return;
            }
            //old ver
            //var block = chain.MineBlock(new List<Transaction>() { tx });
            //utxoSet.Update(block);
            //WholeChainDownloaded?.Invoke(this, EventArgs.Empty);
            //chain.ReindexUTXO();


            //check if not referencing same output, TODO BUG FIX
            //var referencingSameOutput = false;
            //foreach (var transaction in TransactionPool)
            //{
            //    var txOutputs = tx.Outputs;
            //    var poolOutputs = transaction.Value.Outputs;

            //    foreach (var outp in poolOutputs)
            //    {
            //        foreach (var txOutp in txOutputs)
            //        {
            //            if (outp.Equals(txOutp))
            //            {
            //                referencingSameOutput = true;
            //                break;
            //            }
            //        }
            //        if (referencingSameOutput) break;
            //    }
            //    if (referencingSameOutput) break;
            //}

            //if (referencingSameOutput)
            //{
            //    logger.Debug("Referencing same output in tx. Tx not added to TPool");
            //    return;
            //}

            //add to pool
            TransactionPool.Add(HexadecimalEncoding.ToHexString(tx.Id), tx);
            TransactionPoolChanged?.Invoke(this, new TransactionPoolEventArgs(new List<string>(TransactionPool.Keys)));

            //send to all except me and the one who send the tx
            var addressesToExclude = new string[] { _blockchainNetwork.ClientDetails().ToString() };
            var msg = new CommandMessage();
            msg.Command = CommandType.Transaction;
            msg.Client = _blockchainNetwork.ClientDetails();
            msg.Data = tx.Serialize();

            _blockchainNetwork.BroadcastMessageAsyncExceptAddress(addressesToExclude, msg);
            logger.Debug("Sending tx over nettwork");

            if (!miningInProgress)
            {
                if (TransactionPool.Count >= numberOfTransactionsToStartMining)
                {
                    var thread = new Thread(MineTransactions);
                    thread.Start();
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
            logger.Debug("GetBlocks recieved");
            SendInv(message.Client.ToString(), "block", blocks);
        }

        private void HandleBlock(CommandMessage message)
        {
            var block = new Block().DeSerialize(message.Data);
            logger.Debug("Recieved new block from: " + message.Client.ToString());
            chain.AddBlock(block);
            logger.Debug("block added ID:" + Convert.ToBase64String(block.Hash) + "index: " + block.Index);
            BlockOnTheFly.Remove(HexadecimalEncoding.ToHexString(block.Hash));

            //--progress bar
            if (highestIndex < block.Index) highestIndex = block.Index; //check best index
            NewBlockArrived?.Invoke(this, new ProgressBarEventArgs(highestIndex, blocksInTransit.Count, reducedBlockCount)); //invoke this, mainly for progress bar now.
            //--progress bar end

            if (blocksInTransit.Count > 0)
            {
                var blockHash = blocksInTransit[0];
                SendGetData(message.Client.ToString(), "block", blockHash);
                BlockOnTheFly.Add(HexadecimalEncoding.ToHexString(blockHash));
                blocksInTransit.RemoveAt(0);
            }
            else
            {
                if (BlockOnTheFly.Count != 0) return;
                reindexing = true;
                chain.ReindexUTXO(); // asynch
                WholeChainDownloaded?.Invoke(this, EventArgs.Empty); // invoke this after all is downloaded, cause downloading from lastest to oldest, could cause problems after displaying after each block 
                isBusy = false;
                reindexing = false;
                ProcessNextMessage();

            }
        }

        public void ProcessNextMessage()
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
                BlockChainSynchronizing?.Invoke(this, EventArgs.Empty);
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
                    blocksInTransit = ReduceBlocksInTransit(inventory.Items); //reduce blocksInTransit, don't wanna download whole chain again
                                                                              //blocksInTransit = (inventory.Items); //dont reduce blocksInTransit - download whole chain
                    logger.Debug("Reduced blocks, to download : " + blocksInTransit.Count);
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
            reducedBlockCount = items.Count - lastIndex; //how many block are to be downloaded
            items.RemoveRange(lastIndex, items.Count - lastIndex);

            return items;
        }

        private void HandleTransaction(CommandMessage message)
        {

            var tx = new Transaction().DeSerialize(message.Data);
            logger.Debug("New Transaction recieved : " + HexadecimalEncoding.ToHexString(tx.Id));

            //add to pool
            TransactionPool.Add(HexadecimalEncoding.ToHexString(tx.Id), tx);
            TransactionPoolChanged?.Invoke(this, new TransactionPoolEventArgs(new List<string>(TransactionPool.Keys)));

            if (!miningInProgress)
            {
                if (TransactionPool.Count >= numberOfTransactionsToStartMining)
                {
                    //Mine transaction
                    var thread = new Thread(MineTransactions);
                    thread.Start();
                }
            }
        }

        private void MineTransactions()
        {
            miningInProgress = true;
            logger.Debug("Mining started");
            var startTime = DateTime.Now;

            var txList = new List<Transaction>();
            var coinBaseTx = Transaction.CoinBaseTx(_loggedUser.Address, ""); //add later
            txList.Add(coinBaseTx);
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


            chain.HashDiscovered += HashDiscovered;
            var newBlock = chain.MineBlock(txList);

            if (newBlock != null)
            {
                //this means pow was broke 
                chain.ReindexUTXO();
                logger.Debug($"New block mined, mining duration: {DateTime.Now - startTime}");
                chain.HashDiscovered -= HashDiscovered;
                WholeChainDownloaded?.Invoke(this, EventArgs.Empty);
                SendNewBlockMined(newBlock);
                //remove txs from Transaction pool

                RemoveTransactionsFromPool(txList);
            }
            else
            {
                logger.Debug("block added by remote client");
                //I delete txs from txpool in NewBlockMined
            }

            miningInProgress = false;

            if (TransactionPool.Count > 0) MineTransactions();

        }

        private void HashDiscovered(object sender, MinedHashUpdateEventArgs e)
        {
            MinedHashUpdate?.Invoke(this, new MinedHashUpdateEventArgs(e.Hash));
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