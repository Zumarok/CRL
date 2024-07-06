using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Crux.CRL.EnemySystem
{
    [CreateAssetMenu(fileName = "EnemySpawnScriptObject", menuName = "ScriptableObjects/EnemySpawnScriptObject")]
    public class EnemySpawnScriptObject : ScriptableObject
    {
        [SerializeField, ListDrawerSettings(ShowIndexLabels = false, Expanded = true)] 
        private EnemySpawnScenario[] _enemySpawnScenarios;

        public IReadOnlyList<EnemySpawnScenario> EnemySpawnScenarios => _enemySpawnScenarios;

#if UNITY_EDITOR
        
        
#endif

        //public IEnumerator SpawnRandomScenario(int difficulty)
        //{
        //    //var scenario = _enemySpawnScenarios.Where(s => s.difficulty == difficulty).ToArray().GetRandom();
        //    //if (scenario == null)
        //    //    throw new Exception($"Cannot find/load scenario of difficulty ({difficulty})");

        //    //foreach (var e in scenario.enemySpawns)
        //    {
        //        //var go = Instantiate(e.enemyPrefab, e.position, e.rotation);
        //        //go.name = go.name.Replace("(Clone)", "");
        //        //var scale = go.transform.localScale;

        //        //// waiting one frame before scaling so UI elements will position correctly
        //        //var enemy = go.GetComponent<Enemy>();
        //        //var renderers = enemy.Renderers;

        //        //foreach (var renderer in renderers)
        //        //{
        //        //    renderer.enabled = false;
        //        //}

        //        //yield return null;

        //        //foreach (var renderer in renderers)
        //        //{
        //        //    renderer.enabled = true;
        //        //}

        //        //go.transform.localScale = Vector3.zero;
        //        //LeanTween.scale(go, scale, 1f).setEaseInOutBounce();
        //        //yield return WaitFor.Seconds(scenario.spawnDelay);
        //    }

        //    yield return null;
        //}

        //public IEnumerator SpawnEnemies(int count)
        //{
        //    var bounds = _collider.bounds;
        //    var xRange = bounds.max.x - bounds.min.x;
        //    var spawned = 0;
        //    var tries = 0;
        //    while (spawned < count)
        //    {
        //        if (tries > 1000)
        //        {
        //            throw new Exception();
        //        }
        //        tries++;
        //        // random point in the plane
        //        var point = new Vector3(bounds.max.x - xRange * ((float)spawned / count), transform.position.y, Random.Range(bounds.min.z, bounds.max.z));
        //        // check if LoS is blocked by another enemy
        //        if (Physics.Linecast(DataManager.Instance.MainCamera.transform.position, point, Layers.EnemyMask))
        //        {
        //            continue;
        //        }

        //        var tooClose = CombatManager.Instance.GetEnemies().Any(e =>
        //        {
        //            Vector3 position;
        //            return (new Vector2((position = e.Transform.position).x, position.z) - new Vector2(point.x, point.z)).magnitude < .25f;
        //        });

        //        if (tooClose) continue;

        //        var go = Instantiate(_enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)], point, Quaternion.identity);
        //        var scale = go.transform.localScale;
        //        var enemy = go.GetComponent<Enemy>();
        //        var mesh = enemy.MeshRenderer;
        //        go.transform.position = new Vector3(point.x, point.y + mesh.bounds.extents.y * 0.6f, point.z);
        //        mesh.enabled = false;
        //        yield return null;
        //        mesh.enabled = true;
        //        go.transform.localScale = Vector3.zero;
        //        LeanTween.scale(go, scale, 1f).setEaseInOutBounce();
        //        yield return WaitFor.Seconds(0.5f);
        //        spawned++;
        //        tries = 0;
        //    }
        //}

    }
}