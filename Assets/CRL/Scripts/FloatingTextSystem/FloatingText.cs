using System;
using Crux.CRL.CombatSystem;
using Crux.CRL.DataSystem;
using Crux.CRL.PoolSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Crux.CRL.FloatingTextSystem
{
    public class FloatingText : PooledUI
    {
        #region Enums

        public enum BgType
        {
            None,
            Damage
        }

        #endregion

        #region Fields

        [SerializeField] private TMP_Text _text;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _dmgImg;

        #endregion

        #region Properties

        public TMP_Text Text => _text;
        public RectTransform RectTransform => _rectTransform;

        #endregion

        #region Public Functions

        public void Init(string txt, Vector2 screenPos, float dur, Color color, TextSize size, int moveDistPixels = int.MaxValue, BgType bgType = BgType.None,
            LeanTweenType tweenType = LeanTweenType.easeOutQuart, bool fadeInAlpha = false)
        {
            _text.text = txt;
            SetScale(size);
            SetBg(bgType);
            //_text.fontSize = fontSize == 0 ? FloatingTextManager.Instance.MediumFontSize : fontSize;
            _text.color = color;

            if (fadeInAlpha)
            {
                canvasGroup.alpha = 0;
                LeanTween.alphaCanvas(canvasGroup, 1f, DataManager.UI_FADE_DURATION);
            }

            _rectTransform.localPosition = screenPos;
            _rectTransform.SetAsLastSibling();
            if (tweenType != LeanTweenType.notUsed)
            {
                LeanTween.moveY(_rectTransform, _rectTransform.localPosition.y + (moveDistPixels == int.MaxValue ? Random.Range(80,120) : moveDistPixels), dur).setEase(tweenType);
            }

            if (dur > 0)
                LeanTween.delayedCall(dur, () => { LeanTween.alphaCanvas(canvasGroup, 0f, DataManager.UI_FADE_DURATION).setOnComplete(UnSpawn); });
        }

        public void InitEnemyTxt(string txt, Enemy enemy, float duration, float speed, Color color, TextSize size, BgType bgType = BgType.None, LeanTweenType tweenType = LeanTweenType.easeOutQuart)
        {
            _text.text = txt;
            _text.color = color;

            SetBg(bgType);
            var pos = new Vector2(enemy.HealthBarWorldPos.x, enemy.HealthBarWorldPos.y /*+ enemy.HealthBarSize.y * 0.75f * enemy.HealthBarScale.y*/);
            _rectTransform.position = pos;
            _rectTransform.localScale = Vector3.zero;
            _rectTransform.SetAsLastSibling();

            if (tweenType != LeanTweenType.notUsed)
                LeanTween.moveY(_rectTransform, _rectTransform.localPosition.y + Random.Range(80, 120), duration).setEase(tweenType);

            LeanTween.scale(_rectTransform, GetScale(size) , duration * 0.04f).setEase(LeanTweenType.easeOutCirc).setOnComplete(() =>
                {
                    LeanTween.scale(_rectTransform, GetScale(size) * 0.5f, duration - duration * 0.04f).setEase(LeanTweenType.easeOutCirc);
                });
            LeanTween.alphaCanvas(canvasGroup, 0f, duration).setEase(LeanTweenType.easeInExpo).setOnComplete(UnSpawn);
        }

        #endregion

        #region Private Methods

        private void SetBg(BgType bgType)
        {
            switch (bgType)
            {
                case BgType.None:
                    _dmgImg.enabled = false;
                    break;
                case BgType.Damage:
                    _dmgImg.enabled = true;
                    break;
            }
        }

        private void SetScale(TextSize size)
        {
            _rectTransform.localScale = (int)size * 0.4f * Vector3.one;
        }

        private Vector3 GetScale(TextSize size)
        {
            return (int) size * 0.4f * Vector3.one;
        }
        #endregion
    }
}
