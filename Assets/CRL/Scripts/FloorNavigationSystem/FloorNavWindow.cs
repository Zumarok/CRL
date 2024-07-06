using System;
using System.Collections;
using Crux.CRL.AbilitySystem;
using Crux.CRL.DataSystem;
using Crux.CRL.SaveSystem;
using Crux.CRL.SceneSystem;
using Crux.CRL.SlotReelsSystem;
using Crux.CRL.Utils;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using Button = UnityEngine.UI.Button;

namespace Crux.CRL.FloorNavigationSystem
{
    [HierarchyColor]
    public class FloorNavWindow : SingletonMonoBehaviour<FloorNavWindow>, IPointerClickHandler
    {
        #region Editor Fields

        [SerializeField] private float  _animationTime;
        [SerializeField] private float  _preAnimDelay;
        [SerializeField] private float  _maxTowerY;
        [SerializeField] private float  _maxSkyY;
        [SerializeField] private float _floorHeight;
        [SerializeField] private int _skyTransitionFloor;
        

        [SerializeField] private TextMeshProUGUI _currentFloorText;
        [SerializeField] private TextMeshProUGUI _positiveModText;
        [SerializeField] private TextMeshProUGUI _negativeModText;
        [SerializeField] private TextMeshProUGUI _neutralModText;
        [SerializeField] private TextMeshProUGUI _introText;
        [SerializeField] private Transform _towerSpriteTransform;
        [SerializeField] private Transform _skySpriteTransform;
        [SerializeField] private Transform _towerBaseTransform;
        [SerializeField] private Transform _sceneTransform;
        [SerializeField] private RectTransform _navWindowRectTransform;
        [SerializeField] private GameObject _bottomTowerSection;
        [SerializeField] private CanvasGroup _fadeCanvasGroup;
        [SerializeField] private CanvasGroup _navWinCanvas;
        [SerializeField] private CanvasGroup _introTextCanvas;

        [SerializeField] private CanvasGroup _unlockedAreaCanvas;
        [SerializeField] private RectTransform _unlockedAreaTransform;
        [SerializeField] private RectTransform _unlockedTopTextTransform;
        [SerializeField] private RectTransform _unlockedBottomTextTransform;
        [SerializeField] private TextMeshProUGUI _unlockedBottomText;

        [SerializeField] private RectTransform _unlockedCollectTransform;
        [SerializeField] private Image _unlockedCollectIcon;
        [SerializeField] private TextMeshProUGUI _unlockedCollectTopText;
        [SerializeField] private TextMeshProUGUI _unlockedCollectBottomText;

        //buttons
        [SerializeField] private Button _enterButton;
        [SerializeField] private Button _rollButton;
        [SerializeField] private Button _upButton;
        [SerializeField] private Button _downButton;


        #endregion

        #region Private Fields

        private int _currentUIFloor = 1; // the floor the UI lerp is showing differs from selected floor when tweening
        private int _selectedFloor = 1; // the selected destination floor
        private int _currentActualFloor = 1; // the realtime floor, based on tower position

        // tween ids
        private int _floorTextTweenId;
        private int _movementTweenId;
        private int _skySpriteTweenId;
        private int _positiveTweenId;
        private int _negativeTweenId;
        private int _neutralTweenId;
        private int _blackCanvasTweenId;
        private int _navWindowCanvasTweenId;
        private int _navWindowTransformTweenId;
        private int _introTextFadeTweenId;

        private int _unlockedAreaScaleTweenId;
        private int _unlockedAreaTopTextScaleTweenId;
        private int _unlockedAreaBottomTextScaleTweenId;
        private int _unlockedCollectScaleTweenId;
        private int _unlockedCollectMoveTweenId;
        private int _reelContainerScaleTweenId;

        private bool _mouseClicked = false;

        private float _towerXPos;
        private float _towerZPos;

        private float _baseXPos;
        private float _baseZPos;

