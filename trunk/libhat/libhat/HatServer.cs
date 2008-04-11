using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace libhat {
    public class HatServer {
        public event EventHandler ClientConnected;
        private Thread listenerThread;

        private bool exitCalled = false;

        /// <summary>
        /// Start an hat instance on specified IP and port
        /// </summary>
        /// <param name="ip_address">IP for binding</param>
        /// <param name="port">port for listen</param>
        /// <returns>return false if hat would not start</returns>
        public bool Start(string ip_address, int port) {
            Socket listener;
            IPEndPoint ep = new IPEndPoint( IPAddress.Parse( ip_address ), port );
            ThreadPool.SetMinThreads( 5, 2 );
            //TODO configuring from file

            listener = NetworkHelper.GetNewIPv4Socket( ep );
            listener.Listen( 10 );
            
            ParameterizedThreadStart lthread = new ParameterizedThreadStart( listenerThreadStartMethod );
            listenerThread = new Thread( lthread );
            listenerThread.IsBackground = true;
            listenerThread.Start( listener );

            Console.CancelKeyPress += new ConsoleCancelEventHandler( Console_CancelKeyPress );

            while( true ) {
                if( exitCalled ) {
                    break;
                }
                Thread.Sleep( 100 );
            }

            listenerThread.Abort( );
            
            return true;
        }

        void Console_CancelKeyPress( object sender, ConsoleCancelEventArgs e ) {
            exitCalled = true;
        }

        /// <summary>
        /// Thread which accept connections from specified ip/port for client connections in endless loop.
        /// </summary>
        /// <param name="obj">Binded socket</param>
        private void listenerThreadStartMethod( object obj ) {
            Socket listener = obj as Socket;

            if( listener == null ) {
                Console.WriteLine( "invalid argument for listener thread application aborted" );
                Thread.CurrentThread.Abort( );
            }
            while ( true ) {
                Console.WriteLine( "Waiting for a connection..." );
                Socket handler = listener.Accept();

                ThreadPool.QueueUserWorkItem( new WaitCallback( delegate( object stateInfo ) {
                    HandleConnection( handler );} 
                    ) 
                );
            }
        }

        private void HandleConnection( Socket handler ) {
            byte[] buf = new byte[256];
            uint packetLength, unknown1;
            int bytesRec = 0; 
            while( true ){
                try {
                    bytesRec = handler.Receive( buf );
                } catch( Exception ex) {
                    Thread.CurrentThread.Abort( );
                }

                /// move it to separate process
                using ( System.IO.MemoryStream mem = new System.IO.MemoryStream( buf ) ) {
                    mem.Seek( 0, System.IO.SeekOrigin.Begin );
                    System.IO.BinaryReader br = new System.IO.BinaryReader( mem );
                        
                    packetLength = br.ReadUInt32();
                    unknown1 = br.ReadUInt32();

                    Console.WriteLine( "packet recived\n length:{0}\n unknown1: {1}", packetLength, unknown1 );

                    byte[] decoded = new byte[packetLength];
                    Buffer.BlockCopy( buf,
                                      (int) br.BaseStream.Position,
                                      decoded, 0, (int) packetLength );
                    decoded = NetworkHelper.PacketDecoding( decoded );

                    object packet = PacketParse( decoded );
                    byte[] response = ProcessPacket( packet );

                    Console.WriteLine( "response:" );
                    NetworkHelper.DumpArray( Console.OpenStandardOutput(), response );


                    NetworkHelper.DumpArray( Console.OpenStandardOutput(), PrepareMessageToSend( response ) );
                    handler.Send( PrepareMessageToSend( response ) );
                }
                        
            }
        }

        /// <summary>
        /// Processing packet
        /// </summary>
        /// <param name="packet"> return response to recived packet</param>
        /// <returns></returns>
        private byte[] ProcessPacket( object packet ) {
            if( packet is LoginPacket ) {
                LoginPacket pack = (LoginPacket) packet;
                if( LoginUser(pack.login, pack.password)) {
                    return GetCharacterList( pack );
                } else {
                    return processClientMessage( Client_operation.M_SEND_MESSAGE, Client_message.M_INVALID_LOGIN_PASSWORD, null );
                }
            }

            if( packet is Client_message ) {
                Client_message mess = (Client_message)packet;
                return processClientMessage( Client_operation.M_CHECK_NICKNAME, mess, null );
            }

            return null;
        }

        private byte[] processClientMessage( Client_operation op, Client_message packet, byte[] message ) {
            int size = sizeof ( Byte ) + sizeof( Int32 ) + (message!= null ? message.Length : 0);

            byte[] response = new byte[size];

            using( MemoryStream mem = new MemoryStream( response )) {
                BinaryWriter bw = new BinaryWriter( mem );
                
                bw.Write( (byte)op );
                bw.Write( (Int32) packet );
                if( message != null ) {
                    bw.Write( message );
                }
            }

            return response;
        }

        /// <summary>
        /// get list of user's characters
        /// </summary>
        /// <param name="pack">user's credentials</param>
        /// <returns> byte array represents list of user's characters</returns>
        private byte[] GetCharacterList( LoginPacket pack) {

            List<HatUser> characters = getCharacterList(pack.login);

            byte[] chars = new byte[characters.Count * 8 + 9];
            using( MemoryStream mem = new MemoryStream( chars)) {
                BinaryWriter bw = new BinaryWriter( mem );

                bw.Write( (byte)0xCE );
                bw.Write( (characters.Count * 8) + 4);
                bw.Write( Consts.HatIdentifier );
                foreach ( HatUser user in characters ) {
                    bw.Write( user.UserID );
                }
                
            }

            return chars;

            //return new byte[]{0xCE, 0x04, 0x00, 0x00, 0x00, 0xe8, 0x03, 0x00, 0x00};
        }

        // get characters of user from storage
        private List<HatUser> getCharacterList( string login ) {
            return new List<HatUser>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="message"></param>
        private void SendPacketToClient(Socket handler, object message) {
            
        }

        private bool LoginUser( string login, string password ) {
            return true;
        }

        private object PacketParse( byte[] decoded ) {
            object ret = null;

            using ( MemoryStream mem = new MemoryStream( decoded ) ) {
                BinaryReader br = new BinaryReader( mem );
                byte packetID = br.ReadByte();
            
                switch ( packetID ) {
                    case 0xc9: //login
                    {
                        LoginPacket lp = new LoginPacket();
                        NetworkHelper.DumpArray( Console.OpenStandardOutput(), decoded );

                        byte loginlen = br.ReadByte();
                        lp.GameType = (GameType) br.ReadInt16();
                        byte loginOffset = br.ReadByte();
                        byte credentialsLength = br.ReadByte();

                        mem.Seek( loginOffset, SeekOrigin.Begin );

                        lp.login = Encoding.Default.GetString( br.ReadBytes( loginlen ) );
                        lp.password = Encoding.Default.GetString( br.ReadBytes( credentialsLength - loginlen ) );

                        Console.WriteLine( "user logging in" );
                        Console.WriteLine( "GameType: {0}", lp.GameType.ToString() );
                        Console.WriteLine( "Login: {0}", lp.login );
                        Console.WriteLine( "Password: {0}\n", lp.password );
                        Console.WriteLine( "------unparsed data--------------" );
                        NetworkHelper.DumpArray( Console.OpenStandardOutput(),
                                                 br.ReadBytes( (int) ( decoded.Length - mem.Position ) ) );
                        Console.WriteLine( "---------------------------------" );
                        ret = lp;


                        break;
                    }
                    case 0x4e: //check nickname
                    {
                        br.BaseStream.Seek( 5, SeekOrigin.Begin );
                        byte length = br.ReadByte();
                        br.BaseStream.Seek( 1, SeekOrigin.Current );
                        string nickname = Encoding.Default.GetString(br.ReadBytes(length));
                        return check_nickname( nickname );
                    }
                default:
                        ret = null;
                        NetworkHelper.DumpArray( Console.OpenStandardOutput(), decoded );
                        Console.WriteLine( Encoding.GetEncoding( 866 ).GetString( decoded ) );
                        break;
                }
            }

            return ret;
        }

        private Client_message check_nickname( string nickname ) {
            if( nickname == "test") {
                return Client_message.M_ALREADY_EXISTS;
            }

            return Client_message.M_ALL_OK;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private byte[] PrepareMessageToSend(byte[] message) {

            if( message == null ) {
                throw new ArgumentNullException( "cannot send null packet" );
            }

            int packetLength = message.Length + Consts.PacketEnding.Length;

            byte[] packet = new byte[8 + packetLength];
            byte[] encoded = new byte[packetLength];
            
            //compiling response
            using ( MemoryStream mem = new MemoryStream( encoded ) ) {
                BinaryWriter bw = new BinaryWriter( mem );

                bw.Write( message );

                bw.Write( Consts.PacketEnding );

                bw.Flush();
            }

            //making formatted packet
            using( MemoryStream mem = new MemoryStream( packet )) {
                BinaryWriter bw = new BinaryWriter( mem );
                if( packetLength > Consts.PacketMaxLength) {
                    bw.Write( (Int32) 0x8E );
                    bw.Write( (Int32)0 );
                } else {
                    // some redundant paranoia
                    bw.Write( (Int32)( packetLength ) );
                    bw.Write( (Int32)0x01000000 );
                }
                

                bw.Write( NetworkHelper.PacketEncoding( encoded ) );

                bw.Flush();
            }

            return packet;
        }

    }
}
