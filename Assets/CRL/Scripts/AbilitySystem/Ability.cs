using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Crux.CRL.EnemySystem;
using Crux.CRL.CombatSystem;
using Crux.CRL.DataSystem;
using Crux.CRL.KeywordSystem;
using Crux.CRL.Localization;
using Crux.CRL.PoolSystem;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Crux.CRL.AbilitySystem
{
    [Serializable]
    public class Ability : IAbility
    {
        #region Reference Strings
        
        [SerializeField, ReadOnly, FoldoutGroup("Reference Strings")]
        private AbilityName _abilityName;
        [SerializeField, ReadOnly, FoldoutGroup("Reference Strings")]
        private string _enumStringName;
        [SerializeField, ReadOnly, FoldoutGroup("Reference Strings")]
        private string _displayNameId;
        [SerializeField, ReadOnly, FoldoutGroup("Reference Strings")]
        private string _shortDescriptionId;
        [SerializeField, ReadOnly, FoldoutGroup("Reference Strings")]
        private string _longDescriptionId;
        
        #endregion

        #region Conditional Editor Fields
        
        [SerializeField] 
        private AbilityType _abilityType;

        [SerializeField, ShowIfGroup(nameof(HasTargetType))]
        private TargetType _targetType;

        [SerializeField, ShowIfGroup(nameof(IsPlayerAbility))] 
        private int _manaCost;

        [SerializeField, ShowIfGroup(nameof(IsEnemyAbility))]
        private EnemyIntent _intent;

        [SerializeField, ShowIfGroup(nameof(HasBuffOrDebuffFlag))]
        private BuffType _buffType;

        
        [SerializeField, ShowIfGroup(nameof(HasBuffFlatDmgDealt))]
        private short _buffFlatDamageDealtValue;

        [SerializeField, ShowIfGroup(nameof(HasBuffPercentDmgDealt))]
        private float _buffPercentDamageDealtValue;

        [SerializeField, ShowIfGroup(nameof(HasBuffFlatDmgTaken))]
        private short _buffFlatDamageTakenValue;

        [SerializeField, ShowIfGroup(nameof(HasBuffPercentDmgTaken))]
        private float _buffPercentDamageTakenValue;

        [SerializeField, ShowIfGroup(nameof(HasBuffDamageTakenOnCast))]
        private ushort _buffDamageTakenOnCastValue;

        [SerializeField, ShowIfGroup(nameof(HasBuffHealTakenOnCast))]
        private ushort _buffHealTakenOnCastValue;
        
        [SerializeField, ShowIfGroup(nameof(HasBuffExtraCast))]
        private ushort _buffExtraCast;

        [SerializeField, ShowIfGroup(nameof(HasBuffExplodeOnExpire))]
        private ushort _buffExplodeValue;

        [SerializeField, ShowIfGroup(nameof(HasBuffTakeDamageOnTurn))]
        private ushort _buffTakeDamageOnTurnValue;

        [SerializeField, ShowIfGroup(nameof(HasBuffPercentMaxHp))]
        private float _buffPercentMaxHp;


        [SerializeField, ShowIfGroup(nameof(IsTempEffect))]
        private bool _isPermanent;

        [SerializeField, ShowIfGroup(nameof(IsTempEffect))]
        private bool _canStack;

        [SerializeField, ShowIfGroup(nameof(CanAoE))]
        private bool _isAoe;

        [SerializeField, ShowIfGroup(nameof(IsAoE))]
        private float _aoeRadius;

        [SerializeField, ShowIfGroup(nameof(CanMultiHit))] 
        private bool _isMultiHit;

        [SerializeField, ShowIfGroup(nameof(IsMultiHit))]
        private float _delayBetweenHits;

        [SerializeField, ShowIfGroup(nameof(CanMultiTarget))]
        private bool _isMultiTarget;

        [SerializeField, ShowIfGroup(nameof(IsMultiTarget))]
        private float _delayBetweenTargets;

        [SerializeField, ShowIfGroup(nameof(CanRandomTarget))] 
        private bool  _isRandomTarget;

        [SerializeField, ShowIfGroup(nameof(CanRandomSecondaryTarget)), PropertyTooltip("Any secondary targets are randomly chosen.")]
        private bool _isRandomSecondaryTarget;

        [SerializeField, ShowIfGroup(nameof(CanSelfTarget))]
        private bool _isSelfTarget;

        [SerializeField, ShowIfGroup(nameof(HasSetDDValue))] 
        private ushort _ddValue;

        [SerializeField, ShowIfGroup(nameof(IsMultiTarget))]
        private ushort _maxTargets = 1;
        [SerializeField, ShowIfGroup(nameof(IsMultiHit))]
        private ushort _numHits = 1;

        [SerializeField, ShowIfGroup(nameof(HasDDAndSplashValid)), PropertyTooltip("Full damage is dealt at epicenter and scales down to 0 at the AoE radius")]
        private bool   _splashDamage;

        [SerializeField, ShowIfGroup(nameof(HasDDAndAoEValid))]
        private bool _aoeDamage;
        [SerializeField, ShowIfGroup(nameof(HasDDAndAoE))]
        private ushort _aoeDDValue;

        [SerializeField, ShowIfGroup(nameof(HasDoTFlag))] 
        private ushort _dotValue;

        [SerializeField, ShowIfGroup(nameof(HasDoTAndTargetEnemy))]
        private bool _aoeDot;
        [SerializeField, ShowIfGroup(nameof(HasDoTAndAoE))]
        private ushort _aoeDotValue;


        [SerializeField, ShowIfGroup(nameof(HasDoTFlag)), PropertyTooltip("Damage value scales up or down each time it deals damage.")]
        private bool _scalingDot;
        [SerializeField, ShowIfGroup(nameof(HasScaling)), PropertyTooltip("If checked, the scaling value will be multiplied rather than added/subtracted.")]
        private bool _scalingValueIsMultiplier;
        [SerializeField, ShowIfGroup(nameof(HasScaling)), PropertyTooltip("Amount the value will change per round. (+/-)")]
        private float _scalingValue;
        [SerializeField, ShowIfGroup(nameof(HasDoTFlag)), PropertyTooltip("Heals the caster when dealing damage.")]
        private bool _recourseDot;

        [SerializeField, ShowIfGroup(nameof(HasSetHealValue))]
        private ushort _healValue;

        [SerializeField, ShowIfGroup(nameof(HasHotFlag))]
        private ushort _hotValue;
        [SerializeField, ShowIfGroup(nameof(HasHotFlag))]
        private bool _hotRemoveAndHeal;
        [SerializeField, ShowIfGroup(nameof(HasHoTRemoveAndHeal))]
        private float _hotRemoveAndHealMult = 1;
        [SerializeField, ShowIfGroup(nameof(HasHoTAndTargetEnemy))]
        private bool _aoeHot;
        [SerializeField, ShowIfGroup(nameof(HasHotFlag))]
        private bool _scalingHot;

        [SerializeField, ShowIfGroup(nameof(HasCleanseFlag))]
        private ushort _cleanseValue;

        [SerializeField, ShowIfGroup(nameof(HasDispelFlag))]
        private ushort _dispelValue;
        
        [SerializeField, ShowIfGroup(nameof(HasSetAbsorbValue))]
        private ushort _absorbValue;

        [SerializeField, ShowIfGroup(nameof(HasEffectModFlag))]
        private bool _spreadDots;

        [SerializeField, ShowIfGroup(nameof(HasEffectModFlag))]
        private bool _extendDots;
        
        [SerializeField, ShowIfGroup(nameof(HasDuration))]
        private ushort _duration;

        [SerializeField, ShowIfGroup(nameof(HasCanTrigger))]
        private TriggerType _triggerType;

        #if UNITY_EDITOR
        [ValueDropdown(nameof(TriggeredAbilities))]
        #endif
        [SerializeField, ShowIfGroup(nameof(HasCanTrigger))]
        private AbilityName _triggerAbility;

        [SerializeField, ShowIfGroup(nameof(HasTriggerChance))]
        private float _triggerChance;

        [SerializeField, ShowIfGroup(nameof(HasTriggered))]
        private bool _triggerPassesValue;

        [SerializeField, ShowIfGroup(nameof(HasTriggerPassesValue))]
        private float _triggerValuePercent;

        // Reel Mods
        [SerializeField, ShowIfGroup(nameof(IsReelMod))]
        private ReelModType _reelModType;

        [SerializeField, ShowIfGroup(nameof(HasReelExtraWaves))]
        private int _numExtraWaves;

        [SerializeField, ShowIfGroup(nameof(HasReelEnemyHealthPercent))]
        private float _reelEnemyHealthPercent;

        [SerializeField, ShowIfGroup(nameof(HasReelEnemyDamagePercent))]
        private float _reelEnemyDamagePercent;

        [SerializeField, ShowIfGroup(nameof(HasReelEnemyExtraDD))]
        private int _reelNumExtraDD;

        [SerializeField, ShowIfGroup(nameof(HasReelPlayerHealthPercent))]
        private float _reelPlayerHealthPercent;

        [SerializeField, ShowIfGroup(nameof(HasReelPlayerDamagePercent))]
        private float _reelPlayerDamagePercent;

        [SerializeField, ShowIfGroup(nameof(HasReelPlayerFlatMana))]
        private int _reelPlayerFlatMana;

        [SerializeField, ShowIfGroup(nameof(HasReelSpawnElite))]
        private int _reelNumSpawnedElites;

        [SerializeField, ShowIfGroup(nameof(HasReelSpawnGoldMonster))]
        private int _reelNumSpawnedGoldMonsters;

        [SerializeField, ShowIfGroup(nameof(HasReelGoldModifier))]
        private float _reelGoldModifier;

        [SerializeField, ShowIfGroup(nameof(HasReelShardModifier))]
        private float _reelShardModifier;

        [SerializeField, ShowIfGroup(nameof(IsReelMod))]
        private bool _reelUnique;

        [SerializeField, ShowIfGroup(nameof(IsReelMod))]
        private int _reelModMinFloor;

        [SerializeField, ShowIfGroup(nameof(IsReelMod)), Range(1, 20), Tooltip("Higher number means more likely to appear.")]
        private int _reelModWeight;

#if UNITY_EDITOR
        [ValueDropdown(nameof(VfxList))]
        #endif
        [SerializeField]
        private PooledVFX _vfx;
        [SerializeField, ShowIfGroup(nameof(HasVFX))]
        private float _vfxDamageDelay;
        [SerializeField, ShowIfGroup(nameof(HasVFX))]
        private float _vfxPlaybackSpeed = 1f;
        [SerializeField, ShowIfGroup(nameof(HasVFX))]
        private float _vfxLifetime = 10f;
        [SerializeField, ShowIfGroup(nameof(HasVFX))]
        private bool _vfxScaleToEnemy;
        [SerializeField, ShowIfGroup(nameof(HasVFX))]
        private bool _vfxInitTargetOnly;

        [SerializeField, ShowIfGroup(nameof(HasSFX))]
        private string _sfxPath;

        [SerializeField] private Sprite _iconSprite;

        [SerializeField, ShowIfGroup(nameof(IsPlayerAbility))]
        private Material _cardFxMaterial;

        

        #endregion
        
        #region IAbility Properties

        public AbilityName AbilityName => _abilityName;
        public string FormattedName => DataManager.Instance.GetLocalizedString(_displayNameId);//string.Join(" ", Regex.Split(_abilityName.ToString(), @"(?<!^)(?=[A-Z])"));
        public AbilityType Type => _abilityType;
        public string EnumStringName => _enumStringName;

        public string LongDescription => DataManager.Instance.GetLocalizedValueAndKeywordString(_longDescriptionId, this).parsedString;
        public string ShortDescription(Combatant abilityOwner = null, Combatant target = null) => DataManager.Instance.GetLocalizedValueAndKeywordString(_shortDescriptionId, this, abilityOwner, target).parsedString;
        //public string TempEffectShortDesc(Combatant abilityOwner) => DataManager.Instance.GetLocalizedValueAndKeywordString(_shortDescriptionId, this, abilityOwner).parsedString;
        public List<Keyword> Keywords => DataManager.Instance.GetLocalizedValueAndKeywordString(_longDescriptionId, this).keywordList;
        

        public int ManaCost => _manaCost;
        public BuffType BuffType => _buffType;
        public ushort MaxTargets => _maxTargets;
        public bool RandomTarget => _isRandomTarget;
        public bool RandomSecondaryTarget => _isRandomSecondaryTarget;
        public bool SelfTarget => _isSelfTarget;
        public float AbilityRadius => _aoeRadius;
        public ushort NumOfHits => _numHits;
        public TargetType TargetType => _targetType;
        public EnemyIntent EnemyIntent => _intent;
        public bool IsMultiTarget => _isMultiTarget;
        public float DelayBetweenTargets => _delayBetweenTargets;
        public bool IsMultiHit => _isMultiHit;
        public float DelayBetweenHits => _delayBetweenHits;
        public ushort DDValue => _ddValue;
        public short BuffFlatDamageDealtValue => _buffFlatDamageDealtValue;
        public float BuffPercentDamageDealtValue => _buffPercentDamageDealtValue;
        public short BuffFlatDamageTakenValue => _buffFlatDamageTakenValue;
        public float BuffPercentDamageTakenValue => _buffPercentDamageTakenValue;
        public ushort BuffDamageTakenOnCastValue => _buffDamageTakenOnCastValue;
        public ushort BuffHealTakenOnCastValue => _buffHealTakenOnCastValue;
        public ushort BuffExtraCast => _buffExtraCast;
        public ushort BuffExplodeValue => _buffExplodeValue;
        public ushort BuffTakeDamageOnTurnValue => _buffTakeDamageOnTurnValue;
        public float BuffPercentMaxHp => _buffPercentMaxHp;
        public bool SplashDamage => _splashDamage;
        public ushort DoTValue => _dotValue;
        public ushort Duration => _duration;
        public bool AoEDoT => _aoeDot;
        public ushort AoEDotDamage => _aoeDotValue;
        public bool ScalingDoT => _scalingDot;
        public bool ScalingValueIsMultiplier => _scalingValueIsMultiplier;
        public float ScalingValue => _scalingValue;
        public bool RecourseDoT => _recourseDot;
        public ushort HealValue => _healValue;
        public ushort HoTValue => _hotValue;
        public bool ScalingHOT => _scalingHot;
        public float HoTRemoveAndHealMult => _hotRemoveAndHealMult;
        public ushort CleanseValue => _cleanseValue;
        public ushort DispelValue => _dispelValue;
        public ushort AbsorbValue => _absorbValue;
        public TriggerType TriggerType => _triggerType;
        public float TriggerChance => _triggerChance;
        public AbilityName TriggerAbility => _triggerAbility;
        public bool TriggerPassesValue => _triggerPassesValue;
        public float TriggerValuePercent => _triggerValuePercent;
        public bool IsPassive => _abilityType.HasFlag(AbilityType.Passive);

        // Reel Mods
        public ReelModType ReelModType => _reelModType;
        public int NumReelExtraWaves => _numExtraWaves;
        public bool IsReelMod => _abilityType.HasFlag(AbilityType.ReelMod);
        public bool HasReelExtraWaves => IsReelMod && _reelModType.HasFlag(ReelModType.ExtraWave);
        public bool HasNoReelBonus => IsReelMod && _reelModType.HasFlag(ReelModType.None);
        public bool HasReelEnemyHealthPercent => IsReelMod && _reelModType.HasFlag(ReelModType.EnemyHealthPercent);
        public float ReelEnemyHealthPercent => _reelEnemyHealthPercent;
        public bool HasReelEnemyDamagePercent => IsReelMod && _reelModType.HasFlag(ReelModType.EnemyDamagePercent);
        public float ReelEnemyDamagePercent => _reelEnemyDamagePercent;
        public bool HasReelEnemyExtraDD => IsReelMod && _reelModType.HasFlag(ReelModType.EnemyExtraDDAttack);
        public int ReelNumExtraDD => _reelNumExtraDD;
        public bool HasReelPlayerHealthPercent => IsReelMod && _reelModType.HasFlag(ReelModType.PlayerHealthPercent);
        public float ReelPlayerHealthPercent => _reelPlayerHealthPercent;
        public bool HasReelPlayerDamagePercent => IsReelMod && _reelModType.HasFlag(ReelModType.PlayerDamagePercent);
        public float ReelPlayerDamagePercent => _reelPlayerDamagePercent;
        public bool HasReelPlayerFlatMana => IsReelMod && _reelModType.HasFlag(ReelModType.PlayerFlatMana);
        public int ReelPlayerFlatMana => _reelPlayerFlatMana;
        public bool HasReelSpawnElite => IsReelMod && _reelModType.HasFlag(ReelModType.SpawnElite);
        public int ReelNumSpawnedElites => _reelNumSpawnedElites;
        public bool HasReelSpawnBoss => IsReelMod && _reelModType.HasFlag(ReelModType.SpawnBoss);
        public bool HasReelSpawnGoldMonster => IsReelMod && _reelModType.HasFlag(ReelModType.SpawnGoldMonster);
        public int ReelNumSpawnedGoldMonsters => _reelNumSpawnedGoldMonsters;
        public bool HasReelGoldModifier => IsReelMod && _reelModType.HasFlag(ReelModType.LootGoldModifier);
        public float ReelGoldModifier => _reelGoldModifier;
        public bool HasReelShardModifier => IsReelMod && _reelModType.HasFlag(ReelModType.LootShardModifier);
        public float ReelShardModifier => _reelShardModifier;
        public bool HasReelUnique => _reelUnique;
        public int ReelModMinFloor => _reelModMinFloor;
        public int ReelModWeight => _reelModWeight;
        public bool IsReel => _abilityType.HasFlag(AbilityType.Reel);

        public Sprite IconSprite => _iconSprite;
        public Material CardFxMaterial => _cardFxMaterial;
        public string SfxPath => _sfxPath;
        public PooledVFX Vfx => _vfx;
        public bool VfxScaleToEnemy => _vfxScaleToEnemy;
        public float VfxPlaybackSpeed => _vfxPlaybackSpeed;
        public float VfxLifetime => _vfxLifetime;
        public float VfxDamageDelay => _vfxDamageDelay;
        public bool VfxInitTargetOnly => _vfxInitTargetOnly;

        #endregion

        #region ShowGroup Conditional Properties

        public bool HasTargetType => !IsReelMod && !IsReel;
        public bool HasDDFlag => _abilityType.HasFlag(AbilityType.DD);
        public bool HasDDAndSplashValid => _abilityType.HasFlag(AbilityType.DD) && _targetType == TargetType.Enemy && !_aoeDamage;
        public bool HasDDAndAoEValid => _abilityType.HasFlag(AbilityType.DD) && _targetType == TargetType.Enemy && !_splashDamage;
        public bool HasDDAndSplash => _abilityType.HasFlag(AbilityType.DD) && _splashDamage;
        public bool HasDDAndAoE => _abilityType.HasFlag(AbilityType.DD) && _aoeDamage;
        public bool IsEnemyAbility => _abilityType.HasFlag(AbilityType.EnemyAbility) && !IsReelMod;
        public bool IsEnemyBuff => _abilityType.HasFlag(AbilityType.EnemyAbility) && _abilityType.HasFlag(AbilityType.Buff);
        public bool IsPlayerAbility => !_abilityType.HasFlag(AbilityType.EnemyAbility) && !IsReelMod && !IsReel;
        public bool IsEnemyTarget => _targetType == TargetType.Enemy;
        public bool HasDoTFlag => _abilityType.HasFlag(AbilityType.DOT);
        public bool HasDoTAndTargetEnemy => _abilityType.HasFlag(AbilityType.DOT) && _targetType == TargetType.Enemy;
        public bool HasDoTAndAoE => _abilityType.HasFlag(AbilityType.DOT) && _aoeDot;
        public bool HasScaling => _scalingDot || _scalingHot;
        public bool HasScalingAndPercent => (_scalingDot || _scalingHot) && _scalingValueIsMultiplier;
        public bool HasHealFlag => _abilityType.HasFlag(AbilityType.Heal);
        public bool HasHotFlag => _abilityType.HasFlag(AbilityType.HOT);
        public bool HasBuffFlag => _abilityType.HasFlag(AbilityType.Buff);
        public bool HasDebuffFlag => _abilityType.HasFlag(AbilityType.Debuff);
        public bool HasBuffOrDebuffFlag => _abilityType.HasFlag(AbilityType.Buff) || _abilityType.HasFlag(AbilityType.Debuff);
        public bool HasHoTAndTargetEnemy => _abilityType.HasFlag(AbilityType.HOT) && _targetType == TargetType.Enemy;
        public bool HasHoTAndAoE => _abilityType.HasFlag(AbilityType.HOT) && _aoeHot;
        public bool HasHoTRemoveAndHeal => _hotRemoveAndHeal;
        public bool HasCleanseFlag => _abilityType.HasFlag(AbilityType.Cleanse);
        public bool HasDispelFlag => _abilityType.HasFlag(AbilityType.Dispel);
        public bool HasImmunityFlag => _abilityType.HasFlag(AbilityType.Immunity);
        public bool HasAbsorbFlag => _abilityType.HasFlag(AbilityType.Absorb);

        public bool IsTempEffect => _abilityType.HasFlag(AbilityType.DOT) ||
                                    _abilityType.HasFlag(AbilityType.HOT) || _abilityType.HasFlag(AbilityType.Immunity) ||
                                    _abilityType.HasFlag(AbilityType.Buff);

        public bool IsStackingEffect => _canStack;
        public bool HasVFX => _vfx != null;
        public bool HasSFX => true;
        public bool HasEffectModFlag => _abilityType.HasFlag(AbilityType.EffectMod);
        public bool HasSpreadDots => _spreadDots;
        public bool HasExtendDots => _extendDots;
        public bool HasDuration => (HasDoTFlag || HasHotFlag || HasExtendDots || HasImmunityFlag || _spreadDots || HasBuffOrDebuffFlag) && !_isPermanent;

        public bool CanAoE => _targetType == TargetType.Enemy;
        public bool CanMultiTarget => _targetType == TargetType.Enemy && !_isSelfTarget;
        public bool CanMultiHit => _abilityType.HasFlag(AbilityType.DD);
        public bool CanRandomTarget => _targetType == TargetType.Enemy && !_isSelfTarget;
        public bool CanRandomSecondaryTarget => _targetType == TargetType.Enemy && !_isSelfTarget && _isMultiTarget;

        public bool CanSelfTarget => _targetType == TargetType.Enemy && _abilityType.HasFlag(AbilityType.EnemyAbility) &&
                                     (_abilityType.HasFlag(AbilityType.Buff) || _abilityType.HasFlag(AbilityType.Absorb) ||
                                      _abilityType.HasFlag(AbilityType.Cleanse) || _abilityType.HasFlag(AbilityType.HOT) ||
                                      _abilityType.HasFlag(AbilityType.Heal) || _abilityType.HasFlag(AbilityType.Immunity));

        public bool IsAoE => _isAoe;

        // Has Buff defined
        public bool HasBuffFlatDmgDealt => HasBuffOrDebuffFlag && _buffType.HasFlag(BuffType.FlatDamageDealt);
        public bool HasBuffPercentDmgDealt => HasBuffOrDebuffFlag && _buffType.HasFlag(BuffType.PercentDamageDealt);
        public bool HasBuffFlatDmgTaken => HasBuffOrDebuffFlag && _buffType.HasFlag(BuffType.FlatDamageTaken);
        public bool HasBuffPercentDmgTaken => HasBuffOrDebuffFlag && _buffType.HasFlag(BuffType.PercentDamageTaken);
        public bool HasBuffDamageTakenOnCast => HasBuffOrDebuffFlag && _buffType.HasFlag(BuffType.TakeDamageOnCast);
        public bool HasBuffHealTakenOnCast => HasBuffOrDebuffFlag && _buffType.HasFlag(BuffType.TakeHealOnCast);
        public bool HasBuffExtraCast => HasBuffOrDebuffFlag && _buffType.HasFlag(BuffType.ExtraCast);
        public bool HasBuffExplodeOnExpire => HasBuffOrDebuffFlag && _buffType.HasFlag(BuffType.ExplodeOnExpire);
        public bool HasBuffTakeDamageOnTurn => HasBuffOrDebuffFlag && _buffType.HasFlag(BuffType.TakeDamageOnTurn);
        public bool HasBuffPercentMaxHp => HasBuffOrDebuffFlag && _buffType.HasFlag(BuffType.PercentMaxHp);

        // Triggers
        public bool HasCanTrigger => _abilityType.HasFlag(AbilityType.CanTrigger);
        public bool HasTriggered => _abilityType.HasFlag(AbilityType.Triggered);
        public bool HasTriggerChance => HasCanTrigger && _triggerType.HasFlag(TriggerType.PercentChance);
        public bool HasTriggerPassesValue => HasTriggered && _triggerPassesValue;

        public bool HasSetDDValue => HasDDFlag && !HasTriggerPassesValue;
        public bool HasSetAbsorbValue => HasAbsorbFlag && !HasTriggerPassesValue;
        public bool HasSetHealValue => HasHealFlag && !HasTriggerPassesValue;
        
        
        #endregion

        #region Constructor/Destructor

        public Ability(string name, AbilityName abilityName, string displayNameId, string shortDescriptionId, string longDescriptionId)
        {
            _enumStringName = name;
            _abilityName = abilityName;
            _displayNameId = displayNameId;
            _shortDescriptionId = shortDescriptionId;
            _longDescriptionId = longDescriptionId;
            //ColliderArray = new Collider[Constants.MAX_NUM_ENEMIES];
        }

        #endregion

        #region Editor Functions


#if UNITY_EDITOR

        public List<PooledVFX> VfxList()
        {
            var path = Application.dataPath + @"\CRL\Prefabs\VFX\";
            var list = new List<PooledVFX>();
            var vfxStrings = Directory.GetFiles(path).Where(s => s.Contains(".prefab") && !s.Contains(".meta")).ToArray();
            for (var index = 0; index < vfxStrings.Length; index++)
            {
                var s = vfxStrings[index];
                s = s.Replace(Application.dataPath, "");
                s = "Assets/" + s.Replace("\\", "/");
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(s);
                if (obj == null) continue;

                var vfx = obj.GetComponent<PooledVFX>();
                if (vfx != null)
                    list.Add(vfx);
            }

            return list;
        }

        public List<AbilityName> TriggeredAbilities()
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableObject AbilitiesData");
            if (guids?.Length == 0) return new List<AbilityName>();
            var obj = AssetDatabase.LoadAssetAtPath<AbilitiesData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return obj.abilities.Where(a => a._abilityType.HasFlag(AbilityType.Triggered)).Select(a => a.AbilityName).ToList();
        }

        [Button]
        public void GoToAbilityText()
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableObject LocalizationData");
            if (guids?.Length == 0) return;
            var obj = AssetDatabase.LoadAssetAtPath<LocalizationData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            Selection.SetActiveObjectWithContext(obj, null);
        }

#endif


        #endregion

        

    }

}

