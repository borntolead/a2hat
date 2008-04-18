using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Db4objects.Db4o;
using db4o = Db4objects.Db4o.Db4oFactory;
namespace libhat.DBFactory {
    public class Db4oFactory: IDBFactory {
        private static bool isInitialized;
        private static Db4oFactory instance = null;
        private static IObjectContainer dbInstance;


        private Db4oFactory() {
        }

        public static Db4oFactory GetInstance() {
            if( isInitialized == false ) {
                throw new InvalidOperationException( );
            }
            
            if( instance == null) {
                instance = new Db4oFactory();
            }

            return instance;
        }

        public static void Configure() {
            dbInstance = db4o.OpenFile( Path.Combine( Environment.CurrentDirectory, "hat.db") );
            isInitialized = true;
        }

        #region IDBFactory Members

        public T LookupFirst<T>( ICondition condition ) where T :class, IEntity {
            IList<T> ret = Lookup<T>( condition );

            if( ret != null && ret.Count > 0) {
                return ret[0];
            }

            return null;
        }

        public IList<T> Lookup<T>( ICondition condition ) where T :class, IEntity {
            List<T> result = new List<T>( );

            switch ( condition.Name ) {
                case "SELECT_ALL":
                    result.AddRange( dbInstance.Query<T>() );
                    break;
                case "SELECT_BY_CODE":

                    SelectByCodeCondition cond = condition as SelectByCodeCondition;

                    if ( cond == null ) {
                        throw new InvalidDataException( String.Format( "Invalid {0} condition", condition.Name ) );
                    }
                    IList<T> res = dbInstance.Query<T>(delegate (T t){ return cond.Codes.Contains( t.Code ); }) ;

                    result.AddRange( res );
                    break;
                default:
                    throw new NotImplementedException( String.Format( "Condition {0} is not implemented", condition.Name ) );
            }
            dbInstance.Commit();
            return result;
        }

        public void Save<T>( params T[] items ) where T :class, IEntity {
            Save<T>( (IEnumerable<T>) items );
        }

        public void Save<T>( IEnumerable<T> items ) where T :class, IEntity {
            foreach ( T t in items ) {
                dbInstance.Set( t );
            }
            dbInstance.Commit();
        }

        public void Delete<T>( params T[] items ) where T :class, IEntity {
            Delete<T>( (IEnumerable<T>)items );
        }

        public void Delete<T>( IEnumerable<T> items ) where T :class, IEntity {
            foreach ( T t in items ) {
                dbInstance.Delete( t );
            }
            dbInstance.Commit();
        }

        public bool IsInitialized {
            get { return isInitialized; }
        }

        

        #endregion

        #region IDisposable Members

        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        public void Dispose() {
            dbInstance.Close();
        }

        #endregion
    }
}
