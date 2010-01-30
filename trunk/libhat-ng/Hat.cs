using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using libhat;
using libhat_ng.Entity;
using libhat_ng.Helpers;

namespace libhat_ng
{
    public class Hat
    {
        private Socket sock;
        private Thread workerThread;
        private List<Thread> childs;
        private readonly object childLock = new object();
        private log4net.ILog logger = null;
        public static HatConfiguration Configuration = new HatConfiguration();

        public void Start() {
            logger = log4net.LogManager.GetLogger("Hat");

            Configuration.IsRegistrationAllowed = true;

            workerThread = new Thread( new ThreadStart( threadStart ) );

            workerThread.Start();
        }

        private void threadStart() {
            childs = new List<Thread>();
            sock = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP );
            var e = new IPEndPoint( IPAddress.Any, 8888 );
            sock.Bind( e );
            sock.Listen( 10 );
            while ( true ) {
                Socket handler = sock.Accept();
                var t = new Thread( new ParameterizedThreadStart( childThread ) );
                lock ( childLock ) {
                    childs.Add( t );
                }

                t.Start( handler );
            }
        }

        private void childThread( object obj ) {
            var me = Thread.CurrentThread;
            var context = new HatContext();

            try {
                var handler = obj as Socket;
                if ( handler != null ) {
                    while ( true ) {
                        byte[] buffer = new byte[256];
                        int rcvd = handler.Receive( buffer );

                        if ( rcvd > 0 ) {
                            try {
                                var decoded = NetworkHelper.PacketDecoding( buffer );

                                var command = PacketParser.Parse( decoded );

                                var response = command.Execute(context);

                                response = NetworkHelper.PacketEncoding( response );

                                handler.Send(response);
                            }
                            catch ( InvalidPacketException ex ) {
                                logger.Error( "Invalid packet came from client", ex );
                                using ( var mem = new MemoryStream() ) {
                                    NetworkHelper.DumpArray( mem, NetworkHelper.PacketDecoding( buffer ) );

                                    logger.Debug( mem.ToString() );
                                }
                            }
                            catch ( Exception ex ) {
                                logger.Error( "Unexpected exception raised while packet handled", ex );
                                sock.Close();
                                break;
                            }
                        }
                    }
                }
            }
            catch ( ThreadAbortException ex ) {
            }
            catch ( ThreadInterruptedException ex ) {
            }
            finally {
                lock ( childLock ) {
                    childs.Remove( me );
                }

                Thread.CurrentThread.Abort();
            }
        }
        
        public void Stop() {
            workerThread.Abort();

            lock ( childLock ) {
                foreach ( var thread in childs ) {
                    if ( thread.IsAlive )
                    {
                        thread.Abort();
                    }
                }
            }
        }
    }
    
    public class HatContext
    {
        public HatUser User { get; set; }
    }
}
