using System;

namespace libhat_ng.Entity
{
    [Serializable]
    public class HatCharacter {


        public UInt64 CharacterId{ get; set;}

        public byte[] CharacterValue
        {
            get; set;
        }
    }
}