using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Crux.CRL.AbilitySystem;
using Crux.CRL.CombatSystem;
using Crux.CRL.DataSystem;
using Crux.CRL.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;

namespace Crux.CRL.CardSystem
{
    public class CardManager : MonoBehaviour
    {

        #region Properties

        public float CardZoomAmount => _cardZoomAmount;

        public bool AnyCardsFlipping => _cardPrefabs.Any(c => c.CardFlags.HasFlag(CardFlag.Flipping));
        public bool AnyCardsProcessing => _cardPrefabs.Any(c => c.CardFlags.HasFlag(CardFlag.AbilityProcessing));
        public bool AnyCardsTargeting => _cardPrefabs.Any(c=> c.CardFlags.HasFlag(CardFlag.Targeting));
        public int DrawPileCount => _drawPile.Count;
        public int DiscardPileCount => _discardPile.Count;

        #endregion

        #region Unity Editor Fields

        [SerializeField] private float _animDuration = 0.25f;
        [SerializeField, Range(1f, 2f)] private float _cardZoomAmount = 1.3f; 
        [SerializeField] private Transform _cardHandTransform;
        [SerializeField] private Card[] _cardPrefabs;

        [SerializeField] private Transform _targetingSplineTransform;
        [SerializeField] private SpriteShapeRenderer _spriteShapeRenderer;
        [SerializeField] private SpriteShapeController _spriteShapeController;
        [SerializeField] private RectTransform _arrowheadGroupRectTransform;
        [SerializeField] private RectTransform _arrowheadImageRectTransform;
        [SerializeField] private RectTransform _dmgPreviewRectTransform;
        [SerializeField] private CanvasGroup _arrowheadGroupCanvas;
        [SerializeField] private CanvasGroup _dmgPreviewCanvas;
        [SerializeField] private TextMeshProUGUI _dmgPreviewTxt;

        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _notSelectedColor;

        [SerializeField] private CardPileWindow _cardPileWindow; 

        #endregion

        #region Private Fields

        private Vector3 _handInitLocalPos;
        private bool _isEnemyUnderCursor;
        private int _dmgPreviewAlphaTweenId;
        /// <summary>
        /// A list of all CardAbilities in the player's deck.
        /// </summary>
        private readonly List<CardAbility> _deck = new List<CardAbility>(Constants.MAX_NUM_CARDS);
        /// <summary>
        /// Abilities available to be drawn.
        /// </summary>
        private readonly Queue<CardAbility> _drawPile = new Queue<CardAbility>(Constants.MAX_NUM_CARDS);
        /// <summary>
        /// Cards currently in the player's hand.
        /// </summary>
        private readonly List<CardAbility> _currentHand = new List<CardAbility>(Constants.CARD_HAND_SIZE);
        /// <summary>
        /// Abilities that have been played and will be reshuffled into the draw pile as needed.
        /// </summary>
        private readonly List<CardAbility> _discardPile = new List<CardAbility>(Constants.MAX_NUM_CARDS);
        /// <summary>
        /// Abilities that have been played that will not be added back to the draw pile when reshuffling.
        /// </summary>
        private readonly List<CardAbility> _banishedPile = new List<CardAbility>(Constants.MAX_NUM_CARDS / 2);
        /// <summary>
        /// Abilities that have been played and are removed from the draw pile until the end of combat.
        /// </summary>
        private readonly List<CardAbility> _removedPile = new List<CardAbility>(Constants.MAX_NUM_CARDS / 3);

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _handInitLocalPos = _cardHandTransform.localPosition;
            _cardHandTransform.localPosition = new Vector3(_handInitLocalPos.x, _handInitLocalPos.y - 1, _handInitLocalPos.z);

            _spriteShapeRenderer.color = _notSelectedColor;
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Moves the entire hand in or out of view and flags the cards visibility status.
        /// </summary>
        public IEnumerator ToggleHandVisibility(bool shouldShow)
        {
            if (shouldShow)
            {
                for (int i = 0; i < _cardPrefabs.Length; i++)
                {
                    _cardPrefabs[i].OnCardVisible();
                }
            }
            else
            {
                for (int i = 0; i < _cardPrefabs.Length; i++)
                {
                    _cardPrefabs[i].OnCardInvisible();
                }
            }

            LeanTween.moveLocalY(_cardHandTransform.gameObject, shouldShow ? _handInitLocalPos.y : _handInitLocalPos.y - 1f, _animDuration)
                .setEase(LeanTweenType.easeOutBack);
            yield return WaitFor.Seconds(_animDuration);
        }

