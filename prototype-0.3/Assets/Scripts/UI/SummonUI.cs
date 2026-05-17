using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to Canvas in the Summon scene.
///
/// Required hierarchy:
/// Canvas
///   TopBar
///     txt_Gems    (TMP)
///     txt_Gold    (TMP)
///     btn_Back    (Button)
///   SummonPanel
///     btn_Pull1x  (Button)
///     btn_Pull10x (Button)
///     txt_Cost1x  (TMP)
///     txt_Cost10x (TMP)
///     PityBar
///       img_PityFill   (Image — fill type horizontal)
///       txt_PityCount  (TMP)
///   ResultPanel         ← starts inactive
///     ResultGrid
///       Content         ← assign as resultGridContent
///     txt_ResultSummary (TMP)
///     btn_CloseResults  (Button)
///     btn_SummonAgain   (Button)
///   NotificationText    (TMP) ← for "Not enough gems" toast
/// 
/// ResultCard prefab:
///   img_Portrait  (Image)
///   txt_Name      (TMP)
///   txt_Stars     (TMP)
///   obj_NewBadge  (GameObject) ← "NEW" badge, show/hide
///   img_RarityBorder (Image)
/// </summary>
public class SummonUI : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] TMP_Text txt_Gems;
    [SerializeField] TMP_Text txt_Gold;

    [Header("Summon Buttons")]
    [SerializeField] Button   btn_Pull1x;
    [SerializeField] Button   btn_Pull10x;

    [Header("Pity Bar")]
    [SerializeField] Image    img_PityFill;     // set Image type to Filled, horizontal
    [SerializeField] TMP_Text txt_PityCount;

    [Header("Result Panel")]
    [SerializeField] GameObject resultPanel;
    [SerializeField] Transform  resultGridContent;
    [SerializeField] GameObject resultCardPrefab;
    [SerializeField] TMP_Text   txt_ResultSummary;

    [Header("Notification")]
    [SerializeField] TMP_Text   txt_Notification;

    // ── State ─────────────────────────────────────────────
    bool _isSummoning = false;
    List<HeroInstance> _lastResults = new List<HeroInstance>();

    // ─────────────────────────────────────────────────────
    void Start()
    {
        resultPanel.SetActive(false);
        if (txt_Notification != null) txt_Notification.gameObject.SetActive(false);

        btn_Pull1x?.onClick.AddListener(OnPull1x);
        btn_Pull10x?.onClick.AddListener(OnPull10x);

        // Bulletproof dynamic click wiring to guarantee functionality under all circumstances
        var backBtn = transform.Find("TopBar/btn_Back")?.GetComponent<Button>();
        if (backBtn != null)
        {
            backBtn.onClick.RemoveAllListeners();
            backBtn.onClick.AddListener(OnBackButton);
        }

        var closeResultsBtn = resultPanel.transform.Find("btn_CloseResults")?.GetComponent<Button>();
        if (closeResultsBtn != null)
        {
            closeResultsBtn.onClick.RemoveAllListeners();
            closeResultsBtn.onClick.AddListener(OnCloseResults);
        }

        var summonAgainBtn = resultPanel.transform.Find("btn_SummonAgain")?.GetComponent<Button>();
        if (summonAgainBtn != null)
        {
            summonAgainBtn.onClick.RemoveAllListeners();
            summonAgainBtn.onClick.AddListener(OnSummonAgain);
        }

        GameManager.OnResourcesChanged += RefreshResources;
        RefreshResources();
        RefreshPityBar();
    }

    void OnDestroy()
    {
        GameManager.OnResourcesChanged -= RefreshResources;
    }

    // ─────────────────────────────────────────────────────
    // PULL BUTTONS
    // ─────────────────────────────────────────────────────
    void OnPull1x()
    {
        if (_isSummoning) return;
        var gm = GameManager.Instance;

        if (gm.State.gems < GachaSystem.COST_GEMS_1X)
        {
            ShowNotification($"Not enough gems. Need {GachaSystem.COST_GEMS_1X} 💎");
            return;
        }

        _isSummoning = true;
        SetButtonsInteractable(false);

        AudioManager.PlaySummonPull();

        var result = GachaSystem.Pull1x(gm);
        if (result != null)
        {
            _lastResults = new List<HeroInstance> { result };
            StartCoroutine(ShowResultsWithDelay());
        }
        else
        {
            _isSummoning = false;
            SetButtonsInteractable(true);
        }

        RefreshPityBar();
    }

    void OnPull10x()
    {
        if (_isSummoning) return;
        var gm = GameManager.Instance;

        if (gm.State.gems < GachaSystem.COST_GEMS_10X)
        {
            ShowNotification($"Not enough gems. Need {GachaSystem.COST_GEMS_10X} 💎");
            return;
        }

        _isSummoning = true;
        SetButtonsInteractable(false);

        AudioManager.PlaySummonPull();

        _lastResults = GachaSystem.Pull10x(gm);
        StartCoroutine(ShowResultsWithDelay());
        RefreshPityBar();
    }

    // ─────────────────────────────────────────────────────
    // RESULT DISPLAY
    // ─────────────────────────────────────────────────────
    IEnumerator ShowResultsWithDelay()
    {
        // Brief delay for dramatic effect
        yield return new WaitForSeconds(0.5f);
        ShowResults();
    }

    void ShowResults()
    {
        // Clear old cards
        foreach (Transform child in resultGridContent)
            Destroy(child.gameObject);

        // Sort highest rarity first
        _lastResults.Sort((a, b) => b.starRating.CompareTo(a.starRating));

        // Spawn result cards with staggered reveal
        StartCoroutine(SpawnResultCards());

        // Summary line: "3× 3★  2× 2★  5× 1★"
        var counts = new Dictionary<int, int>();
        foreach (var h in _lastResults)
        {
            if (!counts.ContainsKey(h.starRating)) counts[h.starRating] = 0;
            counts[h.starRating]++;
        }

        string summary = "";
        for (int s = 5; s >= 1; s--)
            if (counts.ContainsKey(s))
                summary += $"{counts[s]}× {new string('*', s)}   ";

        if (txt_ResultSummary != null)
            txt_ResultSummary.text = $"Pity: {GameManager.Instance.State.pityCounter}/90   {summary}";

        resultPanel.SetActive(true);
    }

    IEnumerator SpawnResultCards()
    {
        foreach (var hero in _lastResults)
        {
            var card = Instantiate(resultCardPrefab, resultGridContent);
            SetupResultCard(card, hero);
            yield return new WaitForSeconds(0.12f); // staggered reveal
        }
    }

    void SetupResultCard(GameObject card, HeroInstance hero)
    {
        var data = GameManager.Instance.GetHeroData(hero.heroDataId);

        var portrait = card.transform.Find("img_Portrait")?.GetComponent<Image>();
        if (portrait != null && data?.portrait != null)
            portrait.sprite = data.portrait;

        var nameText = card.transform.Find("txt_Name")?.GetComponent<TMP_Text>();
        if (nameText != null) nameText.text = hero.heroName;

        var starsText = card.transform.Find("txt_Stars")?.GetComponent<TMP_Text>();
        if (starsText != null)
        {
            starsText.text  = new string('*', hero.starRating);
            starsText.color = GetRarityColor(hero.starRating);
        }

        var newBadge = card.transform.Find("obj_NewBadge")?.gameObject;
        if (newBadge != null) newBadge.SetActive(hero.isNew);

        var border = card.transform.Find("img_RarityBorder")?.GetComponent<Image>();
        if (border != null) border.color = GetRarityColor(hero.starRating);
    }

    // ─────────────────────────────────────────────────────
    // CLOSE / SUMMON AGAIN
    // ─────────────────────────────────────────────────────
    public void OnCloseResults()
    {
        resultPanel.SetActive(false);
        _isSummoning = false;
        SetButtonsInteractable(true);

        // Mark heroes as no longer new
        foreach (var h in _lastResults)
            h.isNew = false;

        GameManager.Instance.SaveGame();
    }

    public void OnSummonAgain()
    {
        resultPanel.SetActive(false);
        _isSummoning = false;
        SetButtonsInteractable(true);

        // Try same pull count
        if (_lastResults.Count == 1) OnPull1x();
        else OnPull10x();
    }

    // ─────────────────────────────────────────────────────
    // PITY BAR
    // ─────────────────────────────────────────────────────
    void RefreshPityBar()
    {
        int pity = GameManager.Instance.State.pityCounter;
        if (img_PityFill  != null) img_PityFill.fillAmount = pity / (float)GachaSystem.PITY_THRESHOLD;
        if (txt_PityCount != null) txt_PityCount.text = $"{pity} / {GachaSystem.PITY_THRESHOLD}";
    }

    // ─────────────────────────────────────────────────────
    // RESOURCES
    // ─────────────────────────────────────────────────────
    void RefreshResources()
    {
        var gm = GameManager.Instance;
        if (txt_Gems != null) txt_Gems.text = gm.State.gems.ToString("N0");
        if (txt_Gold != null) txt_Gold.text = gm.State.gold.ToString("N0");
    }

    // ─────────────────────────────────────────────────────
    // NOTIFICATION TOAST
    // ─────────────────────────────────────────────────────
    void ShowNotification(string message)
    {
        if (txt_Notification == null) return;
        StopCoroutine(nameof(HideNotification));
        txt_Notification.text = message;
        txt_Notification.gameObject.SetActive(true);
        StartCoroutine(nameof(HideNotification));
    }

    IEnumerator HideNotification()
    {
        yield return new WaitForSeconds(2.5f);
        if (txt_Notification != null)
            txt_Notification.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────
    void SetButtonsInteractable(bool state)
    {
        if (btn_Pull1x  != null) btn_Pull1x.interactable  = state;
        if (btn_Pull10x != null) btn_Pull10x.interactable = state;
    }

    Color GetRarityColor(int stars) => stars switch
    {
        5 => new Color(1f,   0.84f, 0f),
        4 => new Color(0.75f, 0.5f, 1f),
        3 => new Color(0.37f, 0.63f, 1f),
        2 => new Color(0.37f, 1f,   0.5f),
        _ => new Color(0.67f, 0.67f, 0.67f)
    };

    public void OnBackButton() => SceneLoader.GoToLobby();
}
