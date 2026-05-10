using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using PickMeUp.Achievement;
using PickMeUp.Data;
using PickMeUp.Systems;

namespace PickMeUp.UI
{
    public class AchievementUIController : MonoBehaviour
    {
        [Header("Category Tabs")]
        public Button allTab;
        public Button combatTab;
        public Button progressTab;
        public Button collectionTab;
        public Button specialTab;

        [Header("Achievement List")]
        public Transform listContainer;
        public GameObject achievementRowPrefab;

        [Header("Detail Panel")]
        public GameObject detailPanel;
        public Image detailIcon;
        public TextMeshProUGUI detailName;
        public TextMeshProUGUI detailDesc;
        public TextMeshProUGUI detailProgress;
        public Slider detailProgressBar;
        public TextMeshProUGUI detailReward;
        public Button claimButton;

        [Header("Summary")]
        public TextMeshProUGUI completionText;
        public TextMeshProUGUI totalRewardText;

        [Header("Theme")]
        public ThemeConfig theme;

        private AchievementCategory currentFilter = AchievementCategory.Combat;
        private bool showAll = true;
        private List<GameObject> rowObjects = new List<GameObject>();
        private AchievementData selectedAchievement;

        private void OnEnable()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementCompleted += OnAchievementCompleted;
                AchievementManager.Instance.OnAchievementListChanged += RefreshList;
            }

