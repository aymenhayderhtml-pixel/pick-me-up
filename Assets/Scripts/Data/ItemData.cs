using UnityEngine;

namespace PickMeUp.Data
{
    public enum ItemType { Weapon, Armor, Accessory, Consumable, Material }
    public enum ItemRarity { Common = 1, Rare = 2, Epic = 3, Legendary = 4 }

    [CreateAssetMenu(fileName = "Item_X", menuName = "PickMeUp/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;
        public string itemName;
        public ItemType itemType;
        public ItemRarity rarity;
        [TextArea(2, 3)]
        public string description;
        public Sprite icon;

        [Header("Stats")]
        public int hpBonus;
        public int attackBonus;
        public int defenseBonus;
        public int speedBonus;
        public int critRateBonus;

        [Header("Upgrade")]
        public int maxUpgradeLevel = 5;
        public int upgradeGoldCost = 100;
        public float upgradeStatMultiplier = 1.15f;

        [Header("Sell")]
        public int sellValue = 50;

        public string GetRarityStars()
        {
            string stars = "";
            for (int i = 0; i < (int)rarity; i++) stars += "*";
            return stars;
        }

        public Color GetRarityColor()
        {
            switch (rarity)
            {
                case ItemRarity.Common: return new Color(0.7f, 0.7f, 0.7f);
                case ItemRarity.Rare: return new Color(0.2f, 0.6f, 1f);
                case ItemRarity.Epic: return new Color(0.8f, 0.2f, 0.9f);
                case ItemRarity.Legendary: return new Color(1f, 0.8f, 0.1f);
                default: return Color.white;
            }
        }
    }
}
