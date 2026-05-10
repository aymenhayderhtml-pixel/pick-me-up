using System;
using UnityEngine;

namespace PickMeUp.Data
{
    [Serializable]
    public class DungeonProgress
    {
        public string dungeonId;
        public int attemptsUsedToday;
        public int attemptsUsedThisWeek;
        public string lastDailyResetDate = ""; // yyyy-MM-dd
        public string lastWeeklyResetDate = ""; // yyyy-MM-dd
        public DungeonDifficulty highestClearedDifficulty = DungeonDifficulty.Easy;
        public int totalClears = 0;

        public DungeonProgress() { }
        public DungeonProgress(string dungeonId)
        {
            this.dungeonId = dungeonId;
        }

        public bool HasAttemptsLeft(DungeonData data)
        {
            if (data == null) return false;

            switch (data.dungeonType)
            {
                case DungeonType.Daily:
                    return attemptsUsedToday < data.maxAttemptsPerReset;
                case DungeonType.Weekly:
                    return attemptsUsedThisWeek < data.maxAttemptsPerReset;
                case DungeonType.Special:
                    return attemptsUsedToday < data.maxAttemptsPerReset;
                default:
                    return false;
            }
        }

        public int GetRemainingAttempts(DungeonData data)
        {
            if (data == null) return 0;

            switch (data.dungeonType)
            {
                case DungeonType.Daily:
                case DungeonType.Special:
                    return Mathf.Max(0, data.maxAttemptsPerReset - attemptsUsedToday);
                case DungeonType.Weekly:
                    return Mathf.Max(0, data.maxAttemptsPerReset - attemptsUsedThisWeek);
                default:
                    return 0;
            }
        }

        public void UseAttempt(DungeonData data)
        {
            if (data == null) return;

            switch (data.dungeonType)
            {
                case DungeonType.Daily:
                case DungeonType.Special:
                    attemptsUsedToday++;
                    break;
                case DungeonType.Weekly:
                    attemptsUsedThisWeek++;
                    break;
            }
        }

        public void RecordClear(DungeonDifficulty diff)
        {
            totalClears++;
            if (diff > highestClearedDifficulty)
                highestClearedDifficulty = diff;
        }
    }
}
