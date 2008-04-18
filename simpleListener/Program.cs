using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using libhat;

namespace simpleListener {
    class Program {
        static void Main( string[] args ) {
            using ( System.IO.MemoryStream mem = new System.IO.MemoryStream( Consts.SuccessfulResponse ) ) {
                mem.Seek( 0, System.IO.SeekOrigin.Begin );
                System.IO.BinaryReader br = new System.IO.BinaryReader( mem );

                uint pl = br.ReadUInt32();
                br.ReadUInt32();

                byte[] decoded = new byte[pl];
                Buffer.BlockCopy( Consts.SuccessfulResponse,
                                  (int) br.BaseStream.Position,
                                  decoded, 0, (int) pl );
                decoded = NetworkHelper.PacketDecoding( decoded );

                NetworkHelper.DumpArray( Console.OpenStandardOutput( ), decoded );
            }
           
            HatServer srv = new HatServer();
            srv.EventOccured += new HatEvent( srv_EventOccured );
            srv.Start( "127.0.0.1", 8000 );
        }

        static void srv_EventOccured( object sender, HatEventArgs args ) {
            Console.WriteLine( "[{0}]: {1}\n innerException: {2}", args.Type, args.Message , args.Ex );
        }

        


    }
}

