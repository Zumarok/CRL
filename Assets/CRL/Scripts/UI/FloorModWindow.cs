using Crux.CRL.DataSystem;
using Crux.CRL.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class FloorModWindow : SoloWindow<FloorModWindow>, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI _positive;
    [SerializeField] private TextMeshProUGUI _negative;
    [SerializeField] private TextMeshProUGUI _neutral;

    protected override void OnShow()
    {
        base.OnShow();

        _positive.text = DataManager.Instance.TowerFloorInfo.PositiveModString;
        _negative.text = DataManager.Instance.TowerFloorInfo.NegativeModString;
        _neutral.text = DataManager.Instance.TowerFloorInfo.NeutralModString;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Hide();
    }
}
