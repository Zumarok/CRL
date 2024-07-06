using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Crux.CRL.UI;
using Crux.CRL.AbilitySystem;
using Crux.CRL.CardSystem;
using Crux.CRL.CombatSystem;
using Crux.CRL.DataSystem;
using Crux.CRL.EnemySystem;
using Crux.CRL.FloatingTextSystem;
using Crux.CRL.NotificationSystem;
using Crux.CRL.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Crux.CRL.DataSystem.Constants;
using static Crux.CRL.Utils.StaticUtils;
using Crux.CRL.SceneSystem;

public class PlayerUIManager : SingletonMonoBehaviour<PlayerUIManager>
{
    #region Editor Fields

    [SerializeField] private Canvas _mainUICanvas;
    [SerializeField] private CanvasScaler _canvasScaler;
    [SerializeField] private CanvasGroup _fadeCanvasGroup;

    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private TextMeshProUGUI _manaText;

    [SerializeField] private CombatLogWindow _combatLogWindow;
    [SerializeField] private FloorModWindow _floorModWindow;
    
    [SerializeField] private Image _absorbImage;
    
    [SerializeField] private RectTransform _buffsRectTransform;
    [SerializeField] private RectTransform _debuffsRectTransform;
    [SerializeField] private RectTransform _topUiRectTransform;

    [SerializeField] private BuffIcon[] _buffIcons;
    [SerializeField] private BuffIcon[] _debuffIcons;

    [SerializeField] private Image _expBarImage;
    [SerializeField] private TextMeshProUGUI _currentLevelText;
    [SerializeField] private TextMeshProUGUI _currentLevelExpText;
    [SerializeField] private TextMeshProUGUI _nextLevelExpText;
    [SerializeField] private TextMeshProUGUI _floorAndWaveText;

    [SerializeField] private TextMeshProUGUI _drawPileCountTxt;
    [SerializeField] private TextMeshProUGUI _discardPileCountTxt;

    [SerializeField] private Transform _healthFloatingTextSpot;
    [SerializeField] private Transform _manaFloatingTextSpot;

    #endregion

    #region Private Fields
    const float OrbScale = 0.0315f;

    private MeshRenderer _healthOrbMeshRenderer;
    private MeshRenderer _manaOrbMeshRenderer;
    private MeshRenderer _radiusCircleMeshRenderer;
    private Material _healthOrbMaterial;
    private Material _manaOrbMaterial;
    private Material _radiusCircleMaterial;

    private static readonly int Value = Shader.PropertyToID("_Value");
    
    private int _healthOrbTweenId;
    private int _manaOrbTweenId;
    private int _healthTextTweenId;
    private int _manaTextTweenId;
    private int _absorbTweenId;
    private int _absorbTextTweenId;
    private int _blackCanvasTweenId;
    
    private int _currentUiHealth;
    private int _currentUiMana;
    private int _currentUiAbsorb;

    private int _radiusTweenId;

    //private Color _radiusCircleColor;
    //private Color _radiusCircleEmitColor;

    private readonly Dictionary<TempEffect, BuffIcon> _activeTempEffectIcons = new Dictionary<TempEffect, BuffIcon>();
    
    #endregion

    #region Properties

    public Canvas MainUICanvas => _mainUICanvas;

    public bool IsSoloWindowShowing { get; private set; }
    public Type ShowingWindowType { get; private set; }

    public Vector3 HealthWorldPos => _healthText.transform.position;
    public Vector3 ManaWorldPos => _healthText.transform.position;
    public Vector3 BuffbarWorldPos => _buffsRectTransform.position;
    public Vector3 DebuffbarWorldPos => _debuffsRectTransform.position;
    #endregion

    #region Unity Callbacks

