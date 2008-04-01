using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace libhat {
    public class NetworkHelper {
        public static Socket GetNewSocket (ProtocolType type, EndPoint ep) {
            Socket sock = new Socket( AddressFamily.InterNetwork, SocketType.Stream, type );

            sock.Bind( ep );
            return sock;
        }

        public static Socket GetNewIPv4Socket(EndPoint ep) {
            return GetNewSocket( ProtocolType.IPv4, ep );
        }

        public static Socket GetNewIPv6Socket( EndPoint ep ) {
            return GetNewSocket( ProtocolType.IPv6, ep );
        }
    }
}
