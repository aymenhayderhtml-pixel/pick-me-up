using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PickMeUp.Data;
using PickMeUp.Achievement;

namespace PickMeUp.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Database")]
        public ItemData[] itemDatabase;

        [Header("Events")]
        public System.Action OnInventoryChanged;
        public System.Action<RuntimeHero> OnHeroStatsChanged;

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

        public ItemData GetItemData(string itemId)
        {
            if (itemDatabase == null) return null;
            foreach (var item in itemDatabase)
            {
                if (item.itemId == itemId) return item;
            }
            return null;
        }

        // ===== ADD ITEMS =====
        public void AddItem(string itemId, int quantity = 1)
        {
            if (SaveManager.Instance == null) return;

            var inventory = SaveManager.Instance.Inventory;
            var existing = inventory.FirstOrDefault(i => i.itemId == itemId && !i.IsEquipped);

            if (existing != null && GetItemData(itemId)?.itemType == ItemType.Material)
            {
                existing.quantity += quantity;
            }
            else
            {
                for (int i = 0; i < quantity; i++)
                {
                    inventory.Add(new RuntimeItem(itemId, System.Guid.NewGuid().ToString().Substring(0, 8)));
                }
            }

            // Track for achievements
            AchievementManager.Instance?.TrackItemCollect(itemId, quantity);

            SaveManager.Instance.SaveGame();
            OnInventoryChanged?.Invoke();
        }

        public void RemoveItem(string instanceId, int quantity = 1)
        {
            if (SaveManager.Instance == null) return;

            var inventory = SaveManager.Instance.Inventory;
            var item = inventory.FirstOrDefault(i => i.instanceId == instanceId);
            if (item == null) return;

            if (item.quantity > quantity)
            {
                item.quantity -= quantity;
            }
            else
            {
                // Unequip if equipped
                if (item.IsEquipped)
                {
                    UnequipItem(item.instanceId);
                }
                inventory.Remove(item);
            }

            SaveManager.Instance.SaveGame();
            OnInventoryChanged?.Invoke();
        }

        // ===== EQUIP / UNEQUIP =====
        public bool EquipItem(string instanceId, string heroId)
        {
            if (SaveManager.Instance == null) return false;

            var inventory = SaveManager.Instance.Inventory;
            var item = inventory.FirstOrDefault(i => i.instanceId == instanceId);
            if (item == null) return false;

            var itemData = GetItemData(item.itemId);
            if (itemData == null) return false;

            var hero = SaveManager.Instance.ActiveRoster
                .FirstOrDefault(h => h.heroId == heroId);
            if (hero == null) return false;

            // Unequip existing item of same type
            string existingId = itemData.itemType switch
            {
                ItemType.Weapon => hero.weaponInstanceId,
                ItemType.Armor => hero.armorInstanceId,
                ItemType.Accessory => hero.accessoryInstanceId,
                _ => ""
            };

            if (!string.IsNullOrEmpty(existingId))
            {
                UnequipItem(existingId);
            }

            // Equip new
            item.equippedHeroId = heroId;
            switch (itemData.itemType)
            {
                case ItemType.Weapon: hero.weaponInstanceId = instanceId; break;
                case ItemType.Armor: hero.armorInstanceId = instanceId; break;
                case ItemType.Accessory: hero.accessoryInstanceId = instanceId; break;
            }

            RecalculateHeroStats(hero);
            SaveManager.Instance.SaveGame();
            OnInventoryChanged?.Invoke();
            OnHeroStatsChanged?.Invoke(hero);

            return true;
        }

        public bool UnequipItem(string instanceId)
        {
            if (SaveManager.Instance == null) return false;

            var inventory = SaveManager.Instance.Inventory;
            var item = inventory.FirstOrDefault(i => i.instanceId == instanceId);
            if (item == null || !item.IsEquipped) return false;

            var hero = SaveManager.Instance.ActiveRoster
                .FirstOrDefault(h => h.heroId == item.equippedHeroId);
            if (hero == null) return false;

            var itemData = GetItemData(item.itemId);
            if (itemData == null) return false;

            switch (itemData.itemType)
            {
                case ItemType.Weapon: hero.weaponInstanceId = ""; break;
                case ItemType.Armor: hero.armorInstanceId = ""; break;
                case ItemType.Accessory: hero.accessoryInstanceId = ""; break;
            }

            item.equippedHeroId = "";

            RecalculateHeroStats(hero);
            SaveManager.Instance.SaveGame();
            OnInventoryChanged?.Invoke();
            OnHeroStatsChanged?.Invoke(hero);

            return true;
        }

        public void RecalculateHeroStats(RuntimeHero hero)
        {
            if (hero == null) return;

            hero.RecalculateStats();

            var inventory = SaveManager.Instance?.Inventory;
            if (inventory == null) return;

            // Add equipment bonuses
            foreach (var slotId in new[] { hero.weaponInstanceId, hero.armorInstanceId, hero.accessoryInstanceId })
            {
                if (string.IsNullOrEmpty(slotId)) continue;

                var item = inventory.FirstOrDefault(i => i.instanceId == slotId);
                if (item == null) continue;

                var data = GetItemData(item.itemId);
                if (data == null) continue;

                float upgradeMult = Mathf.Pow(data.upgradeStatMultiplier, item.upgradeLevel);

                hero.AddEquipmentBonuses(
                    Mathf.FloorToInt(data.hpBonus * upgradeMult),
                    Mathf.FloorToInt(data.attackBonus * upgradeMult),
                    Mathf.FloorToInt(data.defenseBonus * upgradeMult),
                    Mathf.FloorToInt(data.speedBonus * upgradeMult),
                    Mathf.FloorToInt(data.critRateBonus * upgradeMult)
                );
            }

            // Clamp HP
            hero.currentHP = Mathf.Min(hero.currentHP, hero.maxHP);
        }

        // ===== UPGRADE =====
        public bool CanUpgradeItem(string instanceId)
        {
            if (SaveManager.Instance == null) return false;

            var inventory = SaveManager.Instance.Inventory;
            var item = inventory.FirstOrDefault(i => i.instanceId == instanceId);
            if (item == null) return false;

            var data = GetItemData(item.itemId);
            if (data == null) return false;

            return item.upgradeLevel < data.maxUpgradeLevel &&
                   SaveManager.Instance.Gold >= GetUpgradeCost(item, data);
        }

        public int GetUpgradeCost(RuntimeItem item, ItemData data)
        {
            return Mathf.FloorToInt(data.upgradeGoldCost * Mathf.Pow(1.5f, item.upgradeLevel));
        }

        public bool UpgradeItem(string instanceId)
        {
            if (!CanUpgradeItem(instanceId)) return false;

            var inventory = SaveManager.Instance.Inventory;
            var item = inventory.FirstOrDefault(i => i.instanceId == instanceId);
            var data = GetItemData(item.itemId);

            int cost = GetUpgradeCost(item, data);
            SaveManager.Instance.Gold -= cost;
            item.upgradeLevel++;

            // Recalculate hero stats if equipped
            if (item.IsEquipped)
            {
                var hero = SaveManager.Instance.ActiveRoster
                    .FirstOrDefault(h => h.heroId == item.equippedHeroId);
                if (hero != null)
                {
                    RecalculateHeroStats(hero);
                    OnHeroStatsChanged?.Invoke(hero);
                }
            }

            SaveManager.Instance.SaveGame();
            OnInventoryChanged?.Invoke();

            return true;
        }

        // ===== SELL =====
        public int GetSellValue(string instanceId)
        {
            var inventory = SaveManager.Instance?.Inventory;
            var item = inventory?.FirstOrDefault(i => i.instanceId == instanceId);
            if (item == null) return 0;

            var data = GetItemData(item.itemId);
            if (data == null) return 0;

            return Mathf.FloorToInt(data.sellValue * Mathf.Pow(1.2f, item.upgradeLevel)) * item.quantity;
        }

        public void SellItem(string instanceId)
        {
            if (SaveManager.Instance == null) return;

            int value = GetSellValue(instanceId);
            SaveManager.Instance.Gold += value;
            RemoveItem(instanceId);
        }

        // ===== HELPERS =====
        public List<RuntimeItem> GetItemsByType(ItemType type)
        {
            if (SaveManager.Instance == null) return new List<RuntimeItem>();
            return SaveManager.Instance.Inventory
                .Where(i => GetItemData(i.itemId)?.itemType == type)
                .ToList();
        }

        public List<RuntimeItem> GetEquippedItems(string heroId)
        {
            if (SaveManager.Instance == null) return new List<RuntimeItem>();
            return SaveManager.Instance.Inventory
                .Where(i => i.equippedHeroId == heroId)
                .ToList();
        }

        public RuntimeItem GetEquippedItem(string heroId, ItemType type)
        {
            if (SaveManager.Instance == null) return null;
            return SaveManager.Instance.Inventory
                .FirstOrDefault(i => i.equippedHeroId == heroId && GetItemData(i.itemId)?.itemType == type);
        }
    }
}
