using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using libhat.BE;

namespace libhat {
    public sealed partial class HatServer {
        private void serverInfoListener(object parameters) {
            GameServer srv = null;

            object[] param = null;
            Socket handler = null;
            byte[] buf = null;
            try {
                param = (object[]) parameters;
                handler = (Socket) param[0];
                buf = (byte[]) param[1];
            } catch (Exception ex) {
                LogEvent(EventType.ERROR, ex, "server handler has incorrect parameters");
                if (handler != null) {
                    handler.Close(5);
                }
                return;
            }

            uint packetLength, unknown1;
            while (true) {
                using (var mem = new MemoryStream(buf)) {
                    mem.Seek(0, SeekOrigin.Begin);
                    var binRead = new BinaryReader(mem);

                    packetLength = binRead.ReadUInt32();
                    unknown1 = binRead.ReadUInt32();

                    var decoded = new byte[packetLength];

                    Buffer.BlockCopy(buf,
                                     (int) binRead.BaseStream.Position,
                                     decoded, 0, (int) packetLength);
                    Console.WriteLine("recieved packet");
                    decoded = NetworkHelper.PacketDecoding(decoded);
                    NetworkHelper.DumpArray(Console.OpenStandardOutput(), decoded);

                    using (var mstr = new MemoryStream(decoded)) {
                        var br = new BinaryReader(mstr);
                        byte packetID = br.ReadByte();

                        switch (packetID) {
                                /// bad code warning!
                                /// section below contains processing of 2 in series
                            case 0xD1: {
                                //ident server
                                br.BaseStream.Seek(5, SeekOrigin.Begin);
                                byte length = br.ReadByte();
                                br.BaseStream.Seek(1, SeekOrigin.Current);
                                string addr = Encoding.Default.GetString(br.ReadBytes(length));
                                string[] arr = addr.Split(':');
                                var buffer = new byte[0x8e];
                                EndPoint ep = new IPEndPoint(IPAddress.Parse(arr[0]), Int32.Parse(arr[1]));

                                srv = new GameServer();
                                srv.EndPoint = ep;
                                srv.Code = ep.ToString();

                                handler.Send(PrepareMessageToSend(serverWelcome()));

                                break;
                            }
                            case 0xd2: {
                                if (srv == null) {
                                    break;
                                }
                                byte pID = br.ReadByte();

                                byte pcount = br.ReadByte();
                                byte difficulty = br.ReadByte();
                                byte srvType = br.ReadByte();
                                byte mapSize = br.ReadByte();

                                if (pcount == 0xff && difficulty == 0xff && srvType == 0xff && mapSize == 0xff) {
                                    lock (registeredServers) {
                                        handler.Close(5);
                                        registeredServers.Remove(srv);
                                        srv = null;
                                        return;
                                        break;
                                    }
                                }

                                byte ln = br.ReadByte();

                                br.BaseStream.Seek(1, SeekOrigin.Current);
                                string mapName = Encoding.Default.GetString(br.ReadBytes(ln));

                                int idx = -1;
                                if (registeredServers.Contains(srv)) {
                                    idx = registeredServers.IndexOf(srv);
                                }
                                srv.PlayersCount = pcount;
                                srv.ServerType = (ServerType) srvType;
                                srv.Map = new GameMap();
                                srv.Map.Difficulty = (Difficulty) difficulty;
                                srv.Map.Name = mapName;
                                srv.Map.Height = srv.Map.Width = mapSize;

                                if (idx == -1) {
                                    registeredServers.Add(srv);
                                }
                                else {
                                    registeredServers[idx] = srv;
                                }
                                handler.Send(PrepareMessageToSend(null));
                                break;
                            }
                            case 0xd4: // arena results
                            {
                                LogEvent(EventType.DEBUG, "arena results not supported");
                                handler.Send(PrepareMessageToSend(null));

                                break;
                            }
                            case 0xE0: {
                                // chr_recv update character

                                ///structure:
                                /// 0x00 - packet type
                                /// 0x01 - size
                                /// 0x02 - charID 1
                                /// 0x06 - charID 2
                                /// 0x0A - log_size
                                /// 0x0B - chr_size
                                /// 0x12 - characterData
                                /// 0x12 + chrSize - log_name

                                byte size = br.ReadByte();
                                if (size == 1) {
                                    break;
                                }
                                Int32 characterID1 = br.ReadInt32();
                                Int32 characterID2 = br.ReadInt32();
                                Int32 logSize = br.ReadInt32();

                                byte chr_size = br.ReadByte();

                                byte[] chrData = br.ReadBytes(chr_size);
                                string loginIP = Encoding.Default.GetString(br.ReadBytes(logSize));

                                var chr = new HatCharacter(); // stub

                                if (chr == null) {
                                    chr = new HatCharacter();

                                    chr.IDs[0] = characterID1;
                                    chr.IDs[1] = characterID2;

                                    chr.CharacterData = chrData;
                                }
                                else {
                                    chr.CharacterData = chrData;
                                }

                                //saving

                                break;
                            }
                            case 0xcf: // chr_recv return character
                            {
                                ///structure:
                                /// 0x00 - packet type
                                /// 0x01 - size
                                /// 0x02 - charID 1
                                /// 0x06 - charID 2
                                /// 0x0A - log_size
                                /// 0x0B - chr_size
                                /// 0x12 - characterData
                                /// 0x12 + chrSize - log_name

                                byte size = br.ReadByte();
                                Int32 characterID1 = br.ReadInt32();
                                Int32 characterID2 = br.ReadInt32();
                                Int32 logSize = br.ReadInt32();

                                byte chr_size = br.ReadByte();

                                byte[] chrData = br.ReadBytes(chr_size);
                                string loginIP = Encoding.Default.GetString(br.ReadBytes(logSize));

                                var chr = new HatCharacter();

                                if (chr == null) {
                                    chr = new HatCharacter();

                                    chr.IDs[0] = characterID1;
                                    chr.IDs[1] = characterID2;

                                    chr.CharacterData = chrData;
                                }
                                else {
                                    chr.CharacterData = chrData;
                                }

                                //save

                                handler.Send(PrepareMessageToSend(simpleServerEntrance(loginIP)));

                                break;
                            }
                            case 0xd8: // accept character
                            case 0xd9: // reject character
                            case 0xE1: // server reconnect
                            case 0x64:
                            default: //keep alive
                                Console.WriteLine("default response send");

                                handler.Send(PrepareMessageToSend(null));
                                break;
                        }
                    }
                }
                try {
                    handler.Receive(buf);
                } catch {
                    return;
                }
            }
        }

