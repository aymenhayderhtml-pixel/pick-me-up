using UnityEngine;

namespace PickMeUp.Data
{
    public enum AchievementCategory { Combat, Progress, Collection, Social, Special }
    public enum AchievementTrigger { KillEnemies, ClearFloors, SummonHeroes, ReachLevel, CollectItems, LoginDays, SpendGold, WinStreak }

    [CreateAssetMenu(fileName = "Achievement_X", menuName = "PickMeUp/Achievement")]
    public class AchievementData : ScriptableObject
    {
        [Header("Identity")]
        public string achievementId;
        public string achievementName;
        [TextArea(2, 3)]
        public string description;
        public AchievementCategory category;
        public Sprite icon;

        [Header("Trigger")]
        public AchievementTrigger triggerType;
        public string targetId; // specific enemy/hero/item ID if needed
        public int targetCount = 1; // how many to trigger completion

        [Header("Rewards")]
        public int gemReward = 0;
        public int goldReward = 0;
        public string itemRewardId = "";
        public int itemRewardCount = 0;
        public string titleReward = ""; // cosmetic title

        [Header("Tier")]
        public bool isHidden = false; // secret achievement
        public int displayOrder = 0;
        public string nextTierId = ""; // chain achievements (e.g., Kill 10 → Kill 100 → Kill 1000)
    }
}
