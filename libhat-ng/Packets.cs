using System;
using System.IO;
using System.Text;

namespace libhat_ng
{
    public interface ICommand
    {
        /// <summary>
        /// Parse data array to packet
        /// </summary>
        /// <param name="data"></param>
        void ParseData(byte[] data);

        void Execute();
        
        /// <summary>
        /// Build byte array from packet fields
        /// </summary>
        /// <returns></returns>
        byte[] GetResponse();
        
        int PacketID{
            get;
        }
    }

    public class LoginCommand : ICommand
    {
        public GameType GameType { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public void ParseData(byte[] data)
        {
            using (var mem = new MemoryStream(data))
            {
                mem.Seek(0, SeekOrigin.Begin);
                using (var reader = new BinaryReader(mem))
                {
                    var packetid = reader.ReadByte();

                    if( packetid != PacketID )
                    {
                        throw new ArgumentException("Invalid data");
                    }

                    var loginlen = reader.ReadByte();
                    GameType = (GameType)reader.ReadInt16();
                    var loginOffset = reader.ReadByte();
                    var credentialsLength = reader.ReadByte();

                    mem.Seek(loginOffset, SeekOrigin.Begin);

                    Login = Encoding.Default.GetString(reader.ReadBytes(loginlen));
                    Password = Encoding.Default.GetString(reader.ReadBytes(credentialsLength - loginlen));
                }
            }
        }

        public byte[] GetResponse()
        {
            return null;
        }

        public int PacketID
        {
            get { return 0xc9; }
        }

        public void Execute()
        {
            
        }
    }

    public class UnknownCommand: ICommand
    {
        public void ParseData(byte[] data)
        {
            
        }

        public void Execute()
        {
            
        }

        public byte[] GetResponse()
        {
            return null;
        }

        public int PacketID
        {
            get { return 0xFF; }
        }
    }
}
