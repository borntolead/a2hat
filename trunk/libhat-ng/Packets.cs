using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using libhat_ng.Entity;
using libhat_ng.Helpers;

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

            var characters = new List<object>();

            var chars = new byte[characters.Count * 8 + 9];
            using (var mem = new MemoryStream(chars))
            {
                var bw = new BinaryWriter(mem);

                bw.Write((byte)0xCE);
                bw.Write((characters.Count * 8) + 4);
                bw.Write(Consts.HatIdentifier);
                foreach (var user in characters)
                {
                    //bw.Write(user.CharacterID);
                }

                return chars;
            }
            //return NetworkHelper.ClientMessageBuild(ClientOperation.CheckNickname, ClientMessage.M_ALL_OK,null);
        }
    }

    public class UnknownCommand: ICommand
    {
		public void ParseData(byte[] data)
        {
            NetworkHelper.DumpArray(data);
        }

        public byte[] Execute()
        {
            return null;
        }

        public byte[] GetResponse()
        {
            return null;
        }        
    }
}
