﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChainUtils;
using LightningDB;

namespace CoreLib
{
    public class PersistenceChain
    {
        private static string _dbName = "fricoin";
        private const string DbEnv = "FricoinEnvironment";

        public PersistenceChain()
        {
            using (var env = new LightningEnvironment(DbEnv))
            {
                env.MaxDatabases = 2;
                env.Open();

                //create db
                using (var tx = env.BeginTransaction())
                using (tx.OpenDatabase(_dbName, new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create}))
                {
                    tx.Commit();
                }
            }
        }


        public void TestIterator()
        {
            using (var env = new LightningEnvironment(DbEnv))
            {
                env.MaxDatabases = 2;
                env.Open();

                using (var tx = env.BeginTransaction())

                using (var _db = tx.OpenDatabase(_dbName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))


                //assert

                using (var cur = tx.CreateCursor(_db))
                {
                    while (cur.MoveNext())
                    {
                        var key = ByteHelper.GetStringFromBytesASCI(cur.Current.Key);
                        var value = ByteHelper.GetStringFromBytesASCI(cur.Current.Value);

                        Console.WriteLine(key +" : "+ value);
                      
                    }

                }
            }
        }

        public byte[] Get(byte[] key)
        {
            using (var env = new LightningEnvironment(DbEnv))
            {
                env.MaxDatabases = 2;
                env.Open();

                using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly))
                using (var db = tx.OpenDatabase(_dbName))
                {
                    var result = tx.Get(db, key);
                    return result;
                }
            }
        }

        public void Put(byte[] key, byte[] value)
        {
            using (var env = new LightningEnvironment(DbEnv))
            {
                env.MaxDatabases = 2;
                env.Open();


                using (var tx = env.BeginTransaction())
                using (var db = tx.OpenDatabase(_dbName, new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create}))
                {
                    tx.Put(db, key, value);
                    tx.Commit();
                }
            }
        }

        public void Delete(byte[] key)
        {
            using (var env = new LightningEnvironment(DbEnv))
            {
                env.MaxDatabases = 2;
                env.Open();

                using (var tx = env.BeginTransaction())
                using (var db = tx.OpenDatabase(_dbName))
                {
                    tx.Delete(db, key);
                    tx.Commit();
                }
            }
        }


    }
}