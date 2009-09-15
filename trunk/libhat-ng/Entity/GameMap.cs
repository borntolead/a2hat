using System;

namespace libhat_ng.Entity
{
    [Serializable]
    public class GameMap {
        /// <summary>
        /// Map name
        /// </summary>
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Map difficulty
        /// </summary>
        public Difficulty Difficulty {
            get; set;
        }

        /// <summary>
        /// Width of map
        /// </summary>
        public int Width {
            get; set;
        }

        /// <summary>
        /// Height of map
        /// </summary>
        public int Height {
            get; set;
        }
    }
}