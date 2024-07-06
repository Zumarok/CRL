using System;
using Crux.CRL.AbilitySystem;
using Crux.CRL.DataSystem;
using Crux.CRL.Utils;
using TMPro;
using UnityEngine;

namespace Crux.CRL.UI
{
    public class SmallInfoWindow : SingletonMonoBehaviour<SmallInfoWindow>
    {

        #region Editor Fields

        [SerializeField] private Transform _transform;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        #endregion
        
        #region Unity Callbacks


        #endregion

        #region Public Methods

        public void ShowKeywordTooltip(bool show, string keywordId)
        {
            if (Convert.ToBoolean(_canvasGroup.alpha) == show) return;
            _canvasGroup.SetVisibilityAndInteractive(show);

            if (!show) return;

            var kw = DataManager.Instance.GetLocalizedKeyword(keywordId);
            var mousePos = DataManager.Instance.UICamera.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f, 0.5f);
            _transform.localPosition = new Vector3(mousePos.x * DataManager.Instance.UICamera.pixelWidth,
                mousePos.y * DataManager.Instance.UICamera.pixelHeight);
            _titleText.text = kw.name;
            _descriptionText.text = kw.description;
        }

        public void ShowAbilityShortDesc(IAbility ability = null)
        {
            _canvasGroup.SetVisibilityAndInteractive(ability != null);

            if (ability == null) return;

            var pos = DataManager.Instance.UICamera.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f, 0.5f);
            //var pos = DataManager.Instance.MainCamera.WorldToViewportPoint(enemy.transform.position) - new Vector3(0.5f, 0.5f, 0.5f);
            transform.localPosition = new Vector3(pos.x * DataManager.Instance.UICamera.pixelWidth, pos.y * DataManager.Instance.UICamera.pixelHeight);
            _titleText.text = ability.FormattedName;
            _descriptionText.text = ability.ShortDescription();
        }

        public void ShowTempEffectShortDesc(TempEffect effect = null)
        {
            _canvasGroup.SetVisibilityAndInteractive(effect != null);

            if (effect == null) return;

            var pos = DataManager.Instance.UICamera.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f, 0.5f);
            //var pos = DataManager.Instance.MainCamera.WorldToViewportPoint(enemy.transform.position) - new Vector3(0.5f, 0.5f, 0.5f);
            transform.localPosition = new Vector3(pos.x * DataManager.Instance.UICamera.pixelWidth, pos.y * DataManager.Instance.UICamera.pixelHeight);
            _titleText.text = effect.Name;
            _descriptionText.text = effect.Ability.ShortDescription(effect.Sender, effect.Receiver);
        }

        public void ShowReelTooltip(Ability reelAbility = null, Transform reelTransform = null)
        {
            _canvasGroup.SetVisibilityAndInteractive(reelAbility != null);

            if (reelAbility == null || reelTransform == null) return;

            var pos = DataManager.Instance.UICamera.ScreenToViewportPoint(reelTransform.position) -
                      new Vector3(0.5f, 0.43f, 0.5f);
            //var pos = DataManager.Instance.MainCamera.WorldToViewportPoint(enemy.transform.position) - new Vector3(0.5f, 0.5f, 0.5f);
            transform.localPosition = new Vector3(pos.x * DataManager.Instance.UICamera.pixelWidth,
                pos.y * DataManager.Instance.UICamera.pixelHeight);
            //_titleText.text = reelAbility.FormattedName;
            _descriptionText.text = reelAbility.LongDescription;
        }

        #endregion

        #region Private Methods



        #endregion
    }
}