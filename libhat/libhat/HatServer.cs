using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Db4objects.Db4o.Internal.Mapping;
using libhat.DBFactory;

namespace libhat {
    public sealed partial class HatServer {
        public event EventHandler ClientConnected;
        private Thread listenClientsThread;
        
        private IDBFactory factory;
        private GameServer current = null;

        #region Critical lists
        List<GameServer> registeredServers = new List<GameServer>( );
        private Dictionary<string, LoginPacket> loggedInUsers = new Dictionary<string, LoginPacket>();
        #endregion

        /// <summary>
        /// Calling when hat handling inner exceptions and send other information
        /// </summary>
        public event HatEvent EventOccured;

        private bool exitCalled = false;

        public bool Configure( string pathToConfig ) {
            return Configure( getConfiguration( pathToConfig ) );
        }

        private HatConfiguration getConfiguration( string config ) {
            return null;
        }

        public bool Configure(HatConfiguration conf) {
            return false;
        }

        private void LogEvent( EventType type, Exception ex, string formatMessage, params object[] formatMessageArgs) {
            if( EventOccured != null ) {
                EventOccured( this, new HatEventArgs( type, String.Format( formatMessage, formatMessageArgs ), ex ) );
            }            
        }

        private void LogEvent(EventType type, string formatMessage, params object[] formatMessageArgs) {
            LogEvent( type, null, formatMessage, formatMessageArgs );
        }

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
            Db4oFactory.Configure(  );
            factory = Db4oFactory.GetInstance();

            listener = NetworkHelper.GetNewIPv4Socket( ep );
            listener.Listen( 10 );
            
            ParameterizedThreadStart lthread = new ParameterizedThreadStart( listenerThreadStartMethod );
            listenClientsThread = new Thread( lthread );
            listenClientsThread.IsBackground = true;
            listenClientsThread.Start( listener );

            Console.CancelKeyPress += new ConsoleCancelEventHandler( Console_CancelKeyPress );

            while( true ) {
                if( exitCalled ) {
                    break;
                }
                Thread.Sleep( 100 );
            }

            listenClientsThread.Abort( );
            
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

        private  HatUser GetCurrentUser() {
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot( "currentUser" );
            return (HatUser)Thread.GetData( slot );
        }

        private  void SetCurrentUser(HatUser usr) {
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot( "currentUser" );

            Thread.SetData( slot, usr);
        }

