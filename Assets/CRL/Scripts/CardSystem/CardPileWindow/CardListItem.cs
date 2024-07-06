using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Crux.CRL.CardSystem
{
    public class CardListItem : MonoBehaviour, IPointerEnterHandler
    {
        #region Editor Fields

        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _manaCostText;
        [SerializeField] private CardPreview _cardPreview;

        #endregion

        #region Private Fields

        private CardAbility _cardAbility;

        #endregion

        #region Properties



        #endregion

        #region Unity Callbacks

        public void OnPointerEnter(PointerEventData eventData)
        {
            _cardPreview.InitCardPreview(_cardAbility);
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// If an ability is passed in, the AbilityInfo prefab will be shown, otherwise it will be disabled.
        /// </summary>
        /// <param name="cardAbility">The card to show.</param>
        public void Show(CardAbility cardAbility = null)
        {
            gameObject.SetActive(cardAbility != null);
            if (cardAbility == null) return;

            _cardAbility = cardAbility;
            _manaCostText.text = cardAbility.ManaCost.ToString();
            _nameText.text = cardAbility.FormattedName; 
        }

        #endregion

        #region Private Methods



        #endregion
    }
}