        private float _skyXPos;
        private float _skyZPos;

        private bool _lastMoveDirUp;

        /// <summary>
        /// When the sky is y pos is less than this (value goes down as sky goes up) the BG tower base is no longer visible.
        /// </summary>
        private float _towerTransitionY; 
        #endregion

        #region Properties

        public int Floor => _selectedFloor;
        public bool IsMoving { get; private set; }

        #endregion

        #region Unity Callbacks

        private IEnumerator Start()
        {
            _fadeCanvasGroup.alpha = 1;
            _fadeCanvasGroup.blocksRaycasts = true;

            _navWinCanvas.alpha = 0;

            _towerTransitionY = _floorHeight * -3;

            var position = _towerSpriteTransform.localPosition;
            _towerXPos = position.x;
            _towerZPos = position.z;

            position = _towerBaseTransform.localPosition;
            _baseXPos = position.x;
            _baseZPos = position.z;

            position = _skySpriteTransform.localPosition;
            _skyXPos = position.x;
            _skyZPos = position.z;

            _currentUIFloor = 1;
            _selectedFloor = 1;

            DataManager.Instance.TowerFloorInfo.ResetMods(_currentUIFloor);
            UpdateModString(false);
            var towerData = DataManager.Instance.GameState.RunData;
            yield return WaitFor.Seconds(1f);
            if (!towerData.TutorialCompleted && towerData.HighestFloorDefeated == 0)
            {
                StartCoroutine(Intro()); // new game intro
            }
            else if (!towerData.TutorialCompleted && towerData.HighestFloorDefeated == 1)
            {
                StartCoroutine(IntroReels()); // new game intro after floor 1
            }
            else if (towerData.HighestFloorDefeated == towerData.HighestFloorReached) // just defeated the current floor and now back to lobby
            {
                if (SlotReelsManager.Instance.ReelUnlockFloors.Keys.Contains(towerData.HighestFloorDefeated))
                {
                    // Reel rewards unlocked, show the rewards before moving up
                    foreach (var reelUnlock in SlotReelsManager.Instance.GetReelUnlocks(towerData.HighestFloorDefeated))
                    {
                        yield return ReelUnlockCollect(reelUnlock);
                    }
                }

                //move the player up a floor
                _selectedFloor = ++towerData.HighestFloorReached;
                StartFloorMovement();
            }
            else
            {
                SetCurrentFloor(towerData.CurrentTowerFloor);
                FadeToBlackAndLoadCombatScene(false);
            }
            yield return null;
        }

        #endregion

        #region Button Callbacks

        /// <summary>
        /// Move up by one floor.
        /// </summary>
        public void OnAscendClicked()
        {
            if (SlotReelsManager.Instance.IsRolling) return;
            if (IsMoving) return;

            _selectedFloor++;
            StartFloorMovement();
        }

        /// <summary>
        /// Move down by one floor.
        /// </summary>
        public void OnDescendClicked()
        {
            if (SlotReelsManager.Instance.IsRolling) return;
            if (IsMoving) return;
            if (_selectedFloor <= 1) return;

            _selectedFloor--;
            StartFloorMovement();
        }


        /// <summary>
        /// (?) Display a list of all available consumables that can apply to this map.
        /// </summary>
        public void OnEmpowerClicked()
        {

        }

        /// <summary>
        /// Load into correct tower level with corresponding mods, etc
        /// </summary>
        public void OnEmbarkClicked()
        {
            if (IsMoving) return;
            if (SlotReelsManager.Instance.IsRolling) return;
            if (!DataManager.Instance.RunData.TutorialCompleted)
                DataManager.Instance.RunData.TutorialCompleted = true; // end of tutorial

            SaveManager.Instance.SaveGame();
            FadeToBlackAndLoadCombatScene(true);
        }

        public void SetCurrentFloor(int floor)
        {
            _selectedFloor = floor;
            _currentUIFloor = floor;
            HandleMovement((floor - 1) * -_floorHeight);
        }

