using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace libhat {
    public class HatConfiguration {
        private string hatName;
        private int hatID;
        private EndPoint hatClientEndPoint;
        private EndPoint hatServerEndPoint;
        private string dbHome;
        private string dbName;
        private bool isRegistrationAllowed;

        public string HatName {
            get { return hatName; }
            set { hatName = value; }
        }

        public int HatID {
            get { return hatID; }
            set { hatID = value; }
        }

        public EndPoint HatClientEndPoint {
            get { return hatClientEndPoint; }
            set { hatClientEndPoint = value; }
        }

        public EndPoint HatServerEndPoint {
            get { return hatServerEndPoint; }
            set { hatServerEndPoint = value; }
        }

        public string DbHome {
            get { return dbHome; }
            set { dbHome = value; }
        }

        public string DbName {
            get { return dbName; }
            set { dbName = value; }
        }

        public bool IsRegistrationAllowed {
            get { return isRegistrationAllowed; }
            set { isRegistrationAllowed = value; }
        }
    }
}