        private void HandleConnection( Socket handler ) {
            
            byte[] buf = new byte[256];
            uint packetLength, unknown1;
            int bytesRec = 0; 
            while( true ){
                try {
                    bytesRec = handler.Receive( buf );
                } catch( Exception ex) {
                    LogEvent( EventType.DEBUG, ex, "recieve failed" );
                    break;
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
                    NetworkHelper.DumpArray( Console.OpenStandardOutput(), decoded );

                    object packet = null;
                    try {
                        packet = ClientPacketParse( decoded, handler );
                    } catch(Exception ex) {
                        LogEvent( EventType.ERROR, ex, "packet {0} was processed with errors. breaking connection");
                        handler.Close( 5 );

                        break;
                    }

                    if ( packet is ParseState ) {
                        switch ( (ParseState)packet ) {
                            case ParseState.SERVER_CONNECTED: {
                                object[] parameters = new object[]{handler, buf};

                                Thread listenServerThread = new Thread( serverInfoListener );
                                listenServerThread.Start( parameters );
                                
                                break;
                            }
                            case ParseState.UNKNOWN_PACKET: {
                                NetworkHelper.DumpArray( Console.OpenStandardOutput(), decoded );
                                Console.WriteLine( Encoding.GetEncoding( 866 ).GetString( decoded ) );

                                handler.Close( 5 );
                                break;
                            }
                        }

                        LogEvent( EventType.DEBUG, "{0} packet was got", packet.ToString() );
                        break;
                    }

                    byte[] response = ProcessPacket( packet );

                    Console.WriteLine( "response:" );

                    NetworkHelper.DumpArray( Console.OpenStandardOutput(), response );
                    NetworkHelper.DumpArray( Console.OpenStandardOutput(), PrepareMessageToSend( response ) );

                    try {
                        handler.Send( PrepareMessageToSend( response ) );
                    } catch ( Exception ex ) {
                        LogEvent( EventType.ERROR, ex, "send failed" );
                    }
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
                    
                    lock(loggedInUsers) {
                        LoginPacket gt;

                        if( loggedInUsers.TryGetValue( pack.login, out gt ) ) {
                            return processClientMessage( Client_operation.M_SEND_MESSAGE, Client_message.M_LOGIN_LOCKED, null );
                        }

                        loggedInUsers.Add( pack.login, pack );
                    }

                    return GetCharacterList( pack );
                } else {
                    return processClientMessage( Client_operation.M_SEND_MESSAGE, Client_message.M_INVALID_LOGIN_PASSWORD, null );
                }
            }

            if( packet is Client_message ) {
                Client_message mess = (Client_message)packet;
                return processClientMessage( Client_operation.M_CHECK_NICKNAME, mess, null );
            }

            if( packet is Client_operation ) {
                return GetServersList();
            }

            return new byte[0];
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
        private byte[] GetCharacterList( LoginPacket pack ) {

            IList<HatCharacter> characters = getCharacterList(pack.login);

            byte[] chars = new byte[characters.Count * 8 + 9];
            using( MemoryStream mem = new MemoryStream( chars)) {
                BinaryWriter bw = new BinaryWriter( mem );

                bw.Write( (byte)0xCE );
                bw.Write( (characters.Count * 8) + 4);
                bw.Write( Consts.HatIdentifier );
                foreach ( HatCharacter user in characters ) {
                    bw.Write( user.CharacterID );
                }
                
            }

            return chars;
        }

        /// <summary>
        /// Gets registred and available servers list
        /// </summary>
        /// <returns></returns>
        private byte[] GetServersList () {
            IList<GameServer> servers = getServerList();

            byte[] message = new byte[1+ 0xFF + servers.Count*0x64];
            long bytesWritten = 0;
            using( MemoryStream mem = new MemoryStream( message )) {
                BinaryWriter bw = new BinaryWriter( mem );
                
                bw.Write( (byte)0xCD);
                bw.Write( Int32.MaxValue );

                long begin = bw.BaseStream.Position;

                lock ( registeredServers ) {
                    char[] mess = String.Format( "CURRENTCOUNT|{0:D2}$BREAK\nTOTALSERVERS|{1:D2}$BREAK\n\n", registeredServers.Count, registeredServers.Count ).ToCharArray(); 
                    bw.Write( mess );


                    foreach ( GameServer server in registeredServers ) {
                        TimeSpan span = DateTime.Now - server.StartTime;
                        IPEndPoint point = ( server.EndPoint as IPEndPoint ) ?? new IPEndPoint( IPAddress.Parse("127.0.0.1"), 8081);

                        bw.Write( 
                            String.Format( 
                                "|[{0:D}:{1:F2}] {2}|1.02|{3}|{4:D}x{5:D}|{6:D}|{7:D}|{8}\n" 
                                , span.Hours
                                , span.Minutes
                                , server.ServerName
                                , server.Map.Name
                                , server.Map.Width
                                , server.Map.Height
                                , server.Map.Difficulty
                                , server.PlayersCount
                                , point.Address
                            )
                        );
                    }
                }
                bytesWritten = bw.BaseStream.Position - begin;
                
                bw.Seek( 1, SeekOrigin.Begin );
                bw.Write( (int)bytesWritten );

                bw.Flush();
            }

            Array.Resize<byte>( ref message, (int)bytesWritten + 5 );

            return message;
        }

        /// <summary>
        /// get characters of user from storage
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        private IList<HatCharacter> getCharacterList( string login ) {
            HatUser user = factory.LookupFirst<HatUser>( new SelectByCodeCondition( login ) );
            if ( user == null ) {
                return new List<HatCharacter>();
            }

            return user.CharacterList;
        }

        /// <summary>
        /// get registered servers list
        /// </summary>
        /// <returns></returns>
        private IList<GameServer> getServerList() {
            return factory.Lookup<GameServer>( new SelectAllCondition() );
        }

        private bool LoginUser( string login, string password ) {
            HatUser u = factory.LookupFirst<HatUser>( new SelectByCodeCondition( login ) );
            if ( u != null && !u.IsLocked && !u.UserLoggedIn && u.Password == password ) {
                //u.UserLoggedIn = true;

                factory.Save<HatUser>( u );

                LocalDataStoreSlot slot = Thread.GetNamedDataSlot( "currentUser" );

                Thread.SetData( slot, u );

                return true;
            }
            if( u == null /*&& conf.IsRegistrationAllowed*/) {
                u = new HatUser();
                u.Login = login;
                u.Password = password;
                u.IsLocked = false;
                //u.UserLoggedIn = true;

                factory.Save<HatUser>( u );

                LocalDataStoreSlot slot = Thread.GetNamedDataSlot( "currentUser" );

                Thread.SetData( slot, u );

                return true;
            }
            return false;
        }

        private bool LogoutUser( string login) {
            HatUser u = factory.LookupFirst<HatUser>( new SelectByCodeCondition( login ) );

            if( u.UserLoggedIn == false ) {
                return false;
            }

            u.UserLoggedIn = false;

            factory.Save<HatUser>( u );

            return true;
        }

        private object ClientPacketParse( byte[] decoded, Socket handler ) {
            object ret = null;

            using ( MemoryStream mem = new MemoryStream( decoded ) ) {
                BinaryReader br = new BinaryReader( mem );
                byte packetID = br.ReadByte();
            
                switch ( packetID ) {
                    case 0xc9: //login
                    {
                        LoginPacket lp = new LoginPacket();
                        
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
                        return lp;
                    }
                    case 0x4e: //check nickname
                    {
                        br.BaseStream.Seek( 5, SeekOrigin.Begin );
                        byte length = br.ReadByte();
                        br.BaseStream.Seek( 1, SeekOrigin.Current );
                        string nickname = Encoding.Default.GetString(br.ReadBytes(length));
                        return check_nickname( nickname );
                    }
                    case 0xC8: {
                        return Client_operation.M_GET_SERVER_LIST;
                        break;
                    }
                    case 0xD1: {
                        return ParseState.SERVER_CONNECTED;
                    }
                    case 0xCB: {
                        // new character to server

                        //                BODY  MIND  SKILL COLOR
                        //                 |     |      |     |
                        // ?? SIZE ID1 ID2 ?? ?? ?? ?? ?? ?? ?? ?? NICK SRV_IP
                        //  |                 |     |     |      |
                        // SIG              REACT SPIRIT PIC  NICK_SIZE 
                        // 1   +4   +4  +4 +1 +1 +1 +1 +1 +1 +1 +1

                        Int32 size = br.ReadInt32();
                        Int32 id1 = br.ReadInt32();
                        Int32 id2 = br.ReadInt32();
                        byte body = br.ReadByte();
                        byte react = br.ReadByte();
                        byte mind = br.ReadByte();
                        byte spirit = br.ReadByte();
                        byte skill = br.ReadByte();
                        byte picture = br.ReadByte();
                        byte color = br.ReadByte();
                        byte nickSize = br.ReadByte();
                        string nickName = Encoding.Default.GetString( br.ReadBytes( nickSize ) );
                        string clanName = "";
                        string srvIP = Encoding.Default.GetString( br.ReadBytes( size - (int)br.BaseStream.Position ) );

                        string[] nick = nickName.Split( '|' );
                        nickName = nick[0];

                        HatCharacter chr = new HatCharacter();
                        chr.IDs[0] = id1;
                        chr.IDs[1] = id2;
                        chr.Clan = clanName;
                        chr.Color = color;
                        chr.Mind = mind;
                        chr.Nickname = nickName;
                        chr.ParentUser = (HatUser)Thread.GetData( Thread.GetNamedDataSlot( "currentUser" ) );
                        
                        bool hasPic = Array.Exists<byte>( Consts.Pics, delegate( byte pic ) { return pic == picture;} );
                        
                        if( hasPic ) {
                            chr.Pic = picture;
                        } else {
                            chr.Pic = Consts.Pics[0];
                        }

                        chr.React = react;
                        chr.Skill = skill;
                        chr.Spirit = spirit;

                        chr.SrvIP = srvIP;

                        Db4oFactory.GetInstance().Save<HatCharacter>( chr );
                        break;
                    }
                    case 0xCA: { //get character
                        Int32 id1, id2;

                        id1 = br.ReadInt32();
                        id2 = br.ReadInt32();


                        break;
                    }
                    default:
                        ret = ParseState.UNKNOWN_PACKET;
                        
                        break;
                }
            }

            return ret;
        }

        private Client_message check_nickname( string nickname ) {
            List<HatCharacter> lst = new List<HatCharacter>(factory.Lookup<HatCharacter>( new SelectAllCondition() ));

            if ( lst.Exists( delegate( HatCharacter ch ) { return ch.Nickname == nickname; } ) ) {
                return Client_message.M_ALREADY_EXISTS;
            }

            return Client_message.M_ALL_OK;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mess"></param>
        /// <returns></returns>
        private byte[] PrepareMessageToSend( byte[] mess ) {
            byte[] message;

            if( mess == null ) {
                message = new byte[0];
            } else {
                message = mess;
            }

            int packetLength = message.Length + Consts.PacketEnding.Length + 1;

            byte[] packet = new byte[8 + packetLength];
            byte[] encoded = new byte[packetLength];
            
            //compiling response
            using ( MemoryStream mem = new MemoryStream( encoded ) ) {
                BinaryWriter bw = new BinaryWriter( mem );
                if ( message.Length != 0 ) {
                    bw.Write( message );
                    bw.Write( (byte) 0x00 );
                }

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

    public delegate void HatEvent( object sender, HatEventArgs args );

    [Serializable]
    public class HatEventArgs {
        /// <summary>
        /// type of event
        /// </summary>
        private EventType type;

        /// <summary>
        /// Text message
        /// </summary>
        private string message;

        /// <summary>
        /// handled exception
        /// </summary>
        private Exception ex;

        public HatEventArgs() {}

        public HatEventArgs( EventType type, string message, Exception ex ) {
            this.type = type;
            this.message = message;
            this.ex = ex;
        }

        public HatEventArgs( EventType type, string message ) {
            this.type = type;
            this.message = message;
        }

        /// <summary>
        /// type of event
        /// </summary>
        public EventType Type {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Text message
        /// </summary>
        public string Message {
            get { return message; }
            set { message = value; }
        }

        /// <summary>
        /// handled exception
        /// </summary>
        public Exception Ex {
            get { return ex; }
            set { ex = value; }
        }
    }

    public enum EventType {
        ERROR
        , INFORMATION
        , WARNING
        , DEBUG
    }
}
