using System.Collections.Generic;
using System.Linq;
using Crux.CRL.AbilitySystem;
using Crux.CRL.DataSystem;
using Crux.CRL.FloorNavigationSystem;
using Crux.CRL.UI;
using Crux.CRL.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Crux.CRL.SlotReelsSystem
{
    [HierarchyColor]
    public class SlotReel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private SlotReelItem _itemPrefab;
        private List<SlotReelItemData> _data;
        private readonly List<SlotReelItem> _spawnedItems = new List<SlotReelItem>();
        public bool IsAnimating { get; private set; }
        public int ResultIndex { get; private set; }
        public Ability ResultAbility { get; private set; }

        private int _tweenId;

        public void InitReel(List<SlotReelItemData> availableModsData)
        {
            if (!enabled) return;

            foreach (var slotReelItem in _spawnedItems)
            {
                Destroy(slotReelItem.gameObject);
            }
            _spawnedItems.Clear();

            _data = availableModsData;
            for (var i = 0; i < _data.Count; i++)
            {
                var data = _data[i];
                var item = Instantiate(_itemPrefab, _itemsContainer);
                item.SetSprite(data.IconSprite);
                item.SetTitleText(data.DisplayName);
                item.SetValueText(data.ShortDescription);
                item.SetIndex(i);
                item.transform.SetSiblingIndex(i);
                _spawnedItems.Add(item);
            }

            // put a dummy copy of the first item at the end, just for appearance as the animation happens
            var d = _data[0];
            var a = Instantiate(_itemPrefab, _itemsContainer);
            a.SetSprite(d.IconSprite);
            a.SetTitleText(d.DisplayName);
            a.SetValueText(d.ShortDescription);
            a.SetIndex(-1);
            a.transform.SetSiblingIndex(_data.Count);
            _spawnedItems.Add(a);

            // dummy copy of the last at the start
            d = _data.Last();
            a = Instantiate(_itemPrefab, _itemsContainer);
            a.SetSprite(d.IconSprite);
            a.SetTitleText(d.DisplayName);
            a.SetValueText(d.ShortDescription);
            a.SetIndex(-1);
            a.transform.SetSiblingIndex(0);
            _spawnedItems.Add(a);
        }

        public Ability Spin(float animDuration, int numRotations, float preDelay, List<Ability> invalidAbilities)
        {
            if (!isActiveAndEnabled) return null;
            int index;
            do
            {
                index = GetSpinResult();
                ResultIndex = index;
                ResultAbility = _data[index].Ability;
            } while (invalidAbilities.Contains(ResultAbility));
            
            PlayAnimation(index, animDuration, numRotations, preDelay);
            return ResultAbility;
        }

        public void ResetReel()
        {
            if (!isActiveAndEnabled) return;
            if (ResultIndex == 0) return;

            var index = 0;
            ResultIndex = index;
            ResultAbility = _data[index].Ability;
            PlayAnimation(index, 1f, 0, 0);
        }

        private int GetSpinResult()
        {
            var totalWeight = _data.Sum(d => d.Weight);

            var randomNum = Utils.ThreadSafeRandom.Ran.Next(1, totalWeight + 1);

            for (int i = 0; i < _data.Count; i++)
            {
                randomNum -= _data[i].Weight;
                if (randomNum <= 0)
                {
                    return i;
                }
            }

            return 0;
        }

        private void PlayAnimation(int index, float animDuration, int numRotations, float preDelay)
        {
            IsAnimating = true;

            LeanTween.cancel(_tweenId);
            var curY = _itemsContainer.localPosition.y;
            var oneRotDist = _data.Count * 100;
            var tarPos = index * 100 + 50;
            //var tarOffset = curY < tarPos ? tarPos - curY : tarPos - (curY - oneRotDist);
            var tarOffset = curY > tarPos ? curY - tarPos : curY + (oneRotDist - tarPos);
            _tweenId = LeanTween.value(gameObject, SetReelPosition, curY, curY - (oneRotDist * numRotations + tarOffset), animDuration)
                .setEaseInOutCirc().setDelay(preDelay).setOnComplete(OnAnimationStop).uniqueId;
        }

        private void SetReelPosition(float value)
        {
            var oneRotDist = _data.Count * 100;

            // Compute the modulus and adjust for negative values
            var adjustedValue = ((value % oneRotDist) + oneRotDist) % oneRotDist;

            _itemsContainer.localPosition = new Vector3(0, adjustedValue, 0);
        }

        private void OnAnimationStart()
        {
            IsAnimating = true;
        }

        private void OnAnimationStop()
        {
            IsAnimating = false;
            DataManager.Instance.TowerFloorInfo.AddAbilityMods(ResultAbility);
            FloorNavWindow.Instance.UpdateModString();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerEnter != gameObject) return;
            if (SlotReelsManager.Instance.IsRolling) return;
            
                SmallInfoWindow.Instance.ShowReelTooltip(ResultAbility, transform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SmallInfoWindow.Instance.ShowReelTooltip();

        }
    }
}
