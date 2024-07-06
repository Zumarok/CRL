using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Crux.CRL.AbilitySystem;
using Crux.CRL.DataSystem;
using Crux.CRL.FloatingTextSystem;
using Crux.CRL.FloorNavigationSystem;
using UnityEngine;
using static Crux.CRL.DataSystem.Constants;

namespace Crux.CRL.CombatSystem
{
    public abstract class Combatant : MonoBehaviour
    {
        #region Fields

        [SerializeField] private int _maxHp;
        [SerializeField] protected AbilityName[] abilities;
        
        protected readonly List<Combatant> abilityTargets = new List<Combatant>(15);
        
        protected List<TempEffect> tempEffects = new List<TempEffect>(16);

        protected List<TempEffect> effectsToRemove = new List<TempEffect>(16);

        private int _currentAbsorb;
        private int _currentHp;
        private bool _isPlayer;

        #endregion

        #region Properties

        public IReadOnlyList<AbilityName> Abilities => abilities;

        public int BaseMaxHp => _maxHp;

        public int CurrentMaxHp => BaseMaxHp + CurrentFlatMaxHpMod + Convert.ToInt32(BaseMaxHp * CurrentPercentMaxHpMod);

        public int CurrentHp
        {
            get => _currentHp;
            protected set
            {
                _currentHp = Mathf.Clamp(value, 0, CurrentMaxHp);
                CurrentHpPercent = (float) _currentHp / CurrentMaxHp;
            }
        }

        public float CurrentHpPercent { get; private set; }

        public int MaxAbsorb => _maxHp;
        public int CurrentAbsorb
        {
            get => _currentAbsorb;
            set
            {
                _currentAbsorb = value;
                CurrentAbsorbPercent = (float) _currentAbsorb / MaxAbsorb;
            }
        }
        public float CurrentAbsorbPercent { get; private set; }
        public int CurrentFlatDamageDealt { get; private set; }
        public float CurrentPercentDamageDealt { get; protected set; }
        public int CurrentFlatDamageTaken { get; private set; }
        public float CurrentPercentDamageTaken { get; protected set; }
        public int CurrentDamageTakenOnCast { get; private set; }
        public int CurrentDamageTakenOnTurn { get; private set; }
        public int CurrentHealTakenOnCast { get; private set; }
        public int CurrentExtraCasts { get; private set; }
        public int CurrentExtraDD { get; protected set; }
        public float CurrentPercentMaxHpMod { get; protected set; }
        public int CurrentFlatMaxHpMod { get; private set; }
        public string Name { get; protected set; }
        public int CombatantId { get; private set; }
        public Transform Transform { get; private set; }
        public IEnumerable<DoT> ActiveDoTs => tempEffects.OfType<DoT>();
        public IEnumerable<Debuff> ActiveDebuffs => tempEffects.OfType<Debuff>();
        public IEnumerable<Buff> ActiveBuffs => tempEffects.OfType<Buff>();

        /// <summary>
        /// All temporary effects that should be processed before other turn actions; ex: DoTs.
        /// </summary>
        public IEnumerable<TempEffect> PreProcessEffects => tempEffects.Where(t => t is DoT || t is HoT);
        /// <summary>
        /// All temporary effects that should be processed after turn actions; ex: stat buffs and debuffs.
        /// </summary>
        public IEnumerable<TempEffect> PostProcessEffects
        {
            get
            {
                var preEffects = new HashSet<TempEffect>(PreProcessEffects);
                return tempEffects.Where(t => !preEffects.Contains(t));
            }
        }
        public int NumActiveDebuffs { get; private set; }
        public int NumActiveBuffs { get; private set; }
        public bool IsAlive => _currentHp > 0;

        public CombatantFlags CombatantFlags { get; protected set; }

        #endregion

        #region Virtual Unity Callbacks

        protected virtual void Start()
        {
            ApplyReelMods(DataManager.Instance.TowerFloorInfo);
            CombatantId = CombatManager.Instance.AddCombatant(this);
            CurrentHp = CurrentMaxHp;
            Name = name; //todo temp
            Transform = transform;
            _isPlayer = GetType() == typeof(Player);
        }

        #endregion

        #region Virtual/Abstract Methods

        public virtual IEnumerator ProcessTurn()
        {
            if (CurrentDamageTakenOnTurn > 0)
                ApplyDamage(new DamageInfo(CurrentDamageTakenOnTurn, null, null, "DOT"));

            yield return null;
        }

        public virtual void SetAbilityTargets(Ability ability)
        {
            abilityTargets.Clear();
        }
        
