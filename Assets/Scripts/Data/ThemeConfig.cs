using UnityEngine;

[CreateAssetMenu(fileName = "ThemeConfig", menuName = "Game/Theme Config")]
public class ThemeConfig : ScriptableObject
{
    [Header("Rarity Colors")]
    public Color commonColor = new Color(0.6f, 0.6f, 0.6f);
    public Color rareColor = new Color(0.2f, 0.5f, 1f);
    public Color epicColor = new Color(0.6f, 0.2f, 0.9f);
    public Color legendaryColor = new Color(1f, 0.84f, 0f);
    
    [Header("UI Colors")]
    public Color backgroundDark = new Color(0.04f, 0.04f, 0.08f);
    public Color panelDark = new Color(0.08f, 0.08f, 0.15f);
    public Color accentGold = new Color(1f, 0.84f, 0f);
    public Color textNormal = new Color(0.9f, 0.9f, 0.9f);
    public Color textMuted = new Color(0.5f, 0.5f, 0.5f);
    
    [Header("References")]
    public Sprite menuButtonFrame;
    public Sprite starIcon;
    
    private static ThemeConfig _instance;
    public static ThemeConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<ThemeConfig>("ThemeConfig");
            return _instance;
        }
    }
    
    public Color GetRarityColor(int stars) => stars switch
    {
        5 => legendaryColor,
        4 => epicColor,
        3 => rareColor,
        _ => commonColor
    };
}
