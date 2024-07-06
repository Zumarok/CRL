using System;

namespace Crux.CRL.CombatSystem
{
    [Flags]
    public enum CombatantFlags
    {
        None = 0,
        Dead = 1 << 1,
    }
}