    protected override void Awake()
    {
        base.Awake();
        _healthOrbMeshRenderer = DataManager.Instance.HealthOrbMeshRenderer;
        _manaOrbMeshRenderer = DataManager.Instance.ManaOrbMeshRenderer;
        _radiusCircleMeshRenderer = DataManager.Instance.RadiusMeshRenderer;
        _healthOrbMaterial = _healthOrbMeshRenderer.material;
        _manaOrbMaterial = _manaOrbMeshRenderer.material;
        //_radiusCircleMaterial = _radiusCircleMeshRenderer.material;
        //_radiusCircleColor = _radiusCircleMaterial.color;
        //_radiusCircleEmitColor = _radiusCircleMaterial.GetColor(EmissionColor);
        NotificationManager.Instance.AddListener(NotiEvt.UI.ViewportResolutionChanged, OnResolutionChanged);
        NotificationManager.Instance.AddListener<Type>(NotiEvt.UI.OnSoloWindowShow, OnSoloWindowShow);
        NotificationManager.Instance.AddListener<Type>(NotiEvt.UI.OnSoloWindowHide, OnSoloWindowHide);
        OnResolutionChanged();
    }

    private void Start()
    {
        _fadeCanvasGroup.alpha = 1;
        _fadeCanvasGroup.blocksRaycasts = true;
        UpdateUIExp();
        FadeToBlackAndLoadLobbyScene(false);
    }

    private void OnDestroy()
    {
        NotificationManager.Instance.RemoveListener(NotiEvt.UI.ViewportResolutionChanged, OnResolutionChanged);
        NotificationManager.Instance.RemoveListener<Type>(NotiEvt.UI.OnSoloWindowShow, OnSoloWindowShow);
        NotificationManager.Instance.RemoveListener<Type>(NotiEvt.UI.OnSoloWindowHide, OnSoloWindowHide);
    }
    
    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the UI health orb and numbers.
    /// </summary>
    /// <param name="healDmgAmt">Amount to show in floating text. Positive numbers are heals, negative are damage.</param>
    /// <param name="shieldAmt">Shield amount to show in floating text.</param>
    /// <param name="abilityName">Name of the ability that healed or caused dmg.</param>
    public void UpdateUIHealth(int healDmgAmt = 0, int shieldAmt = 0, string abilityName = "")
    {
        if (healDmgAmt != 0)
            FloatingTextManager.Instance.ShowFloatingTextUICamera($"{(healDmgAmt > 0 ? "+" : "")}{healDmgAmt} <sup>{abilityName}</sup>",
                _healthFloatingTextSpot, healDmgAmt > 0 ? Color.green : Color.red, TextSize.Medium, healDmgAmt > 0 ? 0 : 0.04f, 2f, 25);
        if (shieldAmt > 0)
            FloatingTextManager.Instance.ShowFloatingTextUICamera($"+{shieldAmt} <sup>absorb</sup>",
                _healthFloatingTextSpot, Color.white, TextSize.Medium, 0.02f, 2f, 25);

        UpdateUIAbsorb();

        LeanTween.cancel(_healthOrbTweenId);
        LeanTween.cancel(_healthTextTweenId);

        _healthOrbTweenId = LeanTween.value(gameObject, SetHealthOrbPercent, _healthOrbMaterial.GetFloat(Value),
            CombatManager.Instance.Player.CurrentHpPercent, 0.25f).uniqueId;
        _healthTextTweenId = LeanTween.value(gameObject, SetHealthText, _currentUiHealth, CombatManager.Instance.Player.CurrentHp, 0.5f).uniqueId;
    }

    /// <summary>
    /// Updates the UI absorb orb and numbers.
    /// </summary>
    private void UpdateUIAbsorb()
    {
        LeanTween.cancel(_absorbTweenId);
        LeanTween.cancel(_absorbTextTweenId);

        _absorbTweenId = LeanTween.value(gameObject, v => _absorbImage.fillAmount = v,
            _absorbImage.fillAmount, CombatManager.Instance.Player.CurrentAbsorbPercent, 0.25f).uniqueId;
        _absorbTextTweenId = LeanTween.value(gameObject, SetAbsorbText, _currentUiAbsorb, CombatManager.Instance.Player.CurrentAbsorb, 0.5f).uniqueId;
    }

    public void UpdateUIExp()
    {
        var data = DataManager.Instance.PlayerData;
        var ci = DataManager.Instance.CulturalInfo;
        _currentLevelText.text = data.CurrentLevel.ToString();
        _currentLevelExpText.text = data.CurrentLevelExp.ToString("N0", ci);
        _nextLevelExpText.text = data.NextLevelStartExp.ToString("N0", ci);
        _expBarImage.fillAmount = (float) data.CurrentLevelExp / data.NextLevelStartExp;
    }

