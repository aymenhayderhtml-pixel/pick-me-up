using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PickMeUp.Daily;
using PickMeUp.Data;
using PickMeUp.Systems;

namespace PickMeUp.UI
{
    public class DailyRewardUIController : MonoBehaviour
    {
        [Header("Calendar Grid")]
        public Transform calendarGrid;
        public GameObject daySlotPrefab;
        public int daysPerRow = 7;

        [Header("Claim Panel")]
        public GameObject claimPanel;
        public Image rewardIcon;
        public TextMeshProUGUI rewardNameText;
        public TextMeshProUGUI rewardAmountText;
        public Button claimButton;
        public Button catchupButton;
        public TextMeshProUGUI catchupCostText;

        [Header("Streak Display")]
        public TextMeshProUGUI streakText;
        public TextMeshProUGUI streakBonusText;

        [Header("Info")]
        public TextMeshProUGUI currentDayText;
        public TextMeshProUGUI nextRewardText;

        [Header("Theme")]
        public ThemeConfig theme;

        private List<GameObject> daySlots = new List<GameObject>();
        private int selectedDay = 0;

        private void OnEnable()
        {
            if (DailyRewardManager.Instance != null)
            {
                DailyRewardManager.Instance.OnDayClaimed += OnDayClaimed;
                DailyRewardManager.Instance.OnStreakUpdated += OnStreakUpdated;
            }

            BuildCalendar();
            RefreshUI();
        }

        private void OnDisable()
        {
            if (DailyRewardManager.Instance != null)
            {
                DailyRewardManager.Instance.OnDayClaimed -= OnDayClaimed;
                DailyRewardManager.Instance.OnStreakUpdated -= OnStreakUpdated;
            }
        }

        private void Start()
        {
            if (claimButton != null)
                claimButton.onClick.AddListener(OnClaimClicked);
            if (catchupButton != null)
                catchupButton.onClick.AddListener(OnCatchUpClicked);

            if (claimPanel != null) claimPanel.SetActive(false);
        }

        private void BuildCalendar()
        {
            foreach (var slot in daySlots)
            {
                if (slot != null) Destroy(slot);
            }
            daySlots.Clear();

            if (DailyRewardManager.Instance == null || DailyRewardManager.Instance.calendarData == null) return;
            if (calendarGrid == null || daySlotPrefab == null) return;

            var calendar = DailyRewardManager.Instance.calendarData;
            int currentDay = DailyRewardManager.Instance.GetCurrentDay();

            for (int i = 1; i <= calendar.rewards.Length; i++)
            {
                var slot = Instantiate(daySlotPrefab, calendarGrid);
                daySlots.Add(slot);

                var entry = calendar.GetReward(i);
                if (entry == null) continue;

                bool isClaimed = DailyRewardManager.Instance.IsDayClaimed(i);
                bool isToday = i == currentDay;
                bool isPast = i < currentDay;
                bool isFuture = i > currentDay;
                bool isMissed = isPast && !isClaimed;

                var dayNum = slot.transform.Find("DayNumber")?.GetComponent<TextMeshProUGUI>();
                if (dayNum != null)
                {
                    dayNum.text = $"Day {i}";
                    dayNum.color = isToday ? theme?.accentGold ?? Color.yellow : Color.white;
                }

                var icon = slot.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null)
                {
                    icon.sprite = entry.icon;
                    icon.color = isClaimed ? new Color(1, 1, 1, 0.3f) : Color.white;
                }

                var rewardTxt = slot.transform.Find("RewardText")?.GetComponent<TextMeshProUGUI>();
                if (rewardTxt != null)
                {
                    rewardTxt.text = entry.GetDisplayText();
                    rewardTxt.color = isClaimed ? Color.gray : Color.white;
                    if (entry.isBonusDay) rewardTxt.color = new Color(1f, 0.8f, 0.1f);
                }

                var status = slot.transform.Find("Status")?.GetComponent<TextMeshProUGUI>();
                if (status != null)
                {
                    if (isClaimed) { status.text = "CLAIMED"; status.color = Color.green; }
                    else if (isMissed) { status.text = "MISSED"; status.color = Color.red; }
                    else if (isToday) { status.text = "TODAY"; status.color = Color.yellow; }
                    else status.text = "";
                }

                var bg = slot.GetComponent<Image>();
                if (bg != null && theme != null)
                {
                    if (isToday) bg.color = theme.accentGold * 0.4f;
                    else if (isMissed) bg.color = new Color(0.3f, 0.1f, 0.1f, 0.8f);
                    else if (isClaimed) bg.color = theme.panelDark * 0.6f;
                    else bg.color = theme.panelDark;
                }

                var btn = slot.GetComponent<Button>();
                if (btn == null) btn = slot.AddComponent<Button>();
                btn.interactable = !isClaimed && !isFuture;

                int captureDay = i;
                btn.onClick.AddListener(() => SelectDay(captureDay));
            }

