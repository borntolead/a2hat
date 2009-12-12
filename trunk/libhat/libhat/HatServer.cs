#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using libhat.BE;
using libhat.DBFactory;

#endregion

namespace libhat {
    public sealed partial class HatServer {
        private GameServer current;

        private bool exitCalled;
        private Thread listenClientsThread;

        #region Critical lists

        private Dictionary<string, LoginPacket> loggedInUsers = new Dictionary<string, LoginPacket>();
        private List<GameServer> registeredServers = new List<GameServer>();

        #endregion

        public event EventHandler ClientConnected;

        /// <summary>
        /// Calling when hat handling inner exceptions and send other information
        /// </summary>
        public event HatEvent EventOccured;

        public bool Configure(string pathToConfig) {
            return Configure(getConfiguration(pathToConfig));
        }

        private HatConfiguration getConfiguration(string config) {
            return null;
        }

        public bool Configure(HatConfiguration conf) {
            return false;
        }

        private void LogEvent(EventType type, Exception ex, string formatMessage, params object[] formatMessageArgs) {
            if (EventOccured != null) {
                EventOccured(this, new HatEventArgs(type, String.Format(formatMessage, formatMessageArgs), ex));
            }
        }

        private void LogEvent(EventType type, string formatMessage, params object[] formatMessageArgs) {
            LogEvent(type, null, formatMessage, formatMessageArgs);
        }

        /// <summary>
        /// Start an hat instance on specified IP and port
        /// </summary>
        /// <param name="ip_address">IP for binding</param>
        /// <param name="port">port for listen</param>
        /// <returns>return false if hat would not start</returns>
        /// <exception cref="ArgumentNullException">throws when ip_address is null</exception>
        public bool Start(string ip_address, int port) {
            var ep = new IPEndPoint(IPAddress.Parse(ip_address), port);
            ThreadPool.SetMinThreads(5, 2);
            //TODO configuring from file
           
            var listener = NetworkHelper.GetNewIPv4Socket(ep);
            listener.Listen(10);

            ParameterizedThreadStart lthread = listenerThreadStartMethod;
            listenClientsThread = new Thread(lthread);
            listenClientsThread.IsBackground = true;
            listenClientsThread.Start(listener);

            Console.CancelKeyPress += Console_CancelKeyPress;

            while (true) {
                if (exitCalled) {
                    break;
                }
                Thread.Sleep(100);
            }

            listenClientsThread.Abort();

            return true;
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e) {
            exitCalled = true;
        }

        /// <summary>
        /// Thread which accept connections from specified ip/port for client connections in endless loop.
        /// </summary>
        /// <param name="obj">Binded socket</param>
        private void listenerThreadStartMethod(object obj) {
            var listener = obj as Socket;

            if (listener == null) {
                Console.WriteLine("invalid argument for listener thread application aborted");
                Thread.CurrentThread.Abort();
                return;
            }

            while (true) {
                Console.WriteLine("Waiting for a connection...");
                var handler = listener.Accept();

                ThreadPool.QueueUserWorkItem(stateInfo => HandleConnection(handler));
            }
        }

        private HatUser GetCurrentUser() {
            var slot = Thread.GetNamedDataSlot("currentUser");
            return (HatUser) Thread.GetData(slot);
        }

        private void SetCurrentUser(HatUser usr) {
            var slot = Thread.GetNamedDataSlot("currentUser");

            Thread.SetData(slot, usr);
        }

        private void HandleConnection(Socket handler) {
            var buf = new byte[2048];
            uint packetLength, unknown1;
            var bytesRec = 0;
            while (true) {
                try {
                    bytesRec = handler.Receive(buf);
                } catch (Exception ex) {
                    LogEvent(EventType.DEBUG, ex, "recieve failed");
                    break;
                }

                /// move it to separate process
                using (var mem = new MemoryStream(buf)) {
                    mem.Seek(0, SeekOrigin.Begin);
                    var br = new BinaryReader(mem);

                    packetLength = br.ReadUInt32();
                    unknown1 = br.ReadUInt32();

                    Console.WriteLine("packet recived\n length:{0}\n unknown1: {1}", packetLength, unknown1);

                    var decoded = new byte[packetLength];

                    Buffer.BlockCopy(buf,
                                     (int) br.BaseStream.Position,
                                     decoded, 0, (int) packetLength);
                    decoded = NetworkHelper.PacketDecoding(decoded);
                    NetworkHelper.DumpArray(Console.OpenStandardOutput(), decoded);

                    object packet = null;
                    try {
                        packet = ClientPacketParse(decoded, handler);
                    } catch (Exception ex) {
                        LogEvent(EventType.ERROR, ex, "packet {0} was processed with errors. breaking connection");
                        handler.Close(5);

                        break;
                    }

                    if (packet is ParseState) {
                        switch ((ParseState) packet) {
                            case ParseState.SERVER_CONNECTED: {
                                var parameters = new object[] {handler, buf};

                                var listenServerThread = new Thread(serverInfoListener);
                                listenServerThread.Start(parameters);

                                break;
                            }
                            case ParseState.UNKNOWN_PACKET: {
                                NetworkHelper.DumpArray(Console.OpenStandardOutput(), decoded);
                                Console.WriteLine(Encoding.GetEncoding(866).GetString(decoded));

                                handler.Close(5);
                                break;
                            }
                        }

                        LogEvent(EventType.DEBUG, "{0} packet was got", packet.ToString());
                        break;
                    }

                    byte[] response = ProcessPacket(packet);

                    Console.WriteLine("response:");

                    NetworkHelper.DumpArray(Console.OpenStandardOutput(), response);
                    NetworkHelper.DumpArray(Console.OpenStandardOutput(), PrepareMessageToSend(response));

                    try {
                        handler.Send(PrepareMessageToSend(response));
                    } catch (Exception ex) {
                        LogEvent(EventType.ERROR, ex, "send failed");
                    }
                }
            }
        }

