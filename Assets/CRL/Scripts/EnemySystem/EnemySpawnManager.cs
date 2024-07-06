using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Crux.CRL.CombatSystem;
using Crux.CRL.DataSystem;
using Crux.CRL.FloorNavigationSystem;
using Crux.CRL.Utils;
using RotaryHeart.Lib.SerializableDictionary;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using static Crux.CRL.DataSystem.Constants;

namespace Crux.CRL.EnemySystem
{
    public class EnemySpawnManager : SingletonMonoBehaviour<EnemySpawnManager>
    {
        #region Editor Fields

        [SerializeField, FoldoutGroup("System References"),
         ListDrawerSettings(HideAddButton = true)]
        private EnemySpawnPoint[] _spawnPoints = new EnemySpawnPoint[MAX_NUM_ENEMIES];

        [SerializeField, FoldoutGroup("System References"), ReadOnly]
        private List<GameObject> _prefabs;

        [SerializeField]
        private EnemyLevelRange _levelRange;

        [SerializeField, InlineEditor, ListDrawerSettings(Expanded = true)] 
        private EnemySpawnScriptObject _scriptObject;

        #endregion

        #region Private Fields

        private EnemySpawnScenario _currentScenario;
        private int _currentWave;

        #endregion

        #region Properties

        public EnemySpawnScenario CurrentScenario => _currentScenario;
        public int CurrentWaveNumber => _currentWave;
        public int WaveCount => _currentScenario?.NumberOfWaves ?? 0;
        public int CurrentWaveEnemyTotal => _currentScenario?.GetSpawnWave(_currentWave).EnemySpawns.Count ?? 0;
        public bool CurrentScenarioFinished { get; private set; }
        public bool MoreWavesToSpawn => _currentWave < _currentScenario?.NumberOfWaves;

        #endregion

        #region Public Functions

        public Transform GetSpawnPointTransformByIndex(int index) => _spawnPoints[index].transform;
        public int GetSpawnPointIndex(EnemySpawnPoint spawnPoint) => Array.IndexOf(_spawnPoints, spawnPoint);

        public void LoadRandomScenario()
        {
            LoadScenario(_scriptObject.EnemySpawnScenarios.GetRandom());
        }

        public void LoadScenarioByName(string scenarioName)
        {
            var scenario = GetCopiedScenarioFromScriptObject(scenarioName);
            if (scenario == null)
            {
                Debug.LogError($"'{scenarioName}' EnemyScenario does not exist!");
                return;
            }

            LoadScenario(scenario);
        }

        public void LoadScenarioByFloor(TowerFloorInfo info)
        {
            if (!DataManager.Instance.RunData.TutorialCompleted && info.Floor == 1)
            {
                LoadScenarioByName("Tutorial");
                return;
            }

            var scenario = new EnemySpawnScenario();
            var standardWaves = MIN_NUM_WAVES + info.SpawnWaves - (info.SpawnBoss ? 1 : 0);
            var elitesPerWave = DistributeElites(info.SpawnElites, standardWaves);

            for (int i = 0; i < standardWaves; i++)
            {
                var wave = GetCopiedScenarioFromScriptObject("Standard").GetRandomWave();
                wave.SetAllowedRacesByFloor(info.Floor, _levelRange);
                wave.RandomizeNumOfSpawns(info.Floor);
                wave.AddElites(elitesPerWave[i]);
                scenario.AddWave(wave);
            }

            if (info.SpawnBoss)
            {
                var wave = GetCopiedScenarioFromScriptObject("Boss").GetRandomWave();
                scenario.AddWave(wave);
            }

            LoadScenario(scenario);
        }

        public void LoadCustomScenario(int floor, int extraWaves, int elites, bool boss, EnemyRace forcedRaces = EnemyRace.None)
        {
            // floor will dictate which enemies can spawn, as well as base stats

            // extra waves - default is 3, loot multiplier will increase on waves 4+

            // forced races - will spawn in at least 1 wave, regardless if allowed by floor restrictions

            // elites - more difficult // more loot

            // boss - last wave is a preset boss wave // can drop *trinkets*
        }

