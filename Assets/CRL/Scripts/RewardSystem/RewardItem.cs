using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crux.CRL.RewardSystem
{
    public class RewardItem : MonoBehaviour
    {
        #region Editor Fields

        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private RectTransform _rectTransform;

        #endregion

        #region Private Fields

        private RewardType _rewardType;

        #endregion

        #region Properties



        #endregion

        #region Unity Callbacks



        #endregion

        #region Public Methods

        public void Init(RewardType rewardType, float scaleDuration)
        {
            _rewardType = rewardType;
            _rectTransform.localScale = Vector3.zero;
            LeanTween.cancel(gameObject);
            LeanTween.scale(_rectTransform, Vector3.one, scaleDuration).setEaseOutBounce();
        }

        #endregion

        #region Private Methods



        #endregion
    }
}