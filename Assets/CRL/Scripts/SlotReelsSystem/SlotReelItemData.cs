using System;
using System.Collections.Generic;
using System.Linq;
using Crux.CRL.AbilitySystem;
using Crux.CRL.DataSystem;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Crux.CRL.SlotReelsSystem
{
    [Serializable]
    public class SlotReelItemData
    {
        public Ability Ability { get; private set; }
        public Sprite IconSprite => Ability.IconSprite;
        public string DisplayName => Ability.FormattedName;
        public string ShortDescription => Ability.ShortDescription();
        public string LongDescription => Ability.LongDescription;
        public int Weight => Ability.ReelModWeight;

        private SlotReelItemData()
        {
            
        }

        public SlotReelItemData(Ability ability)
        {
            Ability = ability;
        }

#if UNITY_EDITOR
        private List<AbilityName> ReelModAbilities()
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableObject AbilitiesData");
            if (guids?.Length == 0) return new List<AbilityName>();
            var obj = AssetDatabase.LoadAssetAtPath<AbilitiesData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return obj.abilities.Where(a => a.IsReelMod).Select(a => a.AbilityName).ToList();
        }
#endif

    }
}
