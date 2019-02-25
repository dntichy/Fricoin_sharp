using Engine.Network.MessageParser;
using P2PLib.Network.Components.Enums;
using P2PLib.Network.Components.Interfaces;
using P2PLib.Network.MessageParser;
using P2PLib.Network.MessageParser.Messages;
using System;

namespace Engine
{
    class Program
    {
        private static ApplicationInputState mCurrentState;

        public enum ApplicationInputState
        {
            EmptyInput,
            TextInput,
            ImageInput,
            StrokeInput
        }

        static void Main(string[] args)
        {
            //var startTime = DateTime.Now;

            //BlockChain friCoin = new BlockChain();

            //friCoin.CreateTransaction(new Transaction("henry", "jozo", 20));
            //friCoin.ProcessPendingTransactions("dusan");
            //friCoin.CreateTransaction(new Transaction("janz", "lenka", 30));
            //friCoin.ProcessPendingTransactions("dusan");
            //Console.WriteLine(JsonConvert.SerializeObject(friCoin, Newtonsoft.Json.Formatting.Indented));
            //var endTime = DateTime.Now;

            //Console.WriteLine($"Duration: {endTime - startTime}");


            //http://csharptest.net/projects/bplustree/

            // Create the OptionsV2 class to specify construction arguments:
            //BPlusTree<string, int>.OptionsV2 options = new BPlusTree<string, int>.OptionsV2(
            //        PrimitiveSerializer.String,
            //        PrimitiveSerializer.Int32,
            //        StringComparer.Ordinal)
            //    {
            //        CreateFile = CreatePolicy.IfNeeded,
            //        FileName = @"Storage.dat",
            //        StoragePerformance = StoragePerformance.CommitToDisk,
            //        TransactionLogLimit = 100 * 1024 * 1024, // 100 MB
            //    }
            //    .CalcBTreeOrder(10, 16);

            //// Create the BPlusTree using these options
            //using (var map = new BPlusTree<string, Int32>(options))
            //{
            //    map.EnableCount();
            //    map.TryAdd("xxzxx", 212341241);

            //    if (!map.ContainsKey("foo")) Console.WriteLine("Failed to find foo");


            //    StreamWriter Tex = new StreamWriter("x.txt");
            //    Tex.AutoFlush = true;
            //    map.Print(Tex, BPlusTree<string, Int32>.DebugFormat.Full);

            //    Console.WriteLine(map.Count);
            //}


            //start nettwork instance - client
            int listenPort = 46807;
            String server = "172.25.96.1";
            int serverListenPort = 46800;
            var mCollaborativeNotes = new CollaborativeNotesClass(listenPort, serverListenPort, server, "group11");

            mCollaborativeNotes.OnReceiveMessage += new OnReceiveMessageDelegate(OnReceivePeerMessage);

            mCurrentState = ApplicationInputState.EmptyInput;

            Console.Read();
        }

        private static void UpdateCurrentView(IMessage msg)
        {
            switch (msg.Type)
            {
                case ((int) MessageType.TextDataMessage):
                {
                    TextMessage rxMsg = (TextMessage) msg;
                    Console.WriteLine(rxMsg.Text);
                    break;
                }
            }
        }

        private static void OnReceivePeerMessage(object sender, CollaborativeNotesReceiveMessageEventArgs e)
        {
            UpdateCurrentView(e.Message);
        }
    }
}