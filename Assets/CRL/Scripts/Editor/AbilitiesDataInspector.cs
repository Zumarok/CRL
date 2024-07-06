//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using Crux.CRL.AbilitySystem;
//using Crux.CRL.Localization;
//using UnityEditor;
//using UnityEngine;

//namespace Crux.CRL.Editor
//{
//    //[CustomEditor(typeof(AbilitiesData))]
//    public class AbilitiesDataInspector : UnityEditor.Editor
//    {
//        private const string ABILITY_NAMES_PATH = @"\CRL\Scripts\AbilitySystem\";
//        private const string ABILITY_NAMES_FILENAME = @"AbilityName.cs";

//        private static GUIContent _deleteButtonContent = new GUIContent("x", "delete");
//        private static GUIContent _addButtonContent = new GUIContent("+", "add");

//        private SerializedProperty _abilityArray;

//        private Dictionary<string, int> _abilityIndexes;
//        private string _abilityNameInput = "";

//        private bool[] _showLocalizedStringIds;
//        private int _shownCount;
//        private int _filteredCount;

//        private void OnEnable()
//        {
//            serializedObject.Update();

//            _abilityArray = serializedObject.FindProperty("abilities");

//            _abilityIndexes = new Dictionary<string, int>();
//            //_abilityArray.ClearArray();
            
//            for (int i = 0; i < _abilityArray.arraySize; i++)
//            {
//                var ability = _abilityArray.GetArrayElementAtIndex(i);
//                var abilityName = ((AbilityName)ability.FindPropertyRelative("_abilityName").enumValueIndex).ToString();
//                if (!_abilityIndexes.ContainsKey(abilityName))
//                    _abilityIndexes.Add(abilityName, i);
//            }
            
//            UpdateDataFromEnumValues();

//            _showLocalizedStringIds = new bool[_abilityArray.arraySize];

//            serializedObject.ApplyModifiedProperties();
//        }

//        public override void OnInspectorGUI()
//        {
//            serializedObject.Update();
//            GUILayout.Space(5);
//            GUILayout.Label("Filter / Add Ability (Type Filter- t:DD)");
//            _abilityNameInput = GUILayout.TextField(_abilityNameInput);

//            GUI.contentColor = Color.green;
//            if (GUILayout.Button(_addButtonContent))
//            {
//                AddNewAbility(_abilityNameInput);
//            }
//            GUI.contentColor = Color.white;
//            GUILayout.Label($"Shown:{_shownCount}, Filtered:{_filteredCount}");
//            EditorUtils.DrawUILine(Color.gray);
//            GUILayout.Space(5);
//            ShowAbilities();
//            serializedObject.ApplyModifiedProperties();
//        }

//        private void ShowAbilities()
//        {
//            //EditorGUILayout.PropertyField(_abilityArray);                           // show title
//            //if (!list.isExpanded) return;                                           // check if list is expanded
//            //EditorGUI.indentLevel++;                                                // indent elements
//            //EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size")); // show array size

//            _shownCount = 0;
//            _filteredCount = 0;
//            for (int i = 0; i < _abilityArray.arraySize; i++)
//            {
//                var ability = _abilityArray.GetArrayElementAtIndex(i);
//                var abilityName = ((AbilityName) ability.FindPropertyRelative("_abilityName").enumValueIndex).ToString();

//                switch (abilityName)
//                {
//                    case "-1":
//                        OnEnable();
//                        return;
//                    case "None":
//                        continue;
//                }

//                var abilityType = ability.FindPropertyRelative("_abilityType");

//                if (AbilityFiltered(abilityName, (AbilityType) abilityType.intValue))
//                {
//                    _filteredCount++;
//                    continue;
//                }

//                _shownCount++;
                
//                ability.isExpanded = EditorGUILayout.Foldout(ability.isExpanded, abilityName);
//                if (!ability.isExpanded) continue;
//                GUILayout.Space(5);

//                EditorGUI.indentLevel++;
//                _showLocalizedStringIds[i] = EditorGUILayout.Foldout(_showLocalizedStringIds[i], "Localized String Ids:");
//                if (_showLocalizedStringIds[i])
//                {
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_displayNameId"));
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_shortDescriptionId"));
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_longDescriptionId"));
//                }
//                EditorGUI.indentLevel--;

//                EditorGUILayout.PropertyField(abilityType);

