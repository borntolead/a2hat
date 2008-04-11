using System;
using System.Collections.Generic;
using System.Text;

namespace libhat {
    public interface PacketInterface {

        /// <summary>
        /// Function for transform packet to byte array
        /// </summary>
        /// <returns>byte array of packet</returns>
        byte[] ToArray();
    }

    public struct LoginPacket {
        public GameType GameType;
        public string login;
        public string password;
        public byte clientLanguage;
        public byte clientVersion;
    }
}