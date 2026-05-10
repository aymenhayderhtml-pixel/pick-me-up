using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using PickMeUp.Inventory;
using PickMeUp.Data;

namespace PickMeUp.UI
{
    public class HeroRosterUIController : MonoBehaviour
    {
        [Header("Roster List")]
        public Transform rosterContainer;
        public GameObject heroButtonPrefab;

        [Header("Hero Detail Panel")]
        public GameObject detailPanel;
        public TextMeshProUGUI heroNameText;
        public TextMeshProUGUI heroRarityText;
        public TextMeshProUGUI heroLevelText;
        public TextMeshProUGUI expText;
        public Slider expSlider;

        [Header("Stats Display")]
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI attackText;
        public TextMeshProUGUI defenseText;
        public TextMeshProUGUI speedText;
        public TextMeshProUGUI critText;

        [Header("Equipment Slots")]
        public Button weaponSlot;
        public Button armorSlot;
        public Button accessorySlot;
        public TextMeshProUGUI weaponText;
        public TextMeshProUGUI armorText;
        public TextMeshProUGUI accessoryText;

        [Header("Actions")]
        public Button upgradeButton;
        public Button backButton;
        public Button inventoryToggleButton;

        [Header("Inventory Panel")]
        public GameObject inventoryPanel;
        public InventoryUIController inventoryUI;

        [Header("Theme")]
        public ThemeConfig theme;

        private List<GameObject> heroButtons = new List<GameObject>();
        private RuntimeHero selectedHero;
        private bool showInventory = false;

        private void OnEnable()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnHeroStatsChanged += OnHeroStatsChanged;

            if (SaveManager.Instance != null)
                SaveManager.Instance.OnDataChanged += RefreshRoster;

            RefreshRoster();
        }

        private void OnDisable()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnHeroStatsChanged -= OnHeroStatsChanged;

