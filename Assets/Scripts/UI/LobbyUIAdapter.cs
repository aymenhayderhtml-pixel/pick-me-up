using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyUIAdapter : MonoBehaviour
{
    public static LobbyUIAdapter Instance { get; private set; }
    
    [Header("Existing Systems (Auto-found)")]
    // Gemini: Replace these with your actual manager types
    // private SaveManager saveManager;
    // private MainMenuNavigation menuNav;
    // private HeroFactory heroFactory;
    
    [Header("UI Panels")]
    [SerializeField] private HeroShowcasePanel showcasePanel;
    [SerializeField] private ResourceDisplay resourceDisplay;
    
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        
        // Auto-find existing systems at runtime (no inspector wiring needed)
        // saveManager = FindObjectOfType<SaveManager>();
        // menuNav = FindObjectOfType<MainMenuNavigation>();
        // heroFactory = FindObjectOfType<HeroFactory>();
    }
    
    private void Start()
    {
        RefreshLobby();
    }

    private void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Android back button pressed
                // TODO: Open pause menu or go back to lobby
                Debug.Log("[Android] Back button pressed");
            }
        }
    }
    
    public void OnTowerClicked() => OpenFeature("Tower");
    public void OnInventoryClicked() => OpenFeature("Inventory");
    public void OnDungeonClicked() => OpenFeature("Dungeon");
    
    public void OpenFeature(string featureId)
    {
        featureId = featureId.Trim();
        Debug.Log($"[Lobby] Opening: {featureId}");
        
        // Route to your existing MainMenuNavigation or scene loader
        // Example: menuNav?.NavigateTo(featureId);
        
        // Temporary: Scene-based routing until you wire it
        switch (featureId)
        {
            case "Summon": LoadScene("summon"); break;
            case "Tower": LoadScene("Tower"); break;
            case "Dungeon": LoadScene("Dungeon"); break;
            case "Hero": LoadScene("HeroManagement"); break;
            case "Inventory": LoadScene("Inventory"); break;
            case "Training": LoadScene("Training"); break;
            case "Memorial": LoadScene("Memorial"); break;
            case "Tavern": LoadScene("Tavern"); break;
            case "Blacksmith": LoadScene("Blacksmith"); break;
            case "Research": LoadScene("Research"); break;
            case "Synthesis": LoadScene("Synthesis"); break;
            default:
                Debug.LogWarning($"[Lobby] Feature '{featureId}' not routed yet.");
                break;
        }
    }
    
    public void RefreshLobby()
    {
        showcasePanel?.Refresh();
        resourceDisplay?.Refresh();
    }
    
    private void LoadScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogError($"[Lobby] Scene '{sceneName}' not in Build Settings!");
    }
}
