using System;
using System.Collections;
using Crux.CRL.CombatSystem;
using Crux.CRL.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Crux.CRL.EnemySystem
{
    public class EnemySpawnPoint : MonoBehaviour
    {
        [SerializeField, ReadOnly] private int _spawnPointIndex;

        [SerializeField] private EnemySpawn _enemySpawn;

        private int _scaleTweenId;

        public int SpawnPointIndex => _spawnPointIndex;
        public EnemySpawn EnemySpawn => _enemySpawn;

        private void Awake()
        {
            _enemySpawn.ClearData();
        }

        public void SpawnEnemy(EnemySpawn enemySpawn, GameObject prefab)
        {
            _enemySpawn = enemySpawn;
            var go = Instantiate(prefab, transform);
            go.name = go.name.Replace("(Clone)", "");
            var goTransform = go.transform;
            goTransform.localScale = Vector3.zero;
            goTransform.localPosition = Vector3.zero;
            goTransform.localRotation = Quaternion.identity;

            LeanTween.cancel(_scaleTweenId);
            _scaleTweenId = LeanTween.scale(go, Vector3.one, 1f).setEaseInOutBounce().uniqueId;
        }


        #region Editor Functions

#if UNITY_EDITOR

        public void ClearSpawnPoint()
        {
            _enemySpawn.RemoveEnemyFromScene();
            _enemySpawn = new EnemySpawn();
            _enemySpawn.SetSpawnPoint(this);
        }

        public void SetSpawnPointIndex(int index)
        {
            _spawnPointIndex = index;
            _enemySpawn.SetSpawnPoint(this);
        }

        public void LoadEnemySpawnIntoScene(EnemySpawn spawn)
        {
            _enemySpawn = spawn;
            spawn.PlaceEnemyPrefabIntoScene();
        }
#endif
        #endregion

    }
}
