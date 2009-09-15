using System;
using System.Collections.Generic;
using System.Text;

namespace libhat_ng.Entity
{
    [Serializable]
    public class HatUser {
        public string Login {
            get;
            set;
        }

        public string Password {
            get;
            set;
        }

        public bool IsLocked {
            get;
            set;
        }

        public bool UserLoggedIn {
            get;
            set;
        }
    }
}