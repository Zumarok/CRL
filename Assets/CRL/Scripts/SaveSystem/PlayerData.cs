using System;
using Crux.CRL.CombatSystem;

namespace Crux.CRL.SaveSystem
{
    [Serializable]
    public class PlayerData : ICloneable
    {
        public long CurrentTotalExp { get; set; }
        public long CurrentLevelStartExp { get; set; }
        public long NextLevelStartExp { get; set; }
        public int CurrentLevel { get; set; }
        public long CurrentLevelExp => CurrentTotalExp - CurrentLevelStartExp;

        public PlayerData()
        {
            CurrentLevel = 1;
            NextLevelStartExp = Player.CalculateExpForLevel(2);
        }

        public PlayerData(long currentTotalExp, long currentLevelStartExp, long nextLevelStartExp, int currentLevel)
        {
            CurrentTotalExp = currentTotalExp;
            CurrentLevelStartExp = currentLevelStartExp;
            NextLevelStartExp = nextLevelStartExp;
            CurrentLevel = currentLevel;
        }
        
        public object Clone()
        {
            var clone = (PlayerData)MemberwiseClone();
            // create new ref instances if needed.
            return clone;
        }
    }
}