        private byte[] serverWelcome() {
            var ret = new byte[9];
            using (var mem = new MemoryStream(ret)) {
                var bw = new BinaryWriter(mem);

                bw.Write((byte) 0xD5);
                bw.Write(Consts.HatIdentifier);
                bw.Write((byte) 0x01);
                bw.Write((byte) 0x00);
                bw.Write((byte) 0x00);
                bw.Write((byte) 0x00);
            }

            return ret;
        }

        /// <summary>
        /// writing character to byte array
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        [Obsolete("not implemented", true)]
        private byte[] serverEntrance(HatCharacter chr) {
            throw new NotImplementedException();

            using (var mem = new MemoryStream()) {
                int sectionSum = 0;
                var bw = new BinaryWriter(mem);

                bw.Write(0x04507989);

                ///making 0xAAAAAAAA section
                Stream csec = new MemoryStream();
                {
                    csec.SetLength(0x34);
                    var wr = new BinaryWriter(csec);

                    ///write character identify
                    wr.Write(chr.IDs[0]);
                    wr.Write(chr.IDs[1]);
                    wr.Write(chr.HatID);

                    ///writing character name

                    wr.Write(chr.Nickname.ToCharArray());
                    if (String.IsNullOrEmpty(chr.Clan) == false) {
                        wr.Write('|');
                        wr.Write(chr.Clan.ToCharArray());
                    }
                    wr.Write((byte) 0x00);

                    wr.BaseStream.Seek(0x2c, SeekOrigin.Begin);

                    ///writing character info
                    wr.Write(chr.Sex);
                    wr.Write(chr.Pic);
                    wr.Write(chr.Unknown1);
                    wr.Write(chr.Unknown2);
                    wr.Write(chr.Unknown3);

                    sectionSum = ChecksumHelper.GetStreamChecksum(mem);
                }
                ///writing 0xAAAAAAAA section encrypted
                int key = 0x0;
                csec = CryptHelper.CharacterStreamCrypt(csec, key);
                var br = new BinaryReader(csec);

                bw.Write(0xAAAAAAAA);
                bw.Write(csec.Length);
                bw.Write(key << 16);
                bw.Write(sectionSum);
                bw.Write(br.ReadBytes((int) csec.Length));

                csec.Dispose();
            }

            return new byte[0];
        }

        private byte[] simpleServerEntrance(string login_name) {
            var result = new byte[login_name.Length + 1 + 4 + 2 + 1];
            using (var mem = new MemoryStream(result)) {
                var bw = new BinaryWriter(mem);

                bw.Write((byte) 0xD3);
                bw.Write(0x0);
                bw.Write(login_name.Length);
                bw.Write(login_name.ToCharArray());

                bw.Flush();
            }

            return result;
        }

        private HatCharacter getCharacter(BinaryReader reader) {
            throw new NotImplementedException();
        }
    }
}