
using System;

namespace Crux.CRL.SaveSystem
{
    [Serializable]
    public class RunData : ICloneable
    {
        public bool TutorialCompleted { get; set; }

        /// <summary>
        /// The floor the tower 'elevator' is currently set at.
        /// </summary>
        public int CurrentTowerFloor { get; set; } = 1;

        /// <summary>
        /// The Highest tower floor that has been unlocked.
        /// </summary>
        public int HighestFloorReached { get; set; } = 1;

        /// <summary>
        /// The highest tower floor that has been defeated.
        /// </summary>
        public int HighestFloorDefeated { get; set; } = 0;

        public int CurrentGold { get; set; }

        public object Clone()
        {
            var clone = (RunData)MemberwiseClone();
            // create new ref instances if needed.
            return clone;
        }
    }
}