//                if (((AbilityType) abilityType.intValue).HasFlag(AbilityType.DD))
//                {
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_ddValue"));
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_maxTargets"));
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_ddNumHits"));
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_randomTarget"));
//                    var splashDmg = ability.FindPropertyRelative("_splashDamage");
//                    EditorGUILayout.PropertyField(splashDmg);
//                    if (splashDmg.boolValue)
//                    {
//                        EditorGUILayout.PropertyField(ability.FindPropertyRelative("_splashRadius"));
//                        EditorGUILayout.PropertyField(ability.FindPropertyRelative("_splashMultiplier"));
//                    }
//                }

//                if (((AbilityType) abilityType.intValue).HasFlag(AbilityType.DOT))
//                {
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_dotValue"));
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_maxTargets"));
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_dotDuration"));
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_dotNumHits"));
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_randomTarget"));
//                }

//                if (((AbilityType)abilityType.intValue).HasFlag(AbilityType.AoE_DD))
//                {
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_aoeDDValue"));
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_maxTargets"));
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_aoeDDNumHits"));
//                }

//                if (((AbilityType)abilityType.intValue).HasFlag(AbilityType.AoE_DOT))
//                {
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_aoeDotValue"));
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_maxTargets"));
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_aoeDotDuration"));
//                }

//                if (((AbilityType)abilityType.intValue).HasFlag(AbilityType.Heal))
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_healValue"));

//                if (((AbilityType) abilityType.intValue).HasFlag(AbilityType.HoT))
//                {
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_healValue"));
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_hotDuration"));
//                }

//                if (((AbilityType)abilityType.intValue).HasFlag(AbilityType.Cleanse))
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_cleanseValue"));

//                if (((AbilityType)abilityType.intValue).HasFlag(AbilityType.Dispel))
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_dispelValue"));

//                if (((AbilityType)abilityType.intValue).HasFlag(AbilityType.Immunity))
//                    EditorGUILayout.PropertyField(ability.FindPropertyRelative("_immunityDuration"));

//                // options for all skill types
//                EditorGUILayout.PropertyField(ability.FindPropertyRelative("_vfx"));
//                EditorGUILayout.PropertyField(ability.FindPropertyRelative("_vfxDamageDelay"));
//                EditorGUILayout.PropertyField(ability.FindPropertyRelative("_sfxPath"));
//                EditorGUILayout.PropertyField(ability.FindPropertyRelative("_manaCost"));

//                GUILayout.BeginHorizontal();
//                GUILayout.FlexibleSpace();
//                GUI.contentColor = Color.red;
//                if (GUILayout.Button(_deleteButtonContent))
//                    RemoveAbility(abilityName);
//                GUI.contentColor = Color.white;
//                GUILayout.EndHorizontal();
//                EditorUtils.DrawUILine(Color.gray);
//            }
//        }
        
//        private void AddNewAbility(string newAbilityName)
//        {
//            if (string.IsNullOrWhiteSpace(_abilityNameInput)) return;

//            if (!char.IsUpper(_abilityNameInput[0]))
//            {
//                _abilityNameInput = "";
//                Debug.LogError("Skill name must start with an upper-case letter.");
//                return;
//            }

//            foreach (var c in newAbilityName)
//            {
//                if (!char.IsLetter(c) && !char.IsDigit(c))
//                {
//                    _abilityNameInput = "";
//                    Debug.LogError("Skill name can only contain numbers and letters");
//                    return;
//                }
//            }

//            newAbilityName = newAbilityName.Replace(" ", string.Empty);
//            var path = Application.dataPath + ABILITY_NAMES_PATH + ABILITY_NAMES_FILENAME;
//            var text = File.ReadAllText(path, Encoding.UTF8);
//            var abilityNamesString = text.Substring(text.LastIndexOf('{') + 1, text.IndexOf('}')+ 1 - text.LastIndexOf('{') - 1);
//            var strsToRemove = new[] {" ", "\t", "\n", "\r", "}"};
//            abilityNamesString = strsToRemove.Aggregate(abilityNamesString, (current, s) => current.Replace(s, string.Empty)).TrimEnd(',');
//            var abilityNameArray = abilityNamesString.Split(',');

//            if (!abilityNameArray.Contains(newAbilityName))
//            {
//                text = text.Insert(text.LastIndexOf(',') + 1, "\n\t\t" + newAbilityName + ",");
//                File.WriteAllText(path, text);
//            }
//            else
//                Debug.LogError($"Ability {newAbilityName} already exists!");

//            _abilityNameInput = "";
//            AssetDatabase.Refresh();
//            OnEnable();
//        }

//        private void RemoveAbility(string abilityName)
//        {
//            var path = Application.dataPath + ABILITY_NAMES_PATH + ABILITY_NAMES_FILENAME;
//            var text = File.ReadAllLines(path, Encoding.UTF8).ToList();
//            for (var i = 0; i < text.Count; i++)
//            {
//                var t = text[i];
//                if (t.Contains(abilityName))
//                {
//                    text.Remove(t);
//                    break;
//                }
//            }

