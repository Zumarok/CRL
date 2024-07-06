using System.Collections.Generic;
using System.Linq;
using Crux.CRL.Localization;
using UnityEngine;

namespace Crux.CRL.DialogSystem
{
    [CreateAssetMenu(fileName = "DialogScriptObject", menuName = "ScriptableObjects/DialogScriptObject")]
    public class DialogScriptObject : ScriptableObject
    {
        public List<EnemyDialog> EnemyDialogs;

        public string GetDialog(string enemyName, DialogEventType type, LocalizationLanguage lang)
        {
           var ed = EnemyDialogs.FirstOrDefault(d => d.EnemyName == enemyName);
           return ed == null ? "" : ed.GetDialog(type, lang);
        }

    }
}

