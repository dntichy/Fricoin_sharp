using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CoreLib;
using LevelDB;


namespace Wallet.Pages
{
    /// <summary>
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : Page
    {
        public Loading()
        {
            InitializeComponent();
            Loaded += Loading_OnLoaded;
        }

        private void Loading_OnLoaded(object sender, RoutedEventArgs e)
        {
            BlockChain b = new BlockChain();

            var genesis = b.GetLatestBlock();
            var bytes = genesis.Serialize();
            var deser = new Block();
            deser = deser.DeSerialize(bytes);
            Console.WriteLine(deser);

            //var options = new Options { CreateIfMissing = true };
            //var db = new DB(options, @"C:\temp\tempdb");
            //var writeOptions = new WriteOptions { Sync = true };
            //db.Put("New York", "blue");


            //using (var it = db.CreateIterator())
            //{
            //    // Iterate in reverse to print the values as strings
            //    for (it.SeekToLast(); it.IsValid(); it.Prev())
            //    {
            //        Console.WriteLine("Value as string: {0}", it.ValueAsString());
            //    }
            //}
            //Console.WriteLine("NEXT-----------");

            //var keys =
            //    from kv in db as IEnumerable<KeyValuePair<string, string>>
            //    select kv.Key;

            //foreach (var key in keys)
            //{
            //    Console.WriteLine("Key: {0}", key);
            //}
            //Console.WriteLine("NEXT-----------");


            //using (var it = db.CreateIterator())
            //{
            //    // Iterate in reverse to print the values as strings
            //    for (it.SeekToLast(); it.IsValid(); it.Prev())
            //    {
            //        Console.WriteLine("Value as string: {0}", it.ValueAsString());
            //    }
            //}


            //if (File.Exists("pub.dat") && File.Exists("pri.dat"))
            //{
            //    var pub = File.ReadAllBytes("pub.dat");
            //    var pri = File.ReadAllBytes("pri.dat");

            //    var hash = Crypto.SignTransaction(Encoding.UTF8.GetBytes("test"), pri);
            //    var isOk = Crypto.VerifyHashed(hash, pub, Encoding.UTF8.GetBytes("test"));
            //    if (isOk)
            //    {
            //        Console.WriteLine("Registered");
            //        // go to Login 
            //        NavigationService?.Navigate(new Login());
            //    }
            //}
            //else
            //{
            //    Console.WriteLine("Not registered");
            //    // continue presenting register form
            //    NavigationService?.Navigate(new Registration());
            //}
        }
    }
}