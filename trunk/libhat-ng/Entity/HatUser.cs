using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.IO;

namespace libhat_ng.Entity
{
    [Serializable]
    public class HatUser:ISerializable {
    	
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

        public List<HatCharacter> GetCharacters()
        {
            throw new NotImplementedException();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Login", Login);
            info.AddValue( "Password", Password);
        }

        protected HatUser(SerializationInfo info, StreamingContext ctxt)
        {
            Login = info.GetString("Login");
            Password = info.GetString("Password");
        }

        public HatUser()
        {
        }
    }
}