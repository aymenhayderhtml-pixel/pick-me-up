using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PickMeUp.Data;

public class HeroShowcasePanel : MonoBehaviour
{
    [SerializeField] private Image heroPortrait;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI starsText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI levelText;

    private void Start()
    {
        Refresh();
    }
    
    public void Refresh()
    {
        var hero = GameDataBridge.Instance?.GetShowcaseHero();
        
        Debug.Log("[HeroShowcase] Refresh called, hero=" + (hero != null ? hero.heroName : "null"));
        
        if (hero == null)
        {
            ShowPlaceholder();
            return;
        }
        
        // Direct field access - no reflection
        nameText.text = hero.heroName;
        starsText.text = new string('*', hero.starRating);
        statsText.text = $"ATK: {hero.attack} | DEF: {hero.defense} | HP: {hero.maxHP}";
        levelText.text = $"Level {hero.level}";
        
        if (ThemeConfig.Instance != null)
            nameText.color = ThemeConfig.Instance.GetRarityColor(hero.starRating);
    }

    private void ShowPlaceholder()
    {
        nameText.text = "Hero Name";
        starsText.text = "*****";
        statsText.text = "ATK: -- | DEF: -- | HP: --";
        levelText.text = "Level --";
        
        if (ThemeConfig.Instance != null)
        {
            nameText.color = ThemeConfig.Instance.accentGold;
        }
    }
}