    public void UpdateUIFloorAndWave()
    {
        var floor = DataManager.Instance.TowerFloorInfo.Floor;
        var wave = EnemySpawnManager.Instance.CurrentWaveNumber;
        var waveCount = EnemySpawnManager.Instance.WaveCount;
        _floorAndWaveText.text = $"Floor {floor} - Wave {wave}/{waveCount}";
    }

    public void Refresh()
    {
        UpdateUIHealth();
        UpdateUIAbsorb();
        UpdateUIExp();
        // todo update mana
    }


    /// <summary>
    /// Sets the UI mana Orb and numbers as well as floating text for amount subtracted.
    /// <param name="amount">Amount to subtract (always a positive int)</param> 
    /// <param name="currentMana">The player's current mana value</param>
    /// </summary>
    public void SubtractMana(int amount, int currentMana)
    {
        FloatingTextManager.Instance.ShowFloatingTextUICamera($"-{amount} <sup>Mana</sup>", _manaFloatingTextSpot, Color.white, TextSize.Medium, 0, 2f, 15);
        SetUIMana(currentMana);
    }

    /// <summary>
    /// Sets the UI mana Orb and numbers as well as floating text for amount added.
    /// <param name="amount">Amount to add(always a positive int)</param> 
    /// <param name="currentMana">The player's current mana value</param>
    /// </summary>
    public void AddMana(int amount, int currentMana)
    {
        FloatingTextManager.Instance.ShowFloatingTextUICamera($"+{amount} <sup>Mana</sup>", _manaFloatingTextSpot, Color.white, TextSize.Medium, 0, 2f, 15);
        SetUIMana(currentMana);
    }

    /// <summary>
    /// Sets the UI Mana orb and numbers.
    /// </summary>
    /// <param name="currentMana"></param>
    public void SetUIMana(int currentMana)
    {
        LeanTween.cancel(_manaOrbTweenId);
        LeanTween.cancel(_manaTextTweenId);

        _manaOrbTweenId = LeanTween.value(gameObject, SetManaOrbPercent, _manaOrbMaterial.GetFloat(Value),
            (float)currentMana / CombatManager.Instance.Player.CurrentMaxMana, 0.25f).uniqueId;

        _manaTextTweenId = LeanTween.value(gameObject, SetManaText, _currentUiMana, currentMana, 0.5f).uniqueId;
    }

    /// <summary>
    /// Toggles the visibility of the radius circle.
    /// </summary>
    public void ShowRadiusCircle(bool show, Vector3 worldPos = default, float radius = 0f)
    {
        if (show)
        {
            var t = _radiusCircleMeshRenderer.transform;
            t.localScale = Vector3.one * (radius * 2.1f);
            t.position = new Vector3(worldPos.x, worldPos.y, worldPos.z);
            _radiusCircleMeshRenderer.enabled = true;
            //LeanTween.cancel(_radiusTweenId);
            //_radiusTweenId = LeanTween.value(gameObject, SetRadiusRingAlpha, 0.7f, 0.9f, 2f).setEaseInOutSine().setLoopPingPong(-1).uniqueId;
        }
        else
        {
            //LeanTween.cancel(_radiusTweenId);
            _radiusCircleMeshRenderer.enabled = false;
        }
    }

    public void InitTempEffectIcon(TempEffect tempEffect)
    {
        var isBuff = tempEffect is Buff;
        var buffIcon = isBuff
            ? _buffIcons.FirstOrDefault(i => !i.isActiveAndEnabled)
            : _debuffIcons.FirstOrDefault(i => !i.isActiveAndEnabled);
        if (buffIcon == null) return;

        buffIcon.Init(tempEffect);
        _activeTempEffectIcons.Add(tempEffect, buffIcon);
        FloatingTextManager.Instance.ShowFloatingTextUICamera("+" + tempEffect.Name, isBuff ? _buffsRectTransform : _debuffsRectTransform,
            isBuff ? ColorFromHex(CLC_BUFF): ColorFromHex(CLC_DEBUFF), TextSize.XSmall, 0.03f, 1.5f, 5);
    }
    
