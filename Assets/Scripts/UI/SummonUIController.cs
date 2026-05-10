using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using PickMeUp.Data;
using PickMeUp.Achievement;

public class SummonUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultsContentText;
    
    [Header("Settings")]
    [SerializeField] private int singleCost = 100;
    [SerializeField] private int multiCost = 1000;

    private void Start()
    {
        UpdateGemsDisplay();
        if (resultsPanel != null) resultsPanel.SetActive(false);
    }

    public void UpdateGemsDisplay()
    {
        if (gemsText != null && GameDataBridge.Instance != null)
        {
            gemsText.text = $"Gems: {GameDataBridge.Instance.GetGems():N0}";
        }
    }

    public void Summon1x()
    {
        Debug.Log("[Summon] Summon1x clicked!");
        PerformSummon(1, singleCost);
    }

    public void Summon10x()
    {
        Debug.Log("[Summon] Summon10x clicked!");
        PerformSummon(10, multiCost);
    }

    private void PerformSummon(int count, int cost)
    {
        if (SaveManager.Instance == null) return;

        if (SaveManager.Instance.Gems < cost)
        {
            Debug.LogWarning("[Summon] Not enough gems!");
            // TODO: Show "Not enough gems" feedback
            return;
        }

        // Deduct gems
        SaveManager.Instance.Gems -= cost;
        SaveManager.Instance.SaveGame();
        UpdateGemsDisplay();

        // Generate heroes
        List<RuntimeHero> pulls = new List<RuntimeHero>();
        string resultStr = $"SUMMON RESULTS ({count})\n\n";

        for (int i = 0; i < count; i++)
        {
            int stars = RollStars();
            RuntimeHero hero = CreateHero(stars);
            SaveManager.Instance.AddHero(hero);
            pulls.Add(hero);
            
            resultStr += $"[{new string('*', stars)}] {hero.heroName} - ATK: {hero.attack:F0}\n";

            // Track summon for achievements
            AchievementManager.Instance?.TrackSummon(stars);
        }

        ShowResults(resultStr);
    }

    private int RollStars()
    {
        float roll = Random.value;
        if (roll < 0.05f) return 5; // 5% for 5-star
        if (roll < 0.20f) return 4; // 15% for 4-star
        return 3; // 80% for 3-star
    }

    private RuntimeHero CreateHero(int stars)
    {
        return new RuntimeHero
        {
            runtimeId = System.Guid.NewGuid().ToString().Substring(0, 8),
            heroId = "GACHA_" + stars,
            heroName = HeroNameGenerator.Generate(stars),
            starRating = stars,
            level = 1,
            maxLevel = stars * 10 + 30,
            attack = Random.Range(10, 20) * stars,
            defense = Random.Range(5, 10) * stars,
            maxHP = Random.Range(50, 100) * stars,
            currentHP = Random.Range(50, 100) * stars,
            isInParty = false
        };
    }

    private void ShowResults(string text)
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
            if (resultsContentText != null) resultsContentText.text = text;
        }
    }

    public void CloseResults()
    {
        if (resultsPanel != null) resultsPanel.SetActive(false);
    }

    public void GoBack()
    {
        Debug.Log("[Summon] Back clicked!");
        if (Application.CanStreamedLevelBeLoaded("Lobby"))
            SceneManager.LoadScene("Lobby");
        else
            Debug.LogError("[Summon] Lobby scene not in build settings!");
    }
}
