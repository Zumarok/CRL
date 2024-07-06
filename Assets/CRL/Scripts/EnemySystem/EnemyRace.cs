using System;

namespace Crux.CRL.EnemySystem
{
    [Flags]
    public enum EnemyRace
    {
        None = 0,
        Goblin = 1 << 0,
        Slime = 1 << 2,
    }
}
