using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using log4net;

namespace libhat_ng.Helpers
{
    public class NetworkHelper {
        public static Socket GetNewSocket (ProtocolType type, EndPoint ep) {
            var sock = new Socket( AddressFamily.InterNetwork, SocketType.Stream, type );

            sock.Bind( ep );
            return sock;
        }

        public static Socket GetNewIPv4Socket(EndPoint ep) {
            return GetNewSocket( ProtocolType.IPv4, ep );
        }

        public static Socket GetNewIPv6Socket( EndPoint ep ) {
            return GetNewSocket( ProtocolType.IPv6, ep );
        }

        public static void DumpArray(Stream outputStream, byte[] array) {
            TextWriter tw = new StreamWriter( outputStream, Encoding.UTF8 );
            bool isEnd = false;
            int i = 0x0000;

            tw.WriteLine( "-------------------------------Begin Dump--------------------------------" );
            while ( i < array.Length ) {
                tw.Write( "{0,8:X4}: ", i );
                
                for( int j=0; j<16; j++) {
                    if( i+j == array.Length) {
                        isEnd = true;
                        break;
                    }
                    tw.Write( "{0,2:X2} ", array[i + j] );
                    if( j % 4 == 3 ) {
                        tw.Write( "| " );
                    }
                }
                tw.Write( "\n" );
                if ( isEnd ) { break; }
                
                tw.Flush();

                

                i+=16;
            }
            tw.WriteLine( "--------------------------------End Dump---------------------------------" );
            tw.Flush();
        }
        
        public static void DumpArray(byte[] array)
        {
            using (var mem = new MemoryStream())
            {
                var logger = LogManager.GetLogger("UnknownPackets");

                DumpArray(mem, array);
                mem.Seek(0, SeekOrigin.Begin);

                var tr = new StreamReader(mem);

                logger.Debug(tr.ReadToEnd());

                tr.Close();
            }

        }

        public static byte[] PacketEncoding(byte[] incoming, long offset) {
            var encoded = new byte[incoming.Length];
            int k = 0;
            for ( var i = offset; k < incoming.Length; i++ ) {
                var l = k < Consts.Passphrase.Length ? k : k % Consts.Passphrase.Length;
                encoded[k] = (byte)( incoming[i] ^ Consts.Passphrase[l] );
                k++;
            }

            return encoded;
        }

        public static byte[] PacketDecoding( byte[] incoming, long offset ) {
            var decoded = new byte[incoming.Length];

            if( incoming.Length > 80 ) {
                throw new ArgumentOutOfRangeException("incoming", "incoming packet length must be <= 80");
            }

            var k = 0;
            for ( long i = offset; k < incoming.Length; i++ ) {
                decoded[k] = (byte)( incoming[i] ^ Consts.Passphrase[k] );
                k++;
            }

            return decoded;
        }

        public static byte[] PacketEncoding(byte[] incoming) {
            return PacketEncoding( incoming, 0 );
        }

        public static byte[] PacketDecoding( byte[] incoming ) {
            return PacketEncoding( incoming, 0 );
        }

        /// <summary>
        /// build response byte array
        /// </summary>
        /// <param name="operation">Client operation to response</param>
        /// <param name="message">Type of response</param>
        /// <param name="response">message to client</param>
        /// <returns></returns>
        public static byte[] ClientMessageBuild(ClientOperation operation, ClientMessage message, byte[] response)
        {
            using (var mem = new MemoryStream())
            {
                var writer = new BinaryWriter(mem);

                writer.Write((byte)operation);
                writer.Write((Int32)message);
                
                if( response != null && response.Length > 0)
                {
                    writer.Write(response);
                }

                writer.Flush();
                return mem.ToArray();
            }
        }
        public static byte[] ClientMessageBuild(ClientOperation operation, ClientMessage message)
        {
            return ClientMessageBuild(operation, message, null);
        }

    }
}