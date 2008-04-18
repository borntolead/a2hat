using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace libhat {
    public interface IHatEntity : ISerializable {

        /// <summary>
        /// 
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