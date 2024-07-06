using System;
using System.Collections.Generic;
using System.Linq;
using Crux.CRL.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Crux.CRL.EnemySystem
{
    [Serializable]
    public class EnemySpawnScenario : ICloneable
    {
        // difficulty, area, etc
        //public ushort difficulty;

        [SerializeField]
        private string _scenarioName;

        [SerializeField, TableList(ShowIndexLabels = false)]
        private List <EnemySpawnWave> _enemySpawnWaves = new List<EnemySpawnWave>();

        public int NumberOfWaves => _enemySpawnWaves.Count;

        public EnemySpawnWave GetSpawnWave(int waveNumber) => _enemySpawnWaves[waveNumber];
        public EnemySpawnWave GetRandomWave() => _enemySpawnWaves.GetRandom();

        public string ScenarioName => _scenarioName;

        public void AddWave(EnemySpawnWave wave) => _enemySpawnWaves.Add(wave);

        public object Clone()
        {
            var clone = (EnemySpawnScenario)MemberwiseClone();
            clone._enemySpawnWaves = this._enemySpawnWaves.Select(w => w.Clone()).Cast<EnemySpawnWave>().ToList();
            return clone;
        }
    }
}
