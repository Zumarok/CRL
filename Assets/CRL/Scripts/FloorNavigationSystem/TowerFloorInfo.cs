using System;
using System.Collections.Generic;
using System.Text;
using Crux.CRL.AbilitySystem;
using static Crux.CRL.DataSystem.Constants;

namespace Crux.CRL.FloorNavigationSystem
{
    [Serializable]
    public class TowerFloorInfo
    {
        public int Floor { get; private set; }
        public string PositiveModString { get; private set; }
        public string NegativeModString { get; private set; }
        public string NeutralModString { get; private set; }

        public int PlayerMaxMana { get; private set; }
        public float PlayerPercentMaxHp { get; private set; }
        public float PlayerPercentDmg { get; private set; }
        public int EnemyExtraDDHits { get; private set; }
        public float EnemyPercentMaxHp { get; private set; }
        public float EnemyPercentDmg { get; private set; }
        public int SpawnElites { get; private set; }
        public int SpawnWaves { get; private set; }
        public bool SpawnBoss { get; private set; }
        public float DropGoldMod { get; private set; }
        public float DropShardMod { get; private set; }

        public void ResetMods(int floor)
        {
            PositiveModString = "";
            NegativeModString = "";
            NeutralModString = "";

            PlayerMaxMana = 0;
            PlayerPercentMaxHp = 0;
            PlayerPercentDmg = 0;
            EnemyExtraDDHits = 0;
            EnemyPercentMaxHp = 0;
            EnemyPercentDmg = 0;
            SpawnElites = 0;
            SpawnWaves = 0;
            DropGoldMod = 0;
            DropShardMod = 0;
            SpawnBoss = false;

            SetFloor(floor);
            FormatStrings();
        }

        /// <summary>
        /// Add the Ability's mods to the current mods.
        /// </summary>
        public void AddAbilityMods(Ability ability)
        {
            AddAbilityModsInternal(ability);
            FormatStrings();
        }

        /// <summary>
        /// Add the list of Abilities mods to the current mods.
        /// </summary>
        public void AddAbilityMods(List<Ability> abilities)
        {
            if (abilities == null) return;

            foreach (var ability in abilities)
            {
                AddAbilityModsInternal(ability);
            }
            FormatStrings();
        }

        private void SetFloor(int floor)
        {
            Floor = floor;
            DropGoldMod += floor * FLOOR_BONUS_MULT;
            DropShardMod += floor * FLOOR_BONUS_MULT;
            EnemyPercentMaxHp += floor * FLOOR_BONUS_MULT;
            EnemyPercentDmg += floor * FLOOR_BONUS_MULT;
        }
        
        private void AddAbilityModsInternal(Ability ability)
        {
            if (ability.HasReelPlayerFlatMana)
                PlayerMaxMana += ability.ReelPlayerFlatMana;
            if (ability.HasReelEnemyExtraDD)
                EnemyExtraDDHits += ability.ReelNumExtraDD;
            if (ability.HasReelSpawnElite)
                SpawnElites += ability.ReelNumSpawnedElites;
            if (ability.HasReelExtraWaves)
                SpawnWaves += ability.NumReelExtraWaves;
            if (ability.HasReelGoldModifier)
                DropGoldMod += ability.ReelGoldModifier;
            if (ability.HasReelShardModifier)
                DropShardMod += ability.ReelShardModifier;
            if (ability.HasReelEnemyHealthPercent)
                EnemyPercentMaxHp += ability.ReelEnemyHealthPercent;
            if (ability.HasReelEnemyDamagePercent)
                EnemyPercentDmg += ability.ReelEnemyDamagePercent;
            if (ability.HasReelPlayerHealthPercent)
                PlayerPercentMaxHp += ability.ReelPlayerHealthPercent;
            if (ability.HasReelPlayerDamagePercent)
                PlayerPercentDmg += ability.ReelPlayerDamagePercent;
            if (ability.HasReelSpawnBoss)
                SpawnBoss = true;
        }

        private void FormatStrings()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{DropGoldMod * 100:F0}% Increased Gold Drops");
            sb.AppendLine($"{DropShardMod * 100 :F0}% Increased Shard Drops");
            if (PlayerMaxMana > 0)
                sb.AppendLine($"+{PlayerMaxMana} to Maximum Mana");
            PositiveModString = sb.ToString();

            sb.Clear();
            sb.AppendLine($"{EnemyPercentMaxHp * 100:F0}% Increased Enemy Health");
            sb.AppendLine($"{EnemyPercentDmg * 100:F0}% Increased Enemy Damage");
            if (EnemyExtraDDHits > 0)
                sb.AppendLine($"{EnemyExtraDDHits} Additional DD Hit{(EnemyExtraDDHits > 1 ? "s" : "")}");
            NegativeModString = sb.ToString();

            sb.Clear();
            if (SpawnWaves > 0)
                sb.AppendLine($"{SpawnWaves} Additional Wave{(SpawnWaves > 1 ? "s" : "")}");
            if (SpawnElites > 0)
                sb.AppendLine($"{SpawnElites} Additional Elite Monster{(SpawnElites > 1 ? "s" : "")}");
            if (SpawnBoss)
                sb.AppendLine("* Boss Encounter *");
            NeutralModString = sb.ToString();
        }

        
        
    }
}
