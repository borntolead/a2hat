using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using BerkeleyDb;

namespace libhat.DBFactory {
    public class BDBFactory : IDBFactory{
        private static BDBFactory factory;
        private static bool isInited = false;
        private static string dbHome;
        private static string dbName;
        private DbBTree dbInstance;
        static Env env = null;
        static BinaryFormatter formatter = new BinaryFormatter();

        static int AppCompare( Db db, ref DbEntry appData, ref DbEntry dbData ) {
            string appStr = Encoding.UTF8.GetString( appData.Buffer, 0, appData.Size );
            string dbStr = Encoding.UTF8.GetString( dbData.Buffer, 0, dbData.Size );
            return string.Compare( appStr, dbStr );
        }

        void initDatabase() {
            Db db = null;
            Txn txn = null;
            

            try {
                env = new Env( EnvCreateFlags.None );
                // configure for error and message reporting
                env.ErrorStream = Console.OpenStandardError( );
                env.MessageStream = Console.OpenStandardOutput( );

                // initialize environment for locking, logging, memory pool and transactions
                Env.OpenFlags envFlags =
                    Env.OpenFlags.Create |
                    Env.OpenFlags.InitLock |
                    Env.OpenFlags.InitLog |
                    Env.OpenFlags.InitMPool |
                    Env.OpenFlags.InitTxn |
                    Env.OpenFlags.Recover;
                env.Open( dbHome, envFlags, 0 );

                // create, configure and open database under a transaction
                txn = env.TxnBegin( null, Txn.BeginFlags.None );
                db = env.CreateDatabase( DbCreateFlags.None );
                // set the BTree comparison function
                db.BTreeCompare = AppCompare;
                // error and message reporting already configured on environment
                // db.ErrorStream = errStream;
                // db.ErrorPrefix = Path.GetFileName(Application.ExecutablePath);
                // db.MessageStream = msgStream;
                dbInstance = (DbBTree)db.Open(
                  txn, dbName, null, DbType.BTree, Db.OpenFlags.Create, 0 );
                txn.Commit( Txn.CommitMode.None );
            }
            catch {

            }

            isInited = true;
        }
    

        public static BDBFactory GetInstance() {
            if( !isInited) {
                throw new InvalidOperationException( "BDBFactory was not configured" );
            }
            
            if ( factory == null ) {
                factory = new BDBFactory();
            }
            

            return factory;
        }

        public static void Configure(string home, string name) {
            if(!isInited) {
                if( !isInited) {
                    dbHome = home;
                }
            }
        }

        private BDBFactory() {}

        #region IDBFactory Members

        public bool IsInitialized {
            get { return isInited; }
        }

        public T LookupFirst<T>( ICondition condition ) where T : class {
            IList<T> lst = Lookup<T>( condition );

            if( lst != null && lst.Count > 0) {
                return lst[0];
            }

            return null;
        }


