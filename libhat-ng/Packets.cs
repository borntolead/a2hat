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

		/// <summary>
		/// Process parsed data and returns unencrypted response 
		/// </summary>
		/// <returns>
		/// A <see cref="System.Byte"/> unencrypted response
		/// </returns>
        byte[] Execute();
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

        public byte[] Execute()
        {
            // search login into database
			
			// if found, then return account data 
			// else return fail or create new account.
        }
    }

    public class UnknownCommand: ICommand
    {
		int packetID;		
		
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
    }
}
