using UnityEngine;

namespace Crux.CRL.PoolSystem
{
    //TODO : Add an editor validation button, for updating
    //TODO : Add update/checking for loop/onshot type
    //TODO : Auto recycle for oneshot 
    public class PooledParticle : PooledObjectBase
    { 
        [SerializeField] bool isOneShot = true;
        [SerializeField] ParticleSystem[] _particleSystems;
        [SerializeField] TrailRenderer[] _trailRenders;

        void Reset()
        {
            if (_particleSystems == null) { _particleSystems = GetComponentsInChildren<ParticleSystem>(true); }
            if (_trailRenders == null) { _trailRenders = GetComponentsInChildren<TrailRenderer>(true); }
        }

        void Update()
        {
            if (isActive && isOneShot)
            {
                var isParticleRootAlive = _particleSystems[0].IsAlive(false);
                if (isParticleRootAlive == false) { UnSpawn(); } 
            }
        }

        public override void Spawn()
        {
            if (isActive)
            {
                Debug.LogError("trying to spawn active poolObject");
            }
            else
            {
                if (_particleSystems == null) { _particleSystems = GetComponentsInChildren<ParticleSystem>(true); }
                if (_trailRenders == null) { _trailRenders = GetComponentsInChildren<TrailRenderer>(true); }
                for (int i = 0; i < _particleSystems.Length; i++) { _particleSystems[i].Play(); }
                for (int i = 0; i < _trailRenders.Length; i++) { _trailRenders[i].enabled = true; }
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
                    for (int i = 0; i < _particleSystems.Length; i++) { _particleSystems[i].Clear(); _particleSystems[i].Stop(); }
                    for (int i = 0; i < _trailRenders.Length; i++) { _trailRenders[i].enabled = false; }
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