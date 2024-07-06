
//using System;
//using UnityEngine;

//namespace Crux.CRL.PoolSystem
//{
//    public class PooledAudio : PooledObjectBase
//    {
//        #region Field
//        [SerializeField]
//        private AudioSource _audioSource;
//        private AudioData _audioData;
//        private Transform _followTransform = null;
//        private int _audioIndex;
//        private Transform _transform;

//        public Action AudioEndCallback;
//        #endregion

//        #region Property
//        public AudioSource AudioSource { get { return _audioSource; } set { _audioSource = value; } }
//        public AudioData AudioData { get { return _audioData; } set { _audioData = value; } }
//        public Transform FollowTransform { set { _followTransform = value; } get { return _followTransform; } }
//        public int AudioIndex { get { return _audioIndex; } }
//        #endregion

//        #region Unity Callback
//        private void Awake()
//        {
//            _transform = transform;
//        }

//        private void Update()
//        {
//            if (!isActive || _audioData == null)
//                return;

//            if (_followTransform != null)
//            { _transform.position = _followTransform.position; }

//            //Loop Type
//            if ((_audioData.loopType == AudioManager.LoopType.Loop || _audioData.loopType == AudioManager.LoopType.RandomLoop))
//            {
//                if (false == _audioSource.loop)
//                    _audioSource.loop = true;

//                /********************************************************************************
//                * The commented-out code below is a custom looping method made by whoever
//                * was working on the firepoint core audio system at the time. It was causing
//                * issues because every time an audio file gets replayed, it must buffer which
//                * causes gaps between each loop iteration. For that reason, I commented the
//                * code out and changed over to using Unity's special looping method in the
//                * code above.
//                * 
//                * ~Pat
//                ********************************************************************************/

//                /*
//                //Keep playing loop audio
//                if (_audioSource.isPlaying == false)
//                {
//                    if (_audioIndex < _audioData.clips.Count - 1) { ++_audioIndex; }
//                    else { _audioIndex = 0; }
//                    AudioManager.instance.PlayAudio(_audioData, this);
//                }
//                */
//            }

//            //One shot 
//            else if ((_audioData.loopType == AudioManager.LoopType.Once || _audioData.loopType == AudioManager.LoopType.RandomOnce))
//            {
//                if (true == _audioSource.loop)
//                    _audioSource.loop = false;

//                if (_audioSource.isPlaying == false)
//                {
//                    StopAudio(); //Stop finished one shot audio
//                }
//            }
//        }
//        #endregion

//        public override void Spawn()
//        {
//            if (isActive)
//            {
//                Debug.LogError("trying to spawn active poolObject");
//            }
//            else
//            {
//                if (_audioSource == null) { _audioSource = GetComponent<AudioSource>(); }
//                if (_audioSource == null) { Debug.LogError("missing audioSource compoent on" + this.name); }

//                _audioSource.Play();
//                isActive = true;
//            }
//        }

//        public override void UnSpawn()
//        {
//            if (isActive)
//            {
//                if (pool == null)
//                {
//                    Debug.LogWarning("missing pool manager, destory this instance");
//                    if (this.gameObject != null) { Destroy(this.gameObject); }
//                }
//                else
//                {
//                    _audioSource.Stop();
//                    isActive = false;
//                    this.pool.stack.Push(this);
//                }
//            }
//            else
//            {
//                Debug.LogWarning("unspawn inactive instance, destory");
//                Destroy(this.gameObject);
//            }
//        }

//        public void StopAudio(float pFadeDuration = 0f)
//        {
//            if (pFadeDuration > 0)
//            {
//                AudioManager.instance.FadeOutAudio(this, pFadeDuration);
//            }
//            else
//            {
//                UnSpawn();
//            }
//            if (AudioEndCallback != null) { AudioEndCallback(); }
//        }
//    }
//}