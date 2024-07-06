
using System;
using System.Globalization;
using Crux.CRL.CombatSystem;

namespace Crux.CRL.AbilitySystem
{
    public interface IDamageOrHealInfo
    {
        int FinalValue { get; }
        string SenderName { get; }
        string ReceiverName { get; }
        string AbilityName { get; }
        Combatant Sender { get; }
        Combatant Receiver { get; }
        
        
    }

    public struct DamageInfo : IDamageOrHealInfo
    {
        public int FinalValue { get; }
        public string SenderName { get; }
        public string ReceiverName { get; }
        public string AbilityName { get; }
        public Combatant Sender { get; }
        public Combatant Receiver { get; }

        public DamageInfo(int value, Combatant sender, Combatant receiver, string abilityName)
        {
            SenderName = sender == null ? "" : sender.Name;
            ReceiverName = receiver == null ? "" : receiver.Name;
            AbilityName = abilityName;
            Sender = sender;
            Receiver = receiver;
            FinalValue = value;
            FinalValue = CalculateFinalDamage(value, sender, receiver);
        }

        /// <summary>
        /// Calculates the final damage number.
        /// This value will include all sender buffs/debuffs, and receiver buffs/debuffs as well as all trinket and global effects.
        /// This is the ONLY place this value should be calculated. All applied damage and tooltips should use this function to get the value.
        /// </summary>
        /// <param name="dmg">The base damage amount.</param>
        /// <param name="sender">The Combatant that used the skill.</param>
        /// <param name="receiver">The Combatant that is the target of the skill damage.</param>
        /// <returns>Returns the final damage value rounded to the nearest int. This value can result in a negative which would heal the target.</returns>
        public static int CalculateFinalDamage(int dmg, Combatant sender = null, Combatant receiver = null)
        {
            // damage bonuses calculated first
            // percent damage calculated before flat damage
            var calcDmg = (float)dmg;
            if (sender != null)
            {
                calcDmg += calcDmg * sender.CurrentPercentDamageDealt;
                calcDmg += sender.CurrentFlatDamageDealt;
            }

            // reduction values
            // flat reductions before percent
            if (receiver != null)
            {
                calcDmg -= receiver.CurrentFlatDamageTaken;
                calcDmg -= calcDmg * receiver.CurrentPercentDamageTaken;
            }

            // round and return the final int value
            return Convert.ToInt32(calcDmg);
        }
    }

    public struct HealInfo : IDamageOrHealInfo
    {
        public int FinalValue { get; }
        public string SenderName { get; }
        public string ReceiverName { get; }
        public string AbilityName { get; }
        public Combatant Sender { get; }
        public Combatant Receiver { get; }

        public HealInfo(int value, Combatant sender, Combatant receiver, string abilityName)
        {
            FinalValue = value;
            SenderName = sender.Name;
            ReceiverName = receiver.name;
            AbilityName = abilityName;
            Sender = sender;
            Receiver = receiver;
        }
    }
}
