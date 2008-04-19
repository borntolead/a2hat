using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using libhat.DBFactory;

namespace libhat {
    [Serializable]
    public class GameServer : IEntity, ICloneable, ISynchronizable {
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

        private ServerType gameType;

        public ServerType ServerType {
            get { return gameType; }
            set { gameType = value; }
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

        #region ICloneable Members

        ///<summary>
        ///Creates a new object that is a copy of the current instance.
        ///</summary>
        ///
        ///<returns>
        ///A new object that is a copy of this instance.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public object Clone() {
            GameServer srv = new GameServer();

            srv.code = code;
            srv.endPoint = endPoint;
            srv.gameType = gameType;
            srv.isActive = isActive;
            srv.map = map;
            srv.playersCount = playersCount;
            srv.serverName = serverName;
            srv.startTime = srv.startTime;

            return srv;
        }

        #region ISynchronizable Members

        /// <summary>
        /// Merge THIS and inputObject.  
        /// </summary>
        /// <param name="inputObject"></param>
        public void Merge( object inputObject ) {
            GameServer gs = inputObject as GameServer;
            
            if ( gs == null ) {
                return;
            }

            serverName = gs.serverName;
            code = gs.Code;
            endPoint = gs.endPoint;
        }

        #endregion

        #endregion
    }
}
