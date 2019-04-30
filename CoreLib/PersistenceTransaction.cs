using ChainUtils;
using LightningDB;
using System;
using System.Collections.Generic;

namespace CoreLib
{
    //todo inheritance with Persistence chain, if there is time 
    public class PersistenceTransaction : IDisposable
    {
        private const string DbName = "fricoinTransactions";
        //private const string DbEnv = "FricoinEnvironment";
        private readonly string DbEnv = "";
        private LightningEnvironment env;
        private bool isDisposed;


        public PersistenceTransaction(string environment)
        {
            DbEnv = environment.Replace(':', '_') ;

            env = new LightningEnvironment(DbEnv)
            {
                MaxDatabases = 2,
            };
            env.MapSize = 1024 * 1024 * 1000;
            env.Open();

            //create db
            using (var tx = env.BeginTransaction())
            using (tx.OpenDatabase(DbName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
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

        ~PersistenceTransaction()
        {
            this.Dispose(true);
        }
        public void TestIterator()
        {
            using (var env = new LightningEnvironment(DbEnv))
            {
                env.MaxDatabases = 2;
                env.Open();

                using (var tx = env.BeginTransaction())

                using (var _db = tx.OpenDatabase(DbName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))


                //assert

                using (var cur = tx.CreateCursor(_db))
                {
                    while (cur.MoveNext())
                    {
                        var key = ByteHelper.GetStringFromBytesASCI(cur.Current.Key);
                        var value = ByteHelper.GetStringFromBytesASCI(cur.Current.Value);

                        Console.WriteLine(key);

                    }

                }
            }
        }

        public byte[] Get(byte[] key)
        {

            using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly))
            using (var db = tx.OpenDatabase(DbName))
            {
                var result = tx.Get(db, key);
                tx.Commit();
                return result;
            }
        }

        public void Put(byte[] key, byte[] value)
        {



            using (var tx = env.BeginTransaction())
            using (var db = tx.OpenDatabase(DbName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            {
                tx.Put(db, key, value);
                tx.Commit();
            }
        }

        public void DeleteKeys(List<byte[]> keysToDelete)
        {
            foreach (var key in keysToDelete)
            {
                Delete(key);
            }
        }

        public IEnumerable<KeyValuePair<byte[], byte[]>> Cursor()
        {


            using (var tx = env.BeginTransaction())

            using (var _db = tx.OpenDatabase(DbName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))

            using (var cur = tx.CreateCursor(_db))
            {
                while (cur.MoveNext())
                {
                    yield return cur.Current;
                }
            }
        }

        public void Delete(byte[] key)
        {


            using (var tx = env.BeginTransaction())
            using (var db = tx.OpenDatabase(DbName))
            {
                tx.Delete(db, key);
                tx.Commit();
            }
        }


    }
}