    public void DecrementTempEffectIcon(TempEffect effect)
    {
        _activeTempEffectIcons[effect].DecrementCounter();

        if (_activeTempEffectIcons[effect].Counter <= 0)
        {
            RemoveTempEffectIcon(effect);
        }
    }

    public void UpdateTempEffectIcon(TempEffect effect)
    {
        _activeTempEffectIcons[effect].UpdateCounters();
    }

    public void RemoveTempEffectIcon(TempEffect tempEffect)
    {
        var isBuff = tempEffect is Buff;
        _activeTempEffectIcons[tempEffect].gameObject.SetActive(false);
        _activeTempEffectIcons.Remove(tempEffect);
        FloatingTextManager.Instance.ShowFloatingTextUICamera("-" + tempEffect.Name, isBuff ? _buffsRectTransform : _debuffsRectTransform,
            Color.white, TextSize.Small, 0.025f, 1.5f, 5);
    }

    public void UpdateDrawPileCount(int count)
    {
        _drawPileCountTxt.text = count.ToString();
    }

    public void UpdateDiscardPileCount(int count)
    {
        _discardPileCountTxt.text = count.ToString();
    }

    /// <summary>
    /// Add the string to the Combat Log.
    /// </summary>
    /// <param name="msg">The message to add; Works with Rich Text tags.</param>
    public void CombatLog(string msg)
    {
        _combatLogWindow.AppendLog(msg);
    }

    #endregion

    #region Button Callbacks

    public void OnEndTurnClicked()
    {
        if (!CombatManager.Instance.Player.TurnActive) return;

        NotificationManager.Instance.SendNotification(NotiEvt.UI.CloseAllSoloWindows);
        CombatManager.Instance.Player.EndTurn();
    }

    public void OnCombatLogButtonClicked()
    {
        if (!CombatManager.Instance.Player.TurnActive) return;

        if (ShowingWindowType == typeof(CombatLogWindow))
        {
            _combatLogWindow.Hide();
        }
        else
        {
            _combatLogWindow.Show();
        }
    }

    public void OnCombatLogCloseClicked()
    {
        _combatLogWindow.Hide();
    }

    public void OnShowDrawPileClicked()
    {
        if (!CombatManager.Instance.Player.TurnActive) return;

        CombatManager.Instance.Player.CardManager.ShowCardPile(CardPileWindow.WindowType.DrawPile);
    }

    public void OnShowDiscardPileClicked()
    {
        if (!CombatManager.Instance.Player.TurnActive) return;

        CombatManager.Instance.Player.CardManager.ShowCardPile(CardPileWindow.WindowType.DiscardPile);
    }

    public void OnShowDeckClicked()
    {
        if (!CombatManager.Instance.Player.TurnActive) return;

        CombatManager.Instance.Player.CardManager.ShowCardPile(CardPileWindow.WindowType.Deck);
    }

    public void OnCharmsClicked()
    {

    }

    public void OnCurrencyClicked()
    {

    }

    public void OnPendingLootClicked()
    {

    }

    public void OnModsClicked()
    {
        if (ShowingWindowType == typeof(FloorModWindow))
        {
            _floorModWindow.Hide();
        }
        else
        {
            _floorModWindow.Show();
        }
    }

    public void OnSettingsClicked()
    {

    }
    #endregion

    #region Private Methods

    private void OnSoloWindowShow(Type type)
    {
        IsSoloWindowShowing = true;
        ShowingWindowType = type;
    }

    private void OnSoloWindowHide(Type type)
    {
        IsSoloWindowShowing = false;
        ShowingWindowType = null;
    }

    private void SetHealthOrbPercent(float value)
    {
        _healthOrbMaterial.SetFloat(Value, value);
    }

    private void SetHealthText(float value)
    {
        // todo consider using string builder here
        var intVal = Convert.ToInt32(value);
        _currentUiHealth = intVal;
        // _healthText.text = $"{intVal}/{CombatManager.Instance.Player.MaxHp}";
        _healthText.text = $"{intVal}{(_currentUiAbsorb > 0 ? "<sup><color=yellow>(" + _currentUiAbsorb + ")</color></sup>" : "")}/{CombatManager.Instance.Player.CurrentMaxHp}";
    }

