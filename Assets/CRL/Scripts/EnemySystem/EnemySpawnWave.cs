using System;
using System.Collections.Generic;
using System.Linq;
using Crux.CRL.DataSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Crux.CRL.EnemySystem
{
    [Serializable]
    public class EnemySpawnWave : ICloneable
    {
        [SerializeField, TableColumnWidth(30)]
        private float _spawnDelay = 0.5f;

        [SerializeField, TableColumnWidth(30)]
        private EnemyRace _race;

        [SerializeField, ReadOnly, TableColumnWidth(100)]
        private EnemySpawn[] _enemySpawns = new EnemySpawn[Constants.MAX_NUM_ENEMIES];

        private int[] _eliteSpawnIndexOrder = { 4, 3, 2, 1, 0, 5, 6, 7, 8, 9 };

        public float SpawnDelay => _spawnDelay;
        public IReadOnlyList<EnemySpawn> EnemySpawns => _enemySpawns;

        public void SetAllowedRacesByFloor(int floor, EnemyLevelRange ranges)
        {
            _race = EnemyRace.None;
            foreach (var race in Enum.GetValues(typeof(EnemyRace)).Cast<EnemyRace>())
            {
                if (race == EnemyRace.None) continue;

                if (floor >= ranges[race]._floorRange.x && floor <= ranges[race]._floorRange.y)
                    _race |= race;
            }

            foreach (var enemySpawn in _enemySpawns)
            {
                enemySpawn.SetAllowedRaces(_race);
            }
        }

        public void RandomizeNumOfSpawns(int floor, int min = 2, int max = Constants.MAX_NUM_ENEMIES)
        {
            // Clamp the floor to the valid range (1 to maxFloor)
            floor = Math.Max(1, Math.Min(floor, Constants.MAX_FLOOR));

            // Calculate the interpolation factor (0 at floor 1, 1 at maxFloor)
            var t = (double)(floor - 1) / (Constants.MAX_FLOOR - 1);

            // Linearly interpolate the number of spawns
            double spawnRange = max - min;
            var numSpawns = min + (int)(spawnRange * t);

            // Add random variation: +/- 10% of the calculated number of spawns
            var variation = (int)(numSpawns * 0.25f);
            numSpawns += Utils.ThreadSafeRandom.Ran.Next(-variation, variation + 1);

            // Clamp the number of spawns to the [min, max] range
            numSpawns = Math.Max(min, Math.Min(numSpawns, max));

            for (int i = numSpawns; i < _enemySpawns.Length; i++)
            {
                _enemySpawns[i].SetSpawnType(EnemySpawnType.None);
            }
        }

        public void AddElites(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _enemySpawns[_eliteSpawnIndexOrder[i]].SetSpawnType(EnemySpawnType.Random);
                _enemySpawns[_eliteSpawnIndexOrder[i]].SetAllowedClassifications(EnemyClassification.Elite);
            }
        }

#if UNITY_EDITOR
        
        [Button("Save"), TableColumnWidth(20)]
        private void Save()
        {
            _enemySpawns = new EnemySpawn[Constants.MAX_NUM_ENEMIES];
            var spawnPoints = Object.FindObjectsOfType<EnemySpawnPoint>();
            foreach (var spawnPoint in spawnPoints)
            {
                var es = spawnPoint.EnemySpawn;
                _enemySpawns[spawnPoint.SpawnPointIndex] = es;
            }
        }

        [Button("Load"), TableColumnWidth(20)]
        private void Load()
        {
            var spawnPoints = Object.FindObjectsOfType<EnemySpawnPoint>();

            foreach (var enemySpawn in _enemySpawns)
            {
                var sp = spawnPoints.First(p => p.SpawnPointIndex == enemySpawn.SpawnPointIndex);
                sp.LoadEnemySpawnIntoScene(enemySpawn);
            }
        }
#endif

        public object Clone()
        {
            var clone = (EnemySpawnWave)MemberwiseClone();
            clone._enemySpawns = this._enemySpawns.Select(es => es.Clone()).Cast<EnemySpawn>().ToArray();
            return clone;
        }
    }
}
