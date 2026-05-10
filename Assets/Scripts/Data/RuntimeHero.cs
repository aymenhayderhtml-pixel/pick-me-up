using System;
using System.Collections.Generic;
using UnityEngine;
using PickMeUp.Achievement;

namespace PickMeUp.Data
{
    [Serializable]
    public class RuntimeHero
    {
        [Header("Identity")]
        public string runtimeId;
        public string heroId;
        public string heroName;
        public int starRating; // 1-5

        [Header("Level & EXP")]
        public int level = 1;
        public int exp = 0;
        public int expToNextLevel = 100;
        public int maxLevel = 80;

        [Header("Base Stats")]
        public int baseMaxHP = 100;
        public int baseAttack = 10;
        public int baseDefense = 5;
        public int baseSpeed = 10;
        public int baseCritRate = 5;

        [Header("Combat Stats (calculated from base + gear)")]
        public int maxHP;
        public int currentHP;
        public int attack;
        public int defense;
        public int speed;
        public int critRate;

        [Header("Skill")]
        public string skillName = "Power Strike";
        public float skillMultiplier = 1.5f;
        public int skillCooldown = 3;

        [Header("Equipment")]
        public string weaponInstanceId = "";
        public string armorInstanceId = "";
        public string accessoryInstanceId = "";

        [Header("State")]
        public bool isInParty;
        public int partySlot;
        public bool isActive = true;

        public RuntimeHero() { }

        public RuntimeHero(string heroId, string heroName, int stars)
        {
            this.runtimeId = Guid.NewGuid().ToString().Substring(0, 8);
            this.heroId = heroId;
            this.heroName = heroName;
            this.starRating = stars;
            this.level = 1;
            this.exp = 0;
            this.expToNextLevel = 100;

            // Base stats scale with rarity
            float rarityMult = 1f + (stars - 3) * 0.25f; // Adjust multiplier based on stars (3 star is baseline)
            baseMaxHP = Mathf.FloorToInt(100 * rarityMult);
            baseAttack = Mathf.FloorToInt(10 * rarityMult);
            baseDefense = Mathf.FloorToInt(5 * rarityMult);
            baseSpeed = Mathf.FloorToInt(10 * rarityMult);
            baseCritRate = 5 + (stars - 3) * 2;

            RecalculateStats();
            currentHP = maxHP;
        }

        public void RecalculateStats()
        {
            maxHP = baseMaxHP + Mathf.FloorToInt(baseMaxHP * 0.05f * (level - 1));
            attack = baseAttack + Mathf.FloorToInt(baseAttack * 0.05f * (level - 1));
            defense = baseDefense + Mathf.FloorToInt(baseDefense * 0.04f * (level - 1));
            speed = baseSpeed + Mathf.FloorToInt(baseSpeed * 0.03f * (level - 1));
            critRate = baseCritRate + (level - 1);

            // Equipment bonuses added by InventoryManager later
        }

        public void LevelUp()
        {
            level++;
            AchievementManager.Instance?.TrackLevelUp(level);
            
            expToNextLevel = Mathf.FloorToInt(expToNextLevel * 1.2f);
            RecalculateStats();
            currentHP = maxHP; // Full heal on level up
        }

        public void AddEquipmentBonuses(int hp, int atk, int def, int spd, int crit)
        {
            maxHP += hp;
            attack += atk;
            defense += def;
            speed += spd;
            critRate += crit;
        }

        public string GetRarityStars()
        {
            string stars = "";
            for (int i = 0; i < starRating; i++) stars += "*";
            return stars;
        }

        public Color GetRarityColor()
        {
            switch (starRating)
            {
                case 3: return new Color(0.2f, 0.6f, 1f); // Rare
                case 4: return new Color(0.8f, 0.2f, 0.9f); // Epic
                case 5: return new Color(1f, 0.8f, 0.1f); // Legendary
                default: return Color.white;
            }
        }
    }
}