    private void SetAbsorbText(float value)
    {
        var intVal = Convert.ToInt32(value);
        _currentUiAbsorb = intVal;
    }

    private void SetManaOrbPercent(float value)
    {
        _manaOrbMaterial.SetFloat(Value, value);
    }

    private void SetManaText(float value)
    {
        var intVal = Convert.ToInt32(value);
        _currentUiMana = intVal;
        _manaText.text = $"{intVal}/{CombatManager.Instance.Player.CurrentMaxMana}";
    }

    //private void SetRadiusRingAlpha(float value)
    //{
    //    //_radiusCircleMaterial.color = new Color(_radiusCircleColor.r, _radiusCircleColor.g, _radiusCircleColor.b, 1 - value);
    //    _radiusCircleMaterial.SetColor(EmissionColor,
    //        new Color(_radiusCircleEmitColor.r * value, _radiusCircleEmitColor.g * value, _radiusCircleEmitColor.b * value));
    //}

    private void OnResolutionChanged()
    {
        _canvasScaler.scaleFactor = Math.Min(DataManager.Instance.MainCamera.pixelWidth / 1920f, 1f);
        StartCoroutine(ResolutionUpdate());
    }

    private IEnumerator ResolutionUpdate()
    {
        yield return WaitFor.EndOfFrame;
        yield return WaitFor.EndOfFrame;
        var mainCam = DataManager.Instance.MainCamera;
        var uiCam = DataManager.Instance.UICamera;
        var healthOrbTrans = _healthOrbMeshRenderer.transform;
        var manaOrbTrans = _manaOrbMeshRenderer.transform;

        var hTxtPos = uiCam.WorldToViewportPoint(_healthText.transform.position);
        healthOrbTrans.position = mainCam.ViewportToWorldPoint(new Vector3(hTxtPos.x, hTxtPos.y, 1f));

        var mTxtPos = uiCam.WorldToViewportPoint(_manaText.transform.position);
        manaOrbTrans.position = mainCam.ViewportToWorldPoint(new Vector3(mTxtPos.x, hTxtPos.y, 1f));
        
        var scale = Math.Min(mainCam.aspect, MAX_ASPECT_RATIO) * OrbScale;
        healthOrbTrans.localScale = Vector3.one * scale;
        manaOrbTrans.localScale = Vector3.one * scale;

        // var topCenterPos = uiCam.ScreenToWorldPoint(new Vector3(uiCam.pixelWidth * 0.5f, uiCam.pixelHeight - _topUiRectTransform.rect.height * 0.5f));
        var topCenterPos = uiCam.ViewportToWorldPoint(new Vector3(0.5f, 1f));
        _topUiRectTransform.position = new Vector3(topCenterPos.x, topCenterPos.y, _topUiRectTransform.position.z);

        _healthOrbMeshRenderer.enabled = true;
        _manaOrbMeshRenderer.enabled = true;

        EnemyHealthBarManager.Instance.UpdateAllHealthBarPosition();
    }

    private void FadeToBlackAndLoadLobbyScene(bool toBlack)
    {
        LeanTween.cancel(_blackCanvasTweenId);
        Action onCompleteAction;


        if (toBlack)
        {
            _fadeCanvasGroup.blocksRaycasts = true;
            onCompleteAction = OnFadeToBlackComplete;
        }
        else
        {
            onCompleteAction = OnFadeFromBlackComplete;
        }

        _blackCanvasTweenId = LeanTween.alphaCanvas(_fadeCanvasGroup, toBlack ? 1 : 0, Constants.FADE_TO_BLACK_TIME)
            .setOnComplete(onCompleteAction).setDelay(toBlack ? 0f : 1f).uniqueId;
    }

    #endregion

    #region Events

    private void OnFadeToBlackComplete()
    {
        _fadeCanvasGroup.SetVisibilityAndInteractive(true);
        SceneLoader.Instance.LoadLobbyScene();
    }

    private void OnFadeFromBlackComplete()
    {
        _fadeCanvasGroup.SetVisibilityAndInteractive(false);
    }

    #endregion
}
