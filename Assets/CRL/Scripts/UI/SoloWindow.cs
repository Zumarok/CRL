using System;
using Crux.CRL.DataSystem;
using Crux.CRL.NotificationSystem;
using Crux.CRL.Utils;
using UnityEngine;

namespace Crux.CRL.UI
{
    /// <summary>
    /// Base class for all Solo Windows.
    /// Provides core window functionality and ensures no more than 1 Solo Window is ever visible.
    /// </summary>
    public class SoloWindow<T> : SingletonMonoBehaviour<T> where T : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        private int _fadeTweenId;
        protected bool IsShowing;

        protected override void Awake()
        {
            base.Awake();
            NotificationManager.Instance.AddListener<Type>(NotiEvt.UI.OnSoloWindowShow, OnSoloWindowShow);
            NotificationManager.Instance.AddListener(NotiEvt.UI.CloseAllSoloWindows, NotificationClose);
        }
        
        private void OnDestroy()
        {
            NotificationManager.Instance.RemoveListener<Type>(NotiEvt.UI.OnSoloWindowShow, OnSoloWindowShow);
            NotificationManager.Instance.RemoveListener(NotiEvt.UI.CloseAllSoloWindows, NotificationClose);
        }

        public virtual void Show()
        {
            ToggleVisibility(true);
        }

        public virtual void Hide()
        {
            ToggleVisibility(false);
        }

        private void NotificationClose()
        {
            if (IsShowing)
                ToggleVisibility(false);
        }

        private void OnSoloWindowShow(Type type)
        {
            if (IsShowing)
                ToggleVisibility(false);
        }

        protected void ToggleVisibility(bool show)
        {
            LeanTween.cancel(_fadeTweenId);
            Action onCompleteAction;

            if (show)
                onCompleteAction = OnShow;
            else
                onCompleteAction = OnHide;

            _fadeTweenId = LeanTween.alphaCanvas(canvasGroup, show ? 1 : 0, Constants.WINDOW_FADE_TIME)
                .setOnComplete(onCompleteAction).uniqueId;
        }

        protected virtual void OnShow()
        {
            canvasGroup.SetVisibilityAndInteractive(true);
            NotificationManager.Instance.SendNotification(NotiEvt.UI.OnSoloWindowShow, GetType());
            IsShowing = true;
        }

        protected virtual void OnHide()
        {
            canvasGroup.SetVisibilityAndInteractive(false);
            IsShowing = false;
            NotificationManager.Instance.SendNotification(NotiEvt.UI.OnSoloWindowHide, GetType());
        }
    }
}