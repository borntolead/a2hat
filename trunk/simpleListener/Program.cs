using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using libhat;

namespace simpleListener {
    class Program {
        private static System.Threading.ManualResetEvent allDone;

        static void Main( string[] args ) {
            IPEndPoint ep = new IPEndPoint( IPAddress.Loopback, 8000);

            Socket listener = NetworkHelper.GetNewIPv4Socket( ep );
            listener.Listen( 10 );
            byte[] buf;

            uint packetLength;
            uint unknown1;
            while ( true ) {
                Console.WriteLine( "Waiting for a connection..." );
                Socket handler = listener.Accept();

                buf = new byte[256];
                int bytesRec = handler.Receive( buf );

                using ( System.IO.MemoryStream mem = new System.IO.MemoryStream( buf ) ) {
                    mem.Seek( 0, System.IO.SeekOrigin.Begin );
                    System.IO.BinaryReader br = new System.IO.BinaryReader( mem );

                    packetLength = br.ReadUInt32();
                    unknown1 = br.ReadUInt32();

                    byte[] decoded = new byte[packetLength];
                    Buffer.BlockCopy( buf, (int)br.BaseStream.Position, decoded, 0, (int)packetLength);
                    
                    NetworkHelper.DumpArray( Console.OpenStandardOutput(), buf );
                    NetworkHelper.DumpArray( Console.OpenStandardOutput(), Consts.Passphrase );
                    NetworkHelper.DumpArray( Console.OpenStandardOutput(), decoded );

                    PacketParse( decoded );

                    handler.Shutdown( SocketShutdown.Both );
                    handler.Close();
                }
                
            }

            //Console.WriteLine( "Text received : {0}", data );

            //byte[] msg = Encoding.ASCII.GetBytes( data );
            //handler.Send( msg );
            
        }

        private static object PacketParse( byte[] decoded ) {
            byte packetID = decoded[0];

            switch( packetID ) {
                case 0x69:
                    LoginPacket pack = new LoginPacket();

                    //pack.login = ;
                    break;
            }

            return null;
        }
    }
}

