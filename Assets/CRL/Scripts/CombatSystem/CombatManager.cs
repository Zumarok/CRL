using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Crux.CRL.EnemySystem;
using Crux.CRL.AbilitySystem;
using Crux.CRL.CardSystem;
using Crux.CRL.DataSystem;
using Crux.CRL.FloatingTextSystem;
using Crux.CRL.RewardSystem;
using Crux.CRL.Utils;
using UnityEngine;
using Random = UnityEngine.Random;
using static Crux.CRL.DataSystem.Constants;

namespace Crux.CRL.CombatSystem
{
    public class CombatManager : SingletonMonoBehaviour<CombatManager>
    {
        #region Properties

        public Player Player { get; private set; }

        public Enemy GetRandomEnemy => _enemies[ThreadSafeRandom.Ran.Next(0, EnemyCount)];

        public int EnemyCount { get; private set; }
        
        public string GameSeed { get; private set; } = "test seed"; // todo remove testing seed

        public Random CRLRandom { get; private set; }

        #endregion

        #region Fields

        private readonly List<Combatant> _combatants = new List<Combatant>(10);
        private readonly List<Enemy> _enemies = new List<Enemy>(10);
        private readonly List<Combatant> _deadCombatants = new List<Combatant>(10);

        private int _combatTier;
        private Coroutine _encounterCoroutine;

        private int _currentTurn = 0;

        #endregion

        #region Unity Callbacks


        private IEnumerator Start()
        {
            yield return WaitFor.Seconds(1);
            StartEncounter(1);
        }
        
        #endregion

        #region Main Combat Loop

        private IEnumerator CombatLoop()
        {
            //var combatantCount = FindObjectsOfType<Combatant>().Length;
            //while (_combatants.Count != combatantCount) yield return null;
            //while (_combatants.Count != EnemySpawnManager.Instance.CurrentWaveEnemyTotal + 1) yield return null;
            Player.CardManager.SetupDecksForNewCombat();

            while (_enemies.Count > 0 && Player.CurrentHp > 0)
            {
                _currentTurn++;
                // player turn
                yield return WaitFor.Seconds(1);
                FloatingTextManager.Instance.ShowStaticCenterText("<u>Your Turn", Color.yellow, TextSize.Large, 3f, 0.19f);
                Debug.Log($"<color=cyan>-- Player Turn: {_currentTurn} --</color>");
                PlayerUIManager.Instance.CombatLog($"<color={CLC_PLAYER}>-- Player Turn: {_currentTurn} --</color>");
                yield return WaitFor.Seconds(2f);
                SetEnemyNextAbilities();
                yield return _combatants[0].ProcessTurn();

                // remove dead enemies
                for (int i = 0; i < _deadCombatants.Count; i++)
                {
                    _combatants.Remove(_deadCombatants[i]);
                }
                _deadCombatants.Clear();

                
                for (var i = 1; i < _combatants.Count; i++) // each enemy
                {
                    if (Player.CurrentHp <= 0) break;
                    if (EnemyCount == 0) break;
                    if (i == 1)  // first enemy's turn
                    {
                        yield return WaitFor.Seconds(1);
                        FloatingTextManager.Instance.ShowStaticCenterText("<u>Enemy Turn", Color.red, TextSize.Large, 3f, 0.19f);
                        Debug.Log($"<color=red>-- Enemy Turn: {_currentTurn} --</color>");
                        PlayerUIManager.Instance.CombatLog($"<color={CLC_ENEMY}>-- Enemy Turn: {_currentTurn} --</color>");
                        yield return WaitFor.Seconds(2f);
                    }

                    yield return _combatants[i].ProcessTurn();
                }
            }
        }
        
        #endregion

        #region Public Methods

        public void StartEncounter(int tier)
        {
            if (_encounterCoroutine != null)
            {
                return;
            }

            _combatTier = tier;
            _encounterCoroutine = StartCoroutine(StartEncounterCo(tier));
        }
        

