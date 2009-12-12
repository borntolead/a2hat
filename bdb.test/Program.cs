using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyDB;
using System.IO;

namespace bdb.test {
    class Program {
        static void Main( string[] args ) {
            try {
                var cfg = new HashDatabaseConfig();
                cfg.Duplicates = DuplicatesPolicy.UNSORTED;
                cfg.Creation = CreatePolicy.IF_NEEDED;
                cfg.CacheSize = new CacheInfo( 0, 64 * 1024, 1 );
                cfg.PageSize = 8 * 1024;

                Database db = HashDatabase.Open( "d:\\test.db", "hat_db", cfg );

                Console.WriteLine("db opened");

                var key = new DatabaseEntry();
                var data = new DatabaseEntry();

                key.Data = System.Text.Encoding.ASCII.GetBytes("key1");
                data.Data = System.Text.Encoding.ASCII.GetBytes("val1");

                try {
                    db.Put( key, data );
                    db.Put( key, data );
                }
                catch ( Exception ex ) {
                    Console.WriteLine( ex.Message ); 
                }


                using ( var dbc = db.Cursor() ) {

                    System.Text.ASCIIEncoding decode = new ASCIIEncoding();

                    /* Walk through the database and print out key/data pairs. */
                    Console.WriteLine( "All key : data pairs:" );
                    foreach ( KeyValuePair<DatabaseEntry, DatabaseEntry> p in dbc )
                        Console.WriteLine( "{0}::{1}",
                            decode.GetString( p.Key.Data ), decode.GetString( p.Value.Data ) );
                }

                db.Close();
                Console.WriteLine( "db closed" );
            } 
            catch ( Exception ex ) {
                Console.WriteLine( ex.Message ); 
            }

            Console.ReadLine();
        }
    }
}