        private void LoadScenario(EnemySpawnScenario scenario)
        {
            _currentScenario = scenario;
            _currentWave = 0;
            CurrentScenarioFinished = false;
        }

        public IEnumerator SpawnNextWave()
        {
            if (_currentWave == _currentScenario.NumberOfWaves)
            {
                OnScenarioComplete();
                yield break;
            }

            var wave = _currentScenario.GetSpawnWave(_currentWave);
            _currentWave++;

            PlayerUIManager.Instance.UpdateUIFloorAndWave();

            foreach (var enemySpawn in wave.EnemySpawns)
            {
                if (enemySpawn.SpawnType == EnemySpawnType.None) continue;
                
                if (enemySpawn.IsSetEnemyReady)
                {
                    _spawnPoints[enemySpawn.SpawnPointIndex].SpawnEnemy(enemySpawn, enemySpawn.Prefab);
                }
                else if (enemySpawn.IsRandomReady)
                {
                    var enemy = _prefabs.Select(go => go.GetComponentInChildren<Enemy>()).Where(e =>
                        enemySpawn.AllowedRaces.HasFlag(e.EnemyRace) &&
                        enemySpawn.AllowedSizes.HasFlag(e.EnemySize) &&
                        enemySpawn.AllowedClassifications.HasFlag(e.EnemyClassification)).ToList().GetRandom();

                    if (enemy == null)
                    {
                        Debug.LogError("No valid random enemies for these settings. Did you add the prefab to the Spawn Manager?");
                        continue;
                    }

                    _spawnPoints[enemySpawn.SpawnPointIndex].SpawnEnemy(enemySpawn, enemy.transform.parent.gameObject);

                }

                yield return WaitFor.Seconds(wave.SpawnDelay);
            }
        }

        #endregion

        #region Private Functions

        private void OnScenarioComplete()
        {
            _currentWave = 0;
            _currentScenario = null;
            CurrentScenarioFinished = true;
        }

        private EnemySpawnScenario GetCopiedScenarioFromScriptObject(string scenarioName)
        {
            return (EnemySpawnScenario)_scriptObject.EnemySpawnScenarios.FirstOrDefault(s => s.ScenarioName == scenarioName)?.Clone();
        }

        private int[] DistributeElites(int totalElites, int numWaves)
        {
            int[] elitesPerWave = new int[numWaves];

            for (int i = 0; i < totalElites; i++)
            {
                elitesPerWave[(numWaves - 1) - (i % numWaves)]++;
            }

            return elitesPerWave;
        }
        #endregion

        #region Editor Functions

#if UNITY_EDITOR


        [Button(ButtonSizes.Small), FoldoutGroup("System References")]
        private void SetAllSpawnPointIndexes()
        {
            foreach (var spawnPoint in _spawnPoints)
            {
                spawnPoint.SetSpawnPointIndex(GetSpawnPointIndex(spawnPoint));
            }
        }

        [Button(ButtonSizes.Small), FoldoutGroup("System References")]
        public void SetEnemyPrefabs()
        {
            var path = Application.dataPath + @"\CRL\Prefabs\Enemy\";
            var list = new List<GameObject>();
            var enemyPaths = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(s => s.Contains(".prefab") && !s.Contains(".meta")).ToArray();
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

            _prefabs = list;
        }

        [Button]
        private void ClearSpawnPoints()
        {
            var spawnPoints = FindObjectsOfType<EnemySpawnPoint>();

            foreach (var spawnPoint in spawnPoints)
            {
                spawnPoint.ClearSpawnPoint();
            }
        }

#endif

        #endregion
    }

    [Serializable, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
    public class EnemyLevelRange : SerializableDictionaryBase<EnemyRace, FloorRange> {}

    [Serializable]
    public class FloorRange
    {
        [SerializeField, MinMaxSlider(1, MAX_FLOOR, true)] 
        public Vector2Int _floorRange = new Vector2Int(1, MAX_FLOOR);
    }
}
