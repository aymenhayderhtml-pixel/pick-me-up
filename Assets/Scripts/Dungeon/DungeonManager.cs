using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using PickMeUp.Data;
using PickMeUp.Inventory;
using PickMeUp.Systems;

namespace PickMeUp.Dungeon
{
    public class DungeonManager : MonoBehaviour
    {
        public static DungeonManager Instance { get; private set; }

        [Header("Database")]
        public DungeonData[] dungeonDatabase;

        [Header("Current Selection")]
        public DungeonData selectedDungeon;
        public DungeonDifficulty selectedDifficulty = DungeonDifficulty.Easy;

        [Header("Events")]
        public System.Action OnDungeonListChanged;
        public System.Action OnAttemptUsed;
        public System.Action<DungeonData, DungeonDifficulty, List<string>> OnDungeonCleared;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ProcessResets();
        }

        public DungeonData GetDungeonData(string dungeonId)
        {
            if (dungeonDatabase == null) return null;
            return dungeonDatabase.FirstOrDefault(d => d.dungeonId == dungeonId);
        }

        // ===== RESET LOGIC =====
        public void ProcessResets()
        {
            if (SaveManager.Instance == null) return;

            var dungeonProgress = SaveManager.Instance.DungeonProgress;
            if (dungeonProgress == null) return;

            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string thisWeek = GetWeekKey(DateTime.Now);

            foreach (var progress in dungeonProgress)
            {
                var data = GetDungeonData(progress.dungeonId);
                if (data == null) continue;

                // Daily reset
                if (data.dungeonType == DungeonType.Daily || data.dungeonType == DungeonType.Special)
                {
                    if (progress.lastDailyResetDate != today)
                    {
                        progress.attemptsUsedToday = 0;
                        progress.lastDailyResetDate = today;
                    }
                }

                // Weekly reset
                if (data.dungeonType == DungeonType.Weekly)
                {
                    if (progress.lastWeeklyResetDate != thisWeek)
                    {
                        progress.attemptsUsedThisWeek = 0;
                        progress.lastWeeklyResetDate = thisWeek;
                    }
                }
            }

            SaveManager.Instance.SaveGame();
            OnDungeonListChanged?.Invoke();
        }

        private string GetWeekKey(DateTime date)
        {
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            int week = cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            return $"{date.Year}-W{week:D2}";
        }

        public DungeonProgress GetProgress(string dungeonId)
        {
            if (SaveManager.Instance == null) return null;

            var dungeonProgress = SaveManager.Instance.DungeonProgress;
            var progress = dungeonProgress.FirstOrDefault(p => p.dungeonId == dungeonId);
            if (progress == null)
            {
                progress = new DungeonProgress(dungeonId);
                dungeonProgress.Add(progress);
                SaveManager.Instance.SaveGame();
            }

            return progress;
        }

        // ===== VALIDATION =====
        public bool IsDungeonUnlocked(DungeonData data)
        {
            if (data == null || SaveManager.Instance == null) return false;
            return SaveManager.Instance.HighestFloor >= data.requiredHighestFloor;
        }

        public bool CanEnterDungeon(DungeonData data, DungeonDifficulty diff)
        {
            if (data == null || SaveManager.Instance == null) return false;

            var progress = GetProgress(data.dungeonId);
            if (progress == null) return false;

            return IsDungeonUnlocked(data) &&
                   data.IsDifficultyUnlocked(diff) &&
                   progress.HasAttemptsLeft(data) &&
                   (StaminaManager.Instance != null ? StaminaManager.Instance.GetCurrentStamina() >= data.staminaCost : SaveManager.Instance.CurrentStamina >= data.staminaCost);
        }

        public string GetLockReason(DungeonData data, DungeonDifficulty diff)
        {
            if (data == null) return "Invalid dungeon.";
            if (!IsDungeonUnlocked(data)) return $"Clear Floor {data.requiredHighestFloor} to unlock.";
            if (!data.IsDifficultyUnlocked(diff)) return "Difficulty not available.";

            var progress = GetProgress(data.dungeonId);
            if (progress != null && !progress.HasAttemptsLeft(data))
                return "No attempts remaining. Resets soon.";

            if (StaminaManager.Instance != null && StaminaManager.Instance.GetCurrentStamina() < data.staminaCost)
                return $"Need {data.staminaCost} Stamina.";
            else if (SaveManager.Instance != null && SaveManager.Instance.CurrentStamina < data.staminaCost)
                return $"Need {data.staminaCost} Stamina.";

            return "";
        }

