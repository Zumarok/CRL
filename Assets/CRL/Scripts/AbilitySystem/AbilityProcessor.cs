using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Crux.CRL.CombatSystem;
using Crux.CRL.DataSystem;
using Crux.CRL.DialogSystem;
using Crux.CRL.EnemySystem;
using Crux.CRL.FloatingTextSystem;
using Crux.CRL.PoolSystem;
using Crux.CRL.Utils;
using Deform;
using Sirenix.Utilities;
using UnityEngine;
using static Crux.CRL.Utils.ThreadSafeRandom;

namespace Crux.CRL.AbilitySystem
{
    public static class AbilityProcessor
    {
        private static Collider[] _colliderArray;

        public static Collider[] ColliderArray => _colliderArray ?? (_colliderArray = new Collider[Constants.MAX_NUM_ENEMIES]);
        public static void ClearColliderArray() => Array.Clear(ColliderArray, 0, ColliderArray.Length);

        #region Process Methods

        public static IEnumerator Process(IAbility data, Combatant sender, IReadOnlyList<Combatant> targets, int overrideValue = 0)
        {
            for (int castCount = 0; castCount <= sender.CurrentExtraCasts; castCount++)
            {
                if (targets.Count == 0) yield break;
                if (!sender.IsAlive) yield break;

                Combatant lastTarget = null;

                for (int t = 0; t < (data.IsMultiTarget ? data.MaxTargets : 1); t++)
                {
                    if (!targets.Any(c => c.IsAlive)) // exit if all targets are dead
                        break;

                    Combatant target = null;
                    if (data.RandomTarget || (data.RandomSecondaryTarget && t != 0))
                    {
                        // try to get a valid target up to 20 times, fail-safe instead of a while loop
                        for (int i = 0; i < 20; i++)
                        {
                            target = targets.GetRandom();

                            if (target != null && target.IsAlive)
                                break;

                            target = null;
                        }
                    }
                    else
                    {
                        target = targets[t];
                    }

                    if (target == null)
                        yield break;

                    lastTarget = target;

                    //play vfx/sfx
                    if (!data.SfxPath.IsNullOrWhitespace())
                        FMODUnity.RuntimeManager.PlayOneShot(data.SfxPath, target.Transform.position);

                    if (data.HasVFX)
                    {
                        if (!data.VfxInitTargetOnly || t == 0)
                        {
                            var vfx = PoolManager.Spawn(data.Vfx);
                            vfx.Init(data.VfxScaleToEnemy ? target.transform.localScale : Vector3.one,
                                target.Transform.position, data.VfxPlaybackSpeed,
                                data.VfxLifetime);
                            if (data.VfxDamageDelay > 0)
                                yield return WaitFor.Seconds(data.VfxDamageDelay);
                        }
                    }

                    if (data.HasDDFlag)
                        yield return ProcessDD(data, sender, target, overrideValue);

                    if (data.HasDoTFlag)
                        yield return ProcessDoT(data, sender, target, overrideValue);

                    if (data.HasEffectModFlag)
                        yield return ProcessEffectMod(data, sender, target, t == 0);

                    if (data.HasCleanseFlag)
                        yield return ProcessCleanse(data, sender, target, overrideValue);

                    if (data.HasDispelFlag)
                        yield return ProcessDispel(data, sender, target, overrideValue);

                    if (data.HasHealFlag)
                        yield return ProcessHeal(data, sender, target, overrideValue);

                    if (data.HasAbsorbFlag)
                        yield return ProcessAbsorb(data, sender, target, overrideValue);

                    if (data.HasHotFlag)
                        yield return ProcessHoT(data, sender, target, overrideValue);

                    if (data.HasBuffFlag)
                        yield return ProcessBuff(data, sender, target);

                    if (data.HasDebuffFlag)
                        yield return ProcessDebuff(data, sender, target);

                    if (data.HasTriggerChance && Ran.NextDouble() < data.TriggerChance)
                        yield return ProcessTrigger(data, sender, target);

                    if (data.IsMultiTarget)
                        yield return WaitFor.Seconds(data.DelayBetweenTargets);
                }

                if (sender.CurrentHealTakenOnCast > 0)
                    sender.ApplyHeal(new HealInfo(sender.CurrentHealTakenOnCast, null, null,
                        "Healed On Cast")); 

                if (sender.CurrentDamageTakenOnCast > 0)
                    sender.ApplyDamage(new DamageInfo(sender.CurrentDamageTakenOnCast, null, null,
                        "Damaged On Cast")); 
            }
        }

