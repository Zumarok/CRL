using Sirenix.OdinInspector;
using System;
using Crux.CRL.CombatSystem;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using Crux.CRL.Utils;
using Object = UnityEngine.Object;

namespace Crux.CRL.EnemySystem
{
    [Serializable]
    public class EnemySpawn : ICloneable
    {
        #region Editor Fields
        [SerializeField, ReadOnly]
        private int _spawnPointIndex;

#if UNITY_EDITOR
        [OnValueChanged(nameof(OnSpawnTypeSet))]
#endif
        [SerializeField] 
        private EnemySpawnType _spawnType;

#if UNITY_EDITOR
        [OnValueChanged(nameof(OnRandomFilterChanged))]
#endif
        [SerializeField, ShowIfGroup(nameof(IsRandomSpawn))] 
        private EnemyRace _allowedRaces;
#if UNITY_EDITOR
        [OnValueChanged(nameof(OnRandomFilterChanged))]
#endif
        [SerializeField, ShowIfGroup(nameof(IsRandomSpawn))] 
        private EnemySize _allowedSizes;

#if UNITY_EDITOR
        [OnValueChanged(nameof(OnRandomFilterChanged))]
#endif
        [SerializeField, ShowIfGroup(nameof(IsRandomSpawn))] 
        private EnemyClassification _allowedClassifications;

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetPrefabList)), OnValueChanged(nameof(OnPrefabSet))]
#endif
        [SerializeField, ShowIfGroup(nameof(IsSetEnemySpawn))]
        private GameObject _prefab; 

        #endregion

        #region Private Fields
        
        // used for editor functions (Spawn Scenario setup); not used in gameplay
        private Enemy _spawnedEnemy;
        private GameObject _spawnedParentGameObject;
        private EnemySpawnPoint _spawnPoint;

        #endregion

        #region Properties

        public GameObject PrefabGameObject => _prefab != null ? _prefab.gameObject : null;
        public GameObject Prefab => _prefab;
        public Enemy SpawnedEnemy => _spawnedEnemy;
        public GameObject SpawnedParentGameObject => _spawnedParentGameObject;
        public int SpawnPointIndex => _spawnPointIndex;

        public EnemySpawnType SpawnType => _spawnType;
        public EnemyRace AllowedRaces => _allowedRaces;
        public EnemySize AllowedSizes => _allowedSizes;
        public EnemyClassification AllowedClassifications => _allowedClassifications;

        #endregion
        
        #region Filter Methods

        public bool IsRandomSpawn => _spawnType == EnemySpawnType.Random;
        public bool IsSetEnemySpawn => _spawnType == EnemySpawnType.SetEnemy;

        public bool IsRandomReady =>
            IsRandomSpawn && _allowedRaces != 0 && _allowedSizes != 0 && _allowedClassifications != 0;

        public bool IsSetEnemyReady => IsSetEnemySpawn && _prefab != null;


        #endregion

        #region Public Functions
        
        public void ClearData()
        {
            _allowedClassifications = 0;
            _allowedRaces = 0;
            _allowedSizes = 0;
            _prefab = null;
            _spawnedEnemy = null;
            _spawnedParentGameObject = null;
        }

        public void SetAllowedClassifications(EnemyClassification c) => _allowedClassifications = c;

        public void SetAllowedRaces(EnemyRace races) => _allowedRaces = races;

        public void SetSpawnType(EnemySpawnType type) => _spawnType = type;

        #endregion

        #region Private Functions


        #endregion

        #region Editor Functions

#if UNITY_EDITOR
        public List<GameObject> GetPrefabList()
        {
            var path = Application.dataPath + @"\CRL\Prefabs\Enemy\";
            var list = new List<GameObject>();
            var enemyPaths = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(s => s.Contains(".prefab") && !s.Contains(".meta")).ToArray();
            for (var index = 0; index < enemyPaths.Length; index++)
            {
                var s = enemyPaths[index];
                s = s.Replace(Application.dataPath, "");
                s = "Assets/" + s.Replace("\\", "/");
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(s);
                if (obj == null) continue;

                var enemy = obj.GetComponentInChildren<Enemy>();
                if (enemy != null)
                    list.Add(enemy.transform.parent.gameObject);
            }

            return list;
        }

        private void OnSpawnTypeSet()
        {
            RemoveEnemyFromScene();
            ClearData();
        }

        private void OnRandomFilterChanged()
        {
            if (!IsRandomReady) return;

            _prefab = GetValidRandomPrefab();
            RemoveEnemyFromScene();
            PlaceEnemyPrefabIntoScene();
        }

        private void OnPrefabSet()
        {
            RemoveEnemyFromScene();
            PlaceEnemyPrefabIntoScene();
        }

        [Button, ShowIfGroup(nameof(IsRandomReady))]
        private void SpawnNextValidEnemy()
        {
            OnRandomFilterChanged();
        }

        private GameObject GetValidRandomPrefab()
        {
            var enemy = GetPrefabList().Select(
                go => go.GetComponentInChildren<Enemy>()).Where(
                e =>
                    _allowedRaces.HasFlag(e.EnemyRace) &&
                    _allowedSizes.HasFlag(e.EnemySize) &&
                    _allowedClassifications.HasFlag(e.EnemyClassification)).ToList().GetRandom();

            if (enemy == null)
            {
                Debug.Log("No valid enemies for these settings.");
                return null;
            }

            return enemy.transform.parent.gameObject;
        }

        public void PlaceEnemyPrefabIntoScene()
        {
            if (IsRandomSpawn && !IsRandomReady) return;
            if (_prefab == null) return;

            if (_spawnPoint == null)
            {
                Debug.LogError("Spawn Point is null...hit that button.");
                return;
            }

            var go = (GameObject)PrefabUtility.InstantiatePrefab(_prefab.gameObject, _spawnPoint.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            _spawnedEnemy = go.GetComponentInChildren<Enemy>();
            _spawnedParentGameObject = go;
        }

        public void RemoveEnemyFromScene()
        {
            if (_spawnedParentGameObject == null) return;

            Object.DestroyImmediate(_spawnedParentGameObject);
            _spawnedEnemy = null;
            _spawnedParentGameObject = null;
        }

        public void SetSpawnPoint(EnemySpawnPoint spawnPoint)
        {
            _spawnPoint = spawnPoint;
            _spawnPointIndex = spawnPoint.SpawnPointIndex;
        }
#endif

        #endregion

        #region Interface Functions

        public object Clone()
        {
            return (EnemySpawn)MemberwiseClone();
        }

        #endregion
    }
}
