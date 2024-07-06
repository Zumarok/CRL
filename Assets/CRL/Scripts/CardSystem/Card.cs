using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Coffee.UISoftMask;
using Crux.CRL.AbilitySystem;
using Crux.CRL.CombatSystem;
using Crux.CRL.DataSystem;
using Crux.CRL.EnemySystem;
using Crux.CRL.FloatingTextSystem;
using Crux.CRL.NotificationSystem;
using Crux.CRL.UI;
using Crux.CRL.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crux.CRL.CardSystem
{
    #region Enums/Flags

    [Flags]
    public enum CardFlag
    {
        None = 0,
        FaceUp = 1 << 1,
        Zoomed = 1 << 2,
        Targeting = 1 << 3,
        Visible = 1 << 4,
        TargetingPlayer = 1 << 5,
        AbilityProcessing = 1 << 6,
        Flipping = 1 << 7,
    }

    #endregion

    public class Card : MonoBehaviour
    {
        #region Properties

        public CardFlag CardFlags { get; private set; }
        public CardAbility Ability { get; private set; }
        public RectTransform Transform { get; private set; }

        public int CardIndex { get; private set; }

        public IReadOnlyList<Combatant> Targets => _targets;

        #endregion

        #region Editor Fields

        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _cost;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _fxImage;
        [SerializeField] private Image _highlightImage;
        [SerializeField] private BoxCollider _collider;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private SoftMask _skillSoftMask;
        [SerializeField] private Image _skillIcon;
        [SerializeField] private TextMeshProUGUI _skillText;
        [SerializeField] private RectTransform _skillParentRect;
        #endregion

        #region Private Fields

        private int _zoomTweenId;
        private int _moveTweenId;
        private int _highlightFadeTweenId;
        private int _skillIconScaleTweenId;
        private int _skillIconScaleTweenId2;
        private int _skillIconPosTweenId;
        private static readonly int HighlightAlpha = Shader.PropertyToID("_HighlightAlpha");
        private Material _highlightMaterial;

        private readonly List<Combatant> _targets = new List<Combatant>(10);
        private Coroutine _clearTargetsCoroutine;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            Transform = gameObject.GetComponent<RectTransform>();
        }

        private void Start()
        {
            _canvas.worldCamera = DataManager.Instance.UICamera;
            _highlightImage.material = Instantiate(_highlightImage.material);
            _highlightMaterial = _highlightImage.material;
        }


        private void OnMouseEnter()
        {
            if (!CombatManager.Instance.Player.TurnActive) return;
            if (CombatManager.Instance.Player.CardManager.AnyCardsProcessing) return;
            if (CombatManager.Instance.Player.CardManager.AnyCardsFlipping) return;
            if (CombatManager.Instance.Player.CardManager.AnyCardsTargeting) return;
            if (!CardFlags.HasFlag(CardFlag.FaceUp)) return;

            NotificationManager.Instance.SendNotification(NotiEvt.UI.CloseAllSoloWindows);
            Zoom(true);
        }

        private void OnMouseOver()
        {
            if (!CombatManager.Instance.Player.TurnActive) return;
            if (CombatManager.Instance.Player.CardManager.AnyCardsProcessing) return;
            if (CombatManager.Instance.Player.CardManager.AnyCardsFlipping) return;
            if (CombatManager.Instance.Player.CardManager.AnyCardsTargeting) return;
            if (!CardFlags.HasFlag(CardFlag.FaceUp)) return;

            var linkIndex = TMP_TextUtilities.FindIntersectingLink(_description, Input.mousePosition, DataManager.Instance.UICamera);
            if (linkIndex < 0)
            {
                SmallInfoWindow.Instance.ShowKeywordTooltip(false, "");
            }
            else
            {
                var linkInfo = _description.textInfo.linkInfo[linkIndex];
                var id = linkInfo.GetLinkID();
                SmallInfoWindow.Instance.ShowKeywordTooltip(true, id);
            }
        }

        private void OnMouseDown()
        {
            if (!CombatManager.Instance.Player.TurnActive) return;
            if (CombatManager.Instance.Player.CardManager.AnyCardsProcessing) return;
            if (CombatManager.Instance.Player.CardManager.AnyCardsTargeting) return;

            if (!CardFlags.HasFlag(CardFlag.FaceUp)) return;

            if (CombatManager.Instance.Player.CurrentMana < Ability.ManaCost)
            {
                FloatingTextManager.Instance.ShowFloatingTextUICamera("Insufficient Mana", Transform, Color.red, TextSize.Medium, 0.2f);
                return;
            }
            
            if (Ability?.TargetType == TargetType.Player)
            {
                SetPlayerAsTarget();
                if (!CardFlags.HasFlag(CardFlag.TargetingPlayer))
                    AddFlag(CardFlag.TargetingPlayer);
            }
        }

        private void OnMouseDrag()
        {
            if (!CombatManager.Instance.Player.TurnActive) return;
            if (Ability == null) return;
            if (Ability.MaxTargets <= 0) return;
            if (Ability.TargetType == TargetType.Player) return;
            if (CombatManager.Instance.Player.CardManager.AnyCardsProcessing) return;
            if (!CardFlags.HasFlag(CardFlag.FaceUp)) return;
            if (CombatManager.Instance.Player.CurrentMana < Ability.ManaCost) return;

            if (!CardFlags.HasFlag(CardFlag.Targeting))
            {
                AddFlag(CardFlag.Targeting);
                if (!Ability.RandomTarget)
                    CombatManager.Instance.Player.CardManager.ShowTargetingArrow(true, Transform.parent.localPosition.x);
                Zoom(false);
            }

            if (Ability.RandomTarget)
            {
                CombatManager.Instance.Player.CardManager.CardFollowMouse(Transform);

                if (CombatManager.Instance.IsEnemyUnderCursor())
                    SetAllEnemiesAsTargets();
                else
                    ClearTargets();
            }
            else
            {
                CombatManager.Instance.Player.CardManager.UpdateTargetingArrowPos(Ability);
                SetTargetAtMouse(Ability);
            }
        }

        private void OnMouseUp()
        {
            if (!CombatManager.Instance.Player.TurnActive) return;
            if (CardFlags.HasFlag(CardFlag.AbilityProcessing)) return;
            if (!CardFlags.HasFlag(CardFlag.FaceUp)) return;

            if (CardFlags.HasFlag(CardFlag.Targeting))
            {
                if (CombatManager.Instance.IsEnemyUnderCursor())
                    CombatManager.Instance.TryUsePlayerAbility(this);
                else
                    ClearTargets();

                CombatManager.Instance.Player.CardManager.ShowTargetingArrow(false);
                RemoveFlag(CardFlag.Targeting);

            }
            else if (CardFlags.HasFlag(CardFlag.TargetingPlayer))
            {
                CombatManager.Instance.TryUsePlayerAbility(this);
                RemoveFlag(CardFlag.TargetingPlayer);
            }

            Zoom(false);
            ToggleHighlight(false);
        }

        private void OnMouseExit()
        {
            if (!CardFlags.HasFlag(CardFlag.Targeting))
                Zoom(false);

            if (CardFlags.HasFlag(CardFlag.AbilityProcessing)) return;
            if (!CardFlags.HasFlag(CardFlag.FaceUp)) return;
            if (Ability?.TargetType == TargetType.Player)
            {
                ClearTargets();
                if (CardFlags.HasFlag(CardFlag.TargetingPlayer))
                    RemoveFlag(CardFlag.TargetingPlayer);
            }
        }

        #endregion

        #region Public Methods

        public void InitCard(CardAbility ability, int cardIndex)
        {
            Ability = ability;
            _name.text = Ability.FormattedName;
            _cost.text = Ability.ManaCost.ToString();
            _description.text = Ability.LongDescription;
            _iconImage.sprite = Ability.IconSprite;

            if (Ability.CardFxMaterial != null)
            {
                _fxImage.material = Ability.CardFxMaterial;
                _fxImage.enabled = true;
            }

            CardIndex = cardIndex;
        }

        /// <summary>
        /// Moves the card to a position closer to the camera so the player can see the details.
        /// </summary>
        public void Zoom(bool zoomIn)
        {
            // don't zoom in if the card is face down or the turn is over
            if (zoomIn && (!CardFlags.HasFlag(CardFlag.FaceUp) || !CombatManager.Instance.Player.TurnActive)) return;
            //var point = _transform.InverseTransformPoint(CardManager.Instance.CardCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.5f)));
            //LeanTween.moveLocal(transform.gameObject, new Vector3(-point.x, point.y), .3f);
            var zoomAmount = CombatManager.Instance.Player.CardManager.CardZoomAmount;
            LeanTween.cancel(_zoomTweenId);
            LeanTween.cancel(_moveTweenId);

            if (zoomIn)
                AddFlag(CardFlag.Zoomed);
            else
                RemoveFlag(CardFlag.Zoomed);
            _zoomTweenId = LeanTween.scale(transform.gameObject, zoomIn ? Vector3.one * zoomAmount : Vector3.one, .25f).setEaseOutBack().uniqueId;
            _moveTweenId = LeanTween.moveLocal(gameObject, zoomIn ? Vector3.up * (_collider.size.y * ((zoomAmount - 1) * 0.5f)) : Vector3.zero, 0.25f).setEaseOutBack().uniqueId;

        }



        public IEnumerator ShowSkillIcon()
        {
            _skillIcon.sprite = Ability.IconSprite;
            _skillText.text = Ability.FormattedName;

            _skillParentRect.localScale = Vector3.zero;
            _skillParentRect.localPosition = Vector3.zero;
            _skillSoftMask.alpha = 1;
            var dur = 0.5f;
            LeanTween.cancel(_skillIconScaleTweenId);
            _skillIconScaleTweenId = LeanTween.scale(_skillParentRect, Vector3.one * 1.25f, dur).setEaseInOutBounce().uniqueId;

            yield return WaitFor.Seconds(dur);
        }


        public IEnumerator PlaySkillIconThrowAnimation()
        {
            var ui = PlayerUIManager.Instance;
            var tarWorldPos = _skillParentRect.position;
            var abilityTarget = _targets.FirstOrDefault();

            if (Ability.TargetType == TargetType.Enemy && abilityTarget != null)
            {
                tarWorldPos = ((Enemy)abilityTarget).HealthBarWorldPos;
            }
            else if (Ability.TargetType == TargetType.Player)
            {
                if (Ability.HasAbsorbFlag || Ability.HasHealFlag)
                    tarWorldPos = ui.HealthWorldPos;
                else if (Ability.HasBuffFlag || Ability.HasHotFlag)
                    tarWorldPos = ui.BuffbarWorldPos;
                else if (Ability.HasDebuffFlag || Ability.HasCleanseFlag || Ability.HasDoTFlag)
                    tarWorldPos = ui.DebuffbarWorldPos;
                else
                    tarWorldPos = _skillParentRect.position;
            }

            var moveTime = 1f;

            LeanTween.cancel(_skillIconPosTweenId);
            _skillIconPosTweenId = LeanTween.move(_skillParentRect.gameObject, tarWorldPos, moveTime).setEaseInQuart()
                .setDelay(0.1f).setOnComplete(() => _skillText.text = "").uniqueId;

            LeanTween.cancel(_skillIconScaleTweenId);
            _skillIconScaleTweenId = LeanTween.scale(_skillParentRect, Vector3.one * 2f, moveTime * 0.5f).setEaseInQuart().uniqueId;

            LeanTween.cancel(_skillIconScaleTweenId2);
            _skillIconScaleTweenId2 = LeanTween.scale(_skillParentRect, Vector3.zero, moveTime * 0.5f).setDelay(moveTime * 0.75f).setEaseInQuart().setOnComplete(() => _skillSoftMask.alpha = 0).uniqueId;

            yield return WaitFor.Seconds(moveTime);
        }


        public void OnAbilityStartProcessing()
        {
            AddFlag(CardFlag.AbilityProcessing);
        }

        public void OnAbilityFinishProcessing()
        {
            RemoveFlag(CardFlag.AbilityProcessing);
            ClearTargets();
        }

        public void OnCardVisible()
        {
            AddFlag(CardFlag.Visible);
        }

        public void OnCardInvisible()
        {
            RemoveFlag(CardFlag.Visible);
        }

        public void OnCardFaceUp()
        {
            AddFlag(CardFlag.FaceUp);
        }

        public void OnCardFaceDown()
        {
            RemoveFlag(CardFlag.FaceUp);
        }

        public void OnCardFlipping()
        {
            AddFlag(CardFlag.Flipping);
        }

        public void OnCardDoneFlipping()
        {
            RemoveFlag(CardFlag.Flipping);
        }
        #endregion

        #region Private Methods

        private void AddFlag(CardFlag flag)
        {
            CardFlags |= flag;
        }

        private void RemoveFlag(CardFlag flag)
        {
            CardFlags &= ~flag;
        }
        
        private void ResetLocalPos()
        {
            LeanTween.cancel(_moveTweenId);
            _moveTweenId = LeanTween.moveLocal(gameObject, Vector3.zero, 0.2f).setEaseOutQuad().uniqueId;
        }

        /// <summary>
        /// Toggles the card highlight border.
        /// </summary>
        /// <param name="enable"></param>
        private void ToggleHighlight(bool enable)
        {
            LeanTween.cancel(_highlightFadeTweenId);
            var curAlpha = _highlightMaterial.GetFloat(HighlightAlpha);
            _highlightFadeTweenId = LeanTween.value(gameObject, f => _highlightMaterial.SetFloat(HighlightAlpha, f),
                curAlpha, enable ? 1 : 0, 0.25f).uniqueId;
        }

        /// <summary>
        /// Add the enemy under the mouse to the currentTargets list.
        /// </summary>
        private void SetTargetAtMouse(IAbility ability)
        {
            if (ability.MaxTargets < 1) 
                return;

            if (_clearTargetsCoroutine != null)
                return;

            var enemy = CombatManager.Instance.GetEnemyAtCursor();

            if (enemy == null)
            {
                ClearTargets();
                return;
            }

            // if the hovered enemy is already in the target list
            // do nothing if primary target
            // clear targets and continue if it is a secondary target
            if (_targets.Contains(enemy))  
            {
                if (_targets[0] == enemy)
                    return;
                
                ClearTargets();
            }

            AddTarget(enemy);
            enemy.SetEnemyOutline(EnemyOutlineState.Selected);

            if (!ability.IsAoE) 
                return;

            var enemyPos = new Vector3(enemy.Transform.position.x, enemy.Collider.bounds.min.y, enemy.transform.position.z);
            PlayerUIManager.Instance.ShowRadiusCircle(true, enemyPos, ability.AbilityRadius);
            Physics.OverlapSphereNonAlloc(enemyPos, ability.AbilityRadius, AbilityProcessor.ColliderArray, Layers.EnemyMask);
           
            foreach (var col in AbilityProcessor.ColliderArray)
            {
                if (col == null)
                    break;
                    
                var e = col.GetComponent<Enemy>();
                if (e == null)
                    break;
                    
                if (_targets.Contains(e))
                    continue;

                AddTarget(e);
                e.SetEnemyOutline(EnemyOutlineState.Selected);
            }
        }

        /// <summary>
        /// Used to select multiple enemies as targets by painting the cursor over them.
        /// Call for Multi-Target abilities that do not use random secondary targeting.
        /// </summary>
        /// <param name="ability"></param>

        private void PaintEnemiesAsTargets(IAbility ability)
        {
            if (ability.MaxTargets < 1)
                return;

            var enemy = CombatManager.Instance.GetEnemyAtCursor();

            if (enemy == null && ability.MaxTargets == 1)
                ClearTargets();

            if (enemy == null)
                return;

            if (_targets.Contains(enemy))
                return;

            if (_targets.Count >= ability.MaxTargets)
                RemoveTarget(_targets[0]);

            AddTarget(enemy);
            enemy.SetEnemyOutline(EnemyOutlineState.Selected);
        }

        private void SetAllEnemiesAsTargets()
        {
            if (_targets.Count > 0) return;

            var enemies = CombatManager.Instance.GetEnemies();
            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                AddTarget(enemy);
                enemy.SetEnemyOutline(EnemyOutlineState.Selected);
            }
        }

        private void SetPlayerAsTarget()
        {
            ClearTargets();
            AddTarget(CombatManager.Instance.Player);
        }

        private void AddTarget(Combatant target)
        {
            if (target == null) return;
            if (_clearTargetsCoroutine != null) return;
            if (_targets.Contains(target)) return;

            if (_targets.Count == 0)
                ToggleHighlight(true);

            _targets.Add(target);
        }

        private void RemoveTarget(Combatant target)
        {
            if (target == null) return;
            if (_targets.Count == 0) return;
            if (!_targets.Contains(target)) return;

            if (_targets.Count == 1)
                ToggleHighlight(false);

            _targets.Remove(target);
            ((Enemy) target).SetEnemyOutline(EnemyOutlineState.Idle);
            PlayerUIManager.Instance.ShowRadiusCircle(false);
        }

        private void ClearTargets()
        {
            if (_clearTargetsCoroutine != null) return;
            _clearTargetsCoroutine = StartCoroutine(ClearTargetsCoroutine());
        }

        private IEnumerator ClearTargetsCoroutine()
        {
            // wait for the ability to finishing firing off before clearing targets
            while (CardFlags.HasFlag(CardFlag.AbilityProcessing)) 
                yield return null;

            if (_targets.Count > 0)
            {
                for (var i = 0; i < _targets.Count; i++)
                {
                    if (_targets[i].GetType() == typeof(Enemy))
                        ((Enemy) _targets[i]).SetEnemyOutline(EnemyOutlineState.Idle);
                }

                _targets.Clear();
                AbilityProcessor.ClearColliderArray();
                PlayerUIManager.Instance.ShowRadiusCircle(false);
            }

            ToggleHighlight(false);
            _clearTargetsCoroutine = null;
        }

        #endregion

    }
}
