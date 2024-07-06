using System;
using System.Collections;
using Crux.CRL.CombatSystem;
using Crux.CRL.DataSystem;
using Crux.CRL.Utils;
using UnityEngine;

namespace Crux.CRL.AbilitySystem
{
    #region TempEffect Base Class

    public class TempEffect
    {
        public int Duration { get; protected set; }
        public int StackCount { get; protected set; }
        public Sprite Icon => Ability.IconSprite;
        public string Name => Ability.FormattedName;
        public IAbility Ability { get; }
        public Combatant Sender { get; }
        public Combatant Receiver { get; }

        public TempEffect(IAbility ability, Combatant sender, Combatant receiver)
        {
            Ability = ability;
            Sender = sender;
            Receiver = receiver;
            Duration = ability.HasDuration ? ability.Duration : 1;
            StackCount = 1;
        }

        public virtual IEnumerator ProcessTurn(Combatant combatant)
        {
            if (Ability.HasDuration && Duration > 0)
            {
                Duration--;

                if (Duration == 0 && Ability.BuffType.HasFlag(BuffType.ExplodeOnExpire))
                {
                    CombatManager.Instance.Player.ApplyDamage(new DamageInfo(Ability.BuffExplodeValue, null, null, Ability.FormattedName));
                    combatant.ApplyDamage(new DamageInfo(Constants.MAX_ENEMY_HEALTH, null, null, "Explode"));
                }
            }

            yield return null;
        }

        public void SetDuration(ushort newDuration)
        {
            Duration = newDuration;
        }

        public void AddStacks(int count)
        {
            StackCount += count;
        }

        public void RemoveStacks(int count)
        {
            StackCount = Math.Max(StackCount - count, 0);
        }
    }

    #endregion

    #region Debuffs

    public class Debuff : TempEffect
    {
        public Debuff(IAbility ability, Combatant sender, Combatant receiver) : base(ability, sender, receiver)
        {
        }
    }

    public class DoT : Debuff
    {
        public ushort DOTDamage { get; private set; }

        public DoT(IAbility ability, Combatant sender, Combatant receiver, ushort overrideDuration = 0, ushort overrideDamage = 0) : base(ability, sender, receiver)
        {
            Duration = overrideDuration == 0 ? ability.Duration : overrideDuration; 
            DOTDamage = overrideDamage == 0 ? Ability.DoTValue : overrideDamage;
        }

        public override IEnumerator ProcessTurn(Combatant combatant)
        {
            yield return base.ProcessTurn(combatant);

            var aoeCount = Ability.AoEDoT ? Physics.OverlapSphereNonAlloc(combatant.Transform.position, Ability.AbilityRadius, AbilityProcessor.ColliderArray, Layers.EnemyMask) : 0;
            
            for (int numOfHits = 0; numOfHits < Ability.NumOfHits; numOfHits++)
            {
                combatant.ApplyDamage(new DamageInfo(DOTDamage, Sender, combatant, Ability.FormattedName));

                if (Ability.RecourseDoT)
                    Sender.ApplyHeal(new HealInfo(DamageInfo.CalculateFinalDamage(DOTDamage, Sender, Receiver), Receiver, Sender, Ability.FormattedName));

                for (int aoeIndex = 0; aoeIndex < aoeCount; aoeIndex++)
                {
                    var aoeTarget = AbilityProcessor.ColliderArray[aoeIndex].transform.GetComponent<Combatant>();
                    if (aoeTarget == null || aoeTarget == combatant) continue;

                    aoeTarget.ApplyDamage(new DamageInfo(DOTDamage, Sender, aoeTarget, Ability.FormattedName));
                }

                if (Ability.ScalingDoT)
                {
                    if (Ability.ScalingValueIsMultiplier)
                    {
                        if (Ability.ScalingValue > 0)
                            DOTDamage = Convert.ToUInt16(DOTDamage * Ability.ScalingValue);
                        else
                            DOTDamage = Convert.ToUInt16(DOTDamage / Mathf.Abs(Ability.ScalingValue));
                    }
                    else
                    {
                        DOTDamage = Convert.ToUInt16(DOTDamage + Ability.ScalingValue);
                    }
                }

                yield return WaitFor.Seconds(0.5f);
            }
        }
    }

    #endregion

    #region Buffs

    public class Buff : TempEffect
    {
        public Buff(IAbility ability, Combatant sender, Combatant receiver) : base(ability, sender, receiver)
        {
        }
    }

    public class HoT : Buff
    {
        public ushort HOTValue { get; private set; }

        public HoT(IAbility ability, Combatant sender, Combatant receiver, ushort overrideDuration = 0, ushort overrideHeal = 0) : base(ability, sender, receiver)
        {
            Duration = overrideDuration == 0 ? ability.Duration : overrideDuration;
            HOTValue = overrideHeal == 0 ? Ability.HoTValue : overrideHeal;
        }

        public override IEnumerator ProcessTurn(Combatant combatant)
        {
            yield return base.ProcessTurn(combatant);

            var aoeCount = Ability.HasHoTAndAoE ? Physics.OverlapSphereNonAlloc(combatant.Transform.position, Ability.AbilityRadius, AbilityProcessor.ColliderArray, Layers.EnemyMask) : 0;

            for (int numOfHits = 0; numOfHits < Ability.NumOfHits; numOfHits++)
            {
                combatant.ApplyHeal(new HealInfo(HOTValue, Sender, combatant, Ability.FormattedName));

                for (int aoeIndex = 0; aoeIndex < aoeCount; aoeIndex++)
                {
                    var aoeTarget = AbilityProcessor.ColliderArray[aoeIndex].transform.GetComponent<Combatant>();
                    if (aoeTarget == null || aoeTarget == combatant) continue;

                    aoeTarget.ApplyHeal(new HealInfo(HOTValue, Sender, aoeTarget, Ability.FormattedName));
                }

                if (Ability.ScalingHOT)
                {
                    if (Ability.ScalingValueIsMultiplier)
                    {
                        if (Ability.ScalingValue > 0)
                            HOTValue = Convert.ToUInt16(HOTValue * Ability.ScalingValue);
                        else
                            HOTValue = Convert.ToUInt16(HOTValue / Mathf.Abs(Ability.ScalingValue));
                    }
                    else
                    {
                        HOTValue = Convert.ToUInt16(HOTValue + Ability.ScalingValue);
                    }
                }

                yield return WaitFor.Seconds(0.5f);
            }
        }
    }
    

    #endregion


}

