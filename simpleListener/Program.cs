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
            using ( var mem = new System.IO.MemoryStream( Consts.SuccessfulResponse ) ) {
                mem.Seek( 0, System.IO.SeekOrigin.Begin );
                var br = new System.IO.BinaryReader( mem );

                var pl = br.ReadUInt32();
                br.ReadUInt32();

                var decoded = new byte[pl];
                Buffer.BlockCopy( Consts.SuccessfulResponse,
                                  (int) br.BaseStream.Position,
                                  decoded, 0, (int) pl );
                decoded = NetworkHelper.PacketDecoding( decoded );

                NetworkHelper.DumpArray( Console.OpenStandardOutput( ), decoded );
            }
           
            var srv = new HatServer();
            srv.EventOccured += srv_EventOccured;
            srv.Start( "127.0.0.1", 8000 );
        }

        static void srv_EventOccured( object sender, HatEventArgs args ) {
            Console.WriteLine( "[{0}]: {1}\n innerException: {2}", args.Type, args.Message , args.Ex );
        }

        


    }
}

