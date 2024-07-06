using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Crux.CRL.EnemySystem;
using Crux.CRL.AbilitySystem;
using Crux.CRL.DataSystem;
using Crux.CRL.DialogSystem;
using Crux.CRL.FloatingTextSystem;
using Crux.CRL.FloorNavigationSystem;
using Crux.CRL.NotificationSystem;
using Crux.CRL.UI;
using Crux.CRL.Utils;
using Deform;
using UnityEngine;

namespace Crux.CRL.CombatSystem
{
    public class Enemy : Combatant
    {
        #region Unity Editor Fields

        [SerializeField] private EnemyRace _enemyRace;
        [SerializeField] private EnemySize _enemySize;
        [SerializeField] private EnemyClassification _enemyClassification;
        [SerializeField] private Material _enemyMaterial;
        [SerializeField] private AbilityName _defaultAttack;
        [SerializeField] private RippleDeformer _rippleDeform;
        [SerializeField] private PerlinNoiseDeformer _perlinDeform;
        [SerializeField] private TwirlDeformer _twirlDeform;
        [SerializeField] private WaveDeformer _waveDeform;
        [SerializeField] private SquashAndStretchDeformer _squashAndStretchDeform;
        [SerializeField] private ParticleSystem _onDeathParticleSystem;
        [SerializeField] private float _attackAnimDuration;
        [SerializeField] private float _deathAnimDuration;
        [SerializeField, Range(-1f, 1f)] private float _healthBarYOffsetPercent = 0.9f;
        [SerializeField, Range(0, 10f)]  private float _preTurnDelay = 0.25f;
        [SerializeField, Range(0, 10f)]  private float _postTurnDelay = 0.25f;
        [SerializeField] private Color _outlineColorIdle = Color.black;
        [SerializeField] private Color _outlineColorSelected = new Color(92,79,0);
        [SerializeField] private Color _outlineColorAttacking = new Color(92,0,0);
        [SerializeField] private Collider _collider;
        [SerializeField] private Renderer _renderer;

        #endregion

        #region Private Fields

        private EnemyHealthBar _healthBar;
        //private List<Renderer> _renderers = new List<Renderer>();

        private readonly List<Combatant> _debuffedTargets = new List<Combatant>(15);
        private readonly List<Combatant> _buffTargets = new List<Combatant>(15);
        private readonly List<Combatant> _healTargets = new List<Combatant>(15);
        private readonly List<Combatant> _absorbTargets = new List<Combatant>(15);

        private int _rippleTweenId;
        private int _squashTweenId;
        private int _twirlTweenId;
        private int _waveTweenId;
        private int _outlineTweenId;
        private int _tintTweenId;

        private bool _isMouseOver;
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

        private readonly Dictionary<TriggerType, Action> _onTriggerEventActions = new Dictionary<TriggerType, Action>();

        #endregion

        #region Properties

        public Vector2 HealthBarWorldPos => _healthBar.RectTransform.position;
        public Vector2 HealthBarSize => _healthBar.RectTransform.sizeDelta;
        public Vector3 HealthBarScale => _healthBar.RectTransform.localScale;
        public Renderer Renderer => _renderer;
        public Collider Collider => _collider;
        public bool IsMouseOver => _isMouseOver;
        public Ability NextAbility { get; private set; }
        public float HealthBarYOffsetPercent => _healthBarYOffsetPercent;
        public IReadOnlyList<Ability> PassiveAbilities { get; private set; }
        public IReadOnlyList<Ability> ActiveAbilities { get; private set; }
        public EnemyRace EnemyRace => _enemyRace; 
        public EnemySize EnemySize => _enemySize; 
        public EnemyClassification EnemyClassification => _enemyClassification; 

        #endregion

        #region Unity Callbacks

        private void OnMouseEnter()
        {
            _isMouseOver = true;
            if (!CombatManager.Instance.Player.TurnActive) return;
            if (CombatManager.Instance.Player.CardManager.AnyCardsTargeting) return;
            if (CombatManager.Instance.Player.CardManager.AnyCardsProcessing) return;
            EnemyInfoWindow.Instance.ShowHideInfo(this);
        }

