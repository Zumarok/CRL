using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crux.CRL.SlotReelsSystem
{
    public class SlotReelItem : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _valueText;

        private int _index;

        public void SetSprite(Sprite sprite) => _image.sprite = sprite;
        public void SetTitleText(string txt) => _titleText.text = txt;
        public void SetValueText(string txt) => _valueText.text = txt;
        public void SetIndex(int index) => _index = index;

    }
}
