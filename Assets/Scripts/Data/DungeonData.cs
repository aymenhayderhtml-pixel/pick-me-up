using UnityEngine;
using System;

namespace PickMeUp.Data
{
    public enum DungeonType { Daily, Weekly, Special }
    public enum DungeonRewardType { Gold, EXP, Material, Gem, Mixed }
    public enum DungeonDifficulty { Easy, Normal, Hard, Hell }

    [CreateAssetMenu(fileName = "Dungeon_X", menuName = "PickMeUp/Dungeon")]
    public class DungeonData : ScriptableObject
    {
        [Header("Identity")]
        public string dungeonId;
        public string dungeonName;
        [TextArea(2, 4)]
        public string description;
        public DungeonType dungeonType;
        public DungeonRewardType rewardType;
        public Sprite icon;
        public Color themeColor = Color.white;

        [Header("Difficulty Scaling")]
        public DungeonDifficulty[] availableDifficulties = new[] { DungeonDifficulty.Easy, DungeonDifficulty.Normal, DungeonDifficulty.Hard };
        public float[] difficultyMultipliers = new[] { 1.0f, 1.5f, 2.5f, 4.0f };
        public int[] difficultyRecommendedLevels = new[] { 10, 25, 45, 70 };

        [Header("Enemy Setup")]
        public string[] enemyIds;
        public int[] enemyLevels;
        public int[] enemyCounts;
        public int baseEnemyLevel = 10;

        [Header("Rewards — Base Values (scaled by difficulty)")]
        public int baseGoldReward = 500;
        public int baseExpReward = 200;
        public string[] possibleMaterialIds;
        public int[] materialDropCounts;
        public int baseGemReward = 0;
        public float dropChance = 1.0f; // 1.0 = guaranteed, 0.5 = 50%

        [Header("Stamina & Attempts")]
        public int staminaCost = 10;
        public int maxAttemptsPerReset = 3;
        public int resetHour = 5; // Daily reset at 5 AM
        public DayOfWeek weeklyResetDay = DayOfWeek.Monday;

        [Header("Unlock Requirements")]
        public int requiredHighestFloor = 1;
        public int requiredPlayerLevel = 1;

        public float GetDifficultyMultiplier(DungeonDifficulty diff)
        {
            int index = (int)diff;
            if (index < 0 || index >= difficultyMultipliers.Length) return 1.0f;
            return difficultyMultipliers[index];
        }

        public int GetRecommendedLevel(DungeonDifficulty diff)
        {
            int index = (int)diff;
            if (index < 0 || index >= difficultyRecommendedLevels.Length) return 1;
            return difficultyRecommendedLevels[index];
        }

        public bool IsDifficultyUnlocked(DungeonDifficulty diff)
        {
            return System.Array.Exists(availableDifficulties, d => d == diff);
        }

        public int GetScaledGold(DungeonDifficulty diff)
        {
            return Mathf.FloorToInt(baseGoldReward * GetDifficultyMultiplier(diff));
        }

        public int GetScaledEXP(DungeonDifficulty diff)
        {
            return Mathf.FloorToInt(baseExpReward * GetDifficultyMultiplier(diff));
        }

        public int GetScaledGem(DungeonDifficulty diff)
        {
            return Mathf.FloorToInt(baseGemReward * GetDifficultyMultiplier(diff));
        }
    }
}