        private static IEnumerator ProcessDD(IAbility data, Combatant sender, Combatant target, int overrideValue = 0)
        {
            var splashCount = data.SplashDamage
                ? Physics.OverlapSphereNonAlloc(target.Transform.position, data.AbilityRadius, ColliderArray, Layers.EnemyMask)
                : 0;
            var hits = (data.IsMultiHit ? data.NumOfHits : 1) + sender.CurrentExtraDD;
            
            for (int i = 0; i < hits; i++)
            {
                var dmg = new DamageInfo(overrideValue != 0 ? overrideValue : data.DDValue, sender, target, data.FormattedName);
                var result = target.ApplyDamage(dmg);

                if (sender is Enemy enemy && target is Player)
                {
                    var hitPercent = (float)dmg.FinalValue / target.CurrentMaxHp;
                    var type = hitPercent < 0.1f ? DialogEventType.OnDealDamageSmall :
                        hitPercent < 0.3f ? DialogEventType.OnDealDamageMid : DialogEventType.OnDealDamageBig;
                    EnemyHealthBarManager.Instance.ShowEnemyDialog(enemy, type);
                }

                for (int splashIndex = 0; splashIndex < splashCount; splashIndex++)
                {
                    var splashTar = ColliderArray[splashIndex].transform.GetComponent<Combatant>();
                    if (splashTar == null || splashTar == target) continue;

                    var splashAmt = Convert.ToUInt16(
                        Mathf.Max((1 - Vector3.Distance(target.Transform.position, splashTar.Transform.position) / data.AbilityRadius) * (overrideValue != 0 ? overrideValue : data.DDValue), 0));
                    if (splashAmt > 0)
                        splashTar.ApplyDamage(new DamageInfo(splashAmt, sender, target, data.FormattedName));
                }

                if (data.Type.HasFlag(AbilityType.CanTrigger) && data.TriggerType.HasFlag(TriggerType.OverKill))
                {
                    yield return ProcessTrigger(data, sender, target, result.overKillAmount );
                }

                if (!target.IsAlive)
                    yield break;

                yield return WaitFor.Seconds(data.DelayBetweenHits);
            }
        }

        private static IEnumerator ProcessDoT(IAbility data, Combatant sender, Combatant target, int overrideValue = 0)
        {
            yield return null; //WaitFor.Seconds(0.5f);

            target.ApplyDebuff(new DoT(data, sender, target, 0, (ushort)overrideValue));
        }

        private static IEnumerator ProcessEffectMod(IAbility data, Combatant sender, Combatant target, bool isInitialTarget)
        {
            if (data.HasSpreadDots && isInitialTarget)
            {
                var infectedTargets = Physics.OverlapSphereNonAlloc(target.Transform.position, data.AbilityRadius, ColliderArray, Layers.EnemyMask);

                for (int i = 0; i < infectedTargets; i++)
                {
                    var dotsToSpread = target.ActiveDoTs;
                    var infectTarget = ColliderArray[i].transform.GetComponent<Combatant>();
                    if (infectTarget == null || infectTarget == target) continue;

                    foreach (var dot in dotsToSpread)
                    {
                        if (!infectTarget.ApplyDebuff(new DoT(dot.Ability, sender, infectTarget, data.Duration, dot.DOTDamage)))
                            FloatingTextManager.Instance.ShowFloatingTextMainCamera("Maximum number of debuffs reached!", target.transform, Color.red, TextSize.Medium);
                    }

                    yield return null;
                }
            }
        }

        private static IEnumerator ProcessCleanse(IAbility data, Combatant sender, Combatant target, int overrideValue = 0)
        {
            yield return null; //WaitFor.Seconds(0.5f);
            if (sender is Enemy enemy && target is Player)
            {
                EnemyHealthBarManager.Instance.ShowEnemyDialog(enemy, DialogEventType.OnDealCleanse);
            }
            target.ApplyCleanse(overrideValue > 0 ? overrideValue : data.CleanseValue, sender.Name);
        }

