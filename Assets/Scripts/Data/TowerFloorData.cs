using UnityEngine;

namespace PickMeUp.Data
{
    [CreateAssetMenu(fileName = "TowerFloor_X", menuName = "PickMeUp/Tower Floor")]
    public class TowerFloorData : ScriptableObject
    {
        [Header("Floor Info")]
        public int floorNumber;
        public string floorName;
        [TextArea(2, 4)]
        public string description;

        [Header("Enemy Setup")]
        public string[] enemyIds;
        public int[] enemyLevels;
        public int[] enemyCounts;

        [Header("Difficulty")]
        public int recommendedLevel = 1;
        public float difficultyMultiplier = 1.0f;

        [Header("Rewards")]
        public int goldReward = 100;
        public int expReward = 50;
        public string[] possibleLootIds;

        [Header("Stamina")]
        public int staminaCost = 5;

        public string GetDisplayName()
        {
            return string.IsNullOrEmpty(floorName) ? $"Floor {floorNumber}" : $"Floor {floorNumber}: {floorName}";
        }
    }
}
