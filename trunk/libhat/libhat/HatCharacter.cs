using System;
using System.Collections.Generic;
using System.Text;
using libhat.DBFactory;

namespace libhat {
    [Serializable]
    public class HatCharacter :IEntity{
        private string parentUserCode;
        private int characterID;
        private byte[] characterData;
        private string nickname;
        
        #region IEntity Members

        public string Code {
            get { return nickname; }
            set { nickname = value; }
        }

        #endregion

        public HatUser ParentUser {
            get { return Db4oFactory.GetInstance().LookupFirst<HatUser>( new SelectByCodeCondition( parentUserCode )); }
            set { parentUserCode = value != null ? value.Code : ""; }
        }

        public string ParentUserCode {
            get { return parentUserCode; }
            set { parentUserCode = value; }
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
    }
}
