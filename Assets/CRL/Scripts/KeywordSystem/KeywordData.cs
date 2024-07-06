using System;
using Crux.CRL.Localization;
using RotaryHeart.Lib.SerializableDictionary;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Crux.CRL.KeywordSystem
{
    [CreateAssetMenu(fileName = "KeywordData", menuName = "ScriptableObjects/KeywordData")]
    public class KeywordData : ScriptableObject
    {
        #pragma warning disable IDE0052, 414
        [DetailedInfoBox("Instructions", "Keyword: Add the keyword, then fill in info in LocalizationData.\nValue: Add the value, then add the value to the KeywordParser.cs->GetValue() switch statement.")]
        [BoxGroup("Keyword and Value Input")]
        [InlineButton("RemoveKeyword"), InlineButton("AddKeyword"), ShowInInspector] 
        private string _keywordLookupKey;

        [BoxGroup("Keyword and Value Input")]
        [InlineButton("RemoveValue"), InlineButton("AddValue"), ShowInInspector]
        private string _valueLookupKey;
        #pragma warning restore IDE0052, 414

        [ReadOnly, BoxGroup("Dictionaries")]
        public KeywordLookupDictionary keywordDictionary;
        [ReadOnly, BoxGroup("Dictionaries")]
        public ValueLookupDictionary valueDictionary;

#if UNITY_EDITOR

        private void AddKeyword(string keywordName)
        {
            if (string.IsNullOrWhiteSpace(keywordName)) return;

            try
            {
                keywordDictionary.Add(keywordName, $"&{keywordName}&");
                var guids = AssetDatabase.FindAssets("t:ScriptableObject LocalizationData");
                if (guids?.Length == 0) return;
                var locStrings = AssetDatabase.LoadAssetAtPath<LocalizationData>(AssetDatabase.GUIDToAssetPath(guids?[0])).localizedStrings;
                locStrings.Add($"Keyword_&{keywordName}&_displayName", new LocalizedString());
                locStrings.Add($"Keyword_&{keywordName}&_description", new LocalizedString());
                _keywordLookupKey = "";
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed to add the keyword. Check for proper format and that it doesn't already exist.");
                Debug.LogError(e.Message);
            }
        }

        private void RemoveKeyword(string keywordName)
        {
            if (string.IsNullOrWhiteSpace(keywordName)) return;

            try
            {
                keywordDictionary.Remove(keywordName);
                _keywordLookupKey = "";
            }
            catch
            {
                Debug.LogWarning("Failed to remove the keyword. Check that the key is valid.");
            }
        }

        private void AddValue(string valueName)
        {
            if (string.IsNullOrWhiteSpace(valueName)) return;

            try
            {
                valueDictionary.Add(valueName, $"^{valueName}^");
                _valueLookupKey = "";
            }
            catch
            {
                Debug.LogWarning("Failed to add the value. Check for proper format and that it doesn't already exist.");
            }
        }

        private void RemoveValue(string valueName)
        {
            if (string.IsNullOrWhiteSpace(valueName)) return;

            try
            {
                valueDictionary.Remove(valueName);
                _valueLookupKey = "";
            }
            catch
            {
                Debug.LogWarning("Failed to remove the value. Check that the key is valid.");
            }
        }
#endif

    }

    [Serializable]
    public class KeywordLookupDictionary : SerializableDictionaryBase<string, string>
    {
    }

    [Serializable]
    public class ValueLookupDictionary : SerializableDictionaryBase<string, string>
    { }
}
