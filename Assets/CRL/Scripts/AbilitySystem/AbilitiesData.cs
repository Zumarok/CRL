using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Crux.CRL.Localization;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Crux.CRL.AbilitySystem
{
    #region Abilities Data Scriptable Object
    public class AbilitiesData : ScriptableObject
    {
        private const string ABILITY_NAMES_PATH = @"\CRL\Scripts\AbilitySystem\";
        private const string ABILITY_NAMES_FILENAME = @"AbilityName.cs";

        [SerializeField, ReadOnly]
        private int _nextAbilityNameEnumId;

        #pragma warning disable IDE0052, 414
        [BoxGroup("Add\\Remove Ability(Type Filter - t:DD)"), ShowInInspector, InlineButton("RemoveAbility"), InlineButton("AddAbility")]
        private string _abilityName;
        #pragma warning restore IDE0052, 414

        [ListDrawerSettings(ShowIndexLabels = false, ListElementLabelName = "_enumStringName", IsReadOnly = true), Space]
        public List<Ability> abilities;

#if UNITY_EDITOR

        private bool FilterAbilities(Ability ability)
        {
            return false;
        }

        private void AddAbility(string abilityName)
        {
            if (string.IsNullOrWhiteSpace(abilityName)) return;

            if (!char.IsUpper(abilityName[0]))
            {
                _abilityName = "";
                Debug.LogError("Skill name must start with an upper-case letter.");
                return;
            }

            foreach (var c in abilityName)
            {
                if (!char.IsLetter(c) && !char.IsDigit(c))
                {
                    _abilityName = "";
                    Debug.LogError("Skill name can only contain numbers and letters");
                    return;
                }
            }

            var abilityEnumCount = Enum.GetNames(typeof(AbilityName)).Length;
            abilityName = abilityName.Replace(" ", string.Empty);
            var path = Application.dataPath + ABILITY_NAMES_PATH + ABILITY_NAMES_FILENAME;
            var text = File.ReadAllText(path, Encoding.UTF8);
            var abilityNamesString = text.Substring(text.LastIndexOf('{') + 1, text.IndexOf('}') + 1 - text.LastIndexOf('{') - 1);
            var strsToRemove = new[] { " ", "\t", "\n", "\r", "}" };
            abilityNamesString = strsToRemove.Aggregate(abilityNamesString, (current, s) => current.Replace(s, string.Empty)).TrimEnd(',');
            var abilityNameArray = abilityNamesString.Split(',');

            if (!abilityNameArray.Contains(abilityName))
            {
                // add to AbilityName.cs enum file
                text = text.Insert(text.LastIndexOf(',') + 1, "\n\t\t" + abilityName + $"={_nextAbilityNameEnumId},");
                File.WriteAllText(path, text);

                // generate localized string IDs and create an entries in the localizationData
                var guids = AssetDatabase.FindAssets("t:ScriptableObject LocalizationData");
                if (guids?.Length == 0) return;
                var locStrings = AssetDatabase.LoadAssetAtPath<LocalizationData>(AssetDatabase.GUIDToAssetPath(guids?[0])).localizedStrings;

                var displayNameId = $"Ability_{abilityName}_displayName";
                var shortDescId = $"Ability_{abilityName}_shortDescription";
                var longDescId = $"Ability_{abilityName}_longDescription";

                foreach (var id in new []{displayNameId,shortDescId,longDescId})
                {
                    if (!locStrings.ContainsKey(id))
                        locStrings.Add(id, new LocalizedString());
                    else
                        Debug.LogWarning($"LocalizationData already contained an entry for ID:{id}");
                }

                // create new ability and add to abilities List
                abilities.Add(new Ability(abilityName, (AbilityName)_nextAbilityNameEnumId, displayNameId, shortDescId, longDescId));
                _nextAbilityNameEnumId++;
            }
            else
                Debug.LogError($"Ability {abilityName} already exists!");

            _abilityName = "";
            abilities = abilities.OrderBy(a => a.EnumStringName).ToList();
            AssetDatabase.Refresh();
        }

        private void RemoveAbility(string abilityName)
        {
            if (string.IsNullOrWhiteSpace(abilityName)) return;
            
            // if the AbilityName enum contains the abilityName remove it from the AbilityName.cs file
            if (Enum.GetNames(typeof(AbilityName)).Contains(abilityName))
            {
                var path = Application.dataPath + ABILITY_NAMES_PATH + ABILITY_NAMES_FILENAME;
                var text = File.ReadAllLines(path, Encoding.UTF8).ToList();
                for (var i = 0; i < text.Count; i++)
                {
                    var t = text[i];
                    if (t.Contains(abilityName))
                    {
                        text.Remove(t);
                        break;
                    }
                }

                File.WriteAllLines(path, text);
            }

            // delete the localization string entries
            var guids = AssetDatabase.FindAssets("t:ScriptableObject LocalizationData");
            if (guids?.Length == 0) return;
            var locStrings = AssetDatabase.LoadAssetAtPath<LocalizationData>(AssetDatabase.GUIDToAssetPath(guids?[0])).localizedStrings;

            var displayNameId = $"Ability_{abilityName}_displayName";
            var shortDescId = $"Ability_{abilityName}_shortDescription";
            var longDescId = $"Ability_{abilityName}_longDescription";
            foreach (var id in new[] { displayNameId, shortDescId, longDescId })
            {
                if (locStrings.ContainsKey(id))
                    locStrings.Remove(id);
                else
                    Debug.LogWarning($"LocalizationData doesn't contain an entry for ID:{id}");
            }

            // delete the ability from the abilities list
            var ability = abilities.FirstOrDefault(a => a.EnumStringName == abilityName);
            if (abilities.Contains(ability))
            {
                abilities.Remove(ability);
            }

            abilities = abilities.OrderBy(a => a.EnumStringName).ToList();
            AssetDatabase.Refresh();
        }


        //[UnityEditor.Callbacks.DidReloadScripts]
        //private static void RealignAbilityNameEnums()
        //{
        //    Debug.Log("Realigning AbilityName enum values for each ability in AbilitiesData scriptableObject");
        //    var guids = AssetDatabase.FindAssets("t:ScriptableObject AbilitiesData");
        //    if (guids?.Length == 0) return;
        //    var abilities = AssetDatabase.LoadAssetAtPath<AbilitiesData>(AssetDatabase.GUIDToAssetPath(guids?[0])).abilities;
        //    foreach (var ability in abilities)
        //    {
        //        ability.RealignAbilityNameEnum();
        //    }
        //}


#endif
    }

    #endregion
}


