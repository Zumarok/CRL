using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Crux.CRL.AbilitySystem;
using Crux.CRL.CombatSystem;
using Crux.CRL.DataSystem;

namespace Crux.CRL.KeywordSystem
{
    public class KeywordParser
    {
        #region Fields

        private readonly StringBuilder _stringBuilder;
        private readonly StringBuilder _valueStringBuilder;

        #endregion

        #region Constructor

        public KeywordParser()
        {
            _stringBuilder = new StringBuilder();
            _valueStringBuilder = new StringBuilder();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Format a string, replacing delimiter-separated values with actual values.
        /// Also searches for Keywords and replaces them with localized keyword names.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="ability">The ability this string is referencing for values.</param>
        /// <param name="cultureInfo">The current culture variant for number formatting.</param>
        /// <param name="abilityOwner">If included, the value will include caster's modifiers in the calculation. </param>
        /// <param name="target">If provided, the value will include the target's modifiers in the calculation.</param>
        /// <returns>(string parsedString, List&lt;Keyword&gt; keywordList) where parsedString is the formatted string for output and keywordList has all relevant keywords.</returns>
        public (string parsedString, List<Keyword> keywordList) ParseKeywordString(string str, Ability ability, CultureInfo cultureInfo, Combatant abilityOwner = null, Combatant target = null)
        {
            var valueMatches = Regex.Matches(str, @"(\^[^\^]*?\^)");
            var keywordMatches = Regex.Matches(str, @"(&[^\^]*?&)");
            var keywords = new List<Keyword>();
            _stringBuilder.Clear();
            _stringBuilder.Append(str);
            foreach (Match match in valueMatches)
            {
                _stringBuilder.Replace(match.Value, GetValue(match.Value, ability, cultureInfo, abilityOwner, target));
            }

            foreach (Match match in keywordMatches)
            {
                var kw = GetKeyword(match.Value);
                keywords.Add(kw);
                _stringBuilder.Replace(match.Value, $"<link=\"{match.Value}\"><color={Constants.KEYWORD_TEXT_COLOR}><u>{kw.name}</u></color></link>");
            }

            return (_stringBuilder.ToString(), keywords);
        }

        public Keyword GetKeyword(string key)
        {
            return new Keyword(DataManager.Instance.GetLocalizedString($"Keyword_{key}_displayName"),
                DataManager.Instance.GetLocalizedString($"Keyword_{key}_description"));
        }

        #endregion

        #region Keyword Definitions

        private string GetValue(string key, Ability ability, CultureInfo cultureInfo, Combatant abilityOwner = null, Combatant target = null)
        {
            Ability triggeredAbility;
            _valueStringBuilder.Clear();
            _valueStringBuilder.AppendFormat("<color={0}>", Constants.VARIABLE_MOD_TEXT_COLOR);
            switch (key)
            {
                case "^DirectDamage^":
                    _valueStringBuilder.Append(DamageInfo.CalculateFinalDamage(ability.DDValue, abilityOwner, target).ToString(cultureInfo));
                    _valueStringBuilder.Append(" damage");  // todo localize these if needed
                    break;
                case "^DoTDamage^":
                    _valueStringBuilder.Append(DamageInfo.CalculateFinalDamage(ability.DoTValue, abilityOwner, target).ToString(cultureInfo));
                    _valueStringBuilder.Append(" damage");
                    break;
                case "^Radius^":
                    _valueStringBuilder.Append(ability.AbilityRadius.ToString(cultureInfo));
                    _valueStringBuilder.Append("m");
                    break;
                case "^Duration^":
                    _valueStringBuilder.Append(ability.Duration.ToString(cultureInfo));
                    _valueStringBuilder.Append(" turns");
                    break;
                case "^NumOfHits^":
                    _valueStringBuilder.Append(ability.NumOfHits.ToString(cultureInfo));
                    _valueStringBuilder.Append("x");
                    break;
                case "^NumOfTargets^":
                    _valueStringBuilder.Append(ability.MaxTargets.ToString(cultureInfo));
                    break;
                case "^AbsorbValue^":
                    _valueStringBuilder.Append(ability.AbsorbValue.ToString(cultureInfo));
                    _valueStringBuilder.Append(" damage");
                    break;
                case "^HoTValue^":
                    _valueStringBuilder.Append(ability.HoTValue.ToString(cultureInfo));
                    _valueStringBuilder.Append(" health");
                    break;
                case "^HoTRemoveAndHealMult^":
                    _valueStringBuilder.AppendFormat("x{0}", ability.HoTRemoveAndHealMult.ToString(cultureInfo));
                    break;
                case "^CleanseValue^":
                    _valueStringBuilder.Append(ability.CleanseValue.ToString(cultureInfo));
                    break;
                case "^DispelValue^":
                    _valueStringBuilder.Append(ability.DispelValue.ToString(cultureInfo));
                    break;
                case "^HealValue^":
                    _valueStringBuilder.Append(ability.HealValue.ToString(cultureInfo));
                    _valueStringBuilder.Append(" health");
                    break;
                case "^BuffFlatDamageDealt^":
                    _valueStringBuilder.Append(Math.Abs(ability.BuffFlatDamageDealtValue).ToString(cultureInfo));
                    _valueStringBuilder.Append(" damage");
                    break;
                case "^BuffPercentDamageDealt^":
                    _valueStringBuilder.Append(Math.Abs(ability.BuffPercentDamageDealtValue * 100).ToString(cultureInfo));
                    _valueStringBuilder.Append("%");
                    break;
                case "^BuffFlatDamageTaken^":
                    _valueStringBuilder.Append(Math.Abs(ability.BuffFlatDamageTakenValue).ToString(cultureInfo));
                    _valueStringBuilder.Append(" damage");
                    break;
                case "^BuffPercentDamageTaken^":
                    _valueStringBuilder.Append(Math.Abs(ability.BuffPercentDamageTakenValue * 100).ToString(cultureInfo));
                    _valueStringBuilder.Append("%");
                    break;
                case "^BuffPercentMaxHp^":
                    _valueStringBuilder.Append(Math.Abs(ability.BuffPercentMaxHp * 100).ToString(cultureInfo));
                    _valueStringBuilder.Append("%");
                    break;
                case "^BuffDamageTakenOnCast^":
                    _valueStringBuilder.Append(ability.BuffDamageTakenOnCastValue.ToString(cultureInfo));
                    _valueStringBuilder.Append(" damage");
                    break;
                case "^BuffHealTakenOnCast^":
                    _valueStringBuilder.Append(ability.BuffHealTakenOnCastValue.ToString(cultureInfo));
                    _valueStringBuilder.Append(" health");
                    break;
                case "^BuffExplodeValue^":
                    _valueStringBuilder.Append(ability.BuffExplodeValue.ToString(cultureInfo));
                    _valueStringBuilder.Append(" damage");
                    break;
                case "^BuffExtraCastValue^":
                    _valueStringBuilder.Append(ability.BuffExtraCast.ToString(cultureInfo));
                    _valueStringBuilder.Append(" additional time(s)");
                    break;
                case "^BuffDamageTakenOnTurn^":
                    _valueStringBuilder.Append(ability.BuffTakeDamageOnTurnValue.ToString(cultureInfo));
                    _valueStringBuilder.Append(" damage");
                    break;
                case "^TriggerValue^":
                    triggeredAbility = DataManager.Instance.GetAbilityData(ability.TriggerAbility);
                    _valueStringBuilder.Append(Math.Abs(triggeredAbility.TriggerValuePercent * 100).ToString(cultureInfo));
                    _valueStringBuilder.Append("%");
                    break;
                case "^TriggerChance^":
                    _valueStringBuilder.Append(Math.Abs(ability.TriggerChance * 100).ToString(cultureInfo));
                    _valueStringBuilder.Append("%");
                    break;
                case "^TriggeredHealValue^":
                    triggeredAbility = DataManager.Instance.GetAbilityData(ability.TriggerAbility);
                    _valueStringBuilder.Append(triggeredAbility.HealValue.ToString(cultureInfo));
                    _valueStringBuilder.Append(" health");
                    break;
                case "^TriggeredBuffPercentDamageDealt^":
                    triggeredAbility = DataManager.Instance.GetAbilityData(ability.TriggerAbility);
                    _valueStringBuilder.Append(Math.Abs(triggeredAbility.BuffPercentDamageDealtValue * 100).ToString(cultureInfo));
                    _valueStringBuilder.Append("%");
                    break;
                case "^TriggeredBuffPercentMaxHp^":
                    triggeredAbility = DataManager.Instance.GetAbilityData(ability.TriggerAbility);
                    _valueStringBuilder.Append(Math.Abs(triggeredAbility.BuffPercentMaxHp * 100).ToString(cultureInfo));
                    _valueStringBuilder.Append("%");
                    break;
                case "^ReelEnemyHealthPercent^":
                    _valueStringBuilder.Append(Math.Abs(ability.ReelEnemyHealthPercent * 100).ToString(cultureInfo));
                    _valueStringBuilder.Append("%");
                    break;
                case "^ReelEnemyDamagePercent^":
                    _valueStringBuilder.Append(Math.Abs(ability.ReelEnemyDamagePercent * 100).ToString(cultureInfo));
                    _valueStringBuilder.Append("%");
                    break;
                case "^ReelGoldDropMod^":
                    _valueStringBuilder.Append(Math.Abs(ability.ReelGoldModifier * 100).ToString(cultureInfo));
                    _valueStringBuilder.Append("%");
                    break;
                case "^ReelShardDropMod^":
                    _valueStringBuilder.Append(Math.Abs(ability.ReelShardModifier * 100).ToString(cultureInfo));
                    _valueStringBuilder.Append("%");
                    break;
                case "^ExtraWaves^":
                    _valueStringBuilder.Append(ability.NumReelExtraWaves.ToString(cultureInfo));
                    _valueStringBuilder.Append(" additional wave(s)");
                    break;
                case "^ReelModExtraDD^":
                    _valueStringBuilder.Append(ability.ReelNumExtraDD.ToString(cultureInfo));
                    break;
                case "^ReelModMaxMana^":
                    _valueStringBuilder.Append(ability.ReelPlayerFlatMana.ToString(cultureInfo));
                    break;
                case "^ReelModEliteSpawns^":
                    _valueStringBuilder.Append(ability.ReelNumSpawnedElites.ToString(cultureInfo));
                    break;
                default:
                    return $"Undefined value key: {key}";
            }
            _valueStringBuilder.Append("</color>");
            return _valueStringBuilder.ToString();
        }
        
        #endregion
    }
}