        public IList<T> Lookup<T>( ICondition condition ) where T : class {
            dbName = typeof ( T ).FullName;
            initDatabase();
            List<T> result = new List<T>( );
            
            Txn trans = env.TxnBegin( null, Txn.BeginFlags.None );

            switch ( condition.Name ) {
                case "SELECT_ALL":
                    using ( DbBTreeCursor cursor = dbInstance.OpenCursor( null, DbFileCursor.CreateFlags.None ) ) {
                        T cust = null;

                        IEnumerable<KeyDataPair> lst = cursor.ItemsForward(false, DbFileCursor.ReadFlags.None );

                        foreach ( KeyDataPair pair in lst ) {
                            using( MemoryStream mem = new MemoryStream(pair.Data.Buffer)) {
                                T t = (T)formatter.Deserialize( mem );

                                result.Add( t );
                            }
                        }
                    }
                    break;
                case "SELECT_BY_CODE":

                    SelectByCodeCondition cond = condition as SelectByCodeCondition;

                    if( cond == null ) {
                        throw new InvalidDataException( String.Format( "Invalid {0} condition", condition.Name ) );
                    }
                    foreach ( int i in cond.HashCodes ) {
                        T t = null;
                        byte[] buf = new byte[1024];
                        MemoryStream key = new MemoryStream( );
                        MemoryStream value = new MemoryStream();
                        formatter.Serialize(  key, i );

                        DbEntry dbKey = DbEntry.InOut(key.ToArray());
                        DbEntry dbVal = DbEntry.Out( buf );
                        while ( true ) {
                            ReadStatus status =
                                dbInstance.GetExact( trans, ref dbKey, ref dbVal, DbFile.ReadFlags.None );

                            
                            switch ( status ) {
                                case ReadStatus.BufferSmall:
                                    if ( dbVal.Buffer.Length < dbVal.Size ) {
                                        value.SetLength( dbVal.Size );
                                        dbVal = DbEntry.Out( value.GetBuffer() );
                                    }
                                    continue;
                                case ReadStatus.KeyEmpty:
                                    goto brk;
                                case ReadStatus.NotFound:
                                    goto brk;
                                case ReadStatus.Success:
                                    value.Position = 0;
                                    value.SetLength( dbVal.Size );
                                    value.Write( dbVal.Buffer, 0, dbVal.Size );
                                    value.Flush();
                                    value.Seek( 0, SeekOrigin.Begin );
                                    t = (T)formatter.Deserialize( value );

                                    result.Add( t );

                                    key.Dispose();
                                    value.Dispose();
                                    break;
                            }
                        }
                    brk:;

                    }

                    break;
                default:
                    throw new NotImplementedException(String.Format( "Condition {0} is not implemented", condition.Name));
            }
            trans.Commit( Txn.CommitMode.Sync );
            env.Close();
            return result;
        }

        public void Save<T>( params T[] items ) where T : class {
            Save( (IEnumerable<T>) items );
        }

        public void Save<T>( IEnumerable<T> items ) where T : class {
            dbName = typeof( T ).FullName;
            initDatabase();
            Txn trans = env.TxnBegin( null, Txn.BeginFlags.ReadCommitted );
            foreach ( T t in items ) {
                MemoryStream memKey = new MemoryStream( );
                MemoryStream memVal = new MemoryStream();

                formatter.Serialize( memKey, t.GetHashCode() );
                formatter.Serialize( memVal, t );

                memKey.Flush();
                memVal.Flush();

                DbEntry key = DbEntry.InOut( memKey.ToArray() );
                DbEntry value = DbEntry.InOut( memVal.ToArray() );

                WriteStatus status = dbInstance.PutUnique( trans, ref key, ref value );

                if( status == WriteStatus.KeyExist) {
                    Debug.Print( "key {0} duplicated", t.GetHashCode() );
                } 

                if( status == WriteStatus.KeyExist) {
                    Delete<T>( t );
                    WriteStatus st = dbInstance.PutUnique( trans, ref key, ref value );
                }
            }

            trans.Commit( Txn.CommitMode.Sync );
            env.Close();
        }

        public void Delete<T>( params T[] items ) where T : class {
            Delete( (IEnumerable <T>) items );
        }

        public void Delete<T>( IEnumerable<T> items ) where T : class {
            dbName = typeof( T ).FullName;
            initDatabase();
            Txn trans = env.TxnBegin( null, Txn.BeginFlags.ReadCommitted );
            foreach ( T t in items ) {
                MemoryStream memKey = new MemoryStream();
                MemoryStream memVal = new MemoryStream();

                formatter.Serialize( memKey, t.GetHashCode() );
                formatter.Serialize( memVal, t );

                DbEntry key = DbEntry.InOut( memKey.ToArray() );
                DeleteStatus status = dbInstance.Delete(trans, ref key );

                if ( status == DeleteStatus.NotFound ) {
                    Debug.Print( "key {0} not found", t.GetHashCode() );
                }
            }

            trans.Commit( Txn.CommitMode.None );
            env.Close();
        }

        #endregion

        #region IDisposable Members

        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        public void Dispose() {
            env.Close();
        }

        #endregion
    }

    
}