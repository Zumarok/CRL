using UnityEngine;

namespace Crux.CRL.PoolSystem
{
    public abstract class PooledObjectBase : MonoBehaviour
    {
        public PoolBase pool { get; set; }
        public bool isActive { get; protected set; }

        public abstract void Spawn(); 
        public abstract void UnSpawn();
    }
}