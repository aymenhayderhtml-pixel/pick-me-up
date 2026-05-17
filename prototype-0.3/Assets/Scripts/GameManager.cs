using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Singleton. Persists across all scenes via DontDestroyOnLoad.
/// Owns the live game state: roster, resources, tower progress.
/// All other systems (GachaSystem, CombatManager, etc.) read/write through here.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── Hero data registry (assign ALL HeroData SOs in Inspector) ──
    [Header("Hero Registry")]
    public HeroData[] allHeroData;      // drag all SO_Hero_* assets here

    // ── Live State ────────────────────────────────────────
    [HideInInspector] public GameState State = new GameState();

    public MoraleSystem MoraleSystem { get; private set; }
    public QuestSystem QuestSystem { get; private set; }

    // ── Events (UI subscribes to these) ───────────────────
    public static event System.Action<HeroInstance> OnHeroSummoned;
    public static event System.Action<HeroInstance> OnHeroDied;
    public static event System.Action<HeroInstance> OnHeroLevelUp;
    public static event System.Action              OnResourcesChanged;
    public static event System.Action              OnRosterChanged;

    // ─────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        MoraleSystem = GetComponent<MoraleSystem>();
        QuestSystem = GetComponent<QuestSystem>();

        // Auto-discover HeroData assets in Resources/Heroes/ if registry is unassigned/empty
        if (allHeroData == null || allHeroData.Length == 0)
        {
            allHeroData = Resources.LoadAll<HeroData>("Heroes");
            Debug.Log($"[GameManager] Automatically registered {allHeroData.Length} HeroData assets from Resources/Heroes/.");
        }

        SaveSystem.Initialize();
        LoadGame();
    }

    // ─────────────────────────────────────────────────────
    // HERO REGISTRY LOOKUP
    // ─────────────────────────────────────────────────────
    public HeroData GetHeroData(string heroDataId)
    {
        return allHeroData.FirstOrDefault(h => h.name == heroDataId);
    }

    public HeroData[] GetPoolByStars(int stars)
    {
        return allHeroData.Where(h => h.starRating == stars).ToArray();
    }

    // ─────────────────────────────────────────────────────
    // ROSTER MANAGEMENT
    // ─────────────────────────────────────────────────────
    public List<HeroInstance> GetActiveRoster()
    {
        return State.roster.Where(h => h.status != HeroStatus.Dead).ToList();
    }

    public List<HeroInstance> GetMemorialHeroes()
    {
        return State.roster.Where(h => h.status == HeroStatus.Dead).ToList();
    }

    public List<HeroInstance> GetFallenHeroes() => GetMemorialHeroes();

    public void AddHeroToRoster(HeroInstance hero)
    {
        State.roster.Add(hero);
        OnHeroSummoned?.Invoke(hero);
        OnRosterChanged?.Invoke();
        SaveGame();
    }

    /// <summary>
    /// Called by CombatManager when a hero reaches 0 HP.
    /// Moves hero to Memorial — does NOT remove from roster list.
    /// </summary>
    public void KillHero(HeroInstance hero)
    {
        hero.Die();
        OnHeroDied?.Invoke(hero);
        OnRosterChanged?.Invoke();

        // Morale hit to all surviving deployed heroes (Compassionate trait amplifies)
        foreach (var h in GetActiveRoster().Where(h => h.isDeployed))
        {
            int moraleHit = -10;
            if (h.trait == PersonalityTrait.Compassionate) moraleHit = -20;
            if (h.trait == PersonalityTrait.Stoic)         moraleHit = 0;
            h.ModifyMorale(moraleHit);
        }

        SaveGame();
    }

    public void LevelUpHero(HeroInstance hero, int xpAmount)
    {
        var data = GetHeroData(hero.heroDataId);
        if (data == null) return;

        bool leveledUp = hero.AddXP(xpAmount, data);
        if (leveledUp)
        {
            OnHeroLevelUp?.Invoke(hero);
        }
        SaveGame();
    }

    // ─────────────────────────────────────────────────────
    // SQUAD
    // ─────────────────────────────────────────────────────
    /// <summary>Max 5 heroes. Returns false if slot taken or hero unavailable.</summary>
    public bool AddToSquad(string instanceId)
    {
        if (State.currentSquad.Count >= 5) return false;
        if (State.currentSquad.Contains(instanceId)) return false;

        var hero = GetHeroByInstanceId(instanceId);
        if (hero == null || hero.status == HeroStatus.Dead || hero.isDeployed) return false;

        State.currentSquad.Add(instanceId);
        hero.isDeployed = true;
        return true;
    }

    public void RemoveFromSquad(string instanceId)
    {
        State.currentSquad.Remove(instanceId);
        var hero = GetHeroByInstanceId(instanceId);
        if (hero != null) hero.isDeployed = false;
    }

    public void ClearSquad()
    {
        foreach (var id in State.currentSquad)
        {
            var hero = GetHeroByInstanceId(id);
            if (hero != null) hero.isDeployed = false;
        }
        State.currentSquad.Clear();
    }

    public List<HeroInstance> GetCurrentSquad()
    {
        return State.currentSquad
            .Select(id => GetHeroByInstanceId(id))
            .Where(h => h != null)
            .ToList();
    }

    public int GetSquadPower()
    {
        return GetCurrentSquad().Sum(h => h.Power);
    }

    public HeroInstance GetHeroByInstanceId(string id)
    {
        return State.roster.FirstOrDefault(h => h.instanceId == id);
    }

    public HeroInstance GetHeroById(string id) => GetHeroByInstanceId(id);

    public List<HeroInstance> GetFullRoster() => new List<HeroInstance>(State.roster);

    // ─────────────────────────────────────────────────────
    // RESOURCES
    // ─────────────────────────────────────────────────────
    public bool SpendGold(int amount)
    {
        if (State.gold < amount) return false;
        State.gold -= amount;
        OnResourcesChanged?.Invoke();
        SaveGame();
        return true;
    }

    public bool SpendGems(int amount)
    {
        if (State.gems < amount) return false;
        State.gems -= amount;
        OnResourcesChanged?.Invoke();
        SaveGame();
        return true;
    }

    public bool SpendStamina(int amount)
    {
        if (State.stamina < amount) return false;
        State.stamina -= amount;
        OnResourcesChanged?.Invoke();
        SaveGame();
        return true;
    }

    public void AddGold(int amount)    { State.gold    += amount; OnResourcesChanged?.Invoke(); SaveGame(); }
    public void AddGems(int amount)    { State.gems    += amount; OnResourcesChanged?.Invoke(); SaveGame(); }
    public void AddStamina(int amount) { State.stamina  = Mathf.Min(State.stamina + amount, State.maxStamina); OnResourcesChanged?.Invoke(); SaveGame(); }
    public void AddEssence(int amount) { State.essence += amount; OnResourcesChanged?.Invoke(); SaveGame(); }

    public void SpendEssence(int amount)
    {
        State.essence = Mathf.Max(0, State.essence - amount);
        OnResourcesChanged?.Invoke();
        SaveGame();
    }

    public int Essence => State.essence;

    public void SendToMemorial(HeroInstance hero)
    {
        RemoveFromSquad(hero.instanceId);
        OnRosterChanged?.Invoke();
        SaveGame();
    }

    public void FireRosterChanged() => OnRosterChanged?.Invoke();

    // ─────────────────────────────────────────────────────
    // TOWER PROGRESS
    // ─────────────────────────────────────────────────────
    public void CompleteFloor(int floor)
    {
        if (floor > State.highestFloorCleared)
        {
            State.highestFloorCleared = floor;
            SaveGame();
        }
    }

    void Start()
    {
        if (QuestSystem != null)
        {
            QuestSystem.Initialize(State?.questSaveData);
        }
    }

    // ─────────────────────────────────────────────────────
    // SAVE / LOAD
    // ─────────────────────────────────────────────────────
    public void SaveGame()
    {
        if (QuestSystem != null)
        {
            State.questSaveData = QuestSystem.GetSaveData();
        }
        SaveSystem.Save(State);
    }

    public void LoadGame()  => State = SaveSystem.Load() ?? new GameState();
}
