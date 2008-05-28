using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using libhat.DBFactory;

namespace libhat {
    [Serializable]
    public class HatCharacter : IEntity{
        private string parentUserCode;
        private int characterID;
        private byte[] characterData;
        private string nickname;
        private string clan;
        private byte body, react,mind,spirit, skill, pic, color;
        private string srvIP;
        private int[] ids = new int[2];

        private byte unknown1, unknown2, unknown3;

        private Int32 monsterKills;
        private Int32 playerKills;
        private Int32 totalKills;
        private Int32 deathCount;
        private UInt32 money;
        private Int32 spells;
        private Int32 activeSpell;
        private Int32 fireExp, waterExp, airExp, earthEx, astralEx;
        private Int32 hatID;
        private byte sex;
        private List<InventoryItem> inventory;
        private List<InventoryItem> wearing;

        private MemoryStream section55555555, section40A40A40;

        public byte Sex {
            get { return sex; }
            set { sex = value; }
        }

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

        public string Clan {
            get { return clan; }
            set { clan = value; }
        }

        public byte Body {
            get { return body; }
            set { body = value; }
        }

        public byte React {
            get { return react; }
            set { react = value; }
        }

        public byte Mind {
            get { return mind; }
            set { mind = value; }
        }

        public byte Spirit {
            get { return spirit; }
            set { spirit = value; }
        }

        public byte Skill {
            get { return skill; }
            set { skill = value; }
        }

        public byte Pic {
            get { return pic; }
            set { pic = value; }
        }

        public byte Color {
            get { return color; }
            set { color = value; }
        }

        public string SrvIP {
            get { return srvIP; }
            set { srvIP = value; }
        }

        public int[] IDs {
            get { return ids; }
            set { ids = value; }
        }

        public int[] Ids {
            get { return ids; }
            set { ids = value; }
        }

        public byte Unknown1 {
            get { return unknown1; }
            set { unknown1 = value; }
        }

        public byte Unknown2 {
            get { return unknown2; }
            set { unknown2 = value; }
        }

        public byte Unknown3 {
            get { return unknown3; }
            set { unknown3 = value; }
        }

        public int MonsterKills {
            get { return monsterKills; }
            set { monsterKills = value; }
        }

        public int PlayerKills {
            get { return playerKills; }
            set { playerKills = value; }
        }

        public int TotalKills {
            get { return totalKills; }
            set { totalKills = value; }
        }

        public int DeathCount {
            get { return deathCount; }
            set { deathCount = value; }
        }

        public uint Money {
            get { return money; }
            set { money = value; }
        }

        public int Spells {
            get { return spells; }
            set { spells = value; }
        }

        public int ActiveSpell {
            get { return activeSpell; }
            set { activeSpell = value; }
        }

        public int FireExp {
            get { return fireExp; }
            set { fireExp = value; }
        }

        public int WaterExp {
            get { return waterExp; }
            set { waterExp = value; }
        }

        public int AirExp {
            get { return airExp; }
            set { airExp = value; }
        }

        public int EarthEx {
            get { return earthEx; }
            set { earthEx = value; }
        }

        public int AstralEx {
            get { return astralEx; }
            set { astralEx = value; }
        }

        public int HatID {
            get { return hatID; }
            set { hatID = value; }
        }

        public List<InventoryItem> Inventory {
            get { return inventory; }
            set { inventory = value; }
        }

        public List<InventoryItem> Wearing {
            get { return wearing; }
            set { wearing = value; }
        }

        public MemoryStream Section55555555 {
            get { return section55555555; }
            set { section55555555 = value; }
        }

        public MemoryStream Section40A40A40 {
            get { return section40A40A40; }
            set { section40A40A40 = value; }
        }

        

        public byte[] ToByteArray() {
            using (MemoryStream mem = new MemoryStream( )) {
                BinaryWriter writer = new BinaryWriter( mem );

                

                return mem.ToArray();
            }
        }
    }

    
}



