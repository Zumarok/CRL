
using Crux.CRL.DataSystem;
using Crux.CRL.UI;
using Crux.CRL.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crux.CRL.CardSystem
{
    public class CardPreview : MonoBehaviour
    {
        #region Editor Fields

        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _cost;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _fxImage;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private BoxCollider _collider;

        #endregion

        #region Private Fields



        #endregion

        #region Properties

        public CardAbility CardAbility { get; private set; }

        #endregion

        #region Unity Callbacks

        private void OnMouseOver()
        {
            var linkIndex = TMP_TextUtilities.FindIntersectingLink(_description, Input.mousePosition, DataManager.Instance.UICamera);
            if (linkIndex < 0)
            {
                SmallInfoWindow.Instance.ShowKeywordTooltip(false, "");
            }
            else
            {
                var linkInfo = _description.textInfo.linkInfo[linkIndex];
                var id = linkInfo.GetLinkID();
                SmallInfoWindow.Instance.ShowKeywordTooltip(true, id);
            }
        }

        #endregion

        #region Public Methods

        public void InitCardPreview(CardAbility cardAbility = null)
        {
            _canvasGroup.SetVisibilityAndInteractive(cardAbility != null);
            if (cardAbility == null) return;

            CardAbility = cardAbility;
            _name.text = cardAbility.FormattedName;
            _cost.text = cardAbility.ManaCost.ToString();
            _description.text = cardAbility.LongDescription;
            _iconImage.sprite = cardAbility.IconSprite;

            if (cardAbility.CardFxMaterial == null) return;

            _fxImage.material = cardAbility.CardFxMaterial;
            _fxImage.enabled = true;
        }

        public void Show()
        {
            _canvasGroup.alpha = 1;
            _collider.enabled = true;
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0;
            _collider.enabled = false;
        }

        #endregion

        #region Private Methods



        #endregion
    }
}
