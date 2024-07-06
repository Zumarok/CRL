using System.Collections.Generic;
using Crux.CRL.Utils;
using UnityEngine;

namespace Crux.CRL.PoolSystem
{
    public class PoolManager : SingletonMonoBehaviour<PoolManager>
    {
        #region Const
        const int INITIAL_AUDIO_INSTANCE_NUMBER = 30;
        #endregion


        #region Static
        public static bool isExist { get { return Instance != null; } }
        #endregion


        #region Field 
        //[SerializeField] PooledAudio pooledAudioPrefab;
        public Dictionary<PooledObjectBase, PoolBase> poolDict = new Dictionary<PooledObjectBase, PoolBase>();
        #endregion


        #region Unity Callback 
        
        #endregion


        #region Util
        //void InitializeAudioPool()
        //{
        //    var tempAudioList = new List<PooledAudio>();
        //    for (int i = 0; i < INITIAL_AUDIO_INSTANCE_NUMBER; i++) { tempAudioList.Add(Spawn(pooledAudioPrefab)); }
        //    for (int i = 0; i < INITIAL_AUDIO_INSTANCE_NUMBER; i++) { tempAudioList[i].UnSpawn(); }
        //    tempAudioList = null;
        //}
        #endregion


        public static T Spawn<T>(T source, Transform poolParent = null) where T : PooledObjectBase
        {
            if (isExist == false)
            {
                Debug.LogError("missing PoolManager instance");
                return null;
            }

            var poolDict = PoolManager.Instance.poolDict;
            PoolBase targetPool = null;

            if (poolDict.TryGetValue(source, out targetPool) == false)
            {
                var go = new GameObject("(Pool)" + typeof(T).Name);
                targetPool = go.AddComponent<PoolBase>();
                targetPool.transform.SetParent(poolParent != null ? poolParent : Instance.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                targetPool.Initialize(source);
                poolDict.Add(source, targetPool);
            }

            T result = null;

            if (targetPool.stack.Count > 0)
            {
                result = (T)targetPool.stack.Pop();
            }
            else
            {
                result = GameObject.Instantiate<T>(source);
                result.pool = targetPool;
                result.transform.SetParent(result.pool.transform,true);
            }

            try { result.Spawn(); }
            catch (System.Exception e) { Debug.LogError(e); }
            return result;
        }

    }
}

