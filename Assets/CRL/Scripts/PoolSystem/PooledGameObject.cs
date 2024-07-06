using UnityEngine;

namespace Crux.CRL.PoolSystem
{
    public class PooledGameObject : PooledObjectBase
    {
        public override void Spawn()
        {
            if (isActive)
            {
                Debug.LogError("trying to spawn active poolObject");
            }
            else
            {
                this.gameObject.SetActive(true);
                isActive = true;
            }
        }

        public override void UnSpawn()
        { 
            if (isActive)
            {
                if (pool == null)
                {
                    Debug.LogWarning("missing pool manager, destory this instance");
                    Destroy(this.gameObject);
                }
                else
                {
                    this.gameObject.SetActive(false);
                    isActive = false;
                    this.pool.stack.Push(this);
                }
            }
            else
            {
                Debug.LogWarning("unspawn inactive instance, destory");
                Destroy(this.gameObject);
            }
        }
    }

}
