using Crux.CRL.Localization;
using System;
using UnityEngine;

namespace Crux.CRL.DialogSystem
{
    [Serializable]
    public class DialogEvent
    {
        [SerializeField] private DialogEventType _eventType;
        [SerializeField] private LocalizedString _dialog;

        public DialogEventType Type => _eventType;

        public string GetDialog(LocalizationLanguage lang)
        {
            return _dialog.TryGetValue(lang, out var value) ? value : "";
        }

        public void SetType(DialogEventType type)
        {
            _eventType = type;
        }

    }

}