        public void UpdateModString(bool animateText = true)
        {
            var info = DataManager.Instance.TowerFloorInfo;

            if (!_positiveModText.text.Equals(info.PositiveModString))
            {
                if (animateText)
                {
                    LeanTween.cancel(_positiveTweenId);
                    _positiveTweenId = LeanTween.scale(_positiveModText.rectTransform, Vector3.one * 1.15f, 0.25f)
                        .setEaseOutCubic().setOnComplete(() =>
                        {
                            _positiveTweenId = LeanTween.scale(_positiveModText.rectTransform, Vector3.one, 0.25f)
                                .uniqueId;
                        }).uniqueId;
                }

                _positiveModText.text = info.PositiveModString;
            }

            if (!_negativeModText.text.Equals(info.NegativeModString))
            {
                if (animateText)
                {
                    LeanTween.cancel(_negativeTweenId);
                    _negativeTweenId = LeanTween.scale(_negativeModText.rectTransform, Vector3.one * 1.15f, 0.25f)
                        .setEaseOutCubic().setOnComplete(() =>
                        {
                            _negativeTweenId = LeanTween.scale(_negativeModText.rectTransform, Vector3.one, 0.25f)
                                .uniqueId;
                        }).uniqueId;
                }

                _negativeModText.text = info.NegativeModString;
            }

            if (!_neutralModText.text.Equals(info.NeutralModString))
            {
                if (animateText)
                {
                    LeanTween.cancel(_neutralTweenId);
                    _neutralTweenId = LeanTween.scale(_neutralModText.rectTransform, Vector3.one * 1.15f, 0.25f)
                        .setEaseOutCubic().setOnComplete(() =>
                        {
                            _neutralTweenId = LeanTween.scale(_neutralModText.rectTransform, Vector3.one, 0.25f)
                                .uniqueId;
                        }).uniqueId;
                }

                _neutralModText.text = info.NeutralModString;
            }
        }

        #endregion

        #region Private Functions


        /// <summary>
        /// Starts the tween for the floor movement effect.
        /// </summary>
        private void StartFloorMovement(float? overridePerFloorDur = null)
        {
            SlotReelsManager.Instance.ResetAllReels();
            DataManager.Instance.TowerFloorInfo.ResetMods(_currentUIFloor);
            UpdateModString(false);

            // show target floor in UI
            var dist = math.abs(_selectedFloor - _currentUIFloor);
            if (dist > 1)
                _currentFloorText.text = $"Floor {_selectedFloor}";

            LeanTween.cancel(_floorTextTweenId);
            LeanTween.cancel(_movementTweenId);

            OnMovementStarted();

            var animTime = (overridePerFloorDur ?? _animationTime) * math.abs(_currentUIFloor - _selectedFloor);
            
            _floorTextTweenId = LeanTween.value(gameObject, SetFloorText, _currentUIFloor, _selectedFloor, animTime)
                .setEaseInOutCubic().setDelay(_preAnimDelay).uniqueId;

            _movementTweenId = LeanTween.value(gameObject, HandleMovement, (_currentActualFloor - 1) * -_floorHeight,
                    (_selectedFloor - 1) * -_floorHeight, animTime).setEaseInOutCubic().setDelay(_preAnimDelay).
                setOnComplete(OnMovementStop).uniqueId;
        }

        /// <summary>
        /// Custom formatter for CurrentFloorText.
        /// </summary>
        /// <param name="value"></param>
        private void SetFloorText(float value)
        {
            var intVal = Convert.ToInt32(value);

            if (_currentUIFloor == intVal) return;
            
            _currentUIFloor = intVal;
            _currentFloorText.text = $"- Floor {_currentUIFloor} -";
        }

