using System;
using UnityEngine;

namespace PickMeUp.Data
{
    [Serializable]
    public class AchievementProgress
    {
        public string achievementId;
        public int currentCount = 0;
        public bool isCompleted = false;
        public bool rewardClaimed = false;
        public string completedDate = ""; // yyyy-MM-dd

        public AchievementProgress() { }
        public AchievementProgress(string id)
        {
            achievementId = id;
        }

        public float GetProgressRatio(int targetCount)
        {
            if (targetCount <= 0) return 0f;
            return Mathf.Clamp01((float)currentCount / targetCount);
        }
    }
}
