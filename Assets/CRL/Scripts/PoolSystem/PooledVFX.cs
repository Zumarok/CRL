
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;

namespace Crux.CRL.PoolSystem
{
    public class PooledVFX : PooledGameObject
    {
        [SerializeField, Required] private Transform _rootTransform;
        [SerializeField] private VisualEffect _vfx;
        [SerializeField] private ParticleSystem _particle;

        public void Init(Vector3 scale, Vector3 worldPos,  float playRate, float lifetime)
        {
            _rootTransform.localScale = scale;
            _rootTransform.position = worldPos;

            if (_vfx != null)
            {
                _vfx.playRate = playRate;
            }
            else if (_particle != null)
            {
                var m = _particle.main;
                m.simulationSpeed = playRate;
            }

            Invoke(nameof(UnSpawn), lifetime);
        }

        public override void Spawn()
        {
            base.Spawn();
            if (_vfx != null)
            {
                _vfx.Reinit();
                _vfx.Play();
            }
            else if (_particle != null)
            {
                _particle.Clear();
                _particle.Play();
            }
        }
    }
}
