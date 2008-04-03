using System;
using System.Collections.Generic;
using System.Text;

namespace simpleListener {
    struct LoginPacket {
        public byte packetID;
        public byte gameType;
        public string login;
        public string password;
    }
}
