using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using PickMeUp.Dungeon;
using PickMeUp.Data;

namespace PickMeUp.UI
{
    public class DungeonUIController : MonoBehaviour
    {
        [Header("Dungeon List")]
        public Transform dungeonListContainer;
        public GameObject dungeonCardPrefab;

        [Header("Detail Panel")]
        public GameObject detailPanel;
        public Image dungeonIcon;
        public TextMeshProUGUI dungeonNameText;
        public TextMeshProUGUI dungeonTypeText;
        public TextMeshProUGUI dungeonDescText;
        public TextMeshProUGUI attemptsText;
        public TextMeshProUGUI resetTimerText;
        public TextMeshProUGUI staminaCostText;
        public TextMeshProUGUI rewardPreviewText;

        [Header("Difficulty Selection")]
        public Transform difficultyContainer;
        public GameObject difficultyButtonPrefab;

        [Header("Action Buttons")]
        public Button enterButton;
        public Button backButton;
        public TextMeshProUGUI enterButtonText;
        public TextMeshProUGUI lockReasonText;

        [Header("Info Bar")]
        public TextMeshProUGUI staminaText;
        public TextMeshProUGUI goldText;

        [Header("Theme")]
        public ThemeConfig theme;

        private List<GameObject> dungeonCards = new List<GameObject>();
        private List<GameObject> difficultyButtons = new List<GameObject>();
        private DungeonData selectedDungeon;
        private DungeonDifficulty selectedDifficulty = DungeonDifficulty.Easy;

        private void OnEnable()
        {
            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance.OnDungeonListChanged += RefreshDungeonList;
                DungeonManager.Instance.OnAttemptUsed += RefreshDetailPanel;
            }

            if (SaveManager.Instance != null)
                SaveManager.Instance.OnDataChanged += RefreshInfoBar;

            RefreshDungeonList();
        }

        private void OnDisable()
        {
            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance.OnDungeonListChanged -= RefreshDungeonList;
                DungeonManager.Instance.OnAttemptUsed -= RefreshDetailPanel;
            }

