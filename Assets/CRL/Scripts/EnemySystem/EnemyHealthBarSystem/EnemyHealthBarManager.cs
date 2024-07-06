using System;
using System.Collections.Generic;
using System.Linq;
using Crux.CRL.CombatSystem;
using Crux.CRL.DataSystem;
using Crux.CRL.DialogSystem;
using Crux.CRL.NotificationSystem;
using Crux.CRL.Utils;
using TMPro;
using UnityEngine;

namespace Crux.CRL.EnemySystem
{
    public class EnemyHealthBarManager : SingletonMonoBehaviour<EnemyHealthBarManager>
    {
        #region Private Fields

        [SerializeField] private EnemyHealthBar _healthBarPrefab;
        [SerializeField] private CanvasGroup _chatBubbleCanvas;
        [SerializeField] private RectTransform _chatRect;
        [SerializeField] private TextMeshProUGUI _chatTxt;

        private readonly Dictionary<Enemy, EnemyHealthBar> _enemyHealthBars = new Dictionary<Enemy, EnemyHealthBar>();

        private Transform _transform;

        private int _chatScaleTweenId;
        private float _lastEnemyChatTime;
        private float _lastIdleChatTry;

        #endregion

        #region Properties

        #endregion

        #region Unity Callbacks

        protected override void Awake()
        {
            base.Awake();
            _transform = transform;
        }

        #endregion

        #region Public Methods

        public EnemyHealthBar CreateHealthBar(Enemy enemy)
        {
            //var position = enemy.Transform.position;
            //var pos = new Vector3(position.x, position.y + enemy.Transform.localScale.y * _healthBarYOffsetPercent, position.z);
            //var a = DataManager.Instance.MainCamera.WorldToScreenPoint(pos);
            var healthBar = Instantiate(_healthBarPrefab, _transform);
            healthBar.Init(enemy);
            _enemyHealthBars.Add(enemy, healthBar);
            SortSiblingIndex();
            return healthBar;
        }

        public void ToggleAllHealthBarPositionTracking(bool isEnabled)
        {
            //foreach (var healthBar in _enemyHealthBars.Values)
            //{
            //    healthBar.ToggleActivePositionTracking(isEnabled);
            //}

            var enumerator = _enemyHealthBars.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Value.ToggleActivePositionTracking(isEnabled);
                }
            }
            finally
            {
                enumerator.Dispose();
            }

        }

        public void UpdateAllHealthBarPosition()
        {
            var enumerator = _enemyHealthBars.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Value.UpdatePosition();
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        public void ShowEnemyDialog(Enemy enemy, DialogEventType eventType)
        {
            if (enemy == null) return;

            if (eventType == DialogEventType.Idle)
            {
                if (Time.timeSinceLevelLoad - _lastIdleChatTry < Constants.MIN_SEC_BETWEEN_EVENT_CHAT) return;

                _lastIdleChatTry = Time.timeSinceLevelLoad;

                var ran = ThreadSafeRandom.Ran.NextDouble();
                if (ran > Constants.IDLE_CHAT_CHANCE) return;
            }
            else
            {
                if (Time.timeSinceLevelLoad - _lastEnemyChatTime < Constants.MIN_SEC_BETWEEN_EVENT_CHAT) return;

                var ran = ThreadSafeRandom.Ran.NextDouble();
                if (ran > Constants.EVENT_CHAT_CHANCE) return;
            }


            var msg = DataManager.Instance.GetLocalizedDialog(enemy.Name, eventType);
            if (string.IsNullOrWhiteSpace(msg)) return;

            _lastEnemyChatTime = Time.timeSinceLevelLoad;
            _chatRect.localScale = Vector3.zero;
            _chatRect.position = enemy.HealthBarWorldPos;
            _chatTxt.text = msg;
            _chatBubbleCanvas.alpha = 1;

            LeanTween.cancel(_chatScaleTweenId);
            _chatScaleTweenId = LeanTween.scale(_chatRect, Vector3.one, 0.5f).setEaseInOutBounce().uniqueId;

            _chatScaleTweenId = LeanTween.scale(_chatRect, Vector3.one * 1.025f, 0.75f).setLoopPingPong(2).setDelay(0.5f).setEaseInOutSine().uniqueId;

            _chatScaleTweenId = LeanTween.scale(_chatRect, Vector3.zero, 0.25f).setDelay(3.5f).setOnComplete(
                () => { _chatBubbleCanvas.alpha = 0; }).setEaseOutCubic().uniqueId;

        }

        #endregion

        #region Private Methods

        private void OnResolutionChanged()
        {
            UpdateAllHealthBarPosition();
        }

        private void SortSiblingIndex()
        {
            var l = _enemyHealthBars.Select(e => e.Value).OrderBy(e => e.Distance);
            foreach (var bar in l)
            {
                bar.transform.SetAsFirstSibling();
            }
        }

        #endregion
    }

}
