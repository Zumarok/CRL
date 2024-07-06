using System.Collections.Generic;
using Crux.CRL.CombatSystem;
using Crux.CRL.DataSystem;
using Crux.CRL.NotificationSystem;
using Crux.CRL.PoolSystem;
using Crux.CRL.Utils;
using UnityEngine;

namespace Crux.CRL.FloatingTextSystem
{
    #region Public Enums

    public enum TextSize
    {
        XXSmall = 1,
        XSmall,
        Small,
        Medium,
        Large,
        XLarge,
        XXLarge
    }

    #endregion

    public class FloatingTextManager : SingletonMonoBehaviour<FloatingTextManager>
    {
        #region Private Fields

        [SerializeField] private FloatingText _floatingTextPrefab;

        private Transform _transform;

        #endregion

        #region Properties

        #endregion

        #region Unity Callbacks

        protected override void Awake()
        {
            base.Awake();
            _transform = transform;
        }

        private void Start()
        {
            InitializePool();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show floating text at a Main Camera rendered transform. 
        /// </summary>
        /// <param name="txt">Text to show</param>
        /// <param name="tar">The transform where the text will show. This should be rendering in the MAIN CAMERA.</param>
        /// <param name="color">Text color</param>
        /// <param name="size">Text size</param>
        /// <param name="yPercentOffset">Y offset for the text spawn. Between -0.5f and 0.5f (screen percent from center)</param>
        /// <param name="duration">duration before fading</param>
        /// <param name="moveDistPixels">The distance to move the text, default value moves up a random value between 80 to 120px.</param>
        public void ShowFloatingTextMainCamera(string txt, Transform tar, Color color, TextSize size, float yPercentOffset = 0, float duration = 2f, int moveDistPixels = int.MaxValue)
        {
            var ft = PoolManager.Spawn(_floatingTextPrefab, _transform);
            var viewPoint = DataManager.Instance.MainCamera.WorldToViewportPoint(tar.position) - new Vector3(0.5f, 0.5f);
            //var screenPos = DataManager.Instance.MainCamera.ViewportToScreenPoint(viewPoint) + Vector3.up * yOffset;
            var yOffset = PlayerUIManager.Instance.MainUICanvas.pixelRect.height * Mathf.Clamp(yPercentOffset, -0.5f, 0.5f);
            var screenPos = new Vector3(viewPoint.x * DataManager.Instance.MainCamera.pixelWidth, viewPoint.y * DataManager.Instance.MainCamera.pixelHeight + yOffset);
            ft.Init(txt, screenPos, duration, color, size, moveDistPixels, FloatingText.BgType.None, moveDistPixels == 0 ? LeanTweenType.notUsed : LeanTweenType.easeOutQuart);
        }

        /// <summary>
        /// Show floating text at a Main Camera rendered transform. 
        /// </summary>
        /// <param name="txt">Text to show</param>
        /// <param name="tar">The transform where the text will show. This should be rendering in the MAIN CAMERA.</param>
        /// <param name="color">Text color</param>
        /// <param name="size">Text size</param>
        /// <param name="yPercentOffset">Y offset for the text spawn. Between -0.5f and 0.5f (screen percent from center)</param>
        /// <param name="duration">duration before fading</param>
        /// <param name="moveDistPixels">The distance to move the text, default value moves up a random value between 80 to 120px.</param>
        public void ShowFloatingTextUICamera(string txt, Transform tar, Color color, TextSize size, float yPercentOffset = 0, float duration = 2f, int moveDistPixels = int.MaxValue)
        {
            var ft = PoolManager.Spawn(_floatingTextPrefab, _transform);
            var canvasPos = PlayerUIManager.Instance.MainUICanvas.transform.InverseTransformPoint(tar.position);
            var yOffset = PlayerUIManager.Instance.MainUICanvas.pixelRect.height * Mathf.Clamp(yPercentOffset, -0.5f, 0.5f);
            var screenPos = new Vector3(canvasPos.x, canvasPos.y + yOffset);
            ft.Init(txt, screenPos, duration, color, size, moveDistPixels, FloatingText.BgType.None, moveDistPixels == 0 ? LeanTweenType.notUsed : LeanTweenType.easeOutQuart);
        }
        
        public void ShowStaticCenterText(string txt, Color color, TextSize size, float duration = 2f, float yPercentOffset = 0.15f)
        {
            var ft = PoolManager.Spawn(_floatingTextPrefab, _transform);
            var yOffset = PlayerUIManager.Instance.MainUICanvas.pixelRect.height * Mathf.Clamp(yPercentOffset, -0.5f, 0.5f);
            var screenPos = new Vector2(0, yOffset);
            ft.Init(txt, screenPos, duration, color, size, 0, FloatingText.BgType.None, LeanTweenType.notUsed, true);
        }


        public void ShowFloatingEnemyDamage(int dmg, TextSize size, Enemy enemy)
        {
            var ft = PoolManager.Spawn(_floatingTextPrefab, _transform);
            ft.InitEnemyTxt($"-{dmg}", enemy, 2f, 1f, Color.white, size, FloatingText.BgType.Damage);
        }

        public void ShowFloatingEnemyHeal(int heal, TextSize size, Enemy enemy)
        {
            var ft = PoolManager.Spawn(_floatingTextPrefab, _transform);
            ft.InitEnemyTxt($"+{heal}", enemy, 2f, 1f, Color.green, size);
        }

        public void ShowFloatingEnemyAbsorb(int absorb, TextSize size, Enemy enemy)
        {
            var ft = PoolManager.Spawn(_floatingTextPrefab, _transform);
            ft.InitEnemyTxt($"+{absorb} absorb", enemy, 2f, 1f, Color.yellow, size);
        }

        #endregion

        #region Private Methods

        private void InitializePool()
        {
            var l = new List<FloatingText>();
            for (int i = 0; i < 10; i++) { l.Add(PoolManager.Spawn(_floatingTextPrefab, _transform)); }
            for (int i = 0; i < 10; i++) { l[i].UnSpawn(); }
        }

        #endregion
    }
}