        /// <summary>
        /// Add a brand new card to the player's deck.
        /// </summary>
        /// <param name="abilityName"></param>
        public void AddNewCardToDeck(AbilityName abilityName)
        {
            _deck.Add(new CardAbility(abilityName));
        }

        public void SetupDecksForNewCombat()
        {
            _drawPile.Clear();
            _discardPile.Clear();
            _banishedPile.Clear();
            _removedPile.Clear();
            _currentHand.Clear();

            _deck.Shuffle();
            foreach (var c in _deck) { _drawPile.Enqueue(c); }

            PlayerUIManager.Instance.UpdateDiscardPileCount(_discardPile.Count);
            PlayerUIManager.Instance.UpdateDrawPileCount(_drawPile.Count);
        }

        /// <summary>
        /// Draws the next card from the drawPile and adds it to the current hand's desired slot.
        /// If the draw pile is empty, the discard pile is shuffled and added to the draw pile first.
        /// </summary>
        /// <param name="cardSlot">The card slot to draw.</param>
        public IEnumerator DrawCard(int cardSlot)
        {
            if (_drawPile.Count == 0)
            {
                if (_discardPile.Count == 0)
                {
                    CombatManager.Instance.Player.ApplyDamage(new DamageInfo(ushort.MaxValue, default, default, ""));
                    yield break;
                }

                _discardPile.Shuffle();
                foreach (var c in _discardPile) { _drawPile.Enqueue(c); }
                _discardPile.Clear();
            }

            var cardAbility = _drawPile.Dequeue();
            PlayerUIManager.Instance.UpdateDiscardPileCount(_discardPile.Count);
            PlayerUIManager.Instance.UpdateDrawPileCount(_drawPile.Count);
            _currentHand.Add(cardAbility);
            InitCard(cardSlot, cardAbility);
            yield return ToggleCardFlipState(cardSlot, true);
        }

        /// <summary>
        ///  Draws a card from the draw pile for the desired slots.
        /// </summary>
        /// <param name="cardSlots">The card slot(s) to draw.</param>
        public IEnumerator DrawCards(int[] cardSlots)
        {
            foreach (var cardSlot in cardSlots)
            {
                yield return DrawCard(cardSlot);
            }
        }

        public IEnumerator DiscardAllCards()
        {
            return _cardPrefabs.Select(DiscardCard).GetEnumerator();
        }

        public IEnumerator DiscardAllCardNoDelay()
        {
            for (int i = 0; i < _cardPrefabs.Length; i++)
            {
                if (i == _cardPrefabs.Length - 1)
                    yield return DiscardCard(_cardPrefabs[i]);
                else
                    StartCoroutine(DiscardCard(_cardPrefabs[i]));
            }
        }

        /// <summary>
        /// Removes the card from the current hand and adds it to the discard pile.
        /// </summary>
        public IEnumerator DiscardCard(Card card)
        {
            _currentHand.Remove(card.Ability);
            _discardPile.Add(card.Ability);
            PlayerUIManager.Instance.UpdateDiscardPileCount(_discardPile.Count);
            PlayerUIManager.Instance.UpdateDrawPileCount(_drawPile.Count);
            card.Zoom(false);
            yield return ToggleCardFlipState(card.CardIndex, false);
        }
        
        public IEnumerator FlipAllCards(bool faceUp)
        {
            for (var i = 0; i < _cardPrefabs.Length; i++)
            {
                var card = _cardPrefabs[i];
                if (faceUp && !card.CardFlags.HasFlag(CardFlag.FaceUp) || !faceUp && card.CardFlags.HasFlag(CardFlag.FaceUp))
                {
                    yield return ToggleCardFlipState(i, faceUp);
                }
            }
        }

        public IEnumerator ToggleCardFlipState(int cardSlot, bool isFaceUp)
        {
            var card = _cardPrefabs[cardSlot];

            _cardPrefabs[cardSlot].OnCardFlipping();

            LeanTween.rotateLocal(card.gameObject, new Vector3(0, isFaceUp ? 180 : 0, 0), _animDuration).setEase(LeanTweenType.easeInCirc);
            yield return WaitFor.Seconds(_animDuration);

            _cardPrefabs[cardSlot].OnCardDoneFlipping();

            if (isFaceUp)
                _cardPrefabs[cardSlot].OnCardFaceUp();
            else
                _cardPrefabs[cardSlot].OnCardFaceDown();
        }