        // ===== ENTER / REWARD =====
        public void EnterDungeon(DungeonData data, DungeonDifficulty diff)
        {
            if (!CanEnterDungeon(data, diff)) return;

            selectedDungeon = data;
            selectedDifficulty = diff;

            // Deduct stamina
            if (StaminaManager.Instance != null)
                StaminaManager.Instance.UseStamina(data.staminaCost);
            else
                SaveManager.Instance.CurrentStamina -= data.staminaCost;

            // Use attempt
            var progress = GetProgress(data.dungeonId);
            progress.UseAttempt(data);

            SaveManager.Instance.SaveGame();
            OnAttemptUsed?.Invoke();

            Debug.Log($"[DungeonManager] Entering {data.dungeonName} [{diff}]...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Combat");
        }

        public void DistributeRewards(DungeonData data, DungeonDifficulty diff)
        {
            if (data == null || SaveManager.Instance == null) return;

            var rewardsGiven = new List<string>();

            // Gold
            int gold = data.GetScaledGold(diff);
            if (gold > 0)
            {
                SaveManager.Instance.Gold += gold;
                rewardsGiven.Add($"{gold} Gold");
            }

            // Gems
            int gems = data.GetScaledGem(diff);
            if (gems > 0)
            {
                SaveManager.Instance.Gems += gems;
                rewardsGiven.Add($"{gems} Gems");
            }

            // EXP to active roster
            int exp = data.GetScaledEXP(diff);
            if (exp > 0)
            {
                foreach (var hero in SaveManager.Instance.ActiveRoster)
                {
                    hero.exp += exp;
                    while (hero.exp >= hero.expToNextLevel)
                    {
                        hero.exp -= hero.expToNextLevel;
                        hero.LevelUp();
                    }
                }
                rewardsGiven.Add($"{exp} EXP per hero");
            }

            // Materials
            if (data.possibleMaterialIds != null && data.possibleMaterialIds.Length > 0)
            {
                for (int i = 0; i < data.possibleMaterialIds.Length; i++)
                {
                    if (UnityEngine.Random.value > data.dropChance) continue;

                    int count = data.materialDropCounts != null && i < data.materialDropCounts.Length 
                        ? data.materialDropCounts[i] 
                        : 1;

                    int scaledCount = Mathf.FloorToInt(count * data.GetDifficultyMultiplier(diff));

                    InventoryManager.Instance?.AddItem(data.possibleMaterialIds[i], scaledCount);
                    rewardsGiven.Add($"{scaledCount}x {data.possibleMaterialIds[i]}");
                }
            }

            // Record clear
            var progress = GetProgress(data.dungeonId);
            progress.RecordClear(diff);

            SaveManager.Instance.SaveGame();
            OnDungeonCleared?.Invoke(data, diff, rewardsGiven);

            Debug.Log($"[DungeonManager] Rewards: {string.Join(", ", rewardsGiven)}");
        }

        // ===== HELPERS =====
        public TimeSpan GetTimeUntilReset(DungeonData data)
        {
            if (data == null) return TimeSpan.Zero;
            DateTime now = DateTime.Now;

            if (data.dungeonType == DungeonType.Daily || data.dungeonType == DungeonType.Special)
            {
                DateTime nextReset = now.Date.AddHours(data.resetHour);
                if (now >= nextReset)
                    nextReset = nextReset.AddDays(1);
                return nextReset - now;
            }
            else if (data.dungeonType == DungeonType.Weekly)
            {
                int daysUntil = ((int)data.weeklyResetDay - (int)now.DayOfWeek + 7) % 7;
                if (daysUntil == 0 && now.Hour >= data.resetHour)
                    daysUntil = 7;
                DateTime nextReset = now.Date.AddDays(daysUntil).AddHours(data.resetHour);
                return nextReset - now;
            }

            return TimeSpan.Zero;
        }

        public string GetResetCountdownText(DungeonData data)
        {
            var time = GetTimeUntilReset(data);
            if (time.TotalHours >= 24)
                return $"{time.Days}d {time.Hours}h";
            return $"{time.Hours}h {time.Minutes}m";
        }
    }
}
