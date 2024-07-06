using Crux.CRL.AbilitySystem;
using Crux.CRL.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Editor Fields

    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;

    #endregion

    #region Private Fields

    private IAbility _ability;

    #endregion
    
    #region Unity Callbacks

    public void OnPointerEnter(PointerEventData eventData)
    {
        SmallInfoWindow.Instance.ShowAbilityShortDesc(_ability);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SmallInfoWindow.Instance.ShowAbilityShortDesc();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// If an ability is passed in, the AbilityInfo prefab will be shown, otherwise it will be disabled.
    /// </summary>
    /// <param name="ability">The ability to show.</param>
    public void Show(IAbility ability = null)
    {
        gameObject.SetActive(ability != null);
        if (ability == null) return;

        _ability = ability;
        _iconImage.sprite = ability.IconSprite;
        _nameText.text = ability.FormattedName; // link to shortDesc
    }

    public void ShowTempEffect(TempEffect effect = null)
    {
        gameObject.SetActive(effect != null);
        if (effect == null) return;

        _ability = effect.Ability;
        _iconImage.sprite = _ability.IconSprite;
        _nameText.text = _ability.FormattedName + (_ability.IsStackingEffect ? " x " + effect.StackCount : ""); // link to shortDesc
    }

    #endregion

}
