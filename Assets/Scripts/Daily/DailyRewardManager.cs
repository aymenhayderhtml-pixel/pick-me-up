using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using PickMeUp.Data;
using PickMeUp.Systems;
using PickMeUp.Inventory;
using PickMeUp.Achievement;

namespace PickMeUp.Daily
{
    public class DailyRewardManager : MonoBehaviour
    {
        public static DailyRewardManager Instance { get; private set; }

        [Header("Calendar Data")]
        public DailyRewardData calendarData;

        [Header("Events")]
        public System.Action<int> OnDayClaimed; // day number
        public System.Action<int> OnStreakUpdated; // current streak
        public System.Action OnCalendarCompleted;

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
            ProcessLogin();
        }

        public void ProcessLogin()
        {
            if (SaveManager.Instance == null || calendarData == null) return;

            string today = DateTime.Now.ToString("yyyy-MM-dd");

            // First login ever
            if (string.IsNullOrEmpty(SaveManager.Instance.LastLoginDate))
            {
                SaveManager.Instance.LastLoginDate = today;
                SaveManager.Instance.CurrentLoginDay = 1;
                SaveManager.Instance.LoginStreak = 1;
                SaveManager.Instance.ClaimedDays = new List<int>();
                SaveManager.Instance.SaveGame();
                OnStreakUpdated?.Invoke(SaveManager.Instance.LoginStreak);
                AchievementManager.Instance?.TrackLogin(SaveManager.Instance.LoginStreak);
                return;
            }

            // Already logged in today
            if (SaveManager.Instance.LastLoginDate == today)
            {
                OnStreakUpdated?.Invoke(SaveManager.Instance.LoginStreak);
                return;
            }

            // Check streak
            DateTime lastLogin = DateTime.Parse(SaveManager.Instance.LastLoginDate);
            DateTime yesterday = DateTime.Now.AddDays(-1);

            if (lastLogin.Date == yesterday.Date)
            {
                // Consecutive day
                SaveManager.Instance.LoginStreak++;
            }
            else
            {
                // Streak broken
                SaveManager.Instance.LoginStreak = 1;
            }

            // Advance day (cycle after 28)
            SaveManager.Instance.CurrentLoginDay++;
            if (SaveManager.Instance.CurrentLoginDay > calendarData.rewards.Length)
            {
                SaveManager.Instance.CurrentLoginDay = 1;
                SaveManager.Instance.ClaimedDays.Clear();
                OnCalendarCompleted?.Invoke();
            }

            SaveManager.Instance.LastLoginDate = today;
            SaveManager.Instance.SaveGame();
            OnStreakUpdated?.Invoke(SaveManager.Instance.LoginStreak);
            AchievementManager.Instance?.TrackLogin(SaveManager.Instance.LoginStreak);

            Debug.Log($"[DailyReward] Day {SaveManager.Instance.CurrentLoginDay}, Streak: {SaveManager.Instance.LoginStreak}");
        }

        public bool CanClaimToday()
        {
            if (SaveManager.Instance == null || calendarData == null) return false;

            string today = DateTime.Now.ToString("yyyy-MM-dd");

            // Must have logged in today and not claimed
            return SaveManager.Instance.LastLoginDate == today && 
                   !SaveManager.Instance.ClaimedDays.Contains(SaveManager.Instance.CurrentLoginDay);
        }

        public bool ClaimReward(int day)
        {
            if (!CanClaimToday() && day == SaveManager.Instance.CurrentLoginDay) return false;
            if (SaveManager.Instance == null || calendarData == null) return false;

            var entry = calendarData.GetReward(day);
            if (entry == null) return false;

            // Distribute
            switch (entry.type)
            {
                case RewardType.Gold:
                    SaveManager.Instance.Gold += entry.amount;
                    break;
                case RewardType.Gem:
                    SaveManager.Instance.Gems += entry.amount;
                    break;
                case RewardType.Stamina:
                    StaminaManager.Instance?.AddStamina(entry.amount, true);
                    break;
                case RewardType.Item:
                    if (!string.IsNullOrEmpty(entry.itemId))
                    {
                        InventoryManager.Instance?.AddItem(entry.itemId, entry.amount);
                    }
                    break;
            }

            if (!SaveManager.Instance.ClaimedDays.Contains(day))
                SaveManager.Instance.ClaimedDays.Add(day);

            // Check week bonus
            if (entry.isBonusDay && day % 7 == 0)
            {
                SaveManager.Instance.Gems += calendarData.weekCompleteBonusGems;
                Debug.Log($"[DailyReward] Week complete bonus: +{calendarData.weekCompleteBonusGems} Gems");
            }

            // Check month bonus
            if (day == calendarData.rewards.Length)
            {
                SaveManager.Instance.Gems += calendarData.monthCompleteBonusGems;
                Debug.Log($"[DailyReward] Month complete bonus: +{calendarData.monthCompleteBonusGems} Gems");
            }

            SaveManager.Instance.SaveGame();
            OnDayClaimed?.Invoke(day);

            return true;
        }

        public bool CanCatchUp(int day)
        {
            if (SaveManager.Instance == null || calendarData == null) return false;
            if (!calendarData.allowMissedDayCatchup) return false;

            string today = DateTime.Now.ToString("yyyy-MM-dd");

            // Can catch up if: logged in today, day is before current, not claimed
            return SaveManager.Instance.LastLoginDate == today &&
                   day < SaveManager.Instance.CurrentLoginDay &&
                   !SaveManager.Instance.ClaimedDays.Contains(day) &&
                   SaveManager.Instance.Gems >= calendarData.catchupGemCost;
        }

        public bool CatchUp(int day)
        {
            if (!CanCatchUp(day)) return false;

            SaveManager.Instance.Gems -= calendarData.catchupGemCost;

            // Claims the specific day
            bool success = ClaimReward(day);

            return success;
        }

        public int GetCurrentDay() => SaveManager.Instance?.CurrentLoginDay ?? 1;
        public int GetStreak() => SaveManager.Instance?.LoginStreak ?? 0;
        public bool IsDayClaimed(int day) => SaveManager.Instance?.ClaimedDays?.Contains(day) ?? false;

        public List<int> GetMissedDays()
        {
            var result = new List<int>();
            if (SaveManager.Instance == null) return result;

            int current = SaveManager.Instance.CurrentLoginDay;

            for (int i = 1; i < current; i++)
            {
                if (!SaveManager.Instance.ClaimedDays.Contains(i))
                    result.Add(i);
            }

            return result;
        }
    }
}
