using Crux.CRL.AbilitySystem;
using Crux.CRL.DialogSystem;
using Crux.CRL.EnemySystem;
using Crux.CRL.KeywordSystem;
using Crux.CRL.Localization;
using UnityEditor;

namespace Crux.CRL.Editor
{
    public class CruxMenuShortcuts
    {
        [MenuItem("Crux/Abilities Data")]
        public static void SelectAbilitiesObject()
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableObject AbilitiesData");
            if (guids?.Length == 0) return;
            var obj = AssetDatabase.LoadAssetAtPath<AbilitiesData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            Selection.SetActiveObjectWithContext(obj, null);
            //GetWindow<BuildWindow>(false, "Project Tools", true);
        }

        [MenuItem("Crux/Localization Data")]
        public static void SelectLocalizationObject()
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableObject LocalizationData");
            if (guids?.Length == 0) return;
            var obj = AssetDatabase.LoadAssetAtPath<LocalizationData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            Selection.SetActiveObjectWithContext(obj, null);
            //GetWindow<BuildWindow>(false, "Project Tools", true);
        }

        [MenuItem("Crux/Keyword Data")]
        public static void SelectKeywordObject()
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableObject KeywordData");
            if (guids?.Length == 0) return;
            var obj = AssetDatabase.LoadAssetAtPath<KeywordData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            Selection.SetActiveObjectWithContext(obj, null);
            //GetWindow<BuildWindow>(false, "Project Tools", true);
        }

        [MenuItem("Crux/Enemy Spawn Data")]
        public static void SelectEnemySpawnObject()
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableObject EnemySpawnScriptObject");
            if (guids?.Length == 0) return;
            var obj = AssetDatabase.LoadAssetAtPath<EnemySpawnScriptObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
            Selection.SetActiveObjectWithContext(obj, null);
            //GetWindow<BuildWindow>(false, "Project Tools", true);
        }

        [MenuItem("Crux/Enemy Dialog")]
        public static void SelectEnemyDialogObject()
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableObject DialogScriptObject");
            if (guids?.Length == 0) return;
            var obj = AssetDatabase.LoadAssetAtPath<DialogScriptObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
            Selection.SetActiveObjectWithContext(obj, null);
            //GetWindow<BuildWindow>(false, "Project Tools", true);
        }
    }
}
