
using System;
using Crux.CRL.AbilitySystem;
using Crux.CRL.CombatSystem;
using Crux.CRL.DataSystem;
using Crux.CRL.PoolSystem;
using UnityEngine;

namespace Crux.CRL.CardSystem
{
    [Serializable]
    public class CardAbility : IAbility
    {
        #region Private Fields

        [NonSerialized]
        private Ability _ability;

        private AbilityName _abilityName;

        #endregion

        #region Properties

        public AbilityName AbilityName => _abilityName;
        public string FormattedName => _ability.FormattedName;
        public string LongDescription => _ability.LongDescription;
        public string ShortDescription(Combatant abilityOwner = null, Combatant target = null) => _ability.ShortDescription(abilityOwner, target);
        //public string TempEffectShortDesc(Combatant abilityOwner) => _ability.TempEffectShortDesc(abilityOwner);
        public Sprite IconSprite => _ability.IconSprite;
        public Material CardFxMaterial => _ability.CardFxMaterial;
        public int ManaCost => _ability.ManaCost;
        public AbilityType Type => _ability.Type;
        public TargetType TargetType => _ability.TargetType;
        public BuffType BuffType => _ability.BuffType;
        public ushort MaxTargets => _ability.MaxTargets;
        public bool RandomTarget => _ability.RandomTarget;
        public bool RandomSecondaryTarget => _ability.RandomSecondaryTarget;
        public bool SelfTarget => _ability.SelfTarget;
        public bool IsAoE => _ability.IsAoE;
        public float AbilityRadius => _ability.AbilityRadius;
        public ushort NumOfHits => _ability.NumOfHits;
        public bool IsMultiTarget => _ability.IsMultiTarget;
        public float DelayBetweenTargets => _ability.DelayBetweenTargets;
        public bool IsMultiHit => _ability.IsMultiHit;
        public float DelayBetweenHits => _ability.DelayBetweenHits;
        public ushort DDValue => _ability.DDValue;

        public short BuffFlatDamageDealtValue => _ability.BuffFlatDamageDealtValue;
        public float BuffPercentDamageDealtValue => _ability.BuffPercentDamageDealtValue;
        public short BuffFlatDamageTakenValue => _ability.BuffFlatDamageTakenValue;
        public float BuffPercentDamageTakenValue => _ability.BuffPercentDamageTakenValue;
        public ushort BuffDamageTakenOnCastValue => _ability.BuffDamageTakenOnCastValue;
        public ushort BuffHealTakenOnCastValue => _ability.BuffHealTakenOnCastValue;
        public ushort BuffExtraCast => _ability.BuffExtraCast;
        public ushort BuffExplodeValue => _ability.BuffExplodeValue;
        public ushort BuffTakeDamageOnTurnValue => _ability.BuffTakeDamageOnTurnValue;
        public float BuffPercentMaxHp => _ability.BuffPercentMaxHp;

        public bool SplashDamage => _ability.SplashDamage;
        public ushort DoTValue => _ability.DoTValue;
        public ushort Duration => _ability.Duration;
        public bool AoEDoT => _ability.AoEDoT;
        public ushort AoEDotDamage => _ability.AoEDotDamage;
        public bool ScalingDoT => _ability.ScalingDoT;
        public bool ScalingValueIsMultiplier => _ability.ScalingValueIsMultiplier;
        public float ScalingValue => _ability.ScalingValue;
        public bool RecourseDoT => _ability.RecourseDoT;
        public ushort HealValue => _ability.HealValue;
        public ushort HoTValue => _ability.HoTValue;
        public bool ScalingHOT => _ability.ScalingHOT;
        public float HoTRemoveAndHealMult => _ability.HoTRemoveAndHealMult;
        public ushort CleanseValue => _ability.CleanseValue;
        public ushort DispelValue => _ability.DispelValue;
        public ushort AbsorbValue => _ability.AbsorbValue;
        public bool HasDDFlag => _ability.HasDDFlag;
        public bool HasDDAndSplashValid => _ability.HasDDAndSplashValid;
        public bool HasDDAndAoEValid => _ability.HasDDAndAoEValid;
        public bool HasDDAndSplash => _ability.HasDDAndSplash;
        public bool HasDDAndAoE => _ability.HasDDAndAoE;
        public bool IsEnemyTarget => _ability.IsEnemyTarget;
        public bool HasDoTFlag => _ability.HasDoTFlag;
        public bool HasDoTAndTargetEnemy => _ability.HasDoTAndTargetEnemy;
        public bool HasDoTAndAoE => _ability.HasDoTAndAoE;
        public bool HasScaling => _ability.HasScaling;
        public bool HasScalingAndPercent => _ability.HasScalingAndPercent;
        public bool HasHealFlag => _ability.HasHealFlag;
        public bool HasHotFlag => _ability.HasHotFlag;
        public bool HasBuffFlag => _ability.HasBuffFlag;
        public bool HasDebuffFlag => _ability.HasDebuffFlag;
        public bool HasHoTAndTargetEnemy => _ability.HasHoTAndTargetEnemy;
        public bool HasHoTAndAoE => _ability.HasHoTAndAoE;
        public bool HasHoTRemoveAndHeal => _ability.HasHoTRemoveAndHeal;
        public bool HasCleanseFlag => _ability.HasCleanseFlag;
        public bool HasDispelFlag => _ability.HasDispelFlag;
        public bool HasImmunityFlag => _ability.HasImmunityFlag;
        public bool HasAbsorbFlag => _ability.HasAbsorbFlag;
        public bool IsTempEffect => _ability.IsTempEffect;
        public bool HasVFX => _ability.HasVFX;
        public bool HasEffectModFlag => _ability.HasEffectModFlag;
        public bool HasSpreadDots => _ability.HasSpreadDots;
        public bool HasExtendDots => _ability.HasExtendDots;
        public bool HasDuration => _ability.HasDuration;
        public bool IsStackingEffect => _ability.IsStackingEffect;
        public TriggerType TriggerType => _ability.TriggerType;
        public float TriggerChance => _ability.TriggerChance;
        public AbilityName TriggerAbility => _ability.TriggerAbility;
        public bool HasTriggerChance => _ability.HasTriggerChance;
        public bool TriggerPassesValue => _ability.TriggerPassesValue;
        public float TriggerValuePercent => _ability.TriggerValuePercent;
        public bool IsPassive => _ability.IsPassive;