        //private void SetTowerPosition(float value)
        //{
        //    if (_bottomTowerSection.gameObject.activeSelf)
        //        _towerSpriteTransform.localPosition = new Vector3(_towerXPos, value % -_maxTowerY, _towerZPos);
        //    //var skyY = _skySpriteTransform.localPosition.y;

        //    //if (skyY >= -1.508203f)
        //    //{
        //    //    _towerSpriteTransform.localPosition = new Vector3(_towerXPos, value % -_maxTowerY, _towerZPos);

        //    //    _towerBaseTransform.localPosition = new Vector3(_baseXPos, value, _baseZPos);
        //    //}

        //    var curFloor = -(int)(value / _floorHeight) + 1;
            
        //    if (curFloor == _currentActualFloor) return;

        //    _currentActualFloor = curFloor;
        //    OnAnyFloorReached();

        //    if (_currentActualFloor == _selectedFloor) return;
        //    OnTargetFloorReached();
        //}

      

        //private void SetSkyPosition(float value)
        //{
        //    _skySpriteTransform.localPosition = new Vector3(_skyXPos, value, _skyZPos);
            
        //}

        //private float CalculateTowerYPosByFloor(int floor)
        //{
        //    return (floor - 1) * _floorHeight;
        //}

        //private float CalculateSkyYPosByFloor(int floor)
        //{
        //    //var currentY = _skySpriteTransform.localPosition.y;

        //    //return floor <= 3 ? (floor - 1) * - _floorHeight : -1 + floor * ((_maxSkyY - currentY) * 0.025f);

        //    if (floor <= 3)
        //    {
        //        return -_floorHeight * (floor - 1);
        //    }
        //    else
        //    {
        //        // Calculate the position for floors between 4 and _maxSkyFloor
        //        var normalizedFloor = (float)(floor - 4) / (_skyTransitionFloor - 4);
        //        return Mathf.Lerp(-_floorHeight * 3, _maxSkyY, normalizedFloor);
        //    }
        //}

        private void HandleMovement(float val)
        {
            const float threshold = 0.05f;
            var div = val / _floorHeight;
            int nearestWholeNumber = Mathf.RoundToInt(div);
            float difference = Mathf.Abs(div - nearestWholeNumber);
            if (difference <= threshold)
            {
                var curFloor = -nearestWholeNumber + 1;

                if (curFloor != _currentActualFloor)
                {
                    _currentActualFloor = curFloor;
                    OnAnyFloorReached();
                }
            }

            // tower sprites should be parented to the sky
            if (val > _towerTransitionY)
            {
                // the tower base on the BG is visible so all movement goes to the BG
                _skySpriteTransform.localPosition = new Vector3(_skyXPos, val, _skyZPos);
                if (!_towerBaseTransform.gameObject.activeSelf)
                {
                    _towerBaseTransform.gameObject.SetActive(true);
                    _bottomTowerSection.SetActive(false);
                }
            }
            else
            {
                // tower base is no longer visible

                // if moving up and the bottom tower sections aren't active, activate them
                // if moving down, and they are active, deactivate them
                if (!_bottomTowerSection.activeSelf)
                {
                    //Debug.Log($"!!bottom.active:{_lastMoveDirUp}, base.active:{!_lastMoveDirUp}!!");
                    _bottomTowerSection.SetActive(true);
                    _towerBaseTransform.gameObject.SetActive(false);
                }

                // sky should now move less than a full floor, and the tower moves the remainder
                var normalizedVal = math.abs(val - _towerTransitionY) / (_skyTransitionFloor * _floorHeight + _towerTransitionY);
                var skyPos = Mathf.Lerp(_towerTransitionY, _maxSkyY, normalizedVal);
                _skySpriteTransform.localPosition = new Vector3(_skyXPos, skyPos, _skyZPos);

                var towerPos = val - skyPos;
                _towerSpriteTransform.localPosition = new Vector3(_towerXPos, towerPos % -_maxTowerY, _towerZPos);
            }
            
            if (_currentActualFloor != _selectedFloor) return;
            OnTargetFloorReached();
        }

