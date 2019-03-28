using ChainUtils;
using LightningDB;
using NLog;
using System;

namespace CoreLib
{
    public class PersistenceChain : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static string _dbName = "fricoin";
        //private const string DbEnv = "FricoinEnvironment";
        private readonly string DbEnv = "";
        private LightningEnvironment env;
        private bool isDisposed;


        public PersistenceChain(string environment)
        {

            DbEnv = environment.Replace(':', '_');
            env = new LightningEnvironment(DbEnv)
            {
                MaxDatabases = 2
            };
            env.Open();

            //create db
            using (var tx = env.BeginTransaction())
            using (tx.OpenDatabase(_dbName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            {
                tx.Commit();
            }

        }

        public void Dispose()
        {
            Dispose(true);

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed && disposing)
            {
                this.env.Dispose();

                isDisposed = true;
            }
        }

        ~PersistenceChain()
        {
            this.Dispose(true);
        }

        public void TestIterator()
        {

            using (var tx = env.BeginTransaction())

            using (var _db = tx.OpenDatabase(_dbName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))


            //assert

            using (var cur = tx.CreateCursor(_db))
            {
                while (cur.MoveNext())
                {
                    var key = ByteHelper.GetStringFromBytesASCI(cur.Current.Key);
                    var value = ByteHelper.GetStringFromBytesASCI(cur.Current.Value);

                    Console.WriteLine(key + " : " + value);

                }
            }

        }

        public byte[] Get(byte[] key)
        {


            using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly))
            using (var db = tx.OpenDatabase(_dbName))
            {
                var result = tx.Get(db, key);
                return result;
            }
        }

        public void Put(byte[] key, byte[] value)
        {

            using (var tx = env.BeginTransaction())
            using (var db = tx.OpenDatabase(_dbName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            {
                tx.Put(db, key, value);
                tx.Commit();
            }
        }

        public void Delete(byte[] key)
        {


            using (var tx = env.BeginTransaction())
            using (var db = tx.OpenDatabase(_dbName))
            {
                tx.Delete(db, key);
                tx.Commit();
            }
        }


    }
}