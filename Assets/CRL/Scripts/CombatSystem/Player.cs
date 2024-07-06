using System;
using System.Collections;
using System.Linq;
using Crux.CRL.AbilitySystem;
using Crux.CRL.CardSystem;
using Crux.CRL.DataSystem;
using Crux.CRL.DialogSystem;
using Crux.CRL.EnemySystem;
using Crux.CRL.FloatingTextSystem;
using Crux.CRL.FloorNavigationSystem;
using Crux.CRL.SaveSystem;
using Crux.CRL.ShakeSystem;
using Crux.CRL.UI;
using Crux.CRL.Utils;
using UnityEngine;

namespace Crux.CRL.CombatSystem
{
    public class Player : Combatant
    {
        #region Local Constants
        
        //private int _roundStartMana = 3;

        #endregion

        #region Editor Fields

        [SerializeField] private int _maxMana;
        [SerializeField] private CardManager _cardManager;
        #endregion

        #region Fields

        private int _currentMana;
        
        #endregion
        
        #region Properties

        public CardManager CardManager => _cardManager;

        public bool TurnActive { get; private set; }
        public int BaseMaxMana => _maxMana;

        public int CurrentMaxMana => BaseMaxMana + CurrentFlatMaxManaMod + Convert.ToInt32(BaseMaxMana * CurrentPercentMaxManaMod);

        public int CurrentMana
        {
            get => _currentMana;
            protected set
            {
                _currentMana = Mathf.Clamp(value, 0, CurrentMaxMana);
                CurrentManaPercent = (float)_currentMana / CurrentMaxMana;
            }
        }

        public int CurrentFlatMaxManaMod { get; private set; }
        public float CurrentPercentMaxManaMod { get; protected set; }

        public float CurrentManaPercent { get; private set; }

        public PlayerData PlayerData => DataManager.Instance.PlayerData;

        #endregion
        
        #region Unity Callbacks

        protected override void Start()
        {
            base.Start();
            //todo  temp, eventually should load player's customized cards from save file
            abilities.Shuffle();
            foreach (var abilityName in abilities)
            {
                CardManager.AddNewCardToDeck(abilityName);
            }

            CurrentMana = CurrentMaxMana;
            PlayerUIManager.Instance.UpdateUIHealth();
            PlayerUIManager.Instance.SetUIMana(CurrentMana);
            StartCoroutine(CardManager.ToggleHandVisibility(true));
        }

        #endregion

        #region Public Override Functions

        public override IEnumerator ProcessTurn()
        {
            //pre turn effects
            var pre = PreProcessEffects.ToArray();
            foreach (var tempEffect in pre)
            {
                PlayerUIManager.Instance.DecrementTempEffectIcon(tempEffect);
            }
            yield return ProcessTempEffects(pre);

            yield return base.ProcessTurn();
            yield return WaitFor.Seconds(1);

            if (CurrentHp <= 0) yield break;

            AddMana(CurrentMaxMana); 
            TurnActive = true;
            // set player's mana to the correct value
            // Draw cards 
            yield return CardManager.DrawCards(new[] {0, 1, 2, 3});

            
            // wait for player to end turn
            while (TurnActive && CombatManager.Instance.EnemyCount > 0)
            {
                
                        var e = CombatManager.Instance.GetRandomEnemy;
                        EnemyHealthBarManager.Instance.ShowEnemyDialog(e,DialogEventType.Idle);
               
                yield return null;
            }

            TurnActive = false;

            //post turn effects
            var post = PostProcessEffects.ToArray();
            foreach (var tempEffect in post)
            {
                PlayerUIManager.Instance.DecrementTempEffectIcon(tempEffect);
            }
            yield return ProcessTempEffects(post);

            // wait for any card flip animations to finish
            while (_cardManager.AnyCardsProcessing || _cardManager.AnyCardsFlipping) yield return null;

            yield return CardManager.DiscardAllCardNoDelay();
            SmallInfoWindow.Instance.ShowAbilityShortDesc();
        }
        
        public override void SetAbilityTargets(Ability ability)
        {
            base.SetAbilityTargets(ability);
            abilityTargets.AddRange(CombatManager.Instance.GetEnemies());
        }

        public override (int damagedAmount, int overKillAmount) ApplyDamage(DamageInfo info)
        {
            var result = base.ApplyDamage(info);
            ShakeManager.Instance.AddCameraTrauma((float)info.FinalValue / CurrentMaxHp);
            PlayerUIManager.Instance.UpdateUIHealth(-info.FinalValue, 0, info.AbilityName);
            //PlayerUIManager.Instance.SubtractHealth(dmg, CurrentHp);
            //Debug.Log($"<color=cyan>{Name} (({CurrentHp}/{MaxHp} {(float)CurrentHp / MaxHp:0%}))</color> takes <color=red>{info.Value} damage!</color>");
            return result;
        }

        public override (int healedAmount, int overHealAmount) ApplyHeal(HealInfo info)
        {
            var result = base.ApplyHeal(info);
            PlayerUIManager.Instance.UpdateUIHealth(info.FinalValue, 0, info.AbilityName);
            //PlayerUIManager.Instance.AddHealth(info.Value, CurrentHp);
            return result;
        }

