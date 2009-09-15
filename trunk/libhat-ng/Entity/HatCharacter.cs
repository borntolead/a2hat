using System;

namespace libhat_ng.Entity
{
    [Serializable]
    public class HatCharacter {

        /// <summary>
        /// Parent hat user
        /// </summary>
        public HatUser User
        {
            get; set;
        }

        public byte[] CharacterValue
        {
            get; set;
        }
    }
}