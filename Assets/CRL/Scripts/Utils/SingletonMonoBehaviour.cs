using System;
using UnityEngine;

namespace Crux.CRL.Utils
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected new Transform transform;
        
        protected virtual void Awake()
        {
            if (FindObjectsOfType<T>().Length > 1)
                throw new Exception(
                    $"Error: More than one instance of {gameObject.name} exists. Either delete any duplicates or derive from MonoBehavior instead of SingletonMonoBehaviour. ");

            Instance = this as T;
            transform = gameObject.transform;
        }
    }
}