        /// <summary>
        /// All damage is applied here. Damage value is calculated in DamageInfo. 
        /// </summary>
        public virtual (int damagedAmount, int overKillAmount) ApplyDamage(DamageInfo info)
        {

            Debug.Log($"<color=yellow>{Name}</color> takes <color=red>{info.FinalValue} damage</color> from <color=orange>{(!string.IsNullOrWhiteSpace(info.SenderName) ? info.SenderName + "'s" : "")}</color> <color=teal>{info.AbilityName}</color>!");
            PlayerUIManager.Instance.CombatLog($"<color={(_isPlayer ? CLC_PLAYER : CLC_ENEMY)}>{Name}</color> takes <color={CLC_DAMAGE}>{info.FinalValue} damage</color> from <color={(!_isPlayer ? CLC_PLAYER : CLC_ENEMY)}>{(!string.IsNullOrWhiteSpace(info.SenderName) ? info.SenderName + "'s" : "")}</color> <color={CLC_SKILL}>{info.AbilityName}</color>!");

            var remainingDmg = info.FinalValue;
            var effHp = CurrentAbsorb + CurrentHp;
            var damagedAmount = effHp > info.FinalValue ? info.FinalValue : effHp;
            var overKillAmount = effHp > info.FinalValue ? 0 : info.FinalValue - effHp;

            if (CurrentAbsorb > 0)
            {
                remainingDmg -= CurrentAbsorb;
                CurrentAbsorb = Mathf.Max(CurrentAbsorb - info.FinalValue, 0);
            }

            if (remainingDmg > 0)
            {
                CurrentHp = Mathf.Max(CurrentHp - remainingDmg, 0);
            }

            if (CurrentHp == 0 && !CombatantFlags.HasFlag(CombatantFlags.Dead))
            {
                OnDeath();
            }

            return (damagedAmount, overKillAmount);
        }

        /// <summary>
        /// All heals are applied here.
        /// </summary>
        public virtual (int healedAmount, int overHealAmount) ApplyHeal(HealInfo info)
        {
            var missingHps = CurrentMaxHp - CurrentHp;
            var healAmount = Mathf.Min(info.FinalValue, missingHps);
            var overheal = Mathf.Max(info.FinalValue - healAmount, 0);
            Debug.Log($"<color=cyan>{Name} (({CurrentHp}/{CurrentMaxHp} {(float)CurrentHp / CurrentMaxHp:0%}))</color> is healed for <color=green>{info.FinalValue} hps!</color>");
            PlayerUIManager.Instance.CombatLog($"<color={(_isPlayer ? CLC_PLAYER : CLC_ENEMY)}>{Name} (({CurrentHp}/{CurrentMaxHp} {(float)CurrentHp / CurrentMaxHp:0%}))</color> is healed for <color={CLC_HEAL}>{info.FinalValue} hps!</color>");
            CurrentHp = Mathf.Min(CurrentHp + info.FinalValue, CurrentMaxHp);
            return (healAmount, overheal);
        }

        public virtual bool ApplyDebuff(Debuff debuff)
        {
            if (NumActiveDebuffs >= Constants.MAX_DEBUFFS)
            {
                FloatingTextManager.Instance.ShowFloatingTextMainCamera("Maximum number of debuffs reached!", debuff.Receiver.transform, Color.red, TextSize.Medium);
                PlayerUIManager.Instance.CombatLog("Maximum number of debuffs reached!");
                return false;
            }

            Debug.Log($"<color=cyan>{debuff.Sender.Name}</color> inflicts <color=cyan>{Name}</color> with <color=red>{debuff.Name}</color>");
            PlayerUIManager.Instance.CombatLog($"<color={(_isPlayer ? CLC_PLAYER : CLC_ENEMY)}>{debuff.Sender.Name}</color> inflicts <color={(!_isPlayer ? CLC_PLAYER : CLC_ENEMY)}>{Name}</color> with <color={CLC_DEBUFF}>{debuff.Name}</color>");

            tempEffects.Add(debuff);
            NumActiveDebuffs++;

            ApplyOrRemoveEffectMods(debuff, false);
            return true;
        }

        public virtual void RemoveDebuff(Debuff debuff)
        {
            ApplyOrRemoveEffectMods(debuff, true);
        }

        public virtual Buff ApplyBuff(Buff newBuff)
        {
            if (NumActiveBuffs >= Constants.MAX_BUFFS && !newBuff.Ability.IsStackingEffect)
            {
                FloatingTextManager.Instance.ShowFloatingTextMainCamera("Maximum number of buffs reached!", newBuff.Receiver.transform, Color.red, TextSize.Medium);
                PlayerUIManager.Instance.CombatLog("Maximum number of buffs reached!");
                return null;
            }

            Debug.Log($"<color=cyan>{newBuff.Sender.Name}</color> grants <color=cyan>{Name}</color> with <color=green>{newBuff.Name}</color>.");
            PlayerUIManager.Instance.CombatLog($"<color={(_isPlayer ? CLC_PLAYER : CLC_ENEMY)}>{newBuff.Sender.Name}</color> grants <color={(_isPlayer ? CLC_PLAYER : CLC_ENEMY)}>{Name}</color> with <color={CLC_BUFF}>{newBuff.Name}</color>.");

            // stack the buff if applicable, otherwise add the new buff
            Buff activeBuff = null;
            if (newBuff.Ability.IsStackingEffect)
            {
                activeBuff = ActiveBuffs.FirstOrDefault(b => b.Name == newBuff.Name);
                activeBuff?.AddStacks(1);
            }
            if (activeBuff == null)
            {
                activeBuff = newBuff;
                tempEffects.Add(activeBuff);
                NumActiveBuffs++;
            }

            ApplyOrRemoveEffectMods(newBuff, false);
            return activeBuff;
        }

        public virtual void RemoveBuff(Buff buff)
        {
            ApplyOrRemoveEffectMods(buff, true);
        }

        public virtual void ApplyCleanse(int count, string senderName)
        {
            Debug.Log($"<color=cyan>{senderName}</color> cleanses <color=cyan>{Name}</color> of <color=green>{count}</color> debuffs.");
            PlayerUIManager.Instance.CombatLog($"<color={(_isPlayer ? CLC_PLAYER : CLC_ENEMY)}>{senderName}</color> cleanses <color={(_isPlayer ? CLC_PLAYER : CLC_ENEMY)}>{Name}</color> of <color={CLC_COUNT}>{count}</color> debuffs.");

            for (int i = 0, c = effectsToRemove.Count; i < c; i++)
            {
                NumActiveDebuffs--;
                RemoveDebuff((Debuff)effectsToRemove[i]);
                tempEffects.Remove(effectsToRemove[i]);
            }
            effectsToRemove.Clear();
        }

        public virtual void ApplyDispel(int count, string senderName)
        {
            Debug.Log($"<color=cyan>{senderName}</color> attempts to dispel <color=cyan>{Name}</color> of <color=green>{count}</color> buffs.");
            PlayerUIManager.Instance.CombatLog($"<color={(_isPlayer ? CLC_PLAYER : CLC_ENEMY)}>{senderName}</color> attempts to dispel <color={(!_isPlayer ? CLC_PLAYER : CLC_ENEMY)}>{Name}</color> of <color={CLC_COUNT}>{count}</color> buffs.");

            for (int i = 0, c = effectsToRemove.Count; i < c; i++)
            {
                NumActiveBuffs--;
                RemoveBuff((Buff)effectsToRemove[i]);
                tempEffects.Remove(effectsToRemove[i]);
            }

            effectsToRemove.Clear();
        }

        /// <summary>
        /// Remove all buffs by the ability name.
        /// </summary>
        /// <param name="buffName"></param>
        /// <returns>Returns the sum of the remaining duration and the avg value of the removed buffs.</returns>
        public virtual (ushort durationSum, ushort avgVal) RemoveAllTempEffectByName(AbilityName effectName)
        {
            if (effectsToRemove.Count == 0) return (0, 0);

            var type = effectsToRemove[0].GetType();
            var isBuff = type == typeof(Buff) || type.IsSubclassOf(typeof(Buff));
            var durationSum = Convert.ToUInt16(effectsToRemove.Sum(e => e.Duration));
            var avgVal = Convert.ToUInt16(type == typeof(HoT) ? effectsToRemove.Average(e => ((HoT) e).HOTValue) :
                type == typeof(DoT) ? effectsToRemove.Average(e => ((DoT)e).DOTDamage) : 0);

            for (int i = 0, c = effectsToRemove.Count; i < c; i++)
            {
                if (isBuff)
                {
                    RemoveBuff((Buff)effectsToRemove[i]);
                    NumActiveBuffs--;
                }
                else
                {
                    RemoveDebuff((Debuff)effectsToRemove[i]);
                    NumActiveDebuffs--;
                }

                tempEffects.Remove(effectsToRemove[i]);
            }
            effectsToRemove.Clear();

            return (durationSum, avgVal);
        }

        public virtual (int absorbAppliedAmount, int overShieldAmount) ApplyAbsorb(int amount, string senderName)
        {
            Debug.Log($"<color=cyan>{senderName}</color> applies <color=green>{amount} absorb</color> to <color=cyan>{Name}</color>");
            PlayerUIManager.Instance.CombatLog($"<color={(_isPlayer ? CLC_PLAYER : CLC_ENEMY)}>{senderName}</color> applies <color={CLC_ABSORB}>{amount} absorb</color> to <color={(_isPlayer ? CLC_PLAYER : CLC_ENEMY)}>{Name}</color>");
            var absorbableAmt = MaxAbsorb - CurrentAbsorb;
            var absorbAppliedAmount = Mathf.Min(absorbableAmt, amount);
            var overShieldAmount = Mathf.Max(amount - absorbableAmt, 0);
            CurrentAbsorb = Mathf.Min(MaxAbsorb, CurrentAbsorb + amount);
            return (absorbAppliedAmount, overShieldAmount);
        }
        
        public virtual void OnDeath()
        {
            CombatantFlags |= CombatantFlags.Dead; // add dead flag
            Debug.Log($"<color=yellow>{Name}</color> is <color=red>dead!</color>");
            PlayerUIManager.Instance.CombatLog($"<color={(_isPlayer ? CLC_PLAYER : CLC_ENEMY)}>{Name}</color> is <color=red>dead!</color>");
            CombatManager.Instance.RemoveCombatant(this);
        }

        protected virtual void ApplyReelMods(TowerFloorInfo result)
        {
            // global combatant reel mods

        }

        public abstract void RefreshUI();

        #endregion

        #region Protected Functions
        
        protected IEnumerator ProcessTempEffects(IEnumerable<TempEffect> effects)
        {
            foreach (var effect in effects)
            {
                yield return effect.ProcessTurn(this);
                if (effect.Duration <= 0 || effect.StackCount <= 0)
                    effectsToRemove.Add(effect);
                if (CurrentHp <= 0) break;
            }

            for (int i = 0, c = effectsToRemove.Count; i < c; i++)
            {
                if (effectsToRemove[i] is Buff)
                {
                    RemoveBuff((Buff)effectsToRemove[i]);
                    NumActiveBuffs--;
                }
                else
                {
                    RemoveDebuff((Debuff)effectsToRemove[i]);
                    NumActiveDebuffs--;
                }

                tempEffects.Remove(effectsToRemove[i]);
            }
            effectsToRemove.Clear();
        }

        #endregion

        #region Private Functions

        private void ApplyOrRemoveEffectMods(TempEffect effect, bool remove)
        {
            int m = (remove ? -1 : 1);
            // handle the buff bonuses
            if (effect.Ability.BuffType.HasFlag(BuffType.FlatDamageDealt))
            {
                CurrentFlatDamageDealt += effect.Ability.BuffFlatDamageDealtValue * m;
                RefreshUI();
            }
            if (effect.Ability.BuffType.HasFlag(BuffType.PercentDamageDealt))
            {
                CurrentPercentDamageDealt += effect.Ability.BuffPercentDamageDealtValue * m;
                RefreshUI();
            }
            if (effect.Ability.BuffType.HasFlag(BuffType.FlatDamageTaken))
            {
                CurrentFlatDamageTaken += effect.Ability.BuffFlatDamageTakenValue * m;
            }
            if (effect.Ability.BuffType.HasFlag(BuffType.PercentDamageTaken))
            {
                CurrentPercentDamageTaken += effect.Ability.BuffPercentDamageTakenValue * m;
            }
            if (effect.Ability.BuffType.HasFlag(BuffType.TakeDamageOnCast))
            {
                CurrentDamageTakenOnCast += effect.Ability.BuffDamageTakenOnCastValue * m;
            }
            if (effect.Ability.BuffType.HasFlag(BuffType.TakeHealOnCast))
            {
                CurrentHealTakenOnCast += effect.Ability.BuffHealTakenOnCastValue * m;
            }
            if (effect.Ability.BuffType.HasFlag(BuffType.ExtraCast))
            {
                CurrentExtraCasts += effect.Ability.BuffExtraCast * m;
            }
            if (effect.Ability.BuffType.HasFlag(BuffType.TakeDamageOnTurn))
            {
                CurrentDamageTakenOnTurn += effect.Ability.BuffTakeDamageOnTurnValue * m;
            }
            if (effect.Ability.BuffType.HasFlag(BuffType.PercentMaxHp))
            {
                var c = CurrentHpPercent;
                CurrentPercentMaxHpMod += effect.Ability.BuffPercentMaxHp * m;
                SetCurrentHealthFromPercent(c);
                RefreshUI();
            }
        }
        #endregion

        #region Public Util Methods

        public void SetCurrentHealthFromPercent(float percentOfMaxHealth)
        {
            CurrentHp = Convert.ToInt32(CurrentMaxHp * percentOfMaxHealth);
        }

        #endregion
    }
}