        private static IEnumerator ProcessDispel(IAbility data, Combatant sender, Combatant target, int overrideValue = 0)
        {
            yield return null; //WaitFor.Seconds(0.5f);
            if (sender is Enemy enemy && target is Player)
            {
                EnemyHealthBarManager.Instance.ShowEnemyDialog(enemy, DialogEventType.OnDealDispel);
            }

            target.ApplyDispel(overrideValue > 0 ? overrideValue : data.DispelValue, sender.Name);
        }

        private static IEnumerator ProcessHeal(IAbility data, Combatant sender, Combatant target, int overrideValue = 0)
        {
            yield return null; //WaitFor.Seconds(0.5f);
            var result = target.ApplyHeal(new HealInfo(overrideValue > 0 ? overrideValue : data.HealValue, sender, target, data.FormattedName));

            if (data.Type.HasFlag(AbilityType.CanTrigger) && data.TriggerType.HasFlag(TriggerType.OverHeal))
            {
                yield return ProcessTrigger(data, sender, target, result.overHealAmount);
            }
        }

        private static IEnumerator ProcessAbsorb(IAbility data, Combatant sender, Combatant target, int overrideValue = 0)
        {
            yield return null; //WaitFor.Seconds(0.5f);

            if (sender is Enemy enemy && target is Enemy)
            {
                EnemyHealthBarManager.Instance.ShowEnemyDialog(enemy, DialogEventType.OnGrantBuff);
            }
            var result = target.ApplyAbsorb(overrideValue > 0 ? overrideValue : data.AbsorbValue, sender.Name);
            if (data.Type.HasFlag(AbilityType.CanTrigger) && data.TriggerType.HasFlag(TriggerType.OverShield))
            {
                yield return ProcessTrigger(data, sender, target, result.overShieldAmount);
            }
        }

        private static IEnumerator ProcessHoT(IAbility data, Combatant sender, Combatant target, int overrideValue = 0)
        {
            yield return null; //WaitFor.Seconds(0.5f);
            if (data.HasHoTRemoveAndHeal)
            {
                var removed = target.RemoveAllTempEffectByName(data.AbilityName);
                var healVal = Convert.ToUInt16(removed.avgVal * removed.durationSum * data.HoTRemoveAndHealMult);
                if (healVal > 0)
                    target.ApplyHeal(new HealInfo(healVal, sender, target, data.FormattedName));
                yield return WaitFor.Seconds(0.5f);
            }

            if (sender is Enemy enemy && target is Enemy)
            {
                EnemyHealthBarManager.Instance.ShowEnemyDialog(enemy, DialogEventType.OnGrantBuff);
            }

            target.ApplyBuff(new HoT(data, sender, target, 0, (ushort)overrideValue));
        }

        private static IEnumerator ProcessBuff(IAbility data, Combatant sender, Combatant target)
        {
            yield return null; //WaitFor.Seconds(0.5f);
            if (sender is Enemy enemy && target is Enemy)
            {
                EnemyHealthBarManager.Instance.ShowEnemyDialog(enemy, DialogEventType.OnGrantBuff);
            }

            target.ApplyBuff(new Buff(data, sender, target));
        }

        private static IEnumerator ProcessDebuff(IAbility data, Combatant sender, Combatant target)
        {
            yield return null; //WaitFor.Seconds(0.5f);
            if (sender is Enemy enemy && target is Player)
            {
                EnemyHealthBarManager.Instance.ShowEnemyDialog(enemy, DialogEventType.OnDealDebuff);
            }
            target.ApplyDebuff(new Debuff(data, sender, target));
        }

        public static IEnumerator ProcessTrigger(IAbility parentAbility, Combatant sender, Combatant parentTarget, int value = 0)
        {
            var ability = DataManager.Instance.GetAbilityData(parentAbility.TriggerAbility);
            value = ability.TriggerPassesValue ? Convert.ToInt32(value * ability.TriggerValuePercent) : value;

            var target = parentTarget;
            if (ability.SelfTarget) target = sender;
            else if (ability.TargetType == TargetType.Player)  target = CombatManager.Instance.Player;
            else if (ability.RandomTarget) target = CombatManager.Instance.GetRandomEnemy;
            
            yield return Process(ability, sender, new List<Combatant> { target }, value);
        }
        #endregion

    }
}