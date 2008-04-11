using System;
using System.Collections.Generic;
using System.Text;

namespace libhat {
    public class GameMap {
        /// <summary>
        /// Map name
        /// </summary>
        private string name;

        /// <summary>
        /// Map difficulty
        /// </summary>
        private Difficulty difficulty;

        /// <summary>
        /// Width of map
        /// </summary>
        private int x;

        /// <summary>
        /// Height of map
        /// </summary>
        private int y;


        /// <summary>
        /// Map name
        /// </summary>
        public string Name {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Map difficulty
        /// </summary>
        public Difficulty Difficulty {
            get { return difficulty; }
            set { difficulty = value; }
        }

        /// <summary>
        /// Width of map
        /// </summary>
        public int Width {
            get { return x; }
            set { x = value; }
        }

        /// <summary>
        /// Height of map
        /// </summary>
        public int Height {
            get { return y; }
            set { y = value; }
        }
    }
}
