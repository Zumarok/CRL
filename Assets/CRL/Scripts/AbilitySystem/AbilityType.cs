using System;

namespace Crux.CRL.AbilitySystem
{
    [Flags]
    public enum AbilityType
    {
        DD = 1,
        DOT = 1 << 1,
        Heal = 1 << 2,
        HOT = 1 << 3,
        Cleanse = 1 << 4,
        Dispel = 1 << 5,
        Immunity = 1 << 6,
        Absorb = 1 << 7,
        EffectMod = 1 << 8,
        EnemyAbility = 1 << 9,
        Buff = 1 << 10,
        Debuff = 1 << 11,
        Triggered = 1 << 12,
        CanTrigger = 1 << 13,
        Passive = 1 << 14,
        ReelMod = 1 << 15,
        Reel = 1 << 16,
    }
}