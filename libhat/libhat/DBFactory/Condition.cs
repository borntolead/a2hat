using System;
using System.Collections.Generic;
using System.Text;

namespace libhat.DBFactory {
    public class SelectAllCondition : ICondition {
        private string name = "SELECT_ALL";

        #region ICondition Members

        public virtual string Name {
            get { return name; }
        }
        #endregion
    }

    public class SelectByCodeCondition : SelectAllCondition {
        private List<string> hashCodes;

        public override string Name {
            get { return "SELECT_BY_CODE"; }
        }

        public SelectByCodeCondition() : this( new string[0] ) { }

        public SelectByCodeCondition( params string[] hashes ) {
            hashCodes = new List<string>( hashes );
        }

        public List<string> Codes {
            get { return hashCodes; }
            set { hashCodes = value; }
        }
    }
}
