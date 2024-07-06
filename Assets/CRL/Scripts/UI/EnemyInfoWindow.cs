using Crux.CRL.EnemySystem;
using Crux.CRL.CardSystem;
using Crux.CRL.CombatSystem;
using Crux.CRL.DataSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Crux.CRL.UI
{
    public class EnemyInfoWindow : SoloWindow<EnemyInfoWindow>, IPointerEnterHandler, IPointerExitHandler
    {

        #region Editor Fields

        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private AbilityListItem[] _abilities = new AbilityListItem[Constants.MAX_BUFFS];
        [SerializeField] private AbilityListItem[] _buffs = new AbilityListItem[Constants.MAX_BUFFS];
        [SerializeField] private AbilityListItem[] _debuffs = new AbilityListItem[Constants.MAX_DEBUFFS];

        #endregion

        #region Private Fields

        private Enemy _displayedEnemy;
        private bool _isMouseOver;
        private RectTransform _rectTrans;
        //private bool _isShowing;

        #endregion

        #region Properties



        #endregion

        #region Unity Callbacks

        private void Start()
        {
            _rectTrans = GetComponent<RectTransform>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isMouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isMouseOver = false;
            ShowHideInfo();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show the window with the enemies information, if enemy is null, close the window.
        /// </summary>
        /// <param name="enemy"></param>
        public void ShowHideInfo(Enemy enemy = null)
        {
            if (PlayerUIManager.Instance.ShowingWindowType == typeof(CombatLogWindow)) return;
            if (PlayerUIManager.Instance.ShowingWindowType == typeof(CardPileWindow)) return;

            // if no enemy is passed in, or if an enemy is passed but the window is showing (probably an enemy under this window)
            if (enemy == null || PlayerUIManager.Instance.IsSoloWindowShowing)
            {
                // if the mouse is over the window or the displayed enemy, do nothing
                if (_isMouseOver || _displayedEnemy == null || _displayedEnemy.IsMouseOver) return;
                ToggleVisibility(false); // otherwise hide the window
                CombatManager.Instance.SetAllEnemyOutlines(EnemyOutlineState.Idle);

                return;
            }

            ToggleVisibility(true);
            enemy.SetEnemyOutline(EnemyOutlineState.Selected);
            SetWindowPos(enemy.Collider);
            SetWindowInfo(enemy);
        }

        /// <summary>
        /// Force the EnemyInfoWindow to close. 
        /// </summary>
        public void ForceClose()
        {
            ToggleVisibility(false);
        }

        #endregion

        #region Private Methods
        
        private void SetWindowPos(Collider col)
        {
            // Get the position of the collider in world space
            var colPos = col.transform.position;

            // Calculate the target position slightly to the right of the collider
            var tarPos = new Vector3(colPos.x, colPos.y, colPos.z + col.bounds.size.z * 0.35f);

            // Convert the target position to viewport coordinates
            var viewportPos = DataManager.Instance.MainCamera.WorldToViewportPoint(tarPos);

            // Convert the viewport coordinates to UI Camera screen coordinates
            var screenPos = DataManager.Instance.UICamera.ViewportToScreenPoint(viewportPos);

            // Get the size of the UI window while accounting for the UI scale factor
            var rect = _rectTrans.rect;
            var scaleFactor = PlayerUIManager.Instance.MainUICanvas.scaleFactor;
            var windowWidth = rect.width * scaleFactor;
            var windowHeight = rect.height * scaleFactor;

            if (screenPos.y + windowHeight > Screen.height)
            {
                // over height, manually set the Y pos
                screenPos = new Vector3(screenPos.x, Screen.height - windowHeight);
            }

            if (screenPos.x + windowWidth > Screen.width)
            {
                // over width, recalculate and manually set the x pos

                // Calculate the target position slightly to the right of the collider
                tarPos = new Vector3(colPos.x, colPos.y, colPos.z - col.bounds.size.z * 0.15f);

                // Convert the target position to viewport coordinates
                viewportPos = DataManager.Instance.MainCamera.WorldToViewportPoint(tarPos);

                // Convert the viewport coordinates to UI Camera screen coordinates
                screenPos = new Vector3(DataManager.Instance.UICamera.ViewportToScreenPoint(viewportPos).x - windowWidth, screenPos.y);
            }

            // Convert the screen coords to a world point
            var desiredWorldPos = DataManager.Instance.UICamera.ScreenToWorldPoint(screenPos);

            // Set the final position of the UI window
            transform.position = new Vector3(desiredWorldPos.x, desiredWorldPos.y, transform.position.z);
        }

        private void SetWindowInfo(Enemy enemy)
        {
            _displayedEnemy = enemy;
            _nameText.text = enemy.Name;
            _healthText.text = $"{enemy.CurrentHp}{(enemy.CurrentAbsorb > 0 ? "<sup><color=yellow>(" + enemy.CurrentAbsorb + ")</color></sup>" : "")}/{enemy.CurrentMaxHp}";

            var enemyAbilityCount = enemy.Abilities.Count;
            var activeAbilities = enemy.ActiveAbilities;
            var passiveAbilities = enemy.PassiveAbilities;
            
            for (int i = 0; i < _abilities.Length; i++) // set active abilities and clear the remainder
            {
                _abilities[i].Show(i < activeAbilities.Count ? activeAbilities[i] : null);
            }

            for (int i = 0; i < passiveAbilities.Count; i++)
            {
                _abilities[_abilities.Length - 1 - i].Show(passiveAbilities[i]);
            }

            using (var activeBuffs = enemy.ActiveBuffs.GetEnumerator())
            {
                _buffs[0].ShowTempEffect(activeBuffs.Current);
                for (int i = 1; i < _buffs.Length; i++)
                {
                    _buffs[i].ShowTempEffect(activeBuffs.MoveNext() ? activeBuffs.Current : null);
                }
            }
            
            using (var activeDebuffs = enemy.ActiveDebuffs.GetEnumerator())
            {
                _debuffs[0].ShowTempEffect(activeDebuffs.Current);
                for (int i = 1; i < _buffs.Length; i++)
                {
                    _debuffs[i].ShowTempEffect(activeDebuffs.MoveNext() ? activeDebuffs.Current : null);
                }
            }
        }

        #endregion


    }
}