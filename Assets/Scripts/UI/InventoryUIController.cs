using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using PickMeUp.Inventory;
using PickMeUp.Data;

namespace PickMeUp.UI
{
    public class InventoryUIController : MonoBehaviour
    {
        [Header("Inventory Grid")]
        public Transform inventoryGrid;
        public GameObject itemSlotPrefab;
        public int slotsPerRow = 6;

        [Header("Item Detail Panel")]
        public GameObject detailPanel;
        public Image detailIcon;
        public TextMeshProUGUI detailName;
        public TextMeshProUGUI detailRarity;
        public TextMeshProUGUI detailType;
        public TextMeshProUGUI detailDescription;
        public TextMeshProUGUI detailStats;
        public TextMeshProUGUI detailUpgradeLevel;
        public Button equipButton;
        public Button upgradeButton;
        public Button sellButton;
        public Button unequipButton;

        [Header("Filter Tabs")]
        public Button allTab;
        public Button weaponTab;
        public Button armorTab;
        public Button accessoryTab;
        public Button materialTab;

        [Header("Info Bar")]
        public TextMeshProUGUI goldText;
        public TextMeshProUGUI itemCountText;

        [Header("Theme")]
        public ThemeConfig theme;

        private List<GameObject> slotObjects = new List<GameObject>();
        private RuntimeItem selectedItem;
        private ItemType currentFilter = ItemType.Weapon; // default, but All overrides
        private bool showAll = true;

        private void OnEnable()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryChanged += RefreshInventory;

            if (SaveManager.Instance != null)
                SaveManager.Instance.OnDataChanged += RefreshInfoBar;

            RefreshInventory();
        }

        private void OnDisable()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryChanged -= RefreshInventory;

