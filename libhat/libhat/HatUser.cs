using System;
using System.Collections.Generic;
using System.Text;
using libhat.DBFactory;

namespace libhat {
    [Serializable]
    public class HatUser : IEntity {
        private int userID;
        private string login;
        private string password;
        private bool isLocked;
        private List<HatCharacter> characterList;
        private bool userLoggedIn;

        #region IEntity Members

        public string Code {
            get { return login; }
            set { login = value; }
        }

        #endregion

        public int UserID {
            get { return userID; }
            set { userID = value; }
        }

        public string Login {
            get { return login; }
            set { login = value; }
        }

        public string Password {
            get { return password; }
            set { password = value; }
        }

        public bool IsLocked {
            get { return isLocked; }
            set { isLocked = value; }
        }
        
        public List<HatCharacter> CharacterList {
            get { return characterList; }
            set { characterList = value; }
        }


        public bool UserLoggedIn {
            get { return userLoggedIn; }
            set { userLoggedIn = value; }
        }

        ///<summary>
        ///Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        ///</summary>
        ///
        ///<returns>
        ///A hash code for the current <see cref="T:System.Object"></see>.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override int GetHashCode() {
            return login.GetHashCode();
        }
    }
}