        private void OnMouseExit()
        {
            _isMouseOver = false;
            EnemyInfoWindow.Instance.ShowHideInfo();
        }

        #endregion

        #region Unity Callback Overrides

        private void Awake()
        { PassiveAbilities = abilities.Select(a => DataManager.Instance.GetAbilityData(a)).Where(a => a.IsPassive).ToList();
            ActiveAbilities = abilities.Select(a => DataManager.Instance.GetAbilityData(a)).Where(a => !a.IsPassive).ToList();
            RegisterPassiveEventListeners();
        }

        protected override void Start()
        {
            base.Start();
            _healthBar = EnemyHealthBarManager.Instance.CreateHealthBar(this);
            _enemyMaterial = Instantiate(_enemyMaterial);
            _renderer.material = _enemyMaterial;

            var ranOffset = ThreadSafeRandom.Ran.Next(1000);
            _rippleDeform.Offset = ranOffset;
            _rippleDeform.Speed += ThreadSafeRandom.Ran.Next(-15, 15) * 0.01f * _rippleDeform.Speed;

            _perlinDeform.OffsetVector = new Vector4(ranOffset, ranOffset, ranOffset, 0);

            //_outlineEnemyMaterial.SetFloat(AseOutlineWidth, outlineWidth);
        }

        private void OnDisable()
        {   
            if (_healthBar != null)
                _healthBar.ToggleVisible(false);
        }

        private void OnDestroy()
        {
            RegisterPassiveEventListeners(false);
        }

        #endregion

        #region Public Override Functions

        public override IEnumerator ProcessTurn()
        {
            SetEnemyOutline(EnemyOutlineState.Attacking);

            //pre turn effects
            var pre = PreProcessEffects.ToArray();
            foreach (var tempEffect in pre)
            {
                _healthBar.DecrementTempEffectIcon(tempEffect);
            }
            yield return ProcessTempEffects(pre);
            yield return base.ProcessTurn();

            if (CurrentHp <= 0) yield break;

            var ability = NextAbility;
            SetAbilityTargets(ability);
            _healthBar.ClearIntent();
            _healthBar.ShowSkillIcon(ability);

            yield return WaitFor.Seconds(_preTurnDelay);
            
            PlayAttackAnim();
            _healthBar.PlaySkillIconThrowAnimation(ability, abilityTargets.FirstOrDefault());
            //FloatingTextManager.Instance.ShowFloatingText(ability.FormattedName, Transform, Color.white, TextSize.Medium);
            yield return WaitFor.Seconds(_attackAnimDuration * 1.25f);
            yield return AbilityProcessor.Process(ability, this, abilityTargets);

            //pre turn effects
            var post = PostProcessEffects.ToArray();
            foreach (var tempEffect in post)
            {
                _healthBar.DecrementTempEffectIcon(tempEffect);
            }
            yield return ProcessTempEffects(post);

            yield return WaitFor.Seconds(_postTurnDelay);
            SetEnemyOutline(EnemyOutlineState.Idle);
        }

        public void SetNextAbility()
        {
            NextAbility = GetRandomActiveAbility();
            _healthBar.SetIntent(NextAbility);
        }