            if (SaveManager.Instance != null)
                SaveManager.Instance.OnDataChanged -= RefreshInfoBar;
        }

        private void Start()
        {
            if (allTab != null) allTab.onClick.AddListener(() => SetFilter(ItemType.Weapon, true));
            if (weaponTab != null) weaponTab.onClick.AddListener(() => SetFilter(ItemType.Weapon, false));
            if (armorTab != null) armorTab.onClick.AddListener(() => SetFilter(ItemType.Armor, false));
            if (accessoryTab != null) accessoryTab.onClick.AddListener(() => SetFilter(ItemType.Accessory, false));
            if (materialTab != null) materialTab.onClick.AddListener(() => SetFilter(ItemType.Material, false));

            if (equipButton != null) equipButton.onClick.AddListener(OnEquipClicked);
            if (unequipButton != null) unequipButton.onClick.AddListener(OnUnequipClicked);
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
            if (sellButton != null) sellButton.onClick.AddListener(OnSellClicked);

            if (detailPanel != null) detailPanel.SetActive(false);
        }

        public void SetFilter(ItemType type, bool all)
        {
            showAll = all;
            currentFilter = type;
            RefreshInventory();
            UpdateTabColors();
        }

        private void UpdateTabColors()
        {
            var tabs = new[] { allTab, weaponTab, armorTab, accessoryTab, materialTab };
            var types = new[] { ItemType.Weapon, ItemType.Weapon, ItemType.Armor, ItemType.Accessory, ItemType.Material };

            for (int i = 0; i < tabs.Length; i++)
            {
                if (tabs[i] == null) continue;
                var img = tabs[i].GetComponent<Image>();
                if (img == null) continue;

                bool active = (i == 0 && showAll) || (i > 0 && !showAll && currentFilter == types[i]);
                img.color = active ? (theme?.accentGold ?? new Color(0.9f, 0.7f, 0.2f)) : (theme?.panelDark ?? new Color(0.2f, 0.2f, 0.25f));
            }
        }

        private void RefreshInventory()
        {
            // Clear existing
            foreach (var slot in slotObjects)
            {
                if (slot != null) Destroy(slot);
            }
            slotObjects.Clear();

            if (SaveManager.Instance == null) return;

            var inventory = SaveManager.Instance.Inventory;
            var filtered = showAll 
                ? inventory 
                : inventory.Where(i => InventoryManager.Instance?.GetItemData(i.itemId)?.itemType == currentFilter).ToList();

            if (inventoryGrid == null || itemSlotPrefab == null) return;

            foreach (var item in filtered)
            {
                var data = InventoryManager.Instance?.GetItemData(item.itemId);
                if (data == null) continue;

                var slot = Instantiate(itemSlotPrefab, inventoryGrid);
                slotObjects.Add(slot);

                // Setup slot visuals
                var iconImg = slot.transform.Find("Icon")?.GetComponent<Image>();
                if (iconImg != null)
                {
                    iconImg.sprite = data.icon;
                    iconImg.color = data.icon != null ? Color.white : Color.clear;
                }

                var rarityImg = slot.transform.Find("RarityBorder")?.GetComponent<Image>();
                if (rarityImg != null)
                    rarityImg.color = data.GetRarityColor();

                var nameTxt = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                if (nameTxt != null)
                    nameTxt.text = data.itemName;

                var qtyTxt = slot.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
                if (qtyTxt != null)
                    qtyTxt.text = item.quantity > 1 ? $"x{item.quantity}" : "";

                var equippedIcon = slot.transform.Find("EquippedIcon")?.gameObject;
                if (equippedIcon != null)
                    equippedIcon.SetActive(item.IsEquipped);

                var upgradeTxt = slot.transform.Find("UpgradeLevel")?.GetComponent<TextMeshProUGUI>();
                if (upgradeTxt != null)
                    upgradeTxt.text = item.upgradeLevel > 0 ? $"+{item.upgradeLevel}" : "";

                // Button
                var btn = slot.GetComponent<Button>();
                if (btn == null) btn = slot.AddComponent<Button>();

                var capture = item;
                btn.onClick.AddListener(() => SelectItem(capture));

                // Background color
                var bg = slot.GetComponent<Image>();
                if (bg != null && theme != null)
                    bg.color = theme.panelDark;
            }

            RefreshInfoBar();
            UpdateTabColors();
        }

        private void SelectItem(RuntimeItem item)
        {
            selectedItem = item;
            var data = InventoryManager.Instance?.GetItemData(item.itemId);
            if (data == null) return;

            if (detailPanel != null)
                detailPanel.SetActive(true);

            if (detailIcon != null)
            {
                detailIcon.sprite = data.icon;
                detailIcon.color = data.icon != null ? Color.white : Color.clear;
            }

            if (detailName != null)
            {
                detailName.text = data.itemName;
                detailName.color = data.GetRarityColor();
            }

            if (detailRarity != null)
                detailRarity.text = $"{data.GetRarityStars()} {data.rarity}";

            if (detailType != null)
                detailType.text = data.itemType.ToString().ToUpper();

            if (detailDescription != null)
                detailDescription.text = data.description;

            if (detailStats != null)
            {
                float mult = Mathf.Pow(data.upgradeStatMultiplier, item.upgradeLevel);
                string stats = "";
                if (data.hpBonus > 0) stats += $"HP +{Mathf.FloorToInt(data.hpBonus * mult)}\n";
                if (data.attackBonus > 0) stats += $"ATK +{Mathf.FloorToInt(data.attackBonus * mult)}\n";
                if (data.defenseBonus > 0) stats += $"DEF +{Mathf.FloorToInt(data.defenseBonus * mult)}\n";
                if (data.speedBonus > 0) stats += $"SPD +{Mathf.FloorToInt(data.speedBonus * mult)}\n";
                if (data.critRateBonus > 0) stats += $"CRIT +{Mathf.FloorToInt(data.critRateBonus * mult)}%\n";
                detailStats.text = string.IsNullOrEmpty(stats) ? "No stat bonuses." : stats.TrimEnd('\n');
            }

            if (detailUpgradeLevel != null)
                detailUpgradeLevel.text = $"Upgrade: +{item.upgradeLevel} / {data.maxUpgradeLevel}";

            // Button states
            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(!item.IsEquipped && data.itemType != ItemType.Material && data.itemType != ItemType.Consumable);
            }

            if (unequipButton != null)
            {
                unequipButton.gameObject.SetActive(item.IsEquipped);
            }

            if (upgradeButton != null)
            {
                bool canUpgrade = InventoryManager.Instance?.CanUpgradeItem(item.instanceId) ?? false;
                upgradeButton.interactable = canUpgrade;
                upgradeButton.gameObject.SetActive(data.itemType != ItemType.Material && data.itemType != ItemType.Consumable);

                var upText = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (upText != null)
                {
                    int cost = InventoryManager.Instance?.GetUpgradeCost(item, data) ?? 0;
                    upText.text = $"UPGRADE ({cost} G)";
                }
            }

            if (sellButton != null)
            {
                var sellText = sellButton.GetComponentInChildren<TextMeshProUGUI>();
                if (sellText != null)
                {
                    int value = InventoryManager.Instance?.GetSellValue(item.instanceId) ?? 0;
                    sellText.text = $"SELL ({value} G)";
                }
            }
        }

        private void OnEquipClicked()
        {
            if (selectedItem == null) return;

            // For now, equip to first hero. In full version, show hero picker.
            var hero = SaveManager.Instance?.ActiveRoster?.FirstOrDefault();
            if (hero == null) return;

            InventoryManager.Instance?.EquipItem(selectedItem.instanceId, hero.heroId);
            SelectItem(selectedItem); // Refresh detail panel
        }

        private void OnUnequipClicked()
        {
            if (selectedItem == null) return;
            InventoryManager.Instance?.UnequipItem(selectedItem.instanceId);
            SelectItem(selectedItem);
        }

        private void OnUpgradeClicked()
        {
            if (selectedItem == null) return;
            bool success = InventoryManager.Instance?.UpgradeItem(selectedItem.instanceId) ?? false;
            if (success)
            {
                SelectItem(selectedItem);
            }
        }

        private void OnSellClicked()
        {
            if (selectedItem == null) return;
            InventoryManager.Instance?.SellItem(selectedItem.instanceId);
            selectedItem = null;
            if (detailPanel != null) detailPanel.SetActive(false);
        }

        private void RefreshInfoBar()
        {
            if (SaveManager.Instance == null) return;

            if (goldText != null)
                goldText.text = $"Gold: {SaveManager.Instance.Gold:N0}";

            if (itemCountText != null)
                itemCountText.text = $"Items: {SaveManager.Instance.Inventory?.Count ?? 0}";
        }
    }
}
