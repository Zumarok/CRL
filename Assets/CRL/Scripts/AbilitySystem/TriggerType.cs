using System;

namespace Crux.CRL.AbilitySystem
{
    [Flags]
    public enum TriggerType
    {
        PercentChance = 1, 
        OverHeal = 1 << 1, 
        OverShield = 1 << 2, 
        OverKill = 1 << 3,
        OnGoblinDeath = 1 << 4,
    }
}
