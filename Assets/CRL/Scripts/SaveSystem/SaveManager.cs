using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Crux.CRL.DataSystem;
using Crux.CRL.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Crux.CRL.SaveSystem
{
    public class SaveManager : SingletonMonoBehaviour<SaveManager>
    {
        #region Properties

        public int CurrentSaveSlot { get; private set; }
        
        #endregion

        #region Public Methods

        public void SaveGame()
        {
            var fileName = FileName(CurrentSaveSlot);
            var clonedGameState = DataManager.Instance.GameState?.Clone() ?? new GameState();
            var bf = new BinaryFormatter();
            try
            {
                using (var fs = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    bf.Serialize(fs, clonedGameState);
                    fs.Close();
                }
                Debug.Log("GameState saved.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void LoadGame()
        {
            var fileName = FileName(CurrentSaveSlot);

            if (!File.Exists(fileName))
            {
                var gs = new GameState();
                DataManager.Instance.SetGameState(gs);
                SaveGame();
                return;
            }

            var bf = new BinaryFormatter();
            try
            {
                using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    var gameState = (GameState)bf.Deserialize(fs);
                    fs.Close();
                    DataManager.Instance.SetGameState(gameState);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private GameState LoadBuildGameState()
        {
            GameState gs = null;
            var bf = new BinaryFormatter();
            try
            {
                using (var fs = File.Open(BuildFileName, FileMode.Open, FileAccess.Read))
                {
                    gs = (GameState)bf.Deserialize(fs);
                    fs.Close();
                    DataManager.Instance.SetGameState(gs);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            return gs;
        }

        public void SaveBuildGame(GameState gs)
        {
            var bf = new BinaryFormatter();
            try
            {
                using (var fs = File.Open(BuildFileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    bf.Serialize(fs, gs);
                    fs.Close();
                }
                Debug.Log("GameState saved.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetCurrentSaveSlot(int slot)
        {
            CurrentSaveSlot = slot;
        }

        #endregion

        private string FileName(int saveSlot) => Application.persistentDataPath + "/" + Constants.SAVE_FILE_NAME + "_" + saveSlot + ".sav";
        private string BuildFileName => @"C:\Users\NickH\AppData\LocalLow\Crux\CRL" + "/" + Constants.SAVE_FILE_NAME + "_0" + ".sav";


        [Button]
        private void AddGold()
        {
            DataManager.Instance.RunData.CurrentGold = int.MaxValue;
        }

        [Button]
        private void AddGoldToBuildSave()
        {
            var gs = LoadBuildGameState();
            gs.RunData.CurrentGold = int.MaxValue;
            SaveBuildGame(gs);
        }

        [Button]
        private void DeleteSaveFile()
        {
            var fileName = FileName(CurrentSaveSlot);

            if (!File.Exists(fileName))
            {
                Debug.Log("Save file not found.");
                return;
            }

            File.Delete(fileName);
            Debug.Log("Save file deleted!");
        }
    }
}