        private void ShowNavWindow(bool show)
        {
            Action onCompleteAction;

            if (show)
            {
                onCompleteAction = OnNavWindowShown;
                _navWindowRectTransform.localScale = Vector3.zero;
                _navWinCanvas.SetVisibilityAndInteractive(true);
            }
            else
            {
                onCompleteAction = OnNavWindowHidden;
            }

            LeanTween.cancel(_navWindowTransformTweenId);
            LeanTween.cancel(_floorTextTweenId);

            _navWindowTransformTweenId = LeanTween.scale(_navWindowRectTransform, Vector3.one, 1f).setEaseInOutBack().setOnComplete(onCompleteAction).uniqueId;
            _floorTextTweenId = LeanTween.scale(_currentFloorText.rectTransform, Vector3.one, 0.5f).setDelay(0.75f).setEaseInOutBack().uniqueId;


            //_navWindowCanvasTweenId = LeanTween.alphaCanvas(_navWinCanvas, show ? 0.5f : 0, Constants.WINDOW_FADE_TIME)
            //    .setOnComplete(onCompleteAction).setDelay(show ? 1f : 0f).uniqueId;
        }

        private void FadeToBlackAndLoadCombatScene(bool toBlack)
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

        private IEnumerator Intro()
        {
            // plays on new game only
            SetCurrentFloor(4);
            // play wind sounds and music
            yield return WaitFor.Seconds(1f);
            
            LeanTween.cancel(_blackCanvasTweenId);
            var fadeTime = 5;
            _blackCanvasTweenId = LeanTween.alphaCanvas(_fadeCanvasGroup, 0, fadeTime).
                setOnComplete(() => _fadeCanvasGroup.SetVisibilityAndInteractive(false)).uniqueId; // fade in over ~3 sec
            yield return WaitFor.Seconds(fadeTime);

            var descendTime = 11f;
            _selectedFloor = 1; // slowly pan down the tower as music and wind continues
            StartFloorMovement(descendTime);

            // text (typing effect, maybe typing/beeping sfx) to the left
            // // 'a tower...the tower...'
            _introText.TypeWriterText(this, "A tower rising beyond the clouds...", 0.16f);
            yield return WaitFor.Seconds(10);
            LeanTween.cancel(_introTextFadeTweenId);
            _introTextFadeTweenId = LeanTween.alphaCanvas(_introTextCanvas, 0, 1f).setOnComplete(() =>
            {
                _introText.text = "";
                _introTextCanvas.alpha = 1;
            }).uniqueId;

            // 'climb to obtain power.'
            yield return WaitFor.Seconds(10);
            _introText.TypeWriterText(this, "Some say it extends beyond this world...", 0.16f);
            yield return WaitFor.Seconds(10);
            LeanTween.cancel(_introTextFadeTweenId);
            _introTextFadeTweenId = LeanTween.alphaCanvas(_introTextCanvas, 0, 1f).setOnComplete(() => _introText.text = "").uniqueId;
            yield return WaitFor.Seconds(5);

            
            
            // 'climb to escape.'  // reach the bottom here, music/sfx hit


            // auto zone into floor 1, zoom in, wind stops, music changes, etc
            SceneLoader.Instance.LoadCombatScene();
        }

