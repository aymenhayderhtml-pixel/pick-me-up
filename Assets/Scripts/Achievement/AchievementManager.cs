using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PickMeUp.Data;
using PickMeUp.Systems;
using PickMeUp.Inventory;

namespace PickMeUp.Achievement
{
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        [Header("Database")]
        public AchievementData[] achievementDatabase;

        [Header("Events")]
        public System.Action<AchievementData> OnAchievementCompleted;
        public System.Action OnAchievementListChanged;

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

        public AchievementData GetAchievementData(string id)
        {
            if (achievementDatabase == null) return null;
            return achievementDatabase.FirstOrDefault(a => a.achievementId == id);
        }

        public AchievementProgress GetProgress(string achievementId)
        {
            if (SaveManager.Instance == null) return null;

            var achievementProgress = SaveManager.Instance.AchievementProgress;
            var progress = achievementProgress.FirstOrDefault(p => p.achievementId == achievementId);
            if (progress == null)
            {
                progress = new AchievementProgress(achievementId);
                achievementProgress.Add(progress);
                SaveManager.Instance.SaveGame();
            }

            return progress;
        }

        // ===== TRIGGER METHODS =====
        public void TrackKill(string enemyId, int count = 1)
        {
            var matches = achievementDatabase?.Where(a => 
                a.triggerType == AchievementTrigger.KillEnemies && 
                (string.IsNullOrEmpty(a.targetId) || a.targetId == enemyId));

            foreach (var ach in matches ?? System.Array.Empty<AchievementData>())
                IncrementAchievement(ach.achievementId, count);
        }

        public void TrackFloorClear(int floorNumber)
        {
            var matches = achievementDatabase?.Where(a => 
                a.triggerType == AchievementTrigger.ClearFloors);

            foreach (var ach in matches ?? System.Array.Empty<AchievementData>())
                IncrementAchievement(ach.achievementId, 1);
        }

        public void TrackSummon(int rarity, int count = 1)
        {
            var matches = achievementDatabase?.Where(a => 
                a.triggerType == AchievementTrigger.SummonHeroes);

            foreach (var ach in matches ?? System.Array.Empty<AchievementData>())
                IncrementAchievement(ach.achievementId, count);
        }

        public void TrackLevelUp(int level)
        {
            var matches = achievementDatabase?.Where(a => 
                a.triggerType == AchievementTrigger.ReachLevel);

            foreach (var ach in matches ?? System.Array.Empty<AchievementData>())
            {
                var progress = GetProgress(ach.achievementId);
                if (progress != null && level > progress.currentCount)
                {
                    progress.currentCount = level;
                    CheckCompletion(ach);
                }
            }
        }

        public void TrackItemCollect(string itemId, int count = 1)
        {
            var matches = achievementDatabase?.Where(a => 
                a.triggerType == AchievementTrigger.CollectItems &&
                (string.IsNullOrEmpty(a.targetId) || a.targetId == itemId));

            foreach (var ach in matches ?? System.Array.Empty<AchievementData>())
                IncrementAchievement(ach.achievementId, count);
        }

        public void TrackLogin(int totalDays)
        {
            var matches = achievementDatabase?.Where(a => 
                a.triggerType == AchievementTrigger.LoginDays);

            foreach (var ach in matches ?? System.Array.Empty<AchievementData>())
            {
                var progress = GetProgress(ach.achievementId);
                if (progress != null && totalDays > progress.currentCount)
                {
                    progress.currentCount = totalDays;
                    CheckCompletion(ach);
                }
            }
        }

        public void TrackGoldSpend(int amount)
        {
            var matches = achievementDatabase?.Where(a => 
                a.triggerType == AchievementTrigger.SpendGold);

            foreach (var ach in matches ?? System.Array.Empty<AchievementData>())
                IncrementAchievement(ach.achievementId, amount);
        }

        public void TrackWinStreak(int streak)
        {
            var matches = achievementDatabase?.Where(a => 
                a.triggerType == AchievementTrigger.WinStreak);

            foreach (var ach in matches ?? System.Array.Empty<AchievementData>())
            {
                var progress = GetProgress(ach.achievementId);
                if (progress != null && streak > progress.currentCount)
                {
                    progress.currentCount = streak;
                    CheckCompletion(ach);
                }
            }
        }

        // ===== CORE LOGIC =====
        private void IncrementAchievement(string achievementId, int amount)
        {
            var ach = GetAchievementData(achievementId);
            if (ach == null) return;

            var progress = GetProgress(achievementId);
            if (progress == null || progress.isCompleted) return;

            progress.currentCount += amount;
            CheckCompletion(ach);

            SaveManager.Instance?.SaveGame();
        }

        private void CheckCompletion(AchievementData ach)
        {
            var progress = GetProgress(ach.achievementId);
            if (progress == null || progress.isCompleted) return;

            if (progress.currentCount >= ach.targetCount)
            {
                CompleteAchievement(ach);
            }

            OnAchievementListChanged?.Invoke();
        }

        private void CompleteAchievement(AchievementData ach)
        {
            var progress = GetProgress(ach.achievementId);
            if (progress == null) return;

            progress.isCompleted = true;
            progress.completedDate = System.DateTime.Now.ToString("yyyy-MM-dd");

            SaveManager.Instance?.SaveGame();
            OnAchievementCompleted?.Invoke(ach);
            OnAchievementListChanged?.Invoke();

            Debug.Log($"[Achievement] Completed: {ach.achievementName}");
        }

        public bool ClaimReward(string achievementId)
        {
            var progress = GetProgress(achievementId);
            var ach = GetAchievementData(achievementId);
            if (progress == null || ach == null || !progress.isCompleted || progress.rewardClaimed) 
                return false;

            if (SaveManager.Instance == null) return false;

            // Distribute rewards
            if (ach.gemReward > 0) SaveManager.Instance.Gems += ach.gemReward;
            if (ach.goldReward > 0) SaveManager.Instance.Gold += ach.goldReward;
            if (!string.IsNullOrEmpty(ach.itemRewardId))
                InventoryManager.Instance?.AddItem(ach.itemRewardId, ach.itemRewardCount);

            progress.rewardClaimed = true;
            SaveManager.Instance?.SaveGame();
            OnAchievementListChanged?.Invoke();

            return true;
        }

        public List<AchievementData> GetAchievementsByCategory(AchievementCategory category)
        {
            if (achievementDatabase == null) return new List<AchievementData>();
            return achievementDatabase.Where(a => a.category == category).OrderBy(a => a.displayOrder).ToList();
        }

        public int GetCompletedCount()
        {
            if (SaveManager.Instance == null) return 0;
            return SaveManager.Instance.AchievementProgress?.Count(p => p.isCompleted) ?? 0;
        }

        public int GetTotalCount()
        {
            return achievementDatabase?.Length ?? 0;
        }
    }
}