            if (SaveManager.Instance != null)
                SaveManager.Instance.OnDataChanged -= RefreshInfoBar;
        }

        private void Start()
        {
            if (enterButton != null)
                enterButton.onClick.AddListener(OnEnterClicked);
            if (backButton != null)
                backButton.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby"));

            if (detailPanel != null) detailPanel.SetActive(false);
        }

        private void Update()
        {
            if (selectedDungeon != null && resetTimerText != null)
            {
                resetTimerText.text = $"Resets in: {DungeonManager.Instance?.GetResetCountdownText(selectedDungeon)}";
            }
        }

        private void RefreshDungeonList()
        {
            foreach (var card in dungeonCards)
            {
                if (card != null) Destroy(card);
            }
            dungeonCards.Clear();

            if (DungeonManager.Instance == null || dungeonListContainer == null || dungeonCardPrefab == null) return;

            foreach (var dungeon in DungeonManager.Instance.dungeonDatabase)
            {
                if (dungeon == null) continue;

                var card = Instantiate(dungeonCardPrefab, dungeonListContainer);
                dungeonCards.Add(card);

                bool isUnlocked = DungeonManager.Instance.IsDungeonUnlocked(dungeon);
                var progress = DungeonManager.Instance.GetProgress(dungeon.dungeonId);
                int remaining = progress?.GetRemainingAttempts(dungeon) ?? 0;

                var iconImg = card.transform.Find("Icon")?.GetComponent<Image>();
                if (iconImg != null)
                {
                    iconImg.sprite = dungeon.icon;
                    iconImg.color = isUnlocked ? Color.white : Color.gray;
                }

                var nameTxt = card.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                if (nameTxt != null)
                {
                    nameTxt.text = dungeon.dungeonName;
                    nameTxt.color = isUnlocked ? (dungeon.themeColor != Color.white ? dungeon.themeColor : Color.white) : Color.gray;
                }

                var typeTxt = card.transform.Find("Type")?.GetComponent<TextMeshProUGUI>();
                if (typeTxt != null)
                {
                    typeTxt.text = dungeon.dungeonType.ToString().ToUpper();
                    typeTxt.color = GetTypeColor(dungeon.dungeonType);
                }

                var attemptsTxt = card.transform.Find("Attempts")?.GetComponent<TextMeshProUGUI>();
                if (attemptsTxt != null)
                {
                    if (isUnlocked)
                    {
                        attemptsTxt.text = $"Attempts: {remaining}/{dungeon.maxAttemptsPerReset}";
                        attemptsTxt.color = remaining > 0 ? new Color(0.2f, 0.9f, 0.3f) : Color.red;
                    }
                    else
                    {
                        attemptsTxt.text = $"Unlock: Floor {dungeon.requiredHighestFloor}";
                        attemptsTxt.color = Color.red;
                    }
                }

                var bg = card.GetComponent<Image>();
                if (bg != null && theme != null)
                {
                    bg.color = isUnlocked ? theme.panelDark : theme.panelDark * 0.5f;
                }

                var btn = card.GetComponent<Button>();
                if (btn == null) btn = card.AddComponent<Button>();
                btn.interactable = isUnlocked;

                var capture = dungeon;
                btn.onClick.AddListener(() => SelectDungeon(capture));
            }

            RefreshInfoBar();
        }

        private void SelectDungeon(DungeonData dungeon)
        {
            selectedDungeon = dungeon;
            selectedDifficulty = DungeonDifficulty.Easy;

            if (detailPanel != null)
                detailPanel.SetActive(true);

            if (dungeonIcon != null)
            {
                dungeonIcon.sprite = dungeon.icon;
                dungeonIcon.color = Color.white;
            }

            if (dungeonNameText != null)
            {
                dungeonNameText.text = dungeon.dungeonName;
                dungeonNameText.color = dungeon.themeColor != Color.white ? dungeon.themeColor : Color.white;
            }

            if (dungeonTypeText != null)
            {
                dungeonTypeText.text = $"{dungeon.dungeonType} — {dungeon.rewardType} Rewards";
                dungeonTypeText.color = GetTypeColor(dungeon.dungeonType);
            }

            if (dungeonDescText != null)
                dungeonDescText.text = dungeon.description;

            if (staminaCostText != null)
                staminaCostText.text = $"Stamina: {dungeon.staminaCost}";

            BuildDifficultyButtons(dungeon);
            RefreshDetailPanel();
        }

        private void BuildDifficultyButtons(DungeonData dungeon)
        {
            foreach (var btn in difficultyButtons)
            {
                if (btn != null) Destroy(btn);
            }
            difficultyButtons.Clear();

            if (difficultyContainer == null || difficultyButtonPrefab == null) return;

            foreach (DungeonDifficulty diff in System.Enum.GetValues(typeof(DungeonDifficulty)))
            {
                if (!dungeon.IsDifficultyUnlocked(diff)) continue;

                var btn = Instantiate(difficultyButtonPrefab, difficultyContainer);
                difficultyButtons.Add(btn);

                var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.text = diff.ToString().ToUpper();
                    txt.fontSize = 16;
                }

                var img = btn.GetComponent<Image>();
                bool isSelected = diff == selectedDifficulty;
                if (img != null && theme != null)
                {
                    img.color = isSelected ? theme.accentGold : theme.panelDark;
                }

                var button = btn.GetComponent<Button>();
                if (button == null) button = btn.AddComponent<Button>();

                var captureDiff = diff;
                button.onClick.AddListener(() => SelectDifficulty(captureDiff));
            }
        }

        private void SelectDifficulty(DungeonDifficulty diff)
        {
            selectedDifficulty = diff;

            for (int i = 0; i < difficultyButtons.Count; i++)
            {
                var img = difficultyButtons[i]?.GetComponent<Image>();
                if (img != null && theme != null)
                {
                    var available = selectedDungeon?.availableDifficulties;
                    if (available != null && i < available.Length)
                    {
                        img.color = (available[i] == diff) ? theme.accentGold : theme.panelDark;
                    }
                }
            }

            RefreshDetailPanel();
        }

        private void RefreshDetailPanel()
        {
            if (selectedDungeon == null) return;

            var progress = DungeonManager.Instance?.GetProgress(selectedDungeon.dungeonId);
            int remaining = progress?.GetRemainingAttempts(selectedDungeon) ?? 0;

            if (attemptsText != null)
            {
                attemptsText.text = $"Attempts Left: {remaining}/{selectedDungeon.maxAttemptsPerReset}";
                attemptsText.color = remaining > 0 ? Color.white : Color.red;
            }

            if (rewardPreviewText != null)
            {
                string rewards = "Reward Preview:\n";
                int gold = selectedDungeon.GetScaledGold(selectedDifficulty);
                int exp = selectedDungeon.GetScaledEXP(selectedDifficulty);
                int gems = selectedDungeon.GetScaledGem(selectedDifficulty);

                if (gold > 0) rewards += $"  {gold} Gold\n";
                if (exp > 0) rewards += $"  {exp} EXP per hero\n";
                if (gems > 0) rewards += $"  {gems} Gems\n";
                if (selectedDungeon.possibleMaterialIds != null && selectedDungeon.possibleMaterialIds.Length > 0)
                {
                    rewards += $"  Materials (x{selectedDungeon.dropChance:P0} drop)\n";
                    foreach (var mat in selectedDungeon.possibleMaterialIds)
                        rewards += $"    - {mat}\n";
                }

                rewardPreviewText.text = rewards.TrimEnd('\n');
            }

            bool canEnter = DungeonManager.Instance?.CanEnterDungeon(selectedDungeon, selectedDifficulty) ?? false;
            if (enterButton != null)
            {
                enterButton.interactable = canEnter;
                var btnText = enterButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                    btnText.text = canEnter ? "ENTER DUNGEON" : "LOCKED";
            }

            if (lockReasonText != null)
            {
                if (canEnter)
                {
                    lockReasonText.text = "";
                }
                else
                {
                    lockReasonText.text = DungeonManager.Instance?.GetLockReason(selectedDungeon, selectedDifficulty) ?? "Locked";
                    lockReasonText.color = Color.red;
                }
            }
        }

        private void OnEnterClicked()
        {
            if (selectedDungeon == null) return;
            DungeonManager.Instance?.EnterDungeon(selectedDungeon, selectedDifficulty);
        }

        private void RefreshInfoBar()
        {
            if (SaveManager.Instance == null) return;

            if (staminaText != null)
                staminaText.text = $"Stamina: {SaveManager.Instance.CurrentStamina}";

            if (goldText != null)
                goldText.text = $"Gold: {SaveManager.Instance.Gold}";
        }

        private Color GetTypeColor(DungeonType type)
        {
            switch (type)
            {
                case DungeonType.Daily: return new Color(0.2f, 0.8f, 0.9f); // Cyan
                case DungeonType.Weekly: return new Color(0.9f, 0.4f, 0.9f); // Magenta
                case DungeonType.Special: return new Color(1f, 0.7f, 0.1f); // Gold
                default: return Color.white;
            }
        }
    }
}
