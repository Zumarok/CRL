using System;
using System.Collections.Generic;
using System.Linq;
using Crux.CRL.Localization;
using Crux.CRL.Utils;
using UnityEngine;

namespace Crux.CRL.DialogSystem
{
    [Serializable]
    public class EnemyDialog
    {
        [SerializeField] private string _enemyName;
        [SerializeField] private List<DialogEvent> _dialogEvents = new List<DialogEvent>();

        public string EnemyName => _enemyName;

        public EnemyDialog()
        {
            foreach (DialogEventType type in Enum.GetValues(typeof(DialogEventType)))
            {
                for (int i = 0; i < 2; i++)
                {
                    var d = new DialogEvent();
                    d.SetType(type);
                    _dialogEvents.Add(d);
                }
            }
        }

        public string GetDialog(DialogEventType type, LocalizationLanguage lang)
        {
            var validDialogs = _dialogEvents.Where(d => d.Type == type).ToList();

            if (validDialogs.Count <= 0) return "";

            validDialogs.Shuffle();
            return validDialogs.First().GetDialog(lang);
        }
    }
}