            if (SaveManager.Instance != null)
                SaveManager.Instance.OnDataChanged -= RefreshRoster;
        }

        private void Start()
        {
            if (weaponSlot != null)
                weaponSlot.onClick.AddListener(() => OnEquipSlotClicked(ItemType.Weapon));
            if (armorSlot != null)
                armorSlot.onClick.AddListener(() => OnEquipSlotClicked(ItemType.Armor));
            if (accessorySlot != null)
                accessorySlot.onClick.AddListener(() => OnEquipSlotClicked(ItemType.Accessory));

            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradeHeroClicked);

            if (backButton != null)
                backButton.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby"));

            if (inventoryToggleButton != null)
                inventoryToggleButton.onClick.AddListener(ToggleInventory);

            if (detailPanel != null) detailPanel.SetActive(false);
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
        }

        private void RefreshRoster()
        {
            foreach (var btn in heroButtons)
            {
                if (btn != null) Destroy(btn);
            }
            heroButtons.Clear();

            if (SaveManager.Instance == null || rosterContainer == null || heroButtonPrefab == null) return;

            var roster = SaveManager.Instance.ActiveRoster;
            if (roster == null) return;

            foreach (var hero in roster)
            {
                var btn = Instantiate(heroButtonPrefab, rosterContainer);
                heroButtons.Add(btn);

                var nameTxt = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (nameTxt != null)
                {
                    nameTxt.text = $"{hero.heroName} Lv.{hero.level}";
                    nameTxt.color = hero.GetRarityColor();
                }

                var bg = btn.GetComponent<Image>();
                if (bg != null && theme != null)
                    bg.color = theme.panelDark;

                var button = btn.GetComponent<Button>();
                if (button == null) button = btn.AddComponent<Button>();

                var capture = hero;
                button.onClick.AddListener(() => SelectHero(capture));
            }

            if (selectedHero != null)
            {
                var updated = roster.FirstOrDefault(h => h.heroId == selectedHero.heroId);
                if (updated != null) SelectHero(updated);
            }
        }

        private void SelectHero(RuntimeHero hero)
        {
            selectedHero = hero;

            if (detailPanel != null)
                detailPanel.SetActive(true);

            if (heroNameText != null)
            {
                heroNameText.text = hero.heroName;
                heroNameText.color = hero.GetRarityColor();
            }

            if (heroRarityText != null)
                heroRarityText.text = hero.GetRarityStars();

            if (heroLevelText != null)
                heroLevelText.text = $"Level {hero.level}";

            if (expText != null)
                expText.text = $"EXP: {hero.exp} / {hero.expToNextLevel}";

            if (expSlider != null)
            {
                expSlider.maxValue = hero.expToNextLevel;
                expSlider.value = hero.exp;
            }

            RefreshStats(hero);
            RefreshEquipment(hero);

            // Highlight selected button
            for (int i = 0; i < heroButtons.Count; i++)
            {
                var bg = heroButtons[i]?.GetComponent<Image>();
                if (bg != null && theme != null)
                {
                    var roster = SaveManager.Instance?.ActiveRoster;
                    if (roster != null && i < roster.Count)
                    {
                        bg.color = (roster[i].heroId == hero.heroId) 
                            ? theme.accentGold 
                            : theme.panelDark;
                    }
                }
            }
        }

        private void RefreshStats(RuntimeHero hero)
        {
            if (hpText != null) hpText.text = $"HP: {hero.maxHP}";
            if (attackText != null) attackText.text = $"ATK: {hero.attack}";
            if (defenseText != null) defenseText.text = $"DEF: {hero.defense}";
            if (speedText != null) speedText.text = $"SPD: {hero.speed}";
            if (critText != null) critText.text = $"CRIT: {hero.critRate}%";
        }

        private void RefreshEquipment(RuntimeHero hero)
        {
            RefreshSlot(hero.weaponInstanceId, weaponSlot, weaponText, "Weapon");
            RefreshSlot(hero.armorInstanceId, armorSlot, armorText, "Armor");
            RefreshSlot(hero.accessoryInstanceId, accessorySlot, accessoryText, "Accessory");
        }

        private void RefreshSlot(string instanceId, Button slot, TextMeshProUGUI text, string defaultLabel)
        {
            if (slot == null || text == null) return;

            if (string.IsNullOrEmpty(instanceId))
            {
                text.text = $"[{defaultLabel}]";
                text.color = Color.gray;
                var img = slot.GetComponent<Image>();
                if (img != null && theme != null) img.color = theme.panelDark * 0.7f;
                return;
            }

            var item = SaveManager.Instance?.Inventory?.FirstOrDefault(i => i.instanceId == instanceId);
            var data = item != null ? InventoryManager.Instance?.GetItemData(item.itemId) : null;

            if (data != null)
            {
                text.text = $"{data.itemName} +{item.upgradeLevel}";
                text.color = data.GetRarityColor();
                var img = slot.GetComponent<Image>();
                if (img != null) img.color = data.GetRarityColor() * 0.3f;
            }
            else
            {
                text.text = $"[{defaultLabel}]";
                text.color = Color.gray;
            }
        }

        private void OnEquipSlotClicked(ItemType type)
        {
            if (selectedHero == null) return;

            // Show inventory filtered to this slot type
            ToggleInventory(true);
            if (inventoryUI != null)
            {
                inventoryUI.SetFilter(type, false);
            }
        }

        private void OnUpgradeHeroClicked()
        {
            // Placeholder: In a full system, this would use hero-specific upgrade materials
            Debug.Log("[HeroRosterUI] Hero upgrade via materials — implement with upgrade items.");
        }

        private void ToggleInventory()
        {
            showInventory = !showInventory;
            if (inventoryPanel != null)
                inventoryPanel.SetActive(showInventory);
        }

        private void ToggleInventory(bool show)
        {
            showInventory = show;
            if (inventoryPanel != null)
                inventoryPanel.SetActive(show);
        }

        private void OnHeroStatsChanged(RuntimeHero hero)
        {
            if (selectedHero != null && hero.heroId == selectedHero.heroId)
            {
                SelectHero(hero);
            }
        }
    }
}
