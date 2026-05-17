using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to a Canvas GameObject in the Roster scene.
/// 
/// Required hierarchy (create in Unity):
/// Canvas
///   TopBar
///     txt_MasterName (TMP)
///     txt_Gold       (TMP)
///     txt_Gems       (TMP)
///     btn_Back       (Button)
///   HeroGrid
///     ScrollRect
///       Content        ← assign as heroGridContent
///   DetailPanel        ← assign as detailPanel (starts inactive)
///     img_Portrait     (Image)
///     txt_Name         (TMP)
///     txt_Stars        (TMP)
///     txt_Class        (TMP)
///     txt_Level        (TMP)
///     txt_HP           (TMP)
///     txt_ATK          (TMP)
///     txt_DEF          (TMP)
///     txt_SPD          (TMP)
///     txt_Morale       (TMP)
///     txt_Trait        (TMP)
///     txt_Status       (TMP)
///     txt_History      (TMP)  ← missions, kills, title
///     btn_Close        (Button)
/// HeroCard prefab (assign as heroCardPrefab)
///   img_Portrait  (Image)
///   txt_Name      (TMP)
///   txt_Stars     (TMP)
///   txt_Level     (TMP)
///   img_StatusIcon (Image)
///   btn_Select    (Button)
/// </summary>
public class RosterUI : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] Transform   heroGridContent;
    [SerializeField] GameObject  heroCardPrefab;

    [Header("Detail Panel")]
    [SerializeField] GameObject  detailPanel;
    [SerializeField] Image       detail_Portrait;
    [SerializeField] TMP_Text    detail_Name;
    [SerializeField] TMP_Text    detail_Stars;
    [SerializeField] TMP_Text    detail_Class;
    [SerializeField] TMP_Text    detail_Level;
    [SerializeField] TMP_Text    detail_HP;
    [SerializeField] TMP_Text    detail_ATK;
    [SerializeField] TMP_Text    detail_DEF;
    [SerializeField] TMP_Text    detail_SPD;
    [SerializeField] TMP_Text    detail_Morale;
    [SerializeField] TMP_Text    detail_Trait;
    [SerializeField] TMP_Text    detail_Status;
    [SerializeField] TMP_Text    detail_History;

    [Header("Top Bar")]
    [SerializeField] TMP_Text    txt_Gold;
    [SerializeField] TMP_Text    txt_Gems;

    [Header("Filter Buttons")]
    [SerializeField] Button      btn_FilterAll;
    [SerializeField] Button      btn_FilterActive;
    [SerializeField] Button      btn_FilterDead;

    // ── State ─────────────────────────────────────────────
    HeroInstance _selectedHero;
    string _currentFilter = "all";
    List<HeroInstance> _displayedHeroes = new List<HeroInstance>();

    // ─────────────────────────────────────────────────────
    void Start()
    {
        detailPanel.SetActive(false);

        // Subscribe to GameManager events
        GameManager.OnRosterChanged  += RefreshGrid;
        GameManager.OnResourcesChanged += RefreshResources;

        // Filter buttons
        btn_FilterAll?.onClick.AddListener(() => SetFilter("all"));
        btn_FilterActive?.onClick.AddListener(() => SetFilter("active"));
        btn_FilterDead?.onClick.AddListener(() => SetFilter("dead"));

        // Bulletproof dynamic click wiring to guarantee functionality under all circumstances
        var backBtn = transform.Find("TopBar/btn_Back")?.GetComponent<Button>();
        if (backBtn != null)
        {
            backBtn.onClick.RemoveAllListeners();
            backBtn.onClick.AddListener(OnBackButton);
        }

        var closeDetailBtn = detailPanel.transform.Find("btn_Close")?.GetComponent<Button>();
        if (closeDetailBtn != null)
        {
            closeDetailBtn.onClick.RemoveAllListeners();
            closeDetailBtn.onClick.AddListener(CloseDetail);
        }

        RefreshGrid();
        RefreshResources();
    }

    void OnDestroy()
    {
        GameManager.OnRosterChanged   -= RefreshGrid;
        GameManager.OnResourcesChanged -= RefreshResources;
    }

    // ─────────────────────────────────────────────────────
    // GRID
    // ─────────────────────────────────────────────────────
    void RefreshGrid()
    {
        // Clear existing cards
        foreach (Transform child in heroGridContent)
            Destroy(child.gameObject);

        _displayedHeroes = GetFilteredHeroes();

        foreach (var hero in _displayedHeroes)
        {
            var card = Instantiate(heroCardPrefab, heroGridContent);
            SetupHeroCard(card, hero);
        }
    }

    void SetupHeroCard(GameObject card, HeroInstance hero)
    {
        var data = GameManager.Instance.GetHeroData(hero.heroDataId);

        // Portrait
        var portrait = card.transform.Find("img_Portrait")?.GetComponent<Image>();
        if (portrait != null && data?.portrait != null)
            portrait.sprite = data.portrait;

        // Name
        var nameText = card.transform.Find("txt_Name")?.GetComponent<TMP_Text>();
        if (nameText != null) nameText.text = hero.DisplayName;

        // Stars
        var starsText = card.transform.Find("txt_Stars")?.GetComponent<TMP_Text>();
        if (starsText != null)
        {
            starsText.text = new string('*', hero.starRating);
            starsText.color = GetRarityColor(hero.starRating);
        }

        // Level
        var levelText = card.transform.Find("txt_Level")?.GetComponent<TMP_Text>();
        if (levelText != null) levelText.text = $"Lv.{hero.level}";

        // Dead overlay
        if (hero.status == HeroStatus.Dead)
        {
            var canvasGroup = card.GetComponent<CanvasGroup>() ?? card.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0.5f;
        }

        // Click → open detail
        var btn = card.GetComponent<Button>();
        var capturedHero = hero;
        btn?.onClick.AddListener(() => OpenDetail(capturedHero));
    }

    // ─────────────────────────────────────────────────────
    // DETAIL PANEL
    // ─────────────────────────────────────────────────────
    void OpenDetail(HeroInstance hero)
    {
        _selectedHero = hero;
        var data = GameManager.Instance.GetHeroData(hero.heroDataId);

        detailPanel.SetActive(true);

        if (detail_Portrait != null && data?.portrait != null)
            detail_Portrait.sprite = data.portrait;

        if (detail_Name    != null) detail_Name.text    = hero.DisplayName;
        if (detail_Stars   != null)
        {
            detail_Stars.text  = new string('*', hero.starRating);
            detail_Stars.color = GetRarityColor(hero.starRating);
        }
        if (detail_Class   != null) detail_Class.text   = data != null ? data.heroClass.ToString() : "Unknown";
        if (detail_Level   != null) detail_Level.text   = $"Level {hero.level} / 50";
        if (detail_HP      != null) detail_HP.text      = $"HP:  {hero.currentHP} / {hero.maxHP}";
        if (detail_ATK     != null) detail_ATK.text     = $"ATK: {hero.atk}";
        if (detail_DEF     != null) detail_DEF.text     = $"DEF: {hero.def}";
        if (detail_SPD     != null) detail_SPD.text     = $"SPD: {hero.spd}";
        if (detail_Morale  != null) detail_Morale.text  = $"Morale: {hero.morale}/100  {GetMoraleLabel(hero.morale)}";
        if (detail_Trait   != null) detail_Trait.text   = $"Trait: {hero.trait}";
        if (detail_Status  != null)
        {
            detail_Status.text  = hero.status.ToString().ToUpper();
            detail_Status.color = GetStatusColor(hero.status);
        }
        if (detail_History != null)
        {
            string title   = string.IsNullOrEmpty(hero.earnedTitle) ? "None" : hero.earnedTitle;
            detail_History.text =
                $"Missions: {hero.missionsCompleted}\n" +
                $"Floors:   {hero.floorsCleared}\n" +
                $"Kills:    {hero.kills}\n" +
                $"Title:    {title}";
        }
    }

    public void CloseDetail()
    {
        detailPanel.SetActive(false);
        _selectedHero = null;
    }

    // ─────────────────────────────────────────────────────
    // FILTERS
    // ─────────────────────────────────────────────────────
    void SetFilter(string filter)
    {
        _currentFilter = filter;
        RefreshGrid();
    }

    List<HeroInstance> GetFilteredHeroes()
    {
        var all = GameManager.Instance.State.roster;
        return _currentFilter switch
        {
            "active" => all.Where(h => h.status != HeroStatus.Dead).ToList(),
            "dead"   => all.Where(h => h.status == HeroStatus.Dead).ToList(),
            _        => all.ToList()
        };
    }

    // ─────────────────────────────────────────────────────
    // RESOURCES
    // ─────────────────────────────────────────────────────
    void RefreshResources()
    {
        if (txt_Gold != null) txt_Gold.text = GameManager.Instance.State.gold.ToString("N0");
        if (txt_Gems != null) txt_Gems.text = GameManager.Instance.State.gems.ToString("N0");
    }

    // ─────────────────────────────────────────────────────
    // NAVIGATION
    // ─────────────────────────────────────────────────────
    public void OnBackButton() => SceneLoader.GoToLobby();

    // ─────────────────────────────────────────────────────
    // COLOR HELPERS
    // ─────────────────────────────────────────────────────
    Color GetRarityColor(int stars) => stars switch
    {
        5 => new Color(1f,   0.84f, 0f),    // gold
        4 => new Color(0.75f, 0.5f, 1f),    // purple
        3 => new Color(0.37f, 0.63f, 1f),   // blue
        2 => new Color(0.37f, 1f,   0.5f),  // green
        _ => new Color(0.67f, 0.67f, 0.67f) // grey
    };

    Color GetStatusColor(HeroStatus status) => status switch
    {
        HeroStatus.Dead     => Color.red,
        HeroStatus.Fatigued => new Color(1f, 0.6f, 0f),
        HeroStatus.Wounded  => new Color(1f, 0.3f, 0.3f),
        _                   => Color.white
    };

    string GetMoraleLabel(int morale) => morale switch
    {
        >= 80 => "⬆ High",
        >= 50 => "→ Stable",
        >= 30 => "⬇ Low",
        _     => "⚠ Critical"
    };
}
