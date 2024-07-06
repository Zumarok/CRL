using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Crux.CRL.AbilitySystem;
using Crux.CRL.CombatSystem;
using Crux.CRL.DataSystem;
using Crux.CRL.FloatingTextSystem;
using Crux.CRL.Utils;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crux.CRL.EnemySystem
{
    public class EnemyHealthBar : SerializedMonoBehaviour
    {
        #region Editor Fields

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _currentHealthBar;
        [SerializeField] private Image _trailingHealthBar;
        [SerializeField] private Image _absorbHealthBar;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private TextMeshProUGUI _intentText;
        [SerializeField] private Dictionary<EnemyIntent, CanvasGroup> _intentIcons;
        [SerializeField] private BuffIcon[] _buffIcons;
        [SerializeField] private BuffIcon[] _debuffIcons;
        [SerializeField] private CanvasGroup _skillCanvasGroup;
        [SerializeField] private Image _skillIcon;
        [SerializeField] private TextMeshProUGUI _skillText;
        [SerializeField] private RectTransform _skillParentRect;

        #endregion

        #region Private Fields

        private Transform _ownerTransform;
        private Enemy _enemy;
        private Transform _transform;
        private Collider _collider;
        private Coroutine _positionTrackingCoroutine;
        private bool _activePosTrackingEnabled;
        private int _currentUiHealth;
        private int _currentUiAbsorb;
        private Ability _intentAbility;

        private int _colorTweenId;
        private int _healthBarTweenId;
        private int _trailBarTweenId;
        private int _healthTextTweenId;
        private int _absorbTweenId;
        private int _absorbTextTweenId;
        private int _skillIconScaleTweenId;
        private int _skillIconScaleTweenId2;
        private int _skillIconPosTweenId;
        private readonly Dictionary<TempEffect, BuffIcon> _activeTempEffectIcons = new Dictionary<TempEffect, BuffIcon>();

        #endregion

        #region Properties

        public RectTransform RectTransform => _rectTransform;
        public float Distance { get; private set; }

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _transform = transform;
        }

        #endregion

        #region Public Methods

        public void Init(Enemy enemy)
        {
            _ownerTransform = enemy.transform;
            _enemy = enemy;
            _collider = enemy.Collider;
            var diameter = _collider.bounds.extents.magnitude;
            Distance = Vector3.Distance(enemy.transform.position, DataManager.Instance.MainCamera.transform.position);
            var pixelWidth = DistanceAndDiameterToPixelSize(Distance, diameter);
            _rectTransform.localScale = Vector3.one * Mathf.Min(0.5f, Mathf.Max(0.3f, pixelWidth / 400));
            _currentUiHealth = enemy.CurrentHp;
            _healthText.text = $"{enemy.CurrentHp}{(enemy.CurrentAbsorb > 0 ? "[" + enemy.CurrentAbsorb + "]" : "")}/{enemy.CurrentMaxHp}";
            //_skillText.color = StaticUtils.ColorFromHex(Constants.CLC_SKILL);
            UpdatePosition();
        }

        public void SetHealthPercent()
        {
            var healthPercent = _enemy.CurrentHpPercent;

            LeanTween.cancel(_colorTweenId);
            LeanTween.cancel(_healthBarTweenId);
            LeanTween.cancel(_trailBarTweenId);
            LeanTween.cancel(_healthTextTweenId);

            _colorTweenId = LeanTween.color(_currentHealthBar.rectTransform, GetCurrentHealthColor(), 0.1f).uniqueId;
            _healthBarTweenId = LeanTween.value(gameObject, v => _currentHealthBar.fillAmount = v,
                _currentHealthBar.fillAmount, healthPercent, 0.1f).uniqueId;
            _trailBarTweenId = LeanTween.value(gameObject, v => _trailingHealthBar.fillAmount = v,
                _trailingHealthBar.fillAmount, healthPercent, 0.1f).setDelay(1).uniqueId;
            _healthTextTweenId = LeanTween.value(gameObject, SetHealthText, _currentUiHealth, _enemy.CurrentHp, 1f).uniqueId;
        }

        public void SetAbsorbPercent()
        {
            LeanTween.cancel(_absorbTweenId);
            LeanTween.cancel(_absorbTextTweenId);

            _absorbTweenId = LeanTween.value(gameObject, v => _absorbHealthBar.fillAmount = v,
                _absorbHealthBar.fillAmount, _enemy.CurrentAbsorbPercent, 0.1f).uniqueId;
            _absorbTextTweenId = LeanTween.value(gameObject, SetAbsorbText, _currentUiAbsorb, _enemy.CurrentAbsorb, 1f).uniqueId;
        }

        public void SetIntent(Ability ability)
        {
            if (ability == null) return;

            foreach (var key in _intentIcons.Keys)
            {
                _intentIcons[key].SetVisibilityAndInteractive(key == ability.EnemyIntent);
            }

            switch (ability.EnemyIntent)
            {
                case EnemyIntent.Attack:
                case EnemyIntent.AttackDebuff:
                    _intentText.text = (DamageInfo.CalculateFinalDamage(ability.DDValue, _enemy, CombatManager.Instance.Player) +
                                       DamageInfo.CalculateFinalDamage(ability.DoTValue, _enemy, CombatManager.Instance.Player) +
                                       (ability.IsMultiHit ? $"<sub> X </sub>{ability.NumOfHits}" : ""));
                    break;
                case EnemyIntent.Defensive:
                case EnemyIntent.Debuff:
                case EnemyIntent.None:
                    _intentText.text = "";
                    break;
            }
            _intentAbility = ability;
        }

        public void ClearIntent()
        {
            foreach (var key in _intentIcons.Keys)
            {
                _intentIcons[key].SetVisibilityAndInteractive(false);
            }
            _intentText.text = "";
            _intentAbility = null;
        }

        public void Refresh()
        {
            SetHealthPercent();
            SetAbsorbPercent();
            SetIntent(_intentAbility);
        }

        public void ToggleActivePositionTracking(bool isEnabled)
        {
            _activePosTrackingEnabled = isEnabled;

            if (_activePosTrackingEnabled && _positionTrackingCoroutine == null)
                _positionTrackingCoroutine = StartCoroutine(ActivePositionTracking());
        }

        public void ToggleVisible(bool isVisible)
        {
            LeanTween.cancel(_rectTransform);
            LeanTween.alphaCanvas(_canvasGroup, isVisible ? 0.75f : 0, DataManager.UI_FADE_DURATION);
        }

        public void UpdatePosition()
        {
            if (_collider == null) return;

            var bounds = _collider.bounds;
            var mainCam = DataManager.Instance.MainCamera;
            var uiCam = DataManager.Instance.UICamera;
            //ownerPos.y + _ownerTransform.localScale.y * EnemyHealthBarManager.Instance.HealthBarYOffsetPercent
            
            var enemyScreenPos = mainCam.WorldToScreenPoint(new Vector3(bounds.center.x, bounds.max.y, bounds.center.z));
            var uiWorldPos = uiCam.ScreenToWorldPoint(enemyScreenPos);
            _transform.position = new Vector3(uiWorldPos.x, uiWorldPos.y, 0);


            //var offsetPos = new Vector3(ownerPos.x, bounds.max.y + (bounds.max.y - bounds.min.y) * _enemy.HealthBarYOffsetPercent,
            //    ownerPos.z);

            //var pos = DataManager.Instance.MainCamera.WorldToViewportPoint(offsetPos) - new Vector3(0.5f, 0.5f, 0.5f);
            //_transform.localPosition = new Vector3(pos.x * DataManager.Instance.UICamera.pixelWidth, pos.y * DataManager.Instance.UICamera.pixelHeight);
        }

        public void InitTempEffectIcon(TempEffect tempEffect)
        {
            var buffIcon = tempEffect.GetType() == typeof(Buff)
                ? _buffIcons.FirstOrDefault(i => !i.isActiveAndEnabled)
                : _debuffIcons.FirstOrDefault(i => !i.isActiveAndEnabled);
            if (buffIcon == null) return;

            buffIcon.Init(tempEffect);
            _activeTempEffectIcons.Add(tempEffect, buffIcon);
            FloatingTextManager.Instance.ShowFloatingTextUICamera("+" + tempEffect.Name, _rectTransform, Color.white, TextSize.XSmall, -0.02f, 1.5f, 0);
        }

        public void DecrementTempEffectIcon(TempEffect effect)
        {
            _activeTempEffectIcons[effect].DecrementCounter();

            if (_activeTempEffectIcons[effect].Counter <= 0)
            {
                RemoveTempEffectIcon(effect);
            }
        }

        public void UpdateTempEffectIcon(TempEffect effect)
        {
            _activeTempEffectIcons[effect].UpdateCounters();
        }

        public void RemoveTempEffectIcon(TempEffect tempEffect)
        {
            _activeTempEffectIcons[tempEffect].gameObject.SetActive(false);
            _activeTempEffectIcons.Remove(tempEffect);
            FloatingTextManager.Instance.ShowFloatingTextMainCamera("-" + tempEffect.Name, _enemy.transform, Color.white, TextSize.Medium);
        }

        public void ShowSkillIcon(Ability ability)
        {
            _skillIcon.sprite = ability.IconSprite;
            _skillText.text = ability.FormattedName;

            _skillParentRect.localScale = Vector3.zero;
            _skillParentRect.localPosition = Vector3.zero;
            _skillCanvasGroup.alpha = 1;

            LeanTween.cancel(_skillIconScaleTweenId);
            _skillIconScaleTweenId = LeanTween.scale(_skillParentRect, Vector3.one * 1.25f, 0.5f).setEaseInOutBounce().uniqueId;
        }


        public void PlaySkillIconThrowAnimation(Ability ability, Combatant abilityTarget)
        {
            var ui = PlayerUIManager.Instance;
            Vector3 tarWorldPos;

            switch (ability.EnemyIntent)
            {
                case EnemyIntent.None:
                case EnemyIntent.Attack:
                    tarWorldPos = ui.HealthWorldPos;
                    break;
                case EnemyIntent.Defensive:
                    if (ability.SelfTarget || ability.IsMultiTarget || abilityTarget == null || abilityTarget.GetType() != typeof(Enemy))
                    {
                        tarWorldPos = _skillParentRect.position;
                    }
                    else
                    {
                        tarWorldPos = ((Enemy)abilityTarget).HealthBarWorldPos;
                    }
                    break;
                case EnemyIntent.Debuff:
                case EnemyIntent.AttackDebuff:

                    if (ability.Type.HasFlag(AbilityType.DD))
                        tarWorldPos = ui.HealthWorldPos;
                    else if (ability.Type.HasFlag(AbilityType.Dispel))
                        tarWorldPos = ui.BuffbarWorldPos;
                    else
                        tarWorldPos = ui.DebuffbarWorldPos;
                    break;
                default:
                    tarWorldPos = _skillParentRect.position;
                    break;
            }

            var moveTime = 1f;

            LeanTween.cancel(_skillIconPosTweenId);
            _skillIconPosTweenId = LeanTween.move(_skillParentRect.gameObject, tarWorldPos, moveTime).setEaseInQuart()
                .setDelay(0.1f).setOnComplete(() => _skillText.text = "").uniqueId;

            LeanTween.cancel(_skillIconScaleTweenId);
            _skillIconScaleTweenId = LeanTween.scale(_skillParentRect, Vector3.one * 2f, moveTime * 0.5f).setEaseInQuart().uniqueId;

            LeanTween.cancel(_skillIconScaleTweenId2);
            _skillIconScaleTweenId2 = LeanTween.scale(_skillParentRect, Vector3.zero, moveTime * 0.5f).setDelay(moveTime * 0.75f).setEaseInQuart().setOnComplete(() => _skillCanvasGroup.alpha = 0).uniqueId;
        }

        #endregion

        #region Private Methods

        private IEnumerator ActivePositionTracking()
        {
            while (_activePosTrackingEnabled)
            {
                UpdatePosition();
                yield return null;
            }

            _positionTrackingCoroutine = null;
        }

        private Color GetCurrentHealthColor()
        {
            return _enemy.CurrentHpPercent <= 0.5f
                ? Color.Lerp(Color.red, Color.yellow, _enemy.CurrentHpPercent / 0.5f)
                : Color.Lerp(Color.yellow, Color.green, (_enemy.CurrentHpPercent - 0.5f) / 0.5f);
        }

        /// <summary>
        /// Get the screen size of an object's diameter in pixels, given its distance and diameter.
        /// https://forum.unity.com/threads/this-script-gives-you-objects-screen-size-in-pixels.48966/
        /// </summary>
        /// <param name="distance">Distance from the camera to the object. ex: Vector3.Distance(target.position, Camera.main.transform.position)</param>
        /// <param name="diameter">Diameter of the object. ex: target.collider.bounds.extents.magnitude</param>
        /// <returns></returns>
        private float DistanceAndDiameterToPixelSize(float distance, float diameter)
        {
            return diameter * Mathf.Rad2Deg * Screen.height / (distance * DataManager.Instance.MainCamera.fieldOfView);
        }

        private void SetHealthText(float value)
        {
            var intVal = Convert.ToInt32(value);
            _currentUiHealth = intVal;
            //_healthText.text = $"{intVal}/{_enemy.MaxHp}";
            _healthText.text = $"{intVal}{(_currentUiAbsorb > 0 ? "<sup><color=yellow>(" + _currentUiAbsorb + ")</color></sup>" : "")}/{_enemy.CurrentMaxHp}";
        }

        private void SetAbsorbText(float value)
        {
            var intVal = Convert.ToInt32(value);
            _currentUiAbsorb = intVal;
        }

        #endregion
        
    }

}
