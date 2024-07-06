using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Crux.CRL.Utils
{
    public static class Extensions
    {
        /// <summary>
        /// Sets the alpha, interactive and block raycast states of the Canvas Group together.
        /// </summary>
        public static void SetVisibilityAndInteractive(this CanvasGroup group, bool active)
        {
            group.alpha = active ? 1 : 0;
            group.interactable = active;
            group.blocksRaycasts = active;
        }

        /// <summary>
        /// Shuffle an IList.
        /// source: https://stackoverflow.com/questions/273313/randomize-a-listt/1262619#1262619
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.Ran.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        
        public static T GetRandom<T>(this IReadOnlyList<T> list)
        {
            var count = list.Count;
            if (count == 0) return default;

            var k = ThreadSafeRandom.Ran.Next(count);
            return list[k];
        }

        /// <summary>
        /// Get the screen size of an object's diameter in pixels, given its distance and diameter.
        /// https://forum.unity.com/threads/this-script-gives-you-objects-screen-size-in-pixels.48966/
        /// </summary>
        public static float PixelSizeInViewport(this Collider col, Camera cam)
        {
            var diameter = col.bounds.extents.magnitude;
            var distance = Vector3.Distance(col.transform.position, cam.transform.position);
            return diameter * Mathf.Rad2Deg* Screen.height / (distance * cam.fieldOfView);
        }


        public static Coroutine TypeWriterText(this TextMeshProUGUI textMesh, MonoBehaviour mono, string msg, float delay = 0.15f)
        {
            textMesh.text = "";
            var sb = new StringBuilder();

            IEnumerator Type()
            {
                foreach (var c in msg)
                {
                    sb.Append(c);
                    textMesh.text = sb.ToString();
                    yield return WaitFor.Seconds(delay);
                }
                
            }

            return mono.StartCoroutine(Type());
        }

        public static int Pulse(this RectTransform trans, float size = 1.1f, float duration = 1f, int numPulses = -1)
        {
            return LeanTween.scale(trans, Vector3.one * size, duration).setEaseInOutCubic().setLoopPingPong(numPulses).uniqueId;
        }

        public static int Pulse(this GameObject gameObject, float size = 1.1f, float duration = 1f, int numPulses = -1)
        {
            return LeanTween.scale(gameObject, Vector3.one * size, duration).setEaseInOutCubic().setLoopPingPong(numPulses).uniqueId;
        }
    }
}