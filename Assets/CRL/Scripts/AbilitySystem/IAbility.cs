using Crux.CRL.CombatSystem;
using Crux.CRL.PoolSystem;
using UnityEngine;

namespace Crux.CRL.AbilitySystem
{
    public interface IAbility
    {
        AbilityName AbilityName { get; }
        string FormattedName { get; }
        string LongDescription { get; }
        string ShortDescription(Combatant abilityOwner = null, Combatant target = null);
        //string TempEffectShortDesc(Combatant abilityOwner);
        Sprite IconSprite { get; }
        Material CardFxMaterial { get; }
        int ManaCost { get; }
        AbilityType Type { get; }
        TargetType TargetType { get; }
        BuffType BuffType { get; }
        ushort MaxTargets { get; }
        bool RandomTarget { get; }
        bool RandomSecondaryTarget { get; }
        bool SelfTarget { get; }
        bool IsAoE { get; }
        float AbilityRadius { get; }
        ushort NumOfHits { get; }
        bool IsMultiTarget { get; }
        float DelayBetweenTargets { get; }
        bool IsMultiHit { get; }
        float DelayBetweenHits { get; }
        ushort DDValue { get; }
        short BuffFlatDamageDealtValue { get; }
        float BuffPercentDamageDealtValue { get; }
        short BuffFlatDamageTakenValue { get; }
        float BuffPercentDamageTakenValue { get; }
        ushort BuffDamageTakenOnCastValue { get; }
        ushort BuffHealTakenOnCastValue { get; }
        ushort BuffExtraCast { get; }
        ushort BuffExplodeValue { get; }
        ushort BuffTakeDamageOnTurnValue { get; }
        float BuffPercentMaxHp { get; }

        bool SplashDamage { get; }
        ushort DoTValue { get; }
        ushort Duration { get; }
        bool AoEDoT { get; }
        ushort AoEDotDamage { get; }
        bool ScalingDoT { get; }
        bool RecourseDoT { get; }
        bool ScalingValueIsMultiplier { get; }
        float ScalingValue { get; }
        ushort HealValue { get; }
        ushort HoTValue { get; }
        bool ScalingHOT { get; }
        float HoTRemoveAndHealMult { get; }
        ushort CleanseValue { get; }
        ushort DispelValue { get; }
        ushort AbsorbValue { get; }
        bool HasDDFlag { get; }
        bool HasDDAndSplashValid { get; }
        bool HasDDAndAoEValid { get; }
        bool HasDDAndSplash { get; }
        bool HasDDAndAoE { get; }
        bool IsEnemyTarget { get; }
        bool HasDoTFlag { get; }
        bool HasDoTAndTargetEnemy { get; }
        bool HasDoTAndAoE { get; }
        bool HasScaling { get; }
        bool HasScalingAndPercent { get; }
        bool HasHealFlag { get; }
        bool HasHotFlag { get; }
        bool HasBuffFlag { get; }
        bool HasDebuffFlag { get; }
        bool HasHoTAndTargetEnemy { get; }
        bool HasHoTAndAoE { get; }
        bool HasHoTRemoveAndHeal { get; }
        bool HasCleanseFlag { get; }
        bool HasDispelFlag { get; }
        bool HasImmunityFlag { get; }
        bool HasAbsorbFlag { get; }
        bool IsTempEffect { get; }
        bool HasVFX { get; }
        bool HasEffectModFlag { get; }
        bool HasSpreadDots { get; }
        bool HasExtendDots { get; }
        bool HasDuration { get; }
        bool HasTriggerChance { get; }
        bool IsStackingEffect { get; }
        TriggerType TriggerType { get; }
        float TriggerChance { get; }
        AbilityName TriggerAbility { get; }
        bool TriggerPassesValue { get; }
        float TriggerValuePercent { get; }
        bool IsPassive { get; }


        //Reel Mods
        ReelModType ReelModType { get; }
        bool HasReelExtraWaves { get; }
        int NumReelExtraWaves { get; }
        bool HasNoReelBonus { get; }
        bool HasReelEnemyHealthPercent { get; }
        float ReelEnemyHealthPercent { get; }
        bool HasReelEnemyDamagePercent { get; }
        float ReelEnemyDamagePercent { get; }
        bool HasReelEnemyExtraDD { get; }
        int ReelNumExtraDD { get; }
        bool HasReelPlayerHealthPercent { get; }
        float ReelPlayerHealthPercent { get; }
        bool HasReelPlayerDamagePercent { get; }
        float ReelPlayerDamagePercent { get; }
        bool HasReelPlayerFlatMana { get; }
        int ReelPlayerFlatMana { get; }
        bool HasReelSpawnElite { get; }
        int ReelNumSpawnedElites { get; }
        bool HasReelSpawnBoss { get; }
        bool HasReelSpawnGoldMonster { get; }
        int ReelNumSpawnedGoldMonsters { get; }
        bool HasReelGoldModifier { get; }
        float ReelGoldModifier { get; }
        bool HasReelShardModifier { get; }
        float ReelShardModifier { get; }
        bool HasReelUnique { get; }
        int ReelModMinFloor { get; }
        int ReelModWeight { get; }
        bool IsReel { get; }

        string SfxPath { get; }
        PooledVFX Vfx { get; }
        bool VfxScaleToEnemy { get; }
        float VfxPlaybackSpeed { get; }
        float VfxLifetime { get; }
        float VfxDamageDelay { get; }
        bool VfxInitTargetOnly { get; }
    }
}