using System;
using System.Collections.Generic;
using System.Text;
using libhat.DBFactory;

namespace libhat {
    [Serializable]
    public class HatCharacter :IEntity{
        private int parentUserID;
        private int characterID;
        private byte[] characterData;
        private string nickname;
        
        #region IEntity Members

        public string Code {
            get { return nickname; }
            set { nickname = value; }
        }

        #endregion

        public int ParentUserID {
            get { return parentUserID; }
            set { parentUserID = value; }
        }

        public int CharacterID {
            get { return characterID; }
            set { characterID = value; }
        }

        public byte[] CharacterData {
            get { return characterData; }
            set { characterData = value; }
        }

        public string Nickname {
            get { return nickname; }
            set { nickname = value; }
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
            return parentUserID;
        }
    }
}
