using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PickMeUp.Tower;
using PickMeUp.Data;

namespace PickMeUp.UI
{
    public class TowerUIController : MonoBehaviour
    {
        [Header("Floor List")]
        public Transform floorListContainer;
        public GameObject floorButtonPrefab;

        [Header("Floor Detail Panel")]
        public GameObject detailPanel;
        public TextMeshProUGUI floorNameText;
        public TextMeshProUGUI floorDescriptionText;
        public TextMeshProUGUI recommendedLevelText;
        public TextMeshProUGUI staminaCostText;
        public TextMeshProUGUI rewardText;
        public TextMeshProUGUI enemyInfoText;

        [Header("Action Buttons")]
        public Button enterButton;
        public Button backButton;

        [Header("Info Bar")]
        public TextMeshProUGUI currentFloorText;
        public TextMeshProUGUI maxFloorText;
        public TextMeshProUGUI staminaText;

        [Header("Theme")]
        public ThemeConfig theme;

        private List<GameObject> floorButtons = new List<GameObject>();
        private TowerFloorData selectedFloorData;

        private void OnEnable()
        {
            if (TowerManager.Instance != null)
            {
                TowerManager.Instance.OnFloorSelected += OnFloorSelected;
                TowerManager.Instance.OnTowerStateChanged += RefreshUI;
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnDataChanged += RefreshInfoBar;
            }

            RefreshUI();
        }

        private void OnDisable()
        {
            if (TowerManager.Instance != null)
            {
                TowerManager.Instance.OnFloorSelected -= OnFloorSelected;
                TowerManager.Instance.OnTowerStateChanged -= RefreshUI;
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnDataChanged -= RefreshInfoBar;
            }
        }

        private void Start()
        {
            if (enterButton != null)
                enterButton.onClick.AddListener(OnEnterClicked);

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }

        private void RefreshUI()
        {
            BuildFloorList();
            RefreshInfoBar();

            if (selectedFloorData == null && TowerManager.Instance != null)
            {
                SelectFloor(TowerManager.Instance.currentSelectedFloor);
            }
        }

        private void BuildFloorList()
        {
            // Clear existing
            foreach (var btn in floorButtons)
            {
                if (btn != null) Destroy(btn);
            }
            floorButtons.Clear();

            if (TowerManager.Instance == null || floorListContainer == null || floorButtonPrefab == null) 
                return;

            int maxFloor = TowerManager.Instance.maxUnlockedFloor;

            for (int i = 1; i <= maxFloor; i++)
            {
                var btn = Instantiate(floorButtonPrefab, floorListContainer);
                floorButtons.Add(btn);

                int floorNum = i; // Capture for closure
                var button = btn.GetComponent<Button>();
                var nameText = btn.GetComponentInChildren<TextMeshProUGUI>();

                var data = TowerManager.Instance.GetFloorData(i);
                string displayName = data != null ? data.GetDisplayName() : $"Floor {i}";

                if (nameText != null)
                    nameText.text = displayName;

                // Visual states
                bool isCompleted = TowerManager.Instance.IsFloorCompleted(i);
                bool isCurrent = i == TowerManager.Instance.currentSelectedFloor;

                var img = btn.GetComponent<Image>();
                if (img != null && theme != null)
                {
                    img.color = isCurrent ? theme.accentGold : 
                               (isCompleted ? theme.backgroundDark : theme.panelDark);
                }

                if (button != null)
                {
                    button.onClick.AddListener(() => SelectFloor(floorNum));
                    button.interactable = true;
                }
            }
        }

        private void SelectFloor(int floorNumber)
        {
            TowerManager.Instance?.SelectFloor(floorNumber);
        }

        private void OnFloorSelected(int floorNumber)
        {
            selectedFloorData = TowerManager.Instance?.GetFloorData(floorNumber);

            if (detailPanel != null)
                detailPanel.SetActive(selectedFloorData != null);

            if (selectedFloorData == null) return;

            if (floorNameText != null)
                floorNameText.text = selectedFloorData.GetDisplayName();

            if (floorDescriptionText != null)
                floorDescriptionText.text = selectedFloorData.description;

            if (recommendedLevelText != null)
                recommendedLevelText.text = $"Rec. Level: {selectedFloorData.recommendedLevel}";

            if (staminaCostText != null)
                staminaCostText.text = $"Stamina: {selectedFloorData.staminaCost}";

            if (rewardText != null)
                rewardText.text = $"Rewards: {selectedFloorData.goldReward} Gold, {selectedFloorData.expReward} EXP";

            if (enemyInfoText != null)
            {
                string enemies = "Enemies: ";
                if (selectedFloorData.enemyIds != null)
                {
                    for (int i = 0; i < selectedFloorData.enemyIds.Length; i++)
                    {
                        enemies += $"{selectedFloorData.enemyCounts[i]}x {selectedFloorData.enemyIds[i]} (Lv.{selectedFloorData.enemyLevels[i]}) ";
                    }
                }
                enemyInfoText.text = enemies;
            }

            // Update enter button state
            if (enterButton != null && TowerManager.Instance != null)
            {
                bool canEnter = TowerManager.Instance.CanEnterFloor(floorNumber);
                enterButton.interactable = canEnter;

                var btnText = enterButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                    btnText.text = canEnter ? "ENTER FLOOR" : "LOCKED";
            }

            // Rebuild list to update highlighting
            BuildFloorList();
        }

        private void RefreshInfoBar()
        {
            if (SaveManager.Instance == null) return;

            if (currentFloorText != null)
                currentFloorText.text = $"Current: {SaveManager.Instance.CurrentFloor}";

            if (maxFloorText != null)
                maxFloorText.text = $"Highest: {SaveManager.Instance.HighestFloor}";

            if (staminaText != null)
                staminaText.text = $"Stamina: {SaveManager.Instance.CurrentStamina}";
        }

        private void OnEnterClicked()
        {
            if (TowerManager.Instance != null && selectedFloorData != null)
            {
                TowerManager.Instance.EnterFloor(selectedFloorData.floorNumber);
            }
        }

        private void OnBackClicked()
        {
            // Return to lobby
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }
    }
}
