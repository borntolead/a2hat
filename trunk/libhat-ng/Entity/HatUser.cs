using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace libhat_ng.Entity
{
    [Serializable]
    public class HatUser {
    	
    	private string login;
        private string password;
        
    	public string Login {
    		get{ return login; }
    		set{ login = value; }
        }

        public string Password {
            get{ return password; }
    		set{ password = value; }
        }

        public bool IsLocked() {
           return false;
        }
		
    	public bool IsLoggedIn () {
            return false;
        }
    }
}