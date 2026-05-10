using UnityEngine;

namespace PickMeUp.Data
{
    [CreateAssetMenu(fileName = "DailyRewardCalendar", menuName = "PickMeUp/Daily Reward Calendar")]
    public class DailyRewardData : ScriptableObject
    {
        [Header("Calendar")]
        public DailyRewardEntry[] rewards; // 28-day calendar (4 weeks)

        [Header("Bonus")]
        public int weekCompleteBonusGems = 50;
        public int monthCompleteBonusGems = 200;

        [Header("Missed Days")]
        public bool allowMissedDayCatchup = true;
        public int catchupGemCost = 20; // per missed day

        public DailyRewardEntry GetReward(int day)
        {
            if (day < 1 || day > rewards.Length) return null;
            return rewards[day - 1];
        }
    }

    [System.Serializable]
    public class DailyRewardEntry
    {
        public string rewardName;
        public Sprite icon;
        public RewardType type;
        public int amount;
        public string itemId; // for Item rewards
        public bool isBonusDay; // Day 7, 14, 21, 28

        public string GetDisplayText()
        {
            switch (type)
            {
                case RewardType.Gold: return $"{amount} Gold";
                case RewardType.Gem: return $"{amount} Gems";
                case RewardType.Stamina: return $"{amount} Stamina";
                case RewardType.Item: return $"{rewardName} x{amount}";
                default: return rewardName;
            }
        }
    }

    public enum RewardType { Gold, Gem, Stamina, Item }
}
