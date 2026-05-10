using UnityEngine;
using System.Collections.Generic;
using System.IO;
using PickMeUp.Data;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    public System.Action OnDataChanged;
    
    [Header("Currency")]
    public int Gems = 1000;
    public int Gold = 50000;
    public int CurrentStamina = 120;
    public int MaxStamina = 120;
    
    [Header("Progress")]
    public int CurrentFloor = 1;
    public int HighestFloor = 1;
    
    [Header("Hero Roster")]
    public List<RuntimeHero> ActiveRoster = new List<RuntimeHero>();
    public List<RuntimeHero> MemorialArchive = new List<RuntimeHero>();
    
    [Header("Inventory")]
    public List<RuntimeItem> Inventory = new List<RuntimeItem>();
    
    [Header("Dungeon")]
    public List<DungeonProgress> DungeonProgress = new List<DungeonProgress>();

    [Header("Stamina Regen")]
    public string LastStaminaTimestamp = "";

    [Header("Daily Rewards")]
    public string LastLoginDate = "";
    public int CurrentLoginDay = 1;
    public int LoginStreak = 0;
    public List<int> ClaimedDays = new List<int>();

    [Header("Achievements")]
    public List<AchievementProgress> AchievementProgress = new List<AchievementProgress>();
    
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        LoadGame();
        
        // TEMP: Add a starter hero if roster is empty
        if (ActiveRoster.Count == 0)
        {
            ActiveRoster.Add(CreateStarterHero());
            SaveGame();
        }
    }
    
    private RuntimeHero CreateStarterHero()
    {
        var hero = new RuntimeHero
        {
            runtimeId = System.Guid.NewGuid().ToString().Substring(0, 8),
            heroId = "HERO_STARTER",
            heroName = "Shadow Monarch",
            starRating = 5,
            level = 1,
            exp = 0,
            maxLevel = 80,
            baseMaxHP = 500,
            baseAttack = 100,
            baseDefense = 50,
            baseSpeed = 10,
            baseCritRate = 10,
            isInParty = true,
            partySlot = 0
        };
        hero.RecalculateStats();
        hero.currentHP = hero.maxHP;
        return hero;
    }
    
    public void AddHero(RuntimeHero hero)
    {
        ActiveRoster.Add(hero);
        SaveGame();
    }
    
    public void SaveGame()
    {
        SaveDataWrapper wrapper = new SaveDataWrapper(this);
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(GetSavePath(), json);
        OnDataChanged?.Invoke();
        Debug.Log("[SaveManager] Game saved.");
    }
    
    public void LoadGame()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            Debug.Log("[SaveManager] No save found. Starting fresh.");
            return;
        }
        
        string json = File.ReadAllText(path);
        SaveDataWrapper wrapper = JsonUtility.FromJson<SaveDataWrapper>(json);
        wrapper.ApplyTo(this);
        Debug.Log("[SaveManager] Game loaded.");
    }
    
    private string GetSavePath() => Application.persistentDataPath + "/pickmeup_save.json";
}

[System.Serializable]
public class SaveDataWrapper
{
    public int Gems;
    public int Gold;
    public int CurrentStamina;
    public int MaxStamina;
    public int CurrentFloor;
    public int HighestFloor;
    public List<RuntimeHero> ActiveRoster;
    public List<RuntimeHero> MemorialArchive;
    public List<RuntimeItem> Inventory;
    public List<DungeonProgress> DungeonProgress;
    public string LastStaminaTimestamp;
    public string LastLoginDate;
    public int CurrentLoginDay;
    public int LoginStreak;
    public List<int> ClaimedDays;
    public List<AchievementProgress> AchievementProgress;
    
    public SaveDataWrapper(SaveManager manager)
    {
        Gems = manager.Gems;
        Gold = manager.Gold;
        CurrentStamina = manager.CurrentStamina;
        MaxStamina = manager.MaxStamina;
        CurrentFloor = manager.CurrentFloor;
        HighestFloor = manager.HighestFloor;
        ActiveRoster = manager.ActiveRoster;
        MemorialArchive = manager.MemorialArchive;
        Inventory = manager.Inventory;
        DungeonProgress = manager.DungeonProgress;
        LastStaminaTimestamp = manager.LastStaminaTimestamp;
        LastLoginDate = manager.LastLoginDate;
        CurrentLoginDay = manager.CurrentLoginDay;
        LoginStreak = manager.LoginStreak;
        ClaimedDays = manager.ClaimedDays;
        AchievementProgress = manager.AchievementProgress;
    }
    
    public void ApplyTo(SaveManager manager)
    {
        manager.Gems = Gems;
        manager.Gold = Gold;
        manager.CurrentStamina = CurrentStamina;
        manager.MaxStamina = MaxStamina;
        manager.CurrentFloor = CurrentFloor;
        manager.HighestFloor = HighestFloor;
        manager.ActiveRoster = ActiveRoster ?? new List<RuntimeHero>();
        manager.MemorialArchive = MemorialArchive ?? new List<RuntimeHero>();
        manager.Inventory = Inventory ?? new List<RuntimeItem>();
        manager.DungeonProgress = DungeonProgress ?? new List<DungeonProgress>();
        manager.LastStaminaTimestamp = LastStaminaTimestamp;
        manager.LastLoginDate = LastLoginDate;
        manager.CurrentLoginDay = CurrentLoginDay != 0 ? CurrentLoginDay : 1;
        manager.LoginStreak = LoginStreak;
        manager.ClaimedDays = ClaimedDays ?? new List<int>();
        manager.AchievementProgress = AchievementProgress ?? new List<AchievementProgress>();
    }
}
