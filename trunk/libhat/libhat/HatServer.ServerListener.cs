using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace libhat {
    public partial class HatServer {
        
        protected virtual void serverInfoListener(object parameters) {
            GameServer srv=null;

            object[] param = null;
            Socket handler = null;
            byte[] buf = null;
            try {
                 param = (object[]) parameters;
                 handler = (Socket) param[0];
                 buf = (byte[]) param[1];
            } catch(Exception ex) {
                LogEvent( EventType.ERROR, ex, "server handler has incorrect parameters" );
                if( handler != null ) {
                    handler.Close( 5 );
                }
                return;
            }

            uint packetLength, unknown1;
            while( true ) {
                using ( System.IO.MemoryStream mem = new System.IO.MemoryStream( buf ) ) {
                    mem.Seek( 0, System.IO.SeekOrigin.Begin );
                    System.IO.BinaryReader binRead = new System.IO.BinaryReader( mem );

                    packetLength = binRead.ReadUInt32();
                    unknown1 = binRead.ReadUInt32();

                    byte[] decoded = new byte[packetLength];

                    Buffer.BlockCopy( buf,
                                      (int)binRead.BaseStream.Position,
                                      decoded, 0, (int) packetLength );
                    decoded = NetworkHelper.PacketDecoding( decoded );
                    NetworkHelper.DumpArray( Console.OpenStandardOutput(), decoded );

                    using( MemoryStream mstr = new MemoryStream( decoded )) {
                        BinaryReader br = new BinaryReader( mstr );
                        byte packetID = br.ReadByte();

                        switch ( packetID ) {
                                /// bad code warning!
                                /// section below contains processing of 2 in series
                            case 0xD1: {
                                //ident server
                                br.BaseStream.Seek( 5, SeekOrigin.Begin );
                                byte length = br.ReadByte();
                                br.BaseStream.Seek( 1, SeekOrigin.Current );
                                string addr = Encoding.Default.GetString( br.ReadBytes( length ) );
                                string[] arr = addr.Split( ':' );
                                byte[] buffer = new byte[0x8e];
                                EndPoint ep = new IPEndPoint( IPAddress.Parse( arr[0] ), Int32.Parse( arr[1] ) );

                                srv = new GameServer();
                                srv.EndPoint = ep;
                                srv.Code = ep.ToString();

                                handler.Send( PrepareMessageToSend(serverWelcome()) );

                                break;
                            }
                            case 0xd2: {
                                if ( srv == null ) {
                                    break;
                                }
                                byte pID = br.ReadByte();

                                byte pcount = br.ReadByte();
                                byte difficulty = br.ReadByte();
                                byte srvType = br.ReadByte();
                                byte mapSize = br.ReadByte();

                                if ( pcount == 0xff && difficulty == 0xff && srvType == 0xff && mapSize == 0xff ) {
                                    lock ( registeredServers ) {
                                        handler.Close( 5 );
                                        registeredServers.Remove( srv );
                                        srv = null;
                                        return;
                                        break;
                                    }
                                }

                                byte ln = br.ReadByte();

                                br.BaseStream.Seek( 1, SeekOrigin.Current );
                                string mapName = Encoding.Default.GetString( br.ReadBytes( ln ) );

                                int idx = -1;
                                if ( registeredServers.Contains( srv ) ) {
                                    idx = registeredServers.IndexOf( srv );
                                }
                                srv.PlayersCount = pcount;
                                srv.ServerType = (ServerType) srvType;
                                srv.Map = new GameMap();
                                srv.Map.Difficulty = (Difficulty) difficulty;
                                srv.Map.Name = mapName;
                                srv.Map.Height = srv.Map.Width = mapSize;

                                if ( idx == -1 ) {
                                    registeredServers.Add( srv );
                                } else {
                                    registeredServers[idx] = srv;
                                }
                                handler.Send( PrepareMessageToSend( null ) );
                                break;
                        }
                            case 0x64:
                                handler.Send( PrepareMessageToSend( null ) );
                                break;
                                default:
                                break;
                        }
                        
                    }
                }
                try {
                    handler.Receive( buf );
                } catch {
                    return;
                }
            }
        }

        private byte[] serverWelcome() {
            byte[] ret = new byte[9];
            using ( MemoryStream mem = new MemoryStream( ret ) ) {
                BinaryWriter bw = new BinaryWriter( mem );

                bw.Write( (byte)0xD5 );
                bw.Write( Consts.HatIdentifier );
                bw.Write( 1 );
            }

            return ret;
        }
    }
}