        public override (int absorbAppliedAmount, int overShieldAmount) ApplyAbsorb(int amount, string senderName)
        {
            var result = base.ApplyAbsorb(amount, senderName);
            PlayerUIManager.Instance.UpdateUIHealth(0, amount);
            return result;
        }

        public override void ApplyCleanse(int count, string senderName)
        {
            for (int i = tempEffects.Count - 1; i >= 0; i--)
            {
                var tempEffect = tempEffects[i];
                if (tempEffect is Debuff)
                {
                    PlayerUIManager.Instance.RemoveTempEffectIcon(tempEffect);
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
                    PlayerUIManager.Instance.RemoveTempEffectIcon(tempEffect);
                    effectsToRemove.Add(tempEffect);
                }

                if (effectsToRemove.Count == count)
                    break;
            }

            base.ApplyDispel(count, senderName);
        }

        public override bool ApplyDebuff(Debuff debuff)
        {
            if (!base.ApplyDebuff(debuff)) return false;

            PlayerUIManager.Instance.InitTempEffectIcon(debuff);
            return true;
        }

        public override Buff ApplyBuff(Buff newBuff)
        {
            var activeBuff = base.ApplyBuff(newBuff);
            if (activeBuff == null) return null;

            PlayerUIManager.Instance.InitTempEffectIcon(activeBuff);
            return activeBuff;
        }

        /// <summary>
        /// Remove all buffs by the ability name.
        /// </summary>
        /// <param name="effectName"></param>
        /// <returns>Returns the sum of the remaining duration and the avg value of the removed buffs.</returns>
        public override (ushort durationSum, ushort avgVal) RemoveAllTempEffectByName(AbilityName effectName)
        {
            for (int i = tempEffects.Count - 1; i >= 0; i--)
            {
                var tempEffect = tempEffects[i];
                if (tempEffect.Ability.AbilityName == effectName)
                {
                    PlayerUIManager.Instance.RemoveTempEffectIcon(tempEffect);
                    effectsToRemove.Add(tempEffect);
                }
            }

            return base.RemoveAllTempEffectByName(effectName);
        }

        protected override void ApplyReelMods(TowerFloorInfo result)
        {
            CurrentPercentMaxHpMod += result.PlayerPercentMaxHp;
            CurrentPercentDamageDealt += result.PlayerPercentDmg;
            CurrentFlatMaxManaMod += result.PlayerMaxMana;
        }

        public override void OnDeath()
        {
            FloatingTextManager.Instance.ShowStaticCenterText("You Died!", Color.red, TextSize.XXLarge);
            SubtractDeathExp();
            base.OnDeath();
        }

        public override void RefreshUI()
        {
            CurrentHp = CurrentHp;
            CurrentMana = CurrentMana;
            PlayerUIManager.Instance.Refresh();
            PlayerUIManager.Instance.SetUIMana(CurrentMana);
        }

        #endregion

        #region Public Functions

        public void AddMana(int value)
        {
            CurrentMana = Mathf.Min(CurrentMana + value, CurrentMaxMana);
            PlayerUIManager.Instance.AddMana(value, CurrentMana);
        }

        public void SubtractMana(int value)
        {
            CurrentMana = Mathf.Max(CurrentMana - value, 0);
            PlayerUIManager.Instance.SubtractMana(value, CurrentMana);
        }

        public void AddExp(long value)
        {
            PlayerData.CurrentTotalExp += value;

            if (PlayerData.CurrentTotalExp > PlayerData.NextLevelStartExp)
            {
                OnLevelUp();
            }
            PlayerUIManager.Instance.UpdateUIExp();
        }

        public void SubtractDeathExp()
        {
            var expLoss = Convert.ToInt64((PlayerData.NextLevelStartExp - PlayerData.CurrentLevelStartExp) * Constants.DEATH_EXP_PENALTY);
            PlayerData.CurrentTotalExp = Math.Max(PlayerData.CurrentTotalExp - expLoss, PlayerData.CurrentLevelStartExp);
        }
        
        public static long CalculateExpForLevel(int level)
        {
            if (level < 2 || level > 99) return -1;
            // experience for level calc: exp=250^(level^0.3)
            return Convert.ToInt64(Math.Pow(Constants.EXP_CALC_BASE_VALUE, Math.Pow(level, Constants.EXP_CALC_EXPONENT)));
        }

        public static int CalculateLevelFromExp(long exp)
        {
            // lvl = (int) (log10(exp)/log10(250))^3.33333
            var v = Math.Pow(Math.Log10(exp) / Math.Log10(Constants.EXP_CALC_BASE_VALUE), 1 / Constants.EXP_CALC_EXPONENT);
            var r = Convert.ToInt32(v);
            if (exp < CalculateExpForLevel(r))
                r--;

            //Debug.Log(r + ", " + v.ToString("F10"));// + ", "+ d.ToString("F10") + ", " + tol.ToString("F10"));
            return r;
        }

        public void EndTurn()
        {
            TurnActive = false;
        }
        
        #endregion

        #region Private Functions

        private void OnLevelUp()
        {
            // Level up message, sound, add skill points, etc
            PlayerData.CurrentLevel++;
            PlayerData.CurrentLevelStartExp = PlayerData.NextLevelStartExp;
            PlayerData.NextLevelStartExp = CalculateExpForLevel(PlayerData.CurrentLevel + 1);
        }
        
        #endregion
    }
}