            RefreshUI();
        }

        private void SelectDay(int day)
        {
            selectedDay = day;
            var entry = DailyRewardManager.Instance?.calendarData?.GetReward(day);
            if (entry == null) return;

            if (claimPanel != null)
                claimPanel.SetActive(true);

            if (rewardIcon != null)
            {
                rewardIcon.sprite = entry.icon;
                rewardIcon.color = Color.white;
            }

            if (rewardNameText != null)
                rewardNameText.text = entry.rewardName;

            if (rewardAmountText != null)
                rewardAmountText.text = entry.GetDisplayText();

            bool canClaim = DailyRewardManager.Instance?.CanClaimToday() ?? false;
            bool isToday = day == DailyRewardManager.Instance?.GetCurrentDay();
            bool canCatchUp = DailyRewardManager.Instance?.CanCatchUp(day) ?? false;

            if (claimButton != null)
            {
                claimButton.gameObject.SetActive(isToday);
                claimButton.interactable = canClaim;
                var claimTxt = claimButton.GetComponentInChildren<TextMeshProUGUI>();
                if (claimTxt != null)
                    claimTxt.text = canClaim ? "CLAIM" : "CLAIMED";
            }

            if (catchupButton != null)
            {
                catchupButton.gameObject.SetActive(!isToday && canCatchUp);
                catchupButton.interactable = canCatchUp;
            }

            if (catchupCostText != null)
            {
                var calendar = DailyRewardManager.Instance?.calendarData;
                catchupCostText.text = calendar != null ? $"Catch up: {calendar.catchupGemCost} Gems" : "";
                catchupCostText.gameObject.SetActive(!isToday && canCatchUp);
            }
        }

        private void OnClaimClicked()
        {
            if (DailyRewardManager.Instance?.ClaimReward(selectedDay) ?? false)
            {
                BuildCalendar();
            }
        }

        private void OnCatchUpClicked()
        {
            if (DailyRewardManager.Instance?.CatchUp(selectedDay) ?? false)
            {
                BuildCalendar();
            }
        }

        private void OnDayClaimed(int day)
        {
            BuildCalendar();
        }

        private void OnStreakUpdated(int streak)
        {
            if (streakText != null)
                streakText.text = $"Streak: {streak} days";

            if (streakBonusText != null)
            {
                if (streak >= 7) streakBonusText.text = "7-day bonus active! +20% rewards";
                else if (streak >= 3) streakBonusText.text = "3-day bonus active! +10% rewards";
                else streakBonusText.text = "";
            }
        }

        private void RefreshUI()
        {
            int currentDay = DailyRewardManager.Instance?.GetCurrentDay() ?? 1;
            int streak = DailyRewardManager.Instance?.GetStreak() ?? 0;

            if (currentDayText != null)
                currentDayText.text = $"Day {currentDay}";

            if (nextRewardText != null)
            {
                var entry = DailyRewardManager.Instance?.calendarData?.GetReward(currentDay);
                nextRewardText.text = entry != null ? $"Next: {entry.GetDisplayText()}" : "Calendar complete!";
            }

            if (streakText != null)
                streakText.text = $"Streak: {streak} days";
        }
    }
}