        public override void SetAbilityTargets(Ability ability)
        {
            base.SetAbilityTargets(ability);
            if (ability.TargetType == TargetType.Player)
            {
                abilityTargets.Add(CombatManager.Instance.Player);
            }
            else
            {
                if (ability.SelfTarget)
                {
                    abilityTargets.Add(this);
                    return;
                }

                PopulateTargetLists();
                for (int i = 0; i < ability.MaxTargets; i++)
                {
                    if (ability.HasCleanseFlag && _debuffedTargets.Count > 0)
                    {
                        var tar = _debuffedTargets.GetRandom();
                        abilityTargets.Add(tar);
                        _debuffedTargets.Remove(tar);
                    }
                    else if (ability.HasHealFlag && _healTargets.Count > 0)
                    {
                        var tar = _healTargets.GetRandom();
                        abilityTargets.Add(tar);
                        _healTargets.Remove(tar);
                    }
                    else if (ability.HasAbsorbFlag && _absorbTargets.Count > 0)
                    {
                        var tar = _absorbTargets.GetRandom();
                        abilityTargets.Add(tar);
                        _absorbTargets.Remove(tar);
                    }
                    else if (abilityTargets.Count == 0)
                    {
                        // if no good buff target, just choose one at random
                        abilityTargets.Add(CombatManager.Instance.GetEnemies().GetRandom());
                    }
                }
            }
        }

        public override (int damagedAmount, int overKillAmount) ApplyDamage(DamageInfo info)
        {
            if (CombatantFlags.HasFlag(CombatantFlags.Dead)) return (0,0);

            var result = base.ApplyDamage(info);
            _healthBar.SetAbsorbPercent();
            _healthBar.SetHealthPercent();
            FloatingTextManager.Instance.ShowFloatingEnemyDamage(info.FinalValue, TextSize.Large ,this);


            if (CurrentHp > 0)
            {
                var hitPercent = (float)info.FinalValue / CurrentMaxHp;
                var type = hitPercent < 0.1f ? DialogEventType.OnTakeDamageSmall :
                    hitPercent < 0.3f ? DialogEventType.OnTakeDamageMid : DialogEventType.OnTakeDamageBig;
                EnemyHealthBarManager.Instance.ShowEnemyDialog(this, type);
                PlayDamagedAnim(info.FinalValue);
            }

            return result;
        }

        public override (int healedAmount, int overHealAmount) ApplyHeal(HealInfo info)
        {
            if (CombatantFlags.HasFlag(CombatantFlags.Dead)) return (0,0);
            
            var result = base.ApplyHeal(info);
            _healthBar.SetHealthPercent();
            FloatingTextManager.Instance.ShowFloatingEnemyHeal(info.FinalValue, TextSize.Large, this);
            return result;
        }

        public override bool ApplyDebuff(Debuff debuff)
        {
            if (!base.ApplyDebuff(debuff)) return false;

            EnemyHealthBarManager.Instance.ShowEnemyDialog(this, DialogEventType.OnTakeDebuff);

            _healthBar.InitTempEffectIcon(debuff);
            return true;
        }

        public override Buff ApplyBuff(Buff newBuff)
        {
            var activeBuff = base.ApplyBuff(newBuff);
            
            if (activeBuff == newBuff)
                _healthBar.InitTempEffectIcon(activeBuff);
            else if (activeBuff != null)
                _healthBar.UpdateTempEffectIcon(activeBuff);

            EnemyHealthBarManager.Instance.ShowEnemyDialog(this, DialogEventType.OnReceiveBuff);

            return activeBuff;
        }

        public override void ApplyCleanse(int count, string senderName)
        {
            for (int i = tempEffects.Count - 1; i >= 0; i--)
            {
                var tempEffect = tempEffects[i];
                if (tempEffect is Debuff)
                {
                    EnemyHealthBarManager.Instance.ShowEnemyDialog(this, DialogEventType.OnTakeCleanse);
                    _healthBar.RemoveTempEffectIcon(tempEffect);
                    effectsToRemove.Add(tempEffect);
                }

                if (effectsToRemove.Count == count)
                    break;
            }

            base.ApplyCleanse(count, senderName);
        }

        public override void ApplyDispel(int count, string senderName)
        {
            for (int i = tempEffects.Count - 1; i >= 0; i--)
            {
                var tempEffect = tempEffects[i];
                if (tempEffect is Buff)
                {
                    EnemyHealthBarManager.Instance.ShowEnemyDialog(this, DialogEventType.OnTakeDispel);
                    _healthBar.RemoveTempEffectIcon(tempEffect);
                    effectsToRemove.Add(tempEffect);
                }

                if (effectsToRemove.Count == count)
                    break;
            }

            base.ApplyDispel(count, senderName);
        }