        /// <summary>
        /// Processing packet
        /// </summary>
        /// <param name="packet"> return response to recived packet</param>
        /// <returns></returns>
        private byte[] ProcessPacket(object packet) {
            if (packet is LoginPacket) {
                LoginPacket pack = (LoginPacket) packet;
                if (LoginUser(pack.login, pack.password)) {
                    lock (loggedInUsers) {
                        LoginPacket gt;

                        if (loggedInUsers.TryGetValue(pack.login, out gt)) {
                            return processClientMessage(Client_operation.M_SEND_MESSAGE, Client_message.M_LOGIN_LOCKED, null);
                        }

                        loggedInUsers.Add(pack.login, pack);
                    }

                    return GetCharacterList(pack);
                }
                
                return processClientMessage(Client_operation.M_SEND_MESSAGE, Client_message.M_INVALID_LOGIN_PASSWORD, null);
            }

            if (packet is Client_message) {
                Client_message mess = (Client_message) packet;
                return processClientMessage(Client_operation.M_CHECK_NICKNAME, mess, null);
            }

            if (packet is Client_operation) {
                return GetServersList();
            }

            return new byte[0];
        }

        private byte[] processClientMessage(Client_operation op, Client_message packet, byte[] message) {
            var size = sizeof (Byte) + sizeof (Int32) + (message != null ? message.Length : 0);

            var response = new byte[size];

            using (var mem = new MemoryStream(response)) {
                var bw = new BinaryWriter(mem);

                bw.Write((byte) op);
                bw.Write((Int32) packet);
                if (message != null) {
                    bw.Write(message);
                }
            }

            return response;
        }

        /// <summary>
        /// get list of user's characters
        /// </summary>
        /// <param name="pack">user's credentials</param>
        /// <returns> byte array represents list of user's characters</returns>
        private byte[] GetCharacterList(LoginPacket pack) {
           var characters = getCharacterList(pack.login);

            var chars = new byte[characters.Count*8 + 9];
            using (var mem = new MemoryStream(chars)) {
                var bw = new BinaryWriter(mem);

                bw.Write((byte) 0xCE);
                bw.Write((characters.Count*8) + 4);
                bw.Write(Consts.HatIdentifier);
                foreach (HatCharacter user in characters) {
                    bw.Write(user.CharacterID);
                }
            }

            return chars;
        }

