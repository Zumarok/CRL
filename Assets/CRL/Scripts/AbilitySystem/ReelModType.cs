using System;

namespace Crux.CRL.AbilitySystem
{
    [Flags]
    public enum ReelModType
    {
        None = 0,
        ExtraWave = 1,
        EnemyHealthPercent = 1 << 1,
        EnemyDamagePercent = 1 << 2,
        EnemyExtraDDAttack = 1 << 3,
        PlayerHealthPercent = 1 << 4,
        PlayerDamagePercent = 1 << 5,
        PlayerFlatMana = 1 << 6,
        SpawnElite = 1 << 7,
        SpawnBoss = 1 << 8,
        SpawnGoldMonster = 1 << 9,
        LootGoldModifier = 1 << 10,
        LootShardModifier = 1 << 11
    }
}
