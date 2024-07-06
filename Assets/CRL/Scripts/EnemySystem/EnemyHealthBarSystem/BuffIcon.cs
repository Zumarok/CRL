using System;
using Crux.CRL.AbilitySystem;
using Crux.CRL.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuffIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Editor Fields

    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _duration;
    [SerializeField] private TextMeshProUGUI _stacks;
    [SerializeField] private RectTransform _rectTransform;

    #endregion

    #region Private Fields

    private TempEffect _tempEffect;
    #endregion

    #region Properties

    public RectTransform RectTransform => _rectTransform;
    public int Counter { get; private set; }

    #endregion

    #region Unity Callbacks
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        SmallInfoWindow.Instance.ShowTempEffectShortDesc(_tempEffect);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SmallInfoWindow.Instance.ShowTempEffectShortDesc();
    }

    #endregion

    #region Public Methods

    public void Init(TempEffect tempEffect)
    {
        _tempEffect = tempEffect;
        _icon.sprite = _tempEffect.Icon;
        Counter = _tempEffect.Duration;
        _duration.text = _tempEffect.Ability.HasDuration ? _tempEffect.Duration.ToString() : "";
        _stacks.text = _tempEffect.Ability.IsStackingEffect ? _tempEffect.StackCount.ToString() : "";
        gameObject.SetActive(true);
    }
    
    public void UpdateCounters()
    {
        _duration.text = _tempEffect.Ability.HasDuration ? Counter.ToString() : "";
        _stacks.text = _tempEffect.Ability.IsStackingEffect ? _tempEffect.StackCount.ToString() : "";
    }

    public void DecrementCounter()
    {
        if (_tempEffect.Ability.HasDuration)
            Counter--;
        UpdateCounters();
    }

    #endregion

    #region Private Methods



    #endregion
}
