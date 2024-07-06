using System;
using Crux.CRL.KeywordSystem;
using RotaryHeart.Lib.SerializableDictionary;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Crux.CRL.Localization
{
    [CreateAssetMenu(fileName = "LocalizationData", menuName = "ScriptableObjects/LocalizationData", order = 1)]
    public class LocalizationData : ScriptableObject
    {
        [ShowInInspector, ReadOnly, BoxGroup("Keywords and Values"), NonSerialized]
        private KeywordLookupDictionary _keywordLookupDictionary;
        [ShowInInspector, ReadOnly, BoxGroup("Keywords and Values"), NonSerialized]
        private ValueLookupDictionary _valueLookupDictionary;

        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.CollapsedFoldout, KeyLabel = "ID", ValueLabel = "Languages", IsReadOnly = true)]
        public LocalizationStringsDictionary localizedStrings;

#if UNITY_EDITOR

        [BoxGroup("Keywords and Values"), Button(ButtonSizes.Large)]
        private void GetKeywordsAndValues()
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableObject KeywordData");
            if (guids?.Length == 0) return;
            var keywordData = AssetDatabase.LoadAssetAtPath<KeywordData>(AssetDatabase.GUIDToAssetPath(guids?[0]));
            _keywordLookupDictionary = keywordData.keywordDictionary;
            _valueLookupDictionary = keywordData.valueDictionary;
        }
#endif

    }

    [Serializable]
    public class LocalizationStringsDictionary : SerializableDictionaryBase<string, LocalizedString>
    {   
    }

    [Serializable, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.CollapsedFoldout, KeyLabel = "Language")]
    public class LocalizedString : SerializableDictionaryBase<LocalizationLanguage, string>
    {
        public LocalizedString()
        {
            foreach (LocalizationLanguage language in Enum.GetValues(typeof(LocalizationLanguage)))
            {
                Add(language, "");
            }
        }
    }
}