        private IEnumerator IntroReels()
        {
            // hard set player gold in case game closed during this tutorial
            DataManager.Instance.RunData.CurrentGold = SlotReelsManager.Instance.CalculateSpinCost(2);
            // plays after first floor 1 clear
            LeanTween.cancel(_blackCanvasTweenId);
            var fadeTime = 2;
            _blackCanvasTweenId = _blackCanvasTweenId = LeanTween.alphaCanvas(_fadeCanvasGroup, 0, fadeTime).
                setOnComplete(() => _fadeCanvasGroup.SetVisibilityAndInteractive(false)).uniqueId; // fade in 
            yield return WaitFor.Seconds(fadeTime);
            
            // 'to the victor, the rewards'
            yield return WaitFor.Seconds(2);
            _introText.TypeWriterText(this, "To the victor, the rewards", 0.12f);
            yield return WaitFor.Seconds(4);
            LeanTween.cancel(_introTextFadeTweenId);
            _introTextFadeTweenId = LeanTween.alphaCanvas(_introTextCanvas, 0, 1f).setOnComplete(() =>
            {
                _introText.text = "";
                _introTextCanvas.alpha = 1;
            }).uniqueId;

            // 'but what reward comes without risk?'
            yield return WaitFor.Seconds(2);
            _introText.TypeWriterText(this, "But what reward comes without risk?", 0.12f);
            yield return WaitFor.Seconds(7);
            LeanTween.cancel(_introTextFadeTweenId);
            _introTextFadeTweenId = LeanTween.alphaCanvas(_introTextCanvas, 0, 1f).setOnComplete(() =>
            {
                _introText.text = "";
                _introTextCanvas.alpha = 1;
            }).uniqueId;
            yield return WaitFor.Seconds(1);

            // text fades out, nav window fades in with buttons disabled
            _enterButton.interactable = false;
            _rollButton.interactable = false;
            _upButton.interactable = false;
            _downButton.interactable = false;
            SlotReelsManager.Instance.ReelContainerRectTransform.localScale = Vector3.zero;
            ShowNavWindow(true); 
            
            // player collects reel unlocks 
            var runData = DataManager.Instance.GameState.RunData;

            yield return WaitFor.Seconds(1f);

            // Reel rewards unlocked, show the rewards before moving up
            var unlocks = SlotReelsManager.Instance.GetReelUnlocks(0);
            for (var i = 0; i < unlocks.Count; i++)
            {
                yield return ReelUnlockCollect(unlocks[i], i == 0, i == unlocks.Count - 1);

                if (unlocks[i].IsReel)
                {
                    SlotReelsManager.Instance.OnFloorChanged(1);
                    _rollButton.interactable = false;
                    yield return WaitFor.Seconds(0.5f);
                }
            }

            yield return WaitFor.Seconds(1);

            // highlight move up button
            runData.HighestFloorReached++;
            _upButton.interactable = true;
            var upRect = _upButton.GetComponent<RectTransform>();
            var originalUpScale = upRect.localScale;
            var tweenId = upRect.Pulse(1.25f);

            // 'Ascend'
            var p = _introText.rectTransform.localPosition;
            _introText.rectTransform.localPosition = new Vector3(-408, 310, p.z);
            LeanTween.cancel(_introTextFadeTweenId);
            _introText.TypeWriterText(this, "Ascend to new heights", 0.12f);
            
            while (_selectedFloor != 2)
            {
                yield return null; 
            }
            LeanTween.cancel(tweenId);
            _upButton.interactable = false;
            upRect.localScale = originalUpScale;

            LeanTween.cancel(_introTextFadeTweenId);
            _introTextFadeTweenId = LeanTween.alphaCanvas(_introTextCanvas, 0, 1f).setOnComplete(() =>
            {
                _introText.text = "";
                _introTextCanvas.alpha = 1;
            }).uniqueId;

            while (_currentUIFloor != 2)
            {
                yield return null; // player moves up
            }

            yield return WaitFor.Seconds(2);

            // highlight roll button
            // roll costs x, roll will always land on 1x extra wave
            LeanTween.cancel(tweenId);
            _rollButton.interactable = true;
            tweenId = _rollButton.gameObject.Pulse();


            LeanTween.cancel(_introTextFadeTweenId);
            _introText.rectTransform.localPosition = new Vector3(-408, -111, p.z);
            _introText.rectTransform.localScale = Vector3.one * 0.6f;
            _introText.TypeWriterText(this, "Test the winds of fate", 0.12f);

            yield return WaitFor.Seconds(4);
            LeanTween.cancel(_introTextFadeTweenId);
            _introTextFadeTweenId = LeanTween.alphaCanvas(_introTextCanvas, 0, 1f).setOnComplete(() =>
            {
                _introText.text = "";
                _introTextCanvas.alpha = 1;
            }).uniqueId;

            while (runData.CurrentGold > 0)
            {
                yield return null;
            }
            
            LeanTween.cancel(tweenId);
            
            // 'the reel device seems to have room for extra reels...'
            // 'perhaps they are on a higher floor.'
            yield return WaitFor.Seconds(6);
            _introText.rectTransform.localPosition = new Vector3(p.x, 310, p.z);
            _introText.rectTransform.localScale = Vector3.one;
            _introText.TypeWriterText(this, "The device is still missing many reels", 0.12f);
            yield return WaitFor.Seconds(6);
            LeanTween.cancel(_introTextFadeTweenId);
            _introTextFadeTweenId = LeanTween.alphaCanvas(_introTextCanvas, 0, 1f).setOnComplete(() =>
            {
                _introText.text = "";
                _introTextCanvas.alpha = 1;
            }).uniqueId;

            yield return WaitFor.Seconds(2);
            _introText.TypeWriterText(this, "Perhaps they are on a higher floor", 0.12f);
            yield return WaitFor.Seconds(7);
            LeanTween.cancel(_introTextFadeTweenId);
            _introTextFadeTweenId = LeanTween.alphaCanvas(_introTextCanvas, 0, 1f).setOnComplete(() =>
            {
                _introText.text = "";
                _introTextCanvas.alpha = 1;
            }).uniqueId;
            yield return WaitFor.Seconds(1);


            LeanTween.cancel(_introTextFadeTweenId);
            _introText.rectTransform.localPosition = new Vector3(-408, 310, p.z);
            _introText.TypeWriterText(this, "Enter and face your fate", 0.12f);
            // highlight enter button
            LeanTween.cancel(tweenId);
            _enterButton.interactable = true;
            _enterButton.gameObject.Pulse();
            yield return null;
        }

