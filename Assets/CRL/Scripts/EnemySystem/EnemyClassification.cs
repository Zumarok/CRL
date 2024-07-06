using System;

namespace Crux.CRL.EnemySystem
{
    [Flags]
    public enum EnemyClassification
    {
        Fodder = 1 << 0,
        Normal = 1 << 1,
        Elite  = 1 << 2,
        Boss   = 1 << 3
    }
}