        public void ShowTargetingArrow(bool shouldShow, float cardLocalXPos = 0f)
        {
            var splineTransform = _targetingSplineTransform;
            var splineLocalPos = splineTransform.localPosition;
            splineLocalPos = new Vector3(cardLocalXPos, splineLocalPos.y, splineLocalPos.z);
            splineTransform.localPosition = splineLocalPos;
            _spriteShapeController.enabled = shouldShow;
            _arrowheadGroupCanvas.alpha = shouldShow ? 1 : 0;
        }

        public void UpdateTargetingArrowPos(CardAbility ability)
        {
            var ssMousePos = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
            var point = DataManager.Instance.UICamera.ViewportToWorldPoint(ssMousePos);
            var localPoint = _targetingSplineTransform.InverseTransformPoint(point);
            var finalPos = new Vector3(localPoint.x, localPoint.y);
            _spriteShapeController.spline.SetPosition(0, finalPos);

            // Calculate the tangent vector at point 0
            var tangent = 2 * _spriteShapeController.spline.GetPosition(1) - _spriteShapeController.spline.GetPosition(0);
            var angleRadians = Mathf.Atan2(tangent.y, tangent.x);
            var angleDegrees = angleRadians * Mathf.Rad2Deg;
            var adjustedAngle = angleDegrees + 90;
            _arrowheadGroupRectTransform.position = new Vector2(point.x, point.y);
            _arrowheadImageRectTransform.localRotation = Quaternion.Euler(0, 0, adjustedAngle);
            _dmgPreviewRectTransform.localRotation = Quaternion.Euler(0, 0, 90 - adjustedAngle);

            var enemy = CombatManager.Instance.GetEnemyAtCursor();
            var overEnemy = enemy != null;
            if (overEnemy == _isEnemyUnderCursor) return;

            _isEnemyUnderCursor = overEnemy;
            //_spriteShapeRenderer.color = _isEnemyUnderCursor ? _selectedColor : _notSelectedColor;

            if (ability == null || (!ability.Type.HasFlag(AbilityType.DD) && !ability.Type.HasFlag(AbilityType.DOT))) return;

            _dmgPreviewTxt.text =
                DamageInfo.CalculateFinalDamage(ability.DDValue, CombatManager.Instance.Player, enemy) +
                DamageInfo.CalculateFinalDamage(ability.DoTValue, CombatManager.Instance.Player, enemy) +
                (ability.IsMultiHit ? $"<sub> X </sub>{ability.NumOfHits}" : "");

            LeanTween.cancel(_dmgPreviewAlphaTweenId);
            LeanTween.alphaCanvas(_dmgPreviewCanvas, overEnemy ? 1 : 0, Constants.WINDOW_FADE_TIME);
            //_dmgPreviewCanvas.alpha = overEnemy ? 1 : 0;
        }

        /// <summary>
        /// Card will move to the mouse position when this is called. (Call per frame to follow)
        /// </summary>
        /// <param name="cardTransform"></param>
        public void CardFollowMouse(Transform cardTransform)
        {
            var ssMousePos = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
            var point = DataManager.Instance.UICamera.ViewportToWorldPoint(ssMousePos);
            var localPoint = cardTransform.transform.parent.InverseTransformPoint(point);
            cardTransform.localPosition = new Vector3(localPoint.x, localPoint.y);
        }

        public void ShowCardPile(CardPileWindow.WindowType type)
        {
            switch (type)
            {
                case CardPileWindow.WindowType.None:
                    _cardPileWindow.ShowWindow();
                    break;
                case CardPileWindow.WindowType.Deck:
                    _cardPileWindow.ShowWindow(_deck);
                    break;
                case CardPileWindow.WindowType.DrawPile:
                    _cardPileWindow.ShowWindow(_drawPile, CardPileWindow.WindowType.DrawPile);
                    break;
                case CardPileWindow.WindowType.DiscardPile:
                    _cardPileWindow.ShowWindow(_discardPile, CardPileWindow.WindowType.DiscardPile);
                    break;
                case CardPileWindow.WindowType.BanishedPile:
                    _cardPileWindow.ShowWindow(_banishedPile, CardPileWindow.WindowType.BanishedPile);
                    break;
                case CardPileWindow.WindowType.RemovedPile:
                    _cardPileWindow.ShowWindow(_removedPile, CardPileWindow.WindowType.RemovedPile);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        #endregion

        #region Private Functions

        private void InitCard(int cardSlot, CardAbility ability)
        {
            _cardPrefabs[cardSlot].InitCard(ability, cardSlot);
        }

        #endregion

        #region Card Input Handlers

        public void OnCardClicked(int cardSlot)
        {

        }

        #endregion

    }
}
