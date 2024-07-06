using System;
using System.Threading;

namespace Crux.CRL.Utils
{
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random _local;

        public static Random Ran => _local ?? (_local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));
    }
}
