using Crux.CRL.DataSystem;
using UnityEngine;

namespace Crux.CRL.UI
{
    public class UISystemManager : MonoBehaviour
    {
        #region Editor Fields

        [SerializeField] private Canvas _canvas;

        #endregion
        
        #region Unity Callbacks

        private void Start()
        {
            _canvas.worldCamera = DataManager.Instance.UICamera;
        }

        #endregion

    }
}