        /// <summary>
        /// Gets registred and available servers list
        /// </summary>
        /// <returns></returns>
        private byte[] GetServersList() {
            var servers = getServerList();

            var message = new byte[1 + 0xFF + servers.Count * 0x64];
            long bytesWritten = 0;
            using( var mem = new MemoryStream( message ) ) {
                var bw = new BinaryWriter( mem );

                bw.Write((byte) 0xCD);
                bw.Write(Int32.MaxValue);

                long begin = bw.BaseStream.Position;

                lock (registeredServers) {
                    char[] mess = String.Format("CURRENTCOUNT|{0:D2}$BREAK\nTOTALSERVERS|{1:D2}$BREAK\n\n", registeredServers.Count, registeredServers.Count).ToCharArray();
                    bw.Write(mess);


                    foreach( var server in registeredServers ) {
                        var span = DateTime.Now - server.StartTime;
                        var point = ( server.EndPoint as IPEndPoint ) ?? new IPEndPoint( IPAddress.Parse( "127.0.0.1" ), 8081 );

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

                bw.Seek(1, SeekOrigin.Begin);
                bw.Write((int) bytesWritten);

                bw.Flush();
            }

            Array.Resize(ref message, (int) bytesWritten + 5);

            return message;
        }

        /// <summary>
        /// get characters of user from storage
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        private IList<HatCharacter> getCharacterList(string login) {
                return new List<HatCharacter>();
        }

        /// <summary>
        /// get registered servers list
        /// </summary>
        /// <returns></returns>
        private IList<GameServer> getServerList() {
            return new List<GameServer>();
        }

        private bool LoginUser(string login, string password) {
            return false;
        }

        private bool LogoutUser(string login) {
            return false;
        }

        private object ClientPacketParse(byte[] decoded, Socket handler) {
            object ret = null;

            using (MemoryStream mem = new MemoryStream(decoded)) {
                BinaryReader br = new BinaryReader(mem);
                byte packetID = br.ReadByte();

                switch (packetID) {
                    case 0xc9: //login
                    {
                        LoginPacket lp = new LoginPacket();

                        byte loginlen = br.ReadByte();
                        lp.GameType = (GameType) br.ReadInt16();
                        byte loginOffset = br.ReadByte();
                        byte credentialsLength = br.ReadByte();

                        mem.Seek(loginOffset, SeekOrigin.Begin);

                        lp.login = Encoding.Default.GetString(br.ReadBytes(loginlen));
                        lp.password = Encoding.Default.GetString(br.ReadBytes(credentialsLength - loginlen));

                        Console.WriteLine("user logging in");
                        Console.WriteLine("GameType: {0}", lp.GameType);
                        Console.WriteLine("Login: {0}", lp.login);
                        Console.WriteLine("Password: {0}\n", lp.password);
                        return lp;
                    }
                    case 0x4e: //check nickname
                    {
                        br.BaseStream.Seek(5, SeekOrigin.Begin);
                        byte length = br.ReadByte();
                        br.BaseStream.Seek(1, SeekOrigin.Current);
                        string nickname = Encoding.Default.GetString(br.ReadBytes(length));
                        return check_nickname(nickname);
                    }
                    case 0xC8: {
                        return Client_operation.M_GET_SERVER_LIST;
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
                        string nickName = Encoding.Default.GetString(br.ReadBytes(nickSize));
                        string clanName = "";
                        string srvIP = Encoding.Default.GetString(br.ReadBytes(size - (int) br.BaseStream.Position));

                        string[] nick = nickName.Split('|');
                        nickName = nick[0];

                        var chr = new HatCharacter();
                        chr.IDs[0] = id1;
                        chr.IDs[1] = id2;
                        chr.Clan = clanName;
                        chr.Color = color;
                        chr.Mind = mind;
                        chr.Nickname = nickName;
                        chr.ParentUser = (HatUser) Thread.GetData(Thread.GetNamedDataSlot("currentUser"));

                        bool hasPic = Array.Exists(Consts.Pics, pic => pic == picture);

                        if (hasPic) {
                            chr.Pic = picture;
                        }
                        else {
                            chr.Pic = Consts.Pics[0];
                        }

                        chr.React = react;
                        chr.Skill = skill;
                        chr.Spirit = spirit;

                        chr.SrvIP = srvIP;

                        //save character
                        throw new NotImplementedException();
                        break;
                    }
                    case 0xCA: {
                        //get character
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

        private Client_message check_nickname(string nickname) {
           return Client_message.M_ALREADY_EXISTS;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mess"></param>
        /// <returns></returns>
        private byte[] PrepareMessageToSend(byte[] mess) {
            byte[] message;

            if (mess == null) {
                message = new byte[0];
            }
            else {
                message = mess;
            }

            int packetLength = message.Length + Consts.PacketEnding.Length + 1;

            byte[] packet = new byte[8 + packetLength];
            byte[] encoded = new byte[packetLength];

            //compiling response
            using (MemoryStream mem = new MemoryStream(encoded)) {
                BinaryWriter bw = new BinaryWriter(mem);
                if (message.Length != 0) {
                    bw.Write(message);
                    bw.Write((byte) 0x00);
                }

                bw.Write(Consts.PacketEnding);

                bw.Flush();
            }

            //making formatted packet
            using (MemoryStream mem = new MemoryStream(packet)) {
                BinaryWriter bw = new BinaryWriter(mem);
                if (packetLength > Consts.PacketMaxLength) {
                    bw.Write(0x8E);
                    bw.Write(0);
                }
                else {
                    // some redundant paranoia
                    bw.Write((packetLength));
                    bw.Write(0x01000000);
                }


                bw.Write(NetworkHelper.PacketEncoding(encoded));

                bw.Flush();
            }

            return packet;
        }
    }

    public delegate void HatEvent(object sender, HatEventArgs args);

    [Serializable]
    public class HatEventArgs {
        public HatEventArgs() {
        }

        public HatEventArgs(EventType type, string message, Exception ex) {
            this.Type = type;
            this.Message = message;
            this.Ex = ex;
        }

        public HatEventArgs(EventType type, string message) : this( type, message, null) {}

        /// <summary>
        /// type of event
        /// </summary>
        public EventType Type { get; set; }

        /// <summary>
        /// Text message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// handled exception
        /// </summary>
        public Exception Ex { get; set; }
    }

    public enum EventType {
        ERROR,
        INFORMATION,
        WARNING,
        DEBUG
    }
}