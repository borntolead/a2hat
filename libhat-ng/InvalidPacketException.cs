using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libhat_ng {
    public class InvalidPacketException : Exception {
        public byte[] Packet{ get; set;}
    }
}