        /// <summary>
        /// Adds the combatant to the _combatant list and returns its index.
        /// </summary>
        /// <param name="combatant"></param>
        /// <returns></returns>
        public int AddCombatant(Combatant combatant)
        {
            if (combatant.GetType() == typeof(Enemy))
            {
                _enemies.Add((Enemy) combatant);
                _combatants.Add(combatant);
                EnemyCount++;
            }
            else if (combatant.GetType() == typeof(Player))
            {
                Player = (Player) combatant;
                _combatants.Insert(0, combatant);
            }

            return _combatants.Count - 1;
        }

        public void RemoveCombatant(Combatant combatant)
        {
            _deadCombatants.Add(combatant);
            if (combatant.GetType() == typeof(Enemy))
            {
                _enemies.Remove((Enemy) combatant);
                EnemyCount--;
            }
            else if (combatant.GetType() == typeof(Player))
            {
                // game over?
                //StopAllCoroutines();
            }
        }

        public IReadOnlyList<Enemy> GetEnemies() => _enemies;
        
        public bool IsEnemyUnderCursor()
        {
            return GetEnemyAtCursor() != null;
        }


        public void TryUsePlayerAbility(Card card)
        {
            if (card.Targets.Count <= 0) return;

            if (card.Ability.ManaCost > Player.CurrentMana) return;

            if (card.Ability.ManaCost > 0)
                Player.SubtractMana(card.Ability.ManaCost);

            StartCoroutine(UsePlayerAbility(card));
        }

        /// <summary>
        /// Returns any enemy object at the mouse cursor. If none exists, returns null.
        /// </summary>
        /// <returns>Enemy or null</returns>
        public Enemy GetEnemyAtCursor() => _enemies.FirstOrDefault(e => e.IsMouseOver);

        public void SetAllEnemyOutlines(EnemyOutlineState outline) => _enemies.ForEach(e => e.SetEnemyOutline(outline));

        #endregion

        #region Private Methods

        private IEnumerator StartEncounterCo(int tier)
        {
            //EnemySpawnManager.Instance.LoadScenarioByFloor(DataManager.Instance.TowerFloorInfo);
            //var floor = DataManager.Instance.RunData.CurrentTowerFloor;
            //if (floor < 10)
            //    EnemySpawnManager.Instance.LoadScenarioByName("Video1");
            //else if (floor < 15)
            //    EnemySpawnManager.Instance.LoadScenarioByName("Video2");
            //else if (floor < 20)
            //    EnemySpawnManager.Instance.LoadScenarioByName("Video3");
            //else if (floor < 25)
            //    EnemySpawnManager.Instance.LoadScenarioByName("Video4");

            while (true)
            {
                yield return WaitFor.Seconds(1f);

                if (EnemySpawnManager.Instance.MoreWavesToSpawn)
                    FloatingTextManager.Instance.ShowStaticCenterText($"Wave {EnemySpawnManager.Instance.CurrentWaveNumber + 1} / {EnemySpawnManager.Instance.WaveCount}",
                        Color.white, TextSize.Medium, 2f, 0.22f);

                yield return WaitFor.Seconds(1f);

                yield return EnemySpawnManager.Instance.SpawnNextWave();
                if (EnemySpawnManager.Instance.CurrentScenarioFinished) break;
                
                yield return StartCoroutine(CombatLoop());

                yield return WaitFor.Seconds(1f);
            }

            if (Player.IsAlive)
            {
                FloatingTextManager.Instance.ShowStaticCenterText("Victory!", Color.green, TextSize.XLarge);
                RewardManager.Instance.GenerateRewards(_combatTier);
                yield return WaitFor.Seconds(2.2f);
                RewardManager.Instance.ShowRewardWindow();
            }

            _encounterCoroutine = null;
        }

        private IEnumerator UsePlayerAbility(Card card)
        {
            card.OnAbilityStartProcessing();
            yield return card.ShowSkillIcon();
            yield return Player.CardManager.DiscardCard(card);
            yield return card.PlaySkillIconThrowAnimation();
            yield return AbilityProcessor.Process(card.Ability, Player, card.Targets);
            card.OnAbilityFinishProcessing();
            yield return Player.CardManager.DrawCard(card.CardIndex);
        }

        private void SetEnemyNextAbilities()
        {
            foreach (var enemy in _enemies)
            {
                enemy.SetNextAbility();
            }
        }

        #endregion

    }
}
