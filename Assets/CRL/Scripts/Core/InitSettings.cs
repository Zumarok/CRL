using System.Collections;
using Crux.CRL.DataSystem;
using Crux.CRL.SaveSystem;
using Crux.CRL.SceneSystem;
using UnityEngine;

namespace Crux.CRL.Core
{
    public class InitSettings : MonoBehaviour
    {
        #region Unity Callbacks

        private void Awake()
        {
            DontDestroyOnLoad(this);
            Application.targetFrameRate = 60; // todo temp, set this from options
            LeanTween.init(800);
        }

        private void Start()
        {
            Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);// todo temporary
            SaveManager.Instance.LoadGame(); 
            SceneLoader.Instance.LoadLobbyScene(); 
            //SceneLoader.Instance.LoadCombatScene(); 
        }

        #endregion

        #region Private Functions


        #endregion
    }
}
