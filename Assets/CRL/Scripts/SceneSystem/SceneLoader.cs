using System;
using System.Collections;
using Crux.CRL.DataSystem;
using Crux.CRL.Utils;
using UnityEngine.SceneManagement;

namespace Crux.CRL.SceneSystem
{
    public class SceneLoader : SingletonMonoBehaviour<SceneLoader>
    {
        #region Public Methods
        
        public void LoadScene(string path, SceneType type)
        {
            SceneManager.LoadSceneAsync(path, LoadSceneMode.Single).completed += operation => OnLevelLoaded(type);
        }

        public void LoadLobbyScene()
        {
            LoadScene("CRL/Scenes/TowerExterior", SceneType.Lobby);
        }

        public void LoadCombatScene()
        {
            LoadScene("CRL/Scenes/Tower1", SceneType.Combat);
        }

        #endregion

        #region Private Methods

        private void OnLevelLoaded(SceneType type)
        {
            switch (type)
            {
                case SceneType.Lobby:
                    //DataManager.Instance.SpawnLobbySingleton();
                    DataManager.Instance.HealthOrbMeshRenderer.enabled = false;
                    DataManager.Instance.ManaOrbMeshRenderer.enabled = false;
                    break;
                case SceneType.Combat:
                    DataManager.Instance.SpawnGameSingleton();
                    break;
                case SceneType.Init:
                case SceneType.MainMenu: 
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        #endregion
    }
}