        private IEnumerator ReelUnlockCollect(Ability unlock, bool firstItem = false, bool lastItem = false)
        {
            _unlockedAreaTransform.localScale = firstItem ? Vector3.zero : Vector3.one;
            _unlockedTopTextTransform.localScale = firstItem ? Vector3.zero : Vector3.one;
            _unlockedBottomTextTransform.localScale = firstItem ? Vector3.zero : Vector3.one;
            _unlockedCollectTransform.localScale = Vector3.zero;
            _unlockedCollectTransform.localPosition = Vector3.zero;

            _unlockedCollectIcon.sprite = unlock.IconSprite;
            _unlockedCollectTopText.text = unlock.FormattedName;
            _unlockedCollectBottomText.text = unlock.ShortDescription();

            _unlockedAreaCanvas.SetVisibilityAndInteractive(true);

            LeanTween.cancel(_unlockedAreaScaleTweenId);
            LeanTween.cancel(_unlockedAreaTopTextScaleTweenId);
            LeanTween.cancel(_unlockedAreaBottomTextScaleTweenId);
            LeanTween.cancel(_unlockedCollectScaleTweenId);

            _unlockedAreaScaleTweenId = LeanTween.scale(_unlockedAreaTransform, Vector3.one, 0.5f).setEaseInOutBack().uniqueId;
            _unlockedAreaTopTextScaleTweenId = LeanTween.scale(_unlockedTopTextTransform, Vector3.one, 0.5f).setDelay(0.25f).setEaseInOutBack().uniqueId;
            _unlockedAreaBottomTextScaleTweenId = LeanTween.scale(_unlockedBottomTextTransform, Vector3.one, 0.5f).setDelay(0.5f).setEaseInOutBack().uniqueId;
            _unlockedCollectScaleTweenId = LeanTween.scale(_unlockedCollectTransform, Vector3.one, 0.5f).setDelay(1f).setEaseInOutCubic().uniqueId;
            _unlockedCollectScaleTweenId = LeanTween.scale(_unlockedCollectTransform, Vector3.one * 1.1f, 1f).setDelay(1.5f).setEaseInOutCubic().setLoopPingPong(-1).uniqueId;

            _mouseClicked = false;
            while (_mouseClicked == false)
            {
                yield return null;
            }

            LeanTween.cancel(_unlockedCollectMoveTweenId);
            LeanTween.cancel(_unlockedCollectScaleTweenId);

            _unlockedCollectMoveTweenId = LeanTween.move(_unlockedCollectTransform.gameObject, SlotReelsManager.Instance.ReelContainerRectTransform.position, 1f)
                .setEaseInQuart().uniqueId;
            _unlockedCollectScaleTweenId = LeanTween.scale(_unlockedCollectTransform, Vector3.one * 1.8f, 0.85f).setEaseOutQuint().setOnComplete(() =>
                {
                    _unlockedCollectScaleTweenId = LeanTween.scale(_unlockedCollectTransform, Vector3.zero, 0.2f).setEaseOutSine().uniqueId;
                }).uniqueId;

            yield return new WaitForSeconds(0.8f);

            LeanTween.cancel(_reelContainerScaleTweenId);
            _reelContainerScaleTweenId = LeanTween
                .scale(SlotReelsManager.Instance.ReelContainerRectTransform, Vector3.one * 1.2f, 0.5f)
                .setEaseInOutBack().setOnComplete(() =>
                    SlotReelsManager.Instance.ReelContainerRectTransform.localScale = Vector3.one).uniqueId;

            yield return new WaitForSeconds(0.5f);

            if (lastItem)
            {
                _unlockedAreaScaleTweenId = LeanTween
                    .scale(_unlockedAreaTransform, Vector3.zero, 0.3f)
                    .setEaseOutSine()
                    .setOnComplete(() =>
                    {
                        _unlockedAreaCanvas.SetVisibilityAndInteractive(false);
                    })
                    .uniqueId;
            }
        }

