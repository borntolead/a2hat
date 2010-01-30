using System;
using System.Runtime.Serialization;

namespace libhat_ng.Entity
{
    [Serializable]
    public class HatCharacter : ISerializable {


        public Int64 CharacterId{ get; set;}

        public byte[] CharacterValue
        {
            get; set;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue( "ID", CharacterId );
            info.AddValue( "Value", CharacterValue);
        }

        protected HatCharacter(SerializationInfo info, StreamingContext ctxt)
        {
            CharacterId = info.GetInt64("ID");
            CharacterValue = info.GetValue("Value", typeof (byte[])) as byte[];
        }

        public HatCharacter()
        {
        }
    }
}