using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using libhat.DBFactory;

namespace libhat {
    [Serializable]
    public class GameServer :IEntity {
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
        /// is this server available for player connection
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Time when server started
        /// </summary>
        private DateTime startTime;

        private string code;

        /// <summary>
        /// Server Name
        /// </summary>
        public string ServerName {
            get { return serverName; }
            set { serverName = value; }
        }

        #region IEntity Members

        public string Code {
            get { return code; }
            set { code = value; }
        }

        #endregion

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

        /// <summary>
        /// is this server available for player connection
        /// </summary>
        public bool IsActive {
            get { return isActive; }
            set { isActive = value; }
        }

        /// <summary>
        /// Time when server started
        /// </summary>
        public DateTime StartTime {
            get { return startTime; }
            set { startTime = value; }
        }
    }
}
