using System;
using System.Collections.Generic;
using System.Text;

namespace libhat {
    public struct InventoryItem {
        private Int32 itemID;
        private bool isMagic;
        private UInt32 price;
        private UInt32 count;
        private List<ItemModifier> modifiers;

        public int ItemID {
            get { return itemID; }
            set { itemID = value; }
        }

        public bool IsMagic {
            get { return isMagic; }
            set { isMagic = value; }
        }

        public uint Price {
            get { return price; }
            set { price = value; }
        }

        public uint Count {
            get { return count; }
            set { count = value; }
        }

        public List<ItemModifier> Modifiers {
            get { return modifiers; }
            set { modifiers = value; }
        }

        public string Description {
            get { return "not implemented"; }
        }
        
        public string Caption {
            get { return "not implemented"; }
        }
    }

    public struct ItemModifier {
        private Int32 id1;
        private Int32 id2;
        private Int32 Value1;
        private Int32 value2;
        private string description;

        public int Id1 {
            get { return id1; }
            set { id1 = value; }
        }


        public int Value1_ {
            get { return Value1; }
            set { Value1 = value; }
        }

        public int Value2 {
            get { return value2; }
            set { value2 = value; }
        }

        public string Description {
            get { return "not implemented"; }
        }
    }
}
