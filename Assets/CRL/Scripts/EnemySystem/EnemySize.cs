using System;

namespace Crux.CRL.EnemySystem
{
    [Flags]
    public enum EnemySize
    {
        Small = 1 << 0,
        Medium = 1 << 1,
        Large = 1 << 2,
    }
}

