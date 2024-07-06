using System;
using System.Collections.Generic;
using System.Globalization;
using Crux.CRL.AbilitySystem;
using Crux.CRL.CombatSystem;
using Crux.CRL.DialogSystem;
using Crux.CRL.FloorNavigationSystem;
using Crux.CRL.KeywordSystem;
using Crux.CRL.Localization;
using Crux.CRL.SaveSystem;
using Crux.CRL.Utils;
using UnityEngine;

namespace Crux.CRL.DataSystem
{
    [DefaultExecutionOrder(-1)]
    public class DataManager : SingletonMonoBehaviour<DataManager>
    {
        #region Constants

        /// <summary>
        /// Duration for the fade effect on UI elements when they disappear.
        /// </summary>
        public const float UI_FADE_DURATION = 0.2f;

        #endregion

        #region Serialized Fields

        [SerializeField] private AbilitiesData    _abilitiesData;
        [SerializeField] private LocalizationData _localizationData;
        [SerializeField] private DialogScriptObject _dialogData;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Camera _uiCamera;
        [SerializeField] private MeshRenderer _healthOrbMeshRenderer;
        [SerializeField] private MeshRenderer _manaOrbMeshRenderer;
        [SerializeField] private MeshRenderer _radiusMeshRenderer;
        [SerializeField] private GameObject _singletonGamePrefab;
        [SerializeField] private GameObject _singletonLobbyPrefab;

        #endregion

        #region Private Fields

        private AbilitiesDictionary _abilities;
        private LocalizationLanguage _selectedLanguage;
        private CultureInfo _cultureInfo;
        private KeywordParser _keywordParser = new KeywordParser();
        private List<Ability> _reelAbilities = new List<Ability>();

        private int _activeFloor;
        #endregion

        #region Properties

        public Camera MainCamera => _mainCamera;
        public Camera UICamera => _uiCamera;
        public MeshRenderer HealthOrbMeshRenderer => _healthOrbMeshRenderer;
        public MeshRenderer ManaOrbMeshRenderer => _manaOrbMeshRenderer;
        public MeshRenderer RadiusMeshRenderer => _radiusMeshRenderer;
        public CultureInfo CulturalInfo => _cultureInfo;
        public GameState GameState { get; private set; }
        public PlayerData PlayerData => GameState.PlayerData;
        public RunData RunData => GameState.RunData; 
        public TowerFloorInfo TowerFloorInfo { get; private set; } = new TowerFloorInfo();
        public IReadOnlyList<Ability> ReelAbilities=> _reelAbilities;
        
        #endregion

        #region Unity Callbacks

        protected override void Awake()
        {
            base.Awake();
            _abilities = new AbilitiesDictionary();

            for (int i = 0, c = _abilitiesData.abilities.Count; i < c; i++)
            {
                var ability = _abilitiesData.abilities[i];
                _abilities.Add(ability.AbilityName, ability);

                if (ability.IsReelMod)
                    _reelAbilities.Add(ability);
            }

            SetLanguage(LocalizationLanguage.EnglishUS);
            //_localizationItems = _localizationData.localizationItems.ToDictionary();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a reference to an abilities data.
        /// </summary>
        public Ability GetAbilityData(AbilityName abilityName) => _abilities.Ability(abilityName);
        

        /// <summary>
        /// Get the localized string for the selected system language.
        /// </summary>
        public string GetLocalizedString(string id)
        {
            try
            {
                return _localizationData.localizedStrings[id][_selectedLanguage];
            }
            catch (Exception e)
            {
                Debug.LogError(id + ": " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Format a string, replacing delimiter-separated values with actual values.
        /// Also searches for Keywords and replaces them with localized keyword names.
        /// </summary>
        /// <param name="id">The id of the string to be parsed, in the LocalizationData.</param>
        /// <param name="ability">The ability this string is referencing for values.</param>
        /// <param name="abilityOwner">If included, the value will include caster's modifiers in the calculation. </param>
        /// <param name="target">If provided, the value will include the target's modifiers in the calculation.</param>
        /// <returns>(string parsedString, List&lt;Keyword&gt; keywordList) where parsedString is the formatted string for output and keywordList has all relevant keywords.</returns>
        public (string parsedString, List<Keyword> keywordList) GetLocalizedValueAndKeywordString(string id, Ability ability, Combatant abilityOwner = null, Combatant target = null)
        {
            try
            {
                return _keywordParser.ParseKeywordString(GetLocalizedString(id), ability, _cultureInfo, abilityOwner, target);
            }
            catch (Exception e)
            {
                Debug.LogError(id + ": " + e.Message);
                throw;
            }
        }

        public Keyword GetLocalizedKeyword(string id)
        {
            try
            {
                return _keywordParser.GetKeyword(id);
            }
            catch (Exception e)
            {
                Debug.LogError(id + ": " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets a random dialog line for this enemy if one exists. Otherwise, returns and empty string.
        /// </summary>
        /// <param name="enemyName">The name of the enemy.</param>
        /// <param name="type">The type of event triggering this dialog.</param>
        /// <returns>The localized dialog, or an empty string.</returns>
        public string GetLocalizedDialog(string enemyName, DialogEventType type)
        {
            return _dialogData.GetDialog(enemyName, type, _selectedLanguage);
        }

        public void SetLanguage(LocalizationLanguage language)
        {
            _selectedLanguage = language;
            switch (language)
            {
                case LocalizationLanguage.EnglishUS:
                    _cultureInfo = new CultureInfo("en-US");
                    break;
                case LocalizationLanguage.ChineseS:
                    _cultureInfo = new CultureInfo("zh-CN");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(language), language, null);
            }
            
        }

        public void SetGameState(GameState gameState)
        {
            GameState = gameState;
        }

        public void SpawnGameSingleton()
        {
            Instantiate(_singletonGamePrefab);
        }

        public void SpawnLobbySingleton()
        {
            Instantiate(_singletonLobbyPrefab);
        }

        #endregion

    }
}
