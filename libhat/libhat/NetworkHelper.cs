using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace libhat {
    public class NetworkHelper {
        public static Socket GetNewSocket (ProtocolType type, EndPoint ep) {
            Socket sock = new Socket( AddressFamily.InterNetwork, SocketType.Stream, type );

            sock.Bind( ep );
            return sock;
        }

        public static Socket GetNewIPv4Socket(EndPoint ep) {
            return GetNewSocket( ProtocolType.Tcp, ep );
        }

        public static Socket GetNewIPv6Socket( EndPoint ep ) {
            return GetNewSocket( ProtocolType.IPv6, ep );
        }

        public static void DumpArray(Stream outputStream, byte[] array) {
            TextWriter tw = new StreamWriter( outputStream, Encoding.GetEncoding( 866 ) );
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

        public static byte[] PacketEncoding(byte[] incoming, long offset) {
            byte[] encoded = new byte[incoming.Length];
            int k = 0;
            for ( long i = offset; k < incoming.Length; i++ ) {
                int l = k < Consts.Passphrase.Length ? k : k % Consts.Passphrase.Length;
                encoded[k] = (byte)( incoming[i] ^ Consts.Passphrase[l] );
                k++;
            }

            return encoded;
        }

        public static byte[] PacketDecoding( byte[] incoming, long offset ) {
            byte[] decoded = new byte[incoming.Length];

            if( incoming.Length > 80 ) {
                throw new ArgumentOutOfRangeException( "packet must be <= 80" );
            }

            int k = 0;
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
    }
}