        public override (int absorbAppliedAmount, int overShieldAmount) ApplyAbsorb(int amount, string senderName)
        {
            if (CombatantFlags.HasFlag(CombatantFlags.Dead)) return (0,0);

            var result = base.ApplyAbsorb(amount, senderName);
            _healthBar.SetAbsorbPercent();
            _healthBar.SetHealthPercent();
            FloatingTextManager.Instance.ShowFloatingEnemyAbsorb(amount, TextSize.Large, this);
            return result;
        }

        public override void OnDeath()
        {
            base.OnDeath();
            FloatingTextManager.Instance.ShowFloatingTextMainCamera("DEAD!", Transform, Color.red, TextSize.Medium);
            _healthBar.ToggleVisible(false);
            
            if (_onDeathParticleSystem != null)
                _onDeathParticleSystem.Play();

            _twirlDeform.update = true;
            LeanTween.cancel(_twirlTweenId);
            _twirlTweenId = LeanTween.value(gameObject, f => { _twirlDeform.Factor = f; },
                0, 200f,_deathAnimDuration).setEaseInCubic().setOnComplete(() => _twirlDeform.update = false).uniqueId;

            var curOutlineColor = _enemyMaterial.GetColor("_OutlineColor");
            var transOutline = curOutlineColor;
            transOutline.a = 0;
            LeanTween.cancel(_outlineTweenId);
            _enemyMaterial.SetColor("_OutlineColor", transOutline);

            var curTintColor = _enemyMaterial.GetColor("_Tint");
            var transTint = curTintColor;
            transTint.a = 0;

            _isMouseOver = false;
            EnemyInfoWindow.Instance.ForceClose();

            if (_enemyRace == EnemyRace.Goblin)
                NotificationManager.Instance.SendNotification(NotiEvt.Combat.OnGoblinDeath);

            LeanTween.cancel(_tintTweenId);
            _tintTweenId = LeanTween.value(gameObject, c => { _enemyMaterial.SetColor("_Tint", c); }, curTintColor, transTint, _deathAnimDuration).setEaseInCubic().setOnComplete(
                () =>
                {
                    Destroy(transform.parent.gameObject);
                }).uniqueId;
        }

        public override void RefreshUI()
        {
            CurrentHp = CurrentHp;
            _healthBar.Refresh();
        }

        private void DisintegrateUpdate(float val)
        {
        }

        #endregion

        #region Public Methods

        //public void ToggleOutlineMaterial(bool outline)
        //{
        //    if (CombatantFlags.HasFlag(CombatantFlags.Dead)) return;
        //    //foreach (var r in _renderers)
        //    //{
        //    //    r.material = outline ? _outlineEnemyMaterial : _enemyMaterial;
        //    //}

        //    //if (!outline) return;
        //    //var pixSize = _collider.PixelSizeInViewport(DataManager.Instance.MainCamera);
        //    //var outlineWidth = 1f / pixSize * 100f / pixSize + 0.01f;
        //    //_outlineEnemyMaterial.SetFloat(AseOutlineWidth, outlineWidth);
        //}

