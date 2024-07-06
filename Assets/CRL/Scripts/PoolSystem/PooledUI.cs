using Crux.CRL.Utils;
using UnityEngine;

namespace Crux.CRL.PoolSystem
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PooledUI : PooledObjectBase
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        protected float duration;
        
        public override void Spawn()
        {
            if (isActive)
            {
                Debug.LogError("trying to spawn active PooledUI");
            }
            else
            {
                if (canvasGroup == null) { canvasGroup = GetComponent<CanvasGroup>(); }
                if (canvasGroup == null) { Debug.LogError("Missing canvasGroup on " + name); return; }

                isActive = true;

                canvasGroup.SetVisibilityAndInteractive(true);
            }
        }

        public override void UnSpawn()
        {
            if (isActive)
            {
                if (pool == null)
                {
                    Debug.LogWarning("missing pool manager, destroy this instance");
                    Destroy(gameObject);
                }
                else
                {
                    canvasGroup.SetVisibilityAndInteractive(false);
                    isActive = false;
                    pool.stack.Push(this);
                }
            }
            else
            {
                Debug.LogWarning("unspawn inactive instance, destroying");
                Destroy(gameObject);
            }
        }
    }
}