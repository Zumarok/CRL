using System;
using System.Collections;
using System.Collections.Generic;
using Crux.CRL.NotificationSystem;
using UnityEngine;

namespace Crux.CRL.Utils
{
    public class ResolutionChangeChecker : MonoBehaviour
    {
        [SerializeField] private Camera _cam;

        private Rect _previousRes;

        private void Start()
        {
            _previousRes = _cam.pixelRect;
            StartCoroutine(CheckForResChange());
        }

        private IEnumerator CheckForResChange()
        {
            while (gameObject.activeInHierarchy)
            {
                if (!_previousRes.Equals(_cam.pixelRect))
                {
                    _previousRes = _cam.pixelRect;
                    NotificationManager.Instance.SendNotification(NotiEvt.UI.ViewportResolutionChanged);
                }

                yield return WaitFor.Seconds(0.5f);
            }
        }
    }
}
