using Crux.CRL.DataSystem;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crux.CRL.UI
{
    public class CombatLogWindow : SoloWindow<CombatLogWindow>
    {
        [SerializeField] private TextMeshProUGUI _combatLogText;
        [SerializeField] private ScrollRect _scrollRect;

        private int _scaleTweenId;
        private readonly StringBuilder _combatLogBuilder = new StringBuilder();

        public void AppendLog(string msg)
        {
            _combatLogBuilder.AppendLine(msg);
            if (IsShowing)
            {
                _combatLogText.text = _combatLogBuilder.ToString();
                //Canvas.ForceUpdateCanvases();  // Ensure the layout is updated before changing the scrollbar value
                _scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        public void ClearLog()
        {
            _combatLogBuilder.Clear();
            _combatLogText.text = "";
        }

        public override void Show()
        {
            _combatLogText.text = _combatLogBuilder.ToString();
            //canvasGroup.transform.localScale = Vector3.zero;

            //LeanTween.cancel(_scaleTweenId);
            //_scaleTweenId = LeanTween.scale(canvasGroup.gameObject, Vector3.one, Constants.WINDOW_FADE_TIME).setEaseInOutBounce().uniqueId;
            //ToggleVisibility(true);
            base.Show();
        }

        public override void Hide()
        {
            _combatLogText.text = "";
            //LeanTween.cancel(_scaleTweenId);
            //_scaleTweenId = LeanTween.scale(canvasGroup.gameObject, Vector3.zero, Constants.WINDOW_FADE_TIME).uniqueId;
            //ToggleVisibility(false);
            base.Hide();
        }

        protected override void OnShow()
        {
            base.OnShow();
            Canvas.ForceUpdateCanvases();  // Ensure the layout is updated before changing the scrollbar value
            _scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
