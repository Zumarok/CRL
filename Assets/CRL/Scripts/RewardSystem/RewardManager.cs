using System;
using System.Collections;
using Crux.CRL.CombatSystem;
using Crux.CRL.DataSystem;
using Crux.CRL.SaveSystem;
using Crux.CRL.SceneSystem;
using Crux.CRL.SlotReelsSystem;
using Crux.CRL.UI;
using Crux.CRL.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crux.CRL.RewardSystem
{
    public class RewardManager : SoloWindow<RewardManager>
    {
        #region Editor Fields

        [SerializeField, Range(0, 5)] private float _textAnimDuration = 0.5f;
        [SerializeField, Range(0, 1)] private float _textScaleDuration = 0.25f;
        [SerializeField, Range(0, 2)] private float _itemScaleDuration = 1f;

        [SerializeField] private TextMeshProUGUI _expRewardText;
        [SerializeField] private TextMeshProUGUI _goldRewardText;

        [SerializeField] private RewardItem _rewardItemPrefab;
        [SerializeField] private RectTransform _rewardContainer;
        [SerializeField] private ScrollRect _scrollRect;

        [SerializeField] private Button _doneButton;

        #endregion

        #region Private Fields

        private int _displayedGoldReward;
        private int _finalGoldReward;

        private int _displayedExpReward;
        private int _finalExpReward;

        private int _textTweenId;
        
        #endregion

        #region Properties
        
        #endregion

        #region Unity Callbacks

        #endregion

        #region Public Methods / Button Callbacks

        public void GenerateRewards(int tier)
        {
            _displayedExpReward = 0;
            _displayedGoldReward = 0;

            _expRewardText.text = "0";
            _goldRewardText.text = "0";

            _finalExpReward = 900;   // todo calculate these
            _finalGoldReward = DataManager.Instance.GameState.RunData.TutorialCompleted
                ? 10
                : SlotReelsManager.Instance.CalculateSpinCost(2);
        }

        public void ShowRewardWindow()
        {
            ToggleVisibility(true);
            StartCoroutine(PlayRewardAnims());
        }

        public void OnDoneButtonPressed()
        {
            ToggleVisibility(false);
            var runData = DataManager.Instance.GameState.RunData;
            if (runData.CurrentTowerFloor > runData.HighestFloorDefeated)
                runData.HighestFloorDefeated = runData.CurrentTowerFloor;

            runData.CurrentGold += _finalGoldReward;

            SaveManager.Instance.SaveGame();
            SceneLoader.Instance.LoadLobbyScene();
        }

        #endregion

        #region Private Methods

        private IEnumerator PlayRewardAnims()
        {
            _doneButton.interactable = false;
            yield return WaitFor.Seconds(1);
            _textTweenId = LeanTween.value(gameObject, SetExpText, _displayedExpReward, _finalExpReward , _textAnimDuration).uniqueId;
            yield return WaitFor.Seconds(_textAnimDuration);
            _textTweenId = LeanTween.scale(_expRewardText.gameObject, Vector3.one * 1.5f, _textScaleDuration * 0.5f).setOnComplete(() =>
                LeanTween.scale(_expRewardText.rectTransform, Vector3.one, _textScaleDuration * 0.5f)).uniqueId;
            yield return WaitFor.Seconds(_textScaleDuration);
            CombatManager.Instance.Player.AddExp(_finalExpReward);
            _textTweenId = LeanTween.value(gameObject, SetGoldText, _displayedGoldReward, _finalGoldReward, _textAnimDuration).uniqueId;
            yield return WaitFor.Seconds(_textAnimDuration);
            _textTweenId = LeanTween.scale(_goldRewardText.gameObject, Vector3.one * 1.5f, _textScaleDuration * 0.5f).setOnComplete(() =>
                LeanTween.scale(_goldRewardText.rectTransform, Vector3.one, _textScaleDuration * 0.5f)).uniqueId;
            yield return WaitFor.Seconds(_textScaleDuration);
            for (int i = 0; i < 7; i++)
            {
                var ri = Instantiate(_rewardItemPrefab, _rewardContainer);
                ri.Init(RewardType.Card, _itemScaleDuration);
                _scrollRect.velocity = new Vector2(0, 1000f);
                yield return WaitFor.Seconds(_itemScaleDuration);
            }
            _doneButton.interactable = true;
        }

        private void SetExpText(float value)
        {
            var intVal = Convert.ToInt32(value);
            _expRewardText.text = intVal.ToString();
        }

        private void SetGoldText(float value)
        {
            var intVal = Convert.ToInt32(value);
            _goldRewardText.text = intVal.ToString();
        }

        #endregion
    }
}
