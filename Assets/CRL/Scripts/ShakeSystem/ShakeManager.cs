using System.Collections;
using System.Collections.Generic;
using Crux.CRL.DataSystem;
using Crux.CRL.EnemySystem;
using Crux.CRL.Utils;
using UnityEngine;

namespace Crux.CRL.ShakeSystem
{
    public class ShakeManager : SingletonMonoBehaviour<ShakeManager>
    {

        #region Private Classes

        private class ShakeInstance
        {
            public float trauma;
            public Coroutine coroutine;

            public ShakeInstance()
            {
                trauma = 0;
                coroutine = null;
            }
        }

        #endregion

        #region Editor Fields

        [SerializeField] [Range(0.25f, 1f)] private float _maxTrauma = 1f;
        [SerializeField] [Range(1f, 3f)] private float _shakePower = 3f;
        [SerializeField] [Range(0, 1000)] private int _maxYaw = 10;
        [SerializeField] [Range(0, 1000)] private int _maxPitch = 0;
        [SerializeField] [Range(0, 1000)] private int _maxRoll = 10;
        [SerializeField] [Range(0.1f, 30f)] private float _shakeSpeed = 15f;
        [SerializeField] [Range(0.1f, 2f)] private float _traumaDeteriorateSpeed = 0.5f;
        [SerializeField] [Range(0.1f, 25f)] private float _noiseScale = 1;

        #endregion

        #region Private Fields

        private Dictionary<Transform, ShakeInstance> _shakeInstances = new Dictionary<Transform, ShakeInstance>();

        #endregion

        #region Unity Callbacks


        #endregion

        #region Public Methods

        public void AddCameraTrauma(float trauma)
        {
            AddTrauma(DataManager.Instance.MainCamera.transform, trauma, true);
        }

        public void AddTrauma(Transform objectToShake, float trauma, bool isCameraShake = false)
        {
            if (!_shakeInstances.ContainsKey(objectToShake))
                _shakeInstances.Add(objectToShake, new ShakeInstance());

            var si = _shakeInstances[objectToShake];

            si.trauma = Mathf.Clamp(si.trauma + trauma, 0, _maxTrauma);
            if (si.coroutine == null)
                si.coroutine = StartCoroutine(RunShake(objectToShake, si, isCameraShake));
        }

        #endregion

        #region Private Methods

        private IEnumerator RunShake(Transform objectToShake, ShakeInstance si, bool isCameraShake)
        {
            var x = Random.value * _noiseScale;
            var y = Random.value * _noiseScale;
            var originEul = objectToShake.localEulerAngles;

            if (isCameraShake)
                EnemyHealthBarManager.Instance.ToggleAllHealthBarPositionTracking(true);

            while (si.trauma > 0)
            {
                var shake = Mathf.Pow(si.trauma + 1, _shakePower) - 1;
                var pitch = originEul.x + _maxPitch * shake * (Mathf.PerlinNoise(x, y) * 2 - 1);
                var yaw   = originEul.y + _maxYaw   * shake * (Mathf.PerlinNoise(x + 0.1f, y + 0.1f) * 2 - 1);
                var roll  = originEul.z + _maxRoll  * shake * (Mathf.PerlinNoise(x + 0.2f, y + 0.2f) * 2 - 1);
                objectToShake.localEulerAngles = new Vector3(pitch, yaw, roll);

                x += Time.deltaTime * _shakeSpeed;
                y += Time.deltaTime * _shakeSpeed;
                si.trauma = Mathf.Clamp01(si.trauma - Time.deltaTime * _traumaDeteriorateSpeed);
                yield return null;
            }

            objectToShake.localEulerAngles = originEul;

            if (isCameraShake)
                EnemyHealthBarManager.Instance.ToggleAllHealthBarPositionTracking(false);

            si.coroutine = null;
            _shakeInstances.Remove(objectToShake);
        }
        
        #endregion

    }
}

