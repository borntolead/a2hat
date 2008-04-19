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
        
        public IList<HatCharacter> CharacterList {
            get { return Db4oFactory.GetInstance().Lookup<HatCharacter>( new SelectCharacterByParentCondition( login ) ); }
        }


        public bool UserLoggedIn {
            get { return userLoggedIn; }
            set { userLoggedIn = value; }
        }
    }

    public class SelectCharacterByParentCondition : ICondition {
        private string parentCode;
        public SelectCharacterByParentCondition( string parentCode ) {
            this.parentCode = parentCode;
        }

        public string ParentCode {
            get { return parentCode; }
            set { parentCode = value; }
        }

        #region ICondition Members

        public string Name {
            get { return "SELECT_CHARACTER_BY_PARENT"; }
        }

        #endregion
    }
}