//            File.WriteAllLines(path, text);
//            AssetDatabase.Refresh();
//            OnEnable();
//        }

//        private bool AbilityFiltered(string abilityName, AbilityType type)
//        {
//            if (string.IsNullOrWhiteSpace(_abilityNameInput)) return false;
//            if (abilityName.ToLower().Contains(_abilityNameInput.ToLower())) return false;
//            if (_abilityNameInput.Contains("t:"))
//            {
//                var filteredTypes = GetFilteredTypes(_abilityNameInput, "t:");

//                foreach (var abilityType in filteredTypes)
//                {
//                    if (!type.HasFlag(abilityType)) return true;
//                }

//                return false;
//            }

//            return true;
//        }

//        private List<AbilityType> GetFilteredTypes(string str, string value)
//        {
//            var indexes = new List<int>();
//            for (int index = 0; ; index++)
//            {
//                index = str.IndexOf(value, index);
//                if (index == -1)
//                    break;
//                indexes.Add(index);
//            }

//            var types = new List<AbilityType>();
//            foreach (var i in indexes)
//            {
//                var end = str.IndexOf(" ", i);
//                if (end == -1)
//                    end = str.Length;
//                if(Enum.TryParse(str.Substring(i, end - i).Substring(2), true, out AbilityType type))
//                    types.Add(type);
//            }

//            return types;
//        }

//        private void UpdateDataFromEnumValues()
//        {
//            var abilityNames = Enum.GetNames(typeof(AbilityName));
//            var localizedStringFields = new[] { "_displayNameId", "_shortDescriptionId", "_longDescriptionId" };

//            // Add new abilities
//            foreach (var abilityName in abilityNames)
//            {
//                if (!_abilityIndexes.ContainsKey(abilityName))
//                {
//                    var i = _abilityArray.arraySize;
//                    _abilityArray.InsertArrayElementAtIndex(i);
//                    var ability = _abilityArray.GetArrayElementAtIndex(i);
//                    Enum.TryParse(abilityName, out AbilityName enumVal);
//                    var propertyVal = ability.FindPropertyRelative("_abilityName");
//                    propertyVal.enumValueIndex = (int)enumVal;

//                    ability.FindPropertyRelative("_enumStringName").stringValue = abilityName;

//                    var guids = AssetDatabase.FindAssets("t:ScriptableObject LocalizationData");
//                    if (guids?.Length == 0) return;
//                    var locStrings = AssetDatabase.LoadAssetAtPath<LocalizationData>(AssetDatabase.GUIDToAssetPath(guids?[0])).localizedStrings;

//                    // generate localized string IDs, set the abilities property and create an entry in the localizationData
//                    foreach (var fieldName in localizedStringFields)
//                    {
//                        var id = string.Join("_", "Ability", abilityName, fieldName.Substring(1, fieldName.Length - 3));
//                        ability.FindPropertyRelative(fieldName).stringValue = id;
//                        if (!locStrings.ContainsKey(id))
//                            locStrings.Add(id, new LocalizedString());
//                        else
//                            Debug.LogWarning($"LocalizationData already contained an entry for ID:{id}");
//                    }
//                }
//            }

//            // remove deleted abilities
//            for (int i = 0; i < _abilityArray.arraySize; i++)
//            {
//                var ability = _abilityArray.GetArrayElementAtIndex(i);
//                var abilityName = ability.FindPropertyRelative("_enumStringName").stringValue;
//                var abilityNameEnum = ability.FindPropertyRelative("_abilityName").enumValueIndex;
//                var guids = AssetDatabase.FindAssets("t:ScriptableObject LocalizationData");
//                if (guids?.Length == 0) return;
//                var locStrings = AssetDatabase.LoadAssetAtPath<LocalizationData>(AssetDatabase.GUIDToAssetPath(guids?[0])).localizedStrings;


//                if (!abilityNames.Contains(abilityName) || abilityNameEnum == -1)
//                {
//                    _abilityArray.DeleteArrayElementAtIndex(i);
//                    // remove localized string IDs from the localizationData
//                    foreach (var fieldName in localizedStringFields)
//                    {
//                        var id = string.Join("_", "Ability", abilityName, fieldName.Substring(1, fieldName.Length - 3));
//                        if (locStrings.ContainsKey(id))
//                            locStrings.Remove(id);
//                    }
//                }
//            }
//        }

//    }
//}