        public ReelModType ReelModType => _ability.ReelModType;
        public bool HasReelExtraWaves => _ability.HasReelExtraWaves;
        public int NumReelExtraWaves => _ability.NumReelExtraWaves;
        public bool HasNoReelBonus => _ability.HasNoReelBonus;
        public bool HasReelEnemyHealthPercent => _ability.HasReelEnemyHealthPercent;
        public float ReelEnemyHealthPercent => _ability.ReelEnemyHealthPercent;
        public bool HasReelEnemyDamagePercent => _ability.HasReelEnemyDamagePercent;
        public float ReelEnemyDamagePercent => _ability.ReelEnemyDamagePercent;
        public bool HasReelEnemyExtraDD => _ability.HasReelEnemyExtraDD;
        public int ReelNumExtraDD => _ability.ReelNumExtraDD;
        public bool HasReelPlayerHealthPercent => _ability.HasReelPlayerHealthPercent;
        public float ReelPlayerHealthPercent => _ability.ReelPlayerHealthPercent;
        public bool HasReelPlayerDamagePercent => _ability.HasReelPlayerDamagePercent;
        public float ReelPlayerDamagePercent => _ability.ReelPlayerDamagePercent;
        public bool HasReelPlayerFlatMana => _ability.HasReelPlayerFlatMana;
        public int ReelPlayerFlatMana => _ability.ReelPlayerFlatMana;
        public bool HasReelSpawnElite => _ability.HasReelSpawnElite;
        public int ReelNumSpawnedElites => _ability.ReelNumSpawnedElites;
        public bool HasReelSpawnBoss => _ability.HasReelSpawnBoss;
        public bool HasReelSpawnGoldMonster => _ability.HasReelSpawnGoldMonster;
        public int ReelNumSpawnedGoldMonsters => _ability.ReelNumSpawnedGoldMonsters;
        public bool HasReelGoldModifier => _ability.HasReelGoldModifier;
        public float ReelGoldModifier => _ability.ReelGoldModifier;
        public bool HasReelShardModifier => _ability.HasReelShardModifier;
        public float ReelShardModifier => _ability.ReelShardModifier;
        public bool HasReelUnique => _ability.HasReelUnique;
        public int ReelModMinFloor => _ability.ReelModMinFloor;
        public int ReelModWeight => _ability.ReelModWeight;
        public bool IsReel => _ability.IsReel;

        public string SfxPath => _ability.SfxPath;
        public PooledVFX Vfx => _ability.Vfx;
        public bool VfxScaleToEnemy => _ability.VfxScaleToEnemy;
        public float VfxPlaybackSpeed => _ability.VfxPlaybackSpeed;
        public float VfxLifetime => _ability.VfxLifetime;
        public float VfxDamageDelay => _ability.VfxDamageDelay;
        public bool VfxInitTargetOnly => _ability.VfxInitTargetOnly;

        #endregion

        #region Constructors

        private CardAbility() {}

        public CardAbility(Ability ability)
        {
            _ability = ability;
            _abilityName = ability.AbilityName;
        }

        public CardAbility(AbilityName abilityName)
        {
            _ability = DataManager.Instance.GetAbilityData(abilityName);
            _abilityName = abilityName;
        }

        #endregion

    }
}