        #endregion

        #region Events

        private void OnAnyFloorReached()
        {
            //Debug.Log($"actual:{_currentActualFloor}, selected:{_selectedFloor}, SkyPos: {_skySpriteTransform.localPosition.y}");
            
            DataManager.Instance.TowerFloorInfo.ResetMods(_currentActualFloor);
            UpdateModString();
        }

        private void OnTargetFloorReached()
        {
            SlotReelsManager.Instance.OnFloorChanged(_currentActualFloor);
            DataManager.Instance.GameState.RunData.CurrentTowerFloor = _currentActualFloor;
        }

        private void OnMovementStarted()
        {
            IsMoving = true;
            _lastMoveDirUp =  _selectedFloor - _currentActualFloor > 0;
            OnAnyFloorReached();
        }

        private void OnMovementStop()
        {
            IsMoving = false;
        }

        private void OnFadeToBlackComplete()
        {
            _fadeCanvasGroup.SetVisibilityAndInteractive(true);
            SceneLoader.Instance.LoadCombatScene();
        }

        private void OnFadeFromBlackComplete()
        {
            _fadeCanvasGroup.SetVisibilityAndInteractive(false);

            // set tower level
            ShowNavWindow(true);
        }

        private void OnNavWindowShown()
        {
        }

        private void OnNavWindowHidden()
        {
            _navWinCanvas.SetVisibilityAndInteractive(false); 
            _navWindowRectTransform.localScale = Vector3.zero;
            
        }

        #endregion

        #region Interface Functions

        public void OnPointerClick(PointerEventData eventData)
        {
            _mouseClicked = true;
        }

        #endregion


    }
}