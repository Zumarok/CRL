using System.Collections.Generic;
using UnityEngine;

namespace Crux.CRL.Utils
{
    public interface IRuntimeTable<in TKey, TValue>
    {
        TValue this[TKey key] { get; set; }
    }

    public abstract class RuntimeTable<TKey, TValue> : ScriptableObject, IRuntimeTable<TKey, TValue>
    {
        public Dictionary<TKey, TValue> keyPair = new Dictionary<TKey, TValue>();

        public void Add(TKey key, TValue value)
        {
            if (!keyPair.ContainsKey(key))
            {
                keyPair.Add(key, value);
            }
        }

        public void Remove(TKey key)
        {
            if (keyPair.ContainsKey(key))
            {
                keyPair.Remove(key);
            }
        }

        public TValue this[TKey key]
        {
            get => keyPair[key];
            set => keyPair[key] = value;
        }
    }
}
