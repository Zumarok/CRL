using System;
using System.Collections.Generic;
using System.Linq;
using Crux.CRL.AbilitySystem;
using Crux.CRL.DataSystem;
using Crux.CRL.FloorNavigationSystem;
using Crux.CRL.SaveSystem;
using Crux.CRL.Utils;
using JetBrains.Annotations;
using RotaryHeart.Lib.SerializableDictionary;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Crux.CRL.SlotReelsSystem
{
    [HierarchyColor]
    public class SlotReelsManager : SingletonMonoBehaviour<SlotReelsManager>
    {
        //[SerializeField, Range(1, 8)] private int _numActiveReels;
        [SerializeField] private float _animationDuration;
        [SerializeField] private float _nextReelDelay;
        [SerializeField] private int _wheelRotations;
        [SerializeField, Range(0.5f, 20f)] private float _spinCostFloorMult = 2f;
        [SerializeField] private ReelUnlockFloors _reelUnlockFloors;

        [SerializeField] private RectTransform _reelContainerTransform;
        [SerializeField] private Button _spinButton;
        [SerializeField] private TextMeshProUGUI _spinCostTxt;

        [SerializeField] 
        private List<SlotReel> _reels;

        private readonly Dictionary<int, List<SlotReelItemData>> _availableModsByFloor = new Dictionary<int, List<SlotReelItemData>>();
        private int _currentModSet = -1;
        private int _currentReelSet = -1;

        private int _containerScaleTweenId;
        private int _containerSizeTweenId;

        public bool IsRolling => _reels.Any(r => r.IsAnimating);

        public ReelUnlockFloors ReelUnlockFloors => _reelUnlockFloors;
        public RectTransform ReelContainerRectTransform => _reelContainerTransform;

        private void Start()
        {
            InitModsByFloorDictionary();
        }
        
        public void OnSpinButtonClicked()
        {
            if (_reels.Any(r => r.IsAnimating)) return;
            if (FloorNavWindow.Instance.IsMoving) return;

            DataManager.Instance.RunData.CurrentGold -= CalculateSpinCost(FloorNavWindow.Instance.Floor);
            SaveManager.Instance.SaveGame();

            UpdateSpinCostUI(FloorNavWindow.Instance.Floor);
            DataManager.Instance.TowerFloorInfo.ResetMods(FloorNavWindow.Instance.Floor);
            FloorNavWindow.Instance.UpdateModString();

            var uniqueRolls = new List<Ability>();

            // if this is in the tutorial, block all abilities other than extra wave1
            if (!DataManager.Instance.GameState.RunData.TutorialCompleted)
            {
                foreach (var data in _availableModsByFloor[0])
                {
                    if (data.Ability.AbilityName != AbilityName.ReelModExtraWave1)
                        uniqueRolls.Add(data.Ability);
                }
            }

            for (var i = 0; i < _reels.Count; i++)
            {
                var ability = _reels[i].Spin(_animationDuration, _wheelRotations, _nextReelDelay * i, uniqueRolls);

                if (ability == null) continue;
                if (!ability.HasReelUnique) continue;

                uniqueRolls.Add(ability);
            }
        }

        public void OnFloorChanged(int floor)
        {
            int roundedFloor = (floor / 5) * 5;
            SetNumberOfReelsByFloor(roundedFloor);
            SetReelModsByFloor(roundedFloor);
            UpdateSpinCostUI(floor);
        }

        public void ResetAllReels()
        {
            foreach (var reel in _reels)
            {
                reel.ResetReel();
            }
        }

        /// <summary>
        /// Returns a List of Abilities that unlock on this floor.
        /// Includes Reel and ReelMod unlocks. 
        /// </summary>
        public List<Ability> GetReelUnlocks(int floor)
        {
            var unlocks = new List<Ability>();

            if (_reelUnlockFloors.ContainsKey(floor)) 
                unlocks.Add(DataManager.Instance.GetAbilityData(AbilityName.Reel1));

            if (_availableModsByFloor.ContainsKey(floor))
            {
                foreach (var data in _availableModsByFloor[floor])
                {
                    if (data.Ability.ReelModMinFloor != floor) continue;
                    unlocks.Add(data.Ability);
                }
            }

            return unlocks;
        }

        public void UpdateSpinCostUI(int floor)
        {
            _spinButton.interactable = DataManager.Instance.GameState.RunData.CurrentGold >= CalculateSpinCost(floor);
            _spinCostTxt.text = CalculateSpinCost(floor).ToString(DataManager.Instance.CulturalInfo);
        }

        private void InitModsByFloorDictionary()
        {
            var abilities = DataManager.Instance.ReelAbilities.OrderBy(s => s.ReelModMinFloor).ToList();

            foreach (var ability in abilities)
            {
                if (!_availableModsByFloor.ContainsKey(ability.ReelModMinFloor))
                    _availableModsByFloor.Add(ability.ReelModMinFloor, new List<SlotReelItemData>());
            }

            foreach (var ability in abilities)
            {
                foreach (var key in _availableModsByFloor.Keys)
                {
                    if (ability.ReelModMinFloor <= key)
                        _availableModsByFloor[key].Add(new SlotReelItemData(ability));
                }
            }
        }
        
        private void SetReelModsByFloor(int roundedFloor = 0)
        {
            if (roundedFloor == _currentModSet || !_availableModsByFloor.ContainsKey(roundedFloor)) return;

            _currentModSet = roundedFloor;
            foreach (var reel in _reels)
            {
                reel.InitReel(_availableModsByFloor[roundedFloor]);
            }
        }

        private void SetNumberOfReelsByFloor(int roundedFloor = 0)
        {
            if (roundedFloor == _currentReelSet || !_reelUnlockFloors.ContainsKey(roundedFloor)) return;

            var isSmaller = roundedFloor < _currentReelSet;
            _currentReelSet = roundedFloor;
            var info = _reelUnlockFloors[roundedFloor];
            LeanTween.cancel(_containerScaleTweenId);
            LeanTween.cancel(_containerSizeTweenId);

            if (isSmaller)
            {
                _containerScaleTweenId = LeanTween.scale(_reelContainerTransform, Vector3.one * 1.1f, 0.2f).setLoopPingPong(1).uniqueId;

                for (int i = 0; i < _reels.Count; i++)
                {
                    _reels[i].gameObject.SetActive(i < info._numOfReels);
                }
            }

            _containerSizeTweenId = LeanTween.value(gameObject, val => _reelContainerTransform.sizeDelta = new Vector2(val, _reelContainerTransform.rect.height),
                    _reelContainerTransform.rect.width, info._containerWidth, 1f).setEaseInOutCubic().setOnComplete(
                () =>
                {
                    if (!isSmaller)
                    {
                        _containerScaleTweenId = LeanTween.scale(_reelContainerTransform, Vector3.one * 1.1f, 0.2f).setLoopPingPong(1).uniqueId;

                        for (int i = 0; i < _reels.Count; i++)
                        {
                            _reels[i].gameObject.SetActive(i < info._numOfReels);
                        }
                    }
                } ).uniqueId;
           
        }

        public int CalculateSpinCost(int floor)
        {
            return 10 + Convert.ToInt32(floor * _spinCostFloorMult);
        }
        
    }

    [Serializable]
    public class ReelUnlockFloors : SerializableDictionaryBase<int, ReelUnlockInfo>{}

    [Serializable]
    public class ReelUnlockInfo
    {
        public int _numOfReels;
        public int _containerWidth;
    }
}
