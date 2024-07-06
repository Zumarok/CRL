using System;
using System.Collections.Generic;
using Crux.CRL.DataSystem;
using Crux.CRL.UI;
using RotaryHeart.Lib.SerializableDictionary;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Crux.CRL.CardSystem
{
    public class CardPileWindow : SoloWindow<CardPileWindow>, IPointerExitHandler
    {
        #region Enums

        public enum WindowType
        {
            None,
            Deck,
            DrawPile,
            DiscardPile,
            BanishedPile,
            RemovedPile
        }

        public enum StringName
        {
            Title
        }

        #endregion

        #region Editor Fields

        [SerializeField] private CanvasGroup _cardsCanvasGroup;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private CardListItem[] _cardListItems;
        [SerializeField] private CardPreview _cardPreview;
        [SerializeField] private WindowPosDictionary _windowLocalPos;

        #endregion

        #region Private Fields
        
        private Dictionary<WindowType, Dictionary<StringName, string>> _localizedStringIds = new Dictionary<WindowType, Dictionary<StringName, string>>
        {
            {
                WindowType.Deck, new Dictionary<StringName, string>
                {
                    {StringName.Title, "Window_cpw_deck_title"}
                }
            },
            {
                WindowType.DrawPile, new Dictionary<StringName, string>
                {
                    {StringName.Title, "Window_cpw_draw_title"}
                }
            },
            {
                WindowType.DiscardPile, new Dictionary<StringName, string>
                {
                    {StringName.Title, "Window_cpw_discard_title"}
                }
            },
            {
                WindowType.BanishedPile, new Dictionary<StringName, string>
                {
                    {StringName.Title, "Window_cpw_banished_title"}
                }
            },
            {
                WindowType.RemovedPile, new Dictionary<StringName, string>
                {
                    {StringName.Title, "Window_cpw_removed_title"}
                }
            }
        };

        #endregion
        
        #region Public Methods

        public void ShowWindow(IEnumerable<CardAbility> cardData = null, WindowType windowType = WindowType.Deck)
        {
            if (cardData == null || PlayerUIManager.Instance.ShowingWindowType == typeof(CardPileWindow))
            {
                ToggleVisibility(false);
                return;
            }

            ToggleVisibility(true);

            transform.localPosition = new Vector3(_windowLocalPos[windowType].x, _windowLocalPos[windowType].y, transform.localPosition.z);
            _titleText.text = DataManager.Instance.GetLocalizedString(_localizedStringIds[windowType][StringName.Title]);
            
            var count = 0;
            // add the data for each card to a card list item, enable and show the first entry in the preview
            foreach (var card in cardData) 
            {
                _cardListItems[count].Show(card);
                if (count == 0)
                {
                    _cardPreview.InitCardPreview(card);
                    _cardPreview.Show();
                }

                count++;
            }

            
            if (count == 0)  // there were no cards in the card data, hide the preview card
            {
                _cardPreview.Hide();
            }

            // hide the unused list items
            for (; count < _cardListItems.Length; count++)
            {
                _cardListItems[count].Show();
            }
            
        }
        
        protected override void OnHide()
        {
            base.OnHide();
            _cardPreview.Hide();
        }

        #endregion

        #region Interface Methods

        public void OnPointerExit(PointerEventData eventData)
        {
            ShowWindow();
        }

        #endregion

    }

    [Serializable]
    public class WindowPosDictionary : SerializableDictionaryBase<CardPileWindow.WindowType, Vector2>
    {
    }
}