            RefreshList();
        }

        private void OnDisable()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementCompleted -= OnAchievementCompleted;
                AchievementManager.Instance.OnAchievementListChanged -= RefreshList;
            }
        }

        private void Start()
        {
            if (allTab != null) allTab.onClick.AddListener(() => SetFilter(AchievementCategory.Combat, true));
            if (combatTab != null) combatTab.onClick.AddListener(() => SetFilter(AchievementCategory.Combat, false));
            if (progressTab != null) progressTab.onClick.AddListener(() => SetFilter(AchievementCategory.Progress, false));
            if (collectionTab != null) collectionTab.onClick.AddListener(() => SetFilter(AchievementCategory.Collection, false));
            if (specialTab != null) specialTab.onClick.AddListener(() => SetFilter(AchievementCategory.Special, false));

            if (claimButton != null)
                claimButton.onClick.AddListener(OnClaimClicked);

            if (detailPanel != null) detailPanel.SetActive(false);
        }

        private void SetFilter(AchievementCategory category, bool all)
        {
            showAll = all;
            currentFilter = category;
            RefreshList();
            UpdateTabColors();
        }

        private void UpdateTabColors()
        {
            var tabs = new[] { allTab, combatTab, progressTab, collectionTab, specialTab };
            var cats = new[] { AchievementCategory.Combat, AchievementCategory.Combat, AchievementCategory.Progress, AchievementCategory.Collection, AchievementCategory.Special };

            for (int i = 0; i < tabs.Length; i++)
            {
                if (tabs[i] == null) continue;
                var img = tabs[i].GetComponent<Image>();
                if (img == null) continue;

                bool active = (i == 0 && showAll) || (i > 0 && !showAll && currentFilter == cats[i]);
                img.color = active ? (theme?.accentGold ?? new Color(0.9f, 0.7f, 0.2f)) : (theme?.panelDark ?? new Color(0.2f, 0.2f, 0.25f));
            }
        }

        private void RefreshList()
        {
            foreach (var row in rowObjects)
            {
                if (row != null) Destroy(row);
            }
            rowObjects.Clear();

            if (AchievementManager.Instance == null || listContainer == null || achievementRowPrefab == null) return;

            var achievements = showAll 
                ? AchievementManager.Instance.achievementDatabase?.ToList() ?? new List<AchievementData>()
                : AchievementManager.Instance.GetAchievementsByCategory(currentFilter);

            if (achievements == null) return;

            foreach (var ach in achievements.OrderBy(a => a.isHidden).ThenBy(a => a.displayOrder))
            {
                var achProgress = AchievementManager.Instance.GetProgress(ach.achievementId);

                if (ach.isHidden)
                {
                    if (achProgress == null || !achProgress.isCompleted) continue; // Hide until completed
                }

                var row = Instantiate(achievementRowPrefab, listContainer);
                rowObjects.Add(row);

                bool isCompleted = achProgress?.isCompleted ?? false;
                bool rewardClaimed = achProgress?.rewardClaimed ?? false;
                float ratio = achProgress?.GetProgressRatio(ach.targetCount) ?? 0f;

                var icon = row.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null)
                {
                    icon.sprite = ach.icon;
                    icon.color = isCompleted ? Color.white : new Color(1, 1, 1, 0.5f);
                }

                var nameTxt = row.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                if (nameTxt != null)
                {
                    nameTxt.text = ach.achievementName;
                    nameTxt.color = isCompleted ? new Color(0.9f, 0.7f, 0.2f) : Color.white;
                }

                var descTxt = row.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
                if (descTxt != null)
                {
                    descTxt.text = ach.description;
                    descTxt.color = isCompleted ? Color.gray : new Color(0.8f, 0.8f, 0.8f);
                }

                // Progress bar
                var progressBar = row.transform.Find("ProgressBar")?.GetComponent<Slider>();
                if (progressBar != null)
                {
                    progressBar.value = ratio;
                    progressBar.gameObject.SetActive(!isCompleted);
                }

                var progressText = row.transform.Find("ProgressText")?.GetComponent<TextMeshProUGUI>();
                if (progressText != null)
                {
                    progressText.text = isCompleted ? "COMPLETED" : $"{achProgress?.currentCount ?? 0} / {ach.targetCount}";
                    progressText.color = isCompleted ? Color.green : Color.white;
                }

                var rewardIndicator = row.transform.Find("RewardIndicator")?.gameObject;
                if (rewardIndicator != null)
                    rewardIndicator.SetActive(isCompleted && !rewardClaimed);

                var bg = row.GetComponent<Image>();
                if (bg != null && theme != null)
                {
                    bg.color = isCompleted ? theme.panelDark * 0.7f : theme.panelDark;
                }

                var btn = row.GetComponent<Button>();
                if (btn == null) btn = row.AddComponent<Button>();

                var capture = ach;
                btn.onClick.AddListener(() => SelectAchievement(capture));
            }

            UpdateTabColors();
            RefreshSummary();
        }

        private void SelectAchievement(AchievementData ach)
        {
            selectedAchievement = ach;
            var progress = AchievementManager.Instance?.GetProgress(ach.achievementId);
            if (progress == null) return;

            if (detailPanel != null)
                detailPanel.SetActive(true);

            if (detailIcon != null)
            {
                detailIcon.sprite = ach.icon;
                detailIcon.color = progress.isCompleted ? Color.white : new Color(1, 1, 1, 0.5f);
            }

            if (detailName != null)
            {
                detailName.text = ach.achievementName;
                detailName.color = progress.isCompleted ? new Color(0.9f, 0.7f, 0.2f) : Color.white;
            }

            if (detailDesc != null)
                detailDesc.text = ach.description;

            if (detailProgress != null)
                detailProgress.text = progress.isCompleted ? "COMPLETED" : $"Progress: {progress.currentCount} / {ach.targetCount}";

            if (detailProgressBar != null)
            {
                detailProgressBar.value = progress.GetProgressRatio(ach.targetCount);
                detailProgressBar.gameObject.SetActive(!progress.isCompleted);
            }

            if (detailReward != null)
            {
                string rewards = "Rewards:\n";
                if (ach.gemReward > 0) rewards += $"  {ach.gemReward} Gems\n";
                if (ach.goldReward > 0) rewards += $"  {ach.goldReward} Gold\n";
                if (!string.IsNullOrEmpty(ach.itemRewardId)) rewards += $"  {ach.itemRewardCount}x Item\n";
                if (!string.IsNullOrEmpty(ach.titleReward)) rewards += $"  Title: {ach.titleReward}\n";
                detailReward.text = rewards.TrimEnd('\n');
            }

            if (claimButton != null)
            {
                claimButton.gameObject.SetActive(progress.isCompleted && !progress.rewardClaimed);
                claimButton.interactable = !progress.rewardClaimed;
                var claimTxt = claimButton.GetComponentInChildren<TextMeshProUGUI>();
                if (claimTxt != null)
                    claimTxt.text = progress.rewardClaimed ? "CLAIMED" : "CLAIM";
            }
        }

        private void OnClaimClicked()
        {
            if (selectedAchievement == null) return;

            if (AchievementManager.Instance?.ClaimReward(selectedAchievement.achievementId) ?? false)
            {
                SelectAchievement(selectedAchievement);
                RefreshList();
            }
        }

        private void OnAchievementCompleted(AchievementData ach)
        {
            RefreshList();
            SelectAchievement(ach);
        }

        private void RefreshSummary()
        {
            if (AchievementManager.Instance == null) return;

            int completed = AchievementManager.Instance.GetCompletedCount();
            int total = AchievementManager.Instance.GetTotalCount();

            if (completionText != null)
                completionText.text = $"Completed: {completed} / {total}";

            if (totalRewardText != null)
            {
                int totalGems = 0;
                int totalGold = 0;
                foreach (var ach in AchievementManager.Instance.achievementDatabase ?? System.Array.Empty<AchievementData>())
                {
                    var progress = AchievementManager.Instance.GetProgress(ach.achievementId);
                    if (progress != null && progress.isCompleted && !progress.rewardClaimed)
                    {
                        totalGems += ach.gemReward;
                        totalGold += ach.goldReward;
                    }
                }
                totalRewardText.text = $"Unclaimed: {totalGems} Gems, {totalGold} Gold";
            }
        }
    }
}
