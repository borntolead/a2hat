using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace libhat_ng.Entity
{
    [Serializable]
    public class GameServer : ICloneable {

        /// <summary>
        /// Server Name
        /// </summary>
        public string ServerName {
            get;
            set;
        }

        public ServerType ServerType {
            get;
            set;
        }

        /// <summary>
        /// Current server map
        /// </summary>
        public GameMap Map {
            get;
            set;
        }

        /// <summary>
        /// How much palyers now play
        /// </summary>
        public int PlayersCount {
            get;
            set;
        }

        /// <summary>
        /// End point of server
        /// </summary>
        public EndPoint EndPoint {
            get;
            set;
        }

        /// <summary>
        /// is this server available for player connection
        /// </summary>
        public bool IsActive {
            get;
            set;
        }

        /// <summary>
        /// Time when server started
        /// </summary>
        public DateTime StartTime {
            get;
            set;
        }

        ///<summary>
        ///Creates a new object that is a copy of the current instance.
        ///</summary>
        ///
        ///<returns>
        ///A new object that is a copy of this instance.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public object Clone() {
            var srv = new GameServer();
            
            srv.EndPoint = EndPoint;
            srv.ServerType = ServerType;
            srv.IsActive = IsActive;
            srv.Map = Map;
            srv.PlayersCount = PlayersCount;
            srv.ServerName = ServerName;
            srv.StartTime = StartTime;

            return srv;
        }
    }
}