        public void SetEnemyOutline(EnemyOutlineState state)
        {
            if (CombatantFlags.HasFlag(CombatantFlags.Dead)) return;
            var curColor = _enemyMaterial.GetColor("_OutlineColor");
            LeanTween.cancel(_outlineTweenId);
            switch (state)
            {
                case EnemyOutlineState.Idle:
                    _outlineTweenId = LeanTween.value(gameObject, c => { _enemyMaterial.SetColor("_OutlineColor", c); }, curColor, _outlineColorIdle, 0.1f).setEaseInSine().uniqueId;
                    break;
                case EnemyOutlineState.Selected:
                    _outlineTweenId = LeanTween.value(gameObject, c => { _enemyMaterial.SetColor("_OutlineColor", c); }, curColor, _outlineColorSelected, 0.1f).setEaseInSine().uniqueId;
                    break;
                case EnemyOutlineState.Attacking:
                    _outlineTweenId = LeanTween.value(gameObject, c => { _enemyMaterial.SetColor("_OutlineColor", c); }, curColor, _outlineColorAttacking, 0.1f).uniqueId;
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void PopulateTargetLists()
        {
            var allTars = CombatManager.Instance.GetEnemies();
            _debuffedTargets.Clear();
            _buffTargets.Clear();
            _healTargets.Clear();
            _absorbTargets.Clear();

            for (int i = 0, c = allTars.Count; i < c; i++)
            {
                var com = allTars[i];
                if (com.NumActiveDebuffs > 0)
                    _debuffedTargets.Add(com);
                if (com.CurrentHpPercent < 1f)
                    _healTargets.Add(com);
                if (com.NumActiveBuffs < Constants.MAX_BUFFS)
                    _buffTargets.Add(com);
                if (com.CurrentAbsorb < MaxAbsorb)
                    _absorbTargets.Add(com);
            }
        }

        private void PlayDamagedAnim(int dmg)
        {
            if (_waveDeform == null) return;
            _waveDeform.update = true;
            // 
            var val = Mathf.Clamp(0.8f + (0.7f * ((dmg - (CurrentMaxHp * 0.1f)) / (CurrentMaxHp * 0.4f))), 0.8f, 1.5f); 
            LeanTween.cancel(_waveTweenId);
            _waveTweenId = LeanTween.value(gameObject, f => { _waveDeform.Steepness = f; },
                val, 0, 1f).setOnComplete(() => _waveDeform.update = false).uniqueId;
        }

        private void PlayAttackAnim()
        {
            if (_squashAndStretchDeform == null) return;
            _squashAndStretchDeform.update = true;
            LeanTween.cancel(_squashTweenId);
            var del = _attackAnimDuration / 3f;
            _squashTweenId = LeanTween.value(gameObject, f => { _squashAndStretchDeform.Factor = f; },
                0, 1, del).uniqueId;
            _squashTweenId = LeanTween.value(gameObject, f => { _squashAndStretchDeform.Factor = f; },
                1, -1, del).setDelay(del).uniqueId;
            _squashTweenId = LeanTween.value(gameObject, f => { _squashAndStretchDeform.Factor = f; },
                -1, 0, del).setDelay(del*2).setOnComplete(() => _squashAndStretchDeform.update = false).uniqueId;
        }

        private Ability GetRandomActiveAbility()
        {
            return ActiveAbilities.Count == 0 ? DataManager.Instance.GetAbilityData(_defaultAttack) : ActiveAbilities.GetRandom();
        }

        private void RegisterPassiveEventListeners(bool register = true)
        {
            foreach (var passiveAbility in PassiveAbilities)
            {
                if (passiveAbility.TriggerType.HasFlag(TriggerType.OnGoblinDeath))
                {
                    if (register)
                    {
                        Action pFunc = () =>
                        {
                            if (IsAlive)
                                StartCoroutine(AbilityProcessor.ProcessTrigger(passiveAbility, this, this));
                        };
                        _onTriggerEventActions.Add(TriggerType.OnGoblinDeath, pFunc);
                        NotificationManager.Instance.AddListener(NotiEvt.Combat.OnGoblinDeath, pFunc);
                    }
                    else
                    {
                        var pFunc = _onTriggerEventActions[TriggerType.OnGoblinDeath];
                        _onTriggerEventActions.Remove(TriggerType.OnGoblinDeath);
                        NotificationManager.Instance.RemoveListener(NotiEvt.Combat.OnGoblinDeath, pFunc);
                    }
                }
            }
        }

        protected override void ApplyReelMods(TowerFloorInfo result)
        {
            CurrentPercentMaxHpMod += result.EnemyPercentMaxHp;
            CurrentPercentDamageDealt += result.EnemyPercentDmg;
            CurrentExtraDD += result.EnemyExtraDDHits;
        }

        #endregion

    }
}

