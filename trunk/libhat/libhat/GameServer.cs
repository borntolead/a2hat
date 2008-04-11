using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace libhat {
    public class GameServer {
        /// <summary>
        /// Server Name
        /// </summary>
        private string serverName;

        /// <summary>
        /// Current server map
        /// </summary>
        private GameMap map;

        /// <summary>
        /// How much palyers now play
        /// </summary>
        private int playersCount;

        /// <summary>
        /// End point of server
        /// </summary>
        private EndPoint endPoint;


        /// <summary>
        /// Server Name
        /// </summary>
        public string ServerName {
            get { return serverName; }
            set { serverName = value; }
        }

        /// <summary>
        /// Current server map
        /// </summary>
        public GameMap Map {
            get { return map; }
            set { map = value; }
        }

        /// <summary>
        /// How much palyers now play
        /// </summary>
        public int PlayersCount {
            get { return playersCount; }
            set { playersCount = value; }
        }

        /// <summary>
        /// End point of server
        /// </summary>
        public EndPoint EndPoint {
            get { return endPoint; }
            set { endPoint = value; }
        }
    }
}
