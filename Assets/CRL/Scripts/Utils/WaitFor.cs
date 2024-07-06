using System;
using UnityEngine;
using System.Collections.Generic;

namespace Crux.CRL.Utils
{
    /// <summary>
    /// Cached Yield Instructions to help prevent unnecessary garbage in Unity coroutines.
    /// </summary>
    public static class WaitFor
    {
        private class FloatComparer : IEqualityComparer<float>
        {
            bool IEqualityComparer<float>.Equals(float x, float y)
            {
                return Math.Abs(x - y) < 0.1f;
            }

            int IEqualityComparer<float>.GetHashCode(float obj)
            {
                return obj.GetHashCode();
            }
        }

        private static readonly Dictionary<float, WaitForSeconds> TimeInterval = new Dictionary<float, WaitForSeconds>(100, new FloatComparer());

        public static WaitForEndOfFrame EndOfFrame { get; } = new WaitForEndOfFrame();

        public static WaitForFixedUpdate FixedUpdate { get; } = new WaitForFixedUpdate();

        public static WaitForSeconds Seconds(float seconds)
        {
            if (!TimeInterval.TryGetValue(seconds, out var wfs))
                TimeInterval.Add(seconds, wfs = new WaitForSeconds(seconds));
            return wfs;
        }
    }
}