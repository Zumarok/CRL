using System;

namespace Crux.CRL.SaveSystem
{
    /// <summary>
    /// The top level save object. Contains references to all sub save objects.
    /// All data must be serializable and referenced data classes must be serializable and implement ICloneable.
    /// </summary>
    [Serializable]
    public class GameState 
    {
        private PlayerData _playerData = new PlayerData();
        private RunData _runData = new RunData();

        public PlayerData PlayerData => _playerData ?? (_playerData = new PlayerData());

        public RunData RunData => _runData ?? (_runData = new RunData());

        /// <summary>
        /// Clones the current save data. Useful if the save operation is done async to avoid the state changing during the save.
        /// </summary>
        /// <returns></returns>
        public GameState Clone()
        {
            var gs = new GameState();
            gs._playerData = (PlayerData)_playerData.Clone();
            gs._runData = (RunData)_runData.Clone();
            //clone other data classes; ICloneable
            return gs;
        }

    }
}

