using System;

namespace Crux.CRL.AbilitySystem
{
    [Flags]
    public enum BuffType
    {
        FlatDamageDealt = 1,
        PercentDamageDealt = 1 << 1,
        FlatDamageTaken = 1 << 2,
        PercentDamageTaken = 1 << 3,
        TakeDamageOnCast = 1 << 4,
        TakeHealOnCast = 1 << 5,
        ExtraCast = 1 << 6,
        ExplodeOnExpire = 1 << 7,
        TakeDamageOnTurn = 1 << 8, // DoTs create a debuff icon, this won't, so can be an additional effect on a buff
        PercentMaxHp = 1 << 9,
    }
}