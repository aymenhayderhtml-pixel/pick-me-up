using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using PickMeUp.Data;

public class GameDataBridge : MonoBehaviour
{
    public static GameDataBridge Instance { get; private set; }
    
    private SaveManager saveManager;
    
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        
        // Search current scene
        saveManager = Object.FindFirstObjectByType<SaveManager>();
        
        // Search DontDestroyOnLoad
        if (saveManager == null)
        {
            var ddol = DontDestroyOnLoadSceneObjects();
            foreach (var obj in ddol)
            {
                saveManager = obj.GetComponent<SaveManager>();
                if (saveManager != null) break;
            }
        }
        
        // Search all scenes
        if (saveManager == null)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                {
                    saveManager = root.GetComponentInChildren<SaveManager>();
                    if (saveManager != null) break;
                }
                if (saveManager != null) break;
            }
        }

        // Auto-create if still not found
        if (saveManager == null)
        {
            GameObject go = new GameObject("SaveManager");
            saveManager = go.AddComponent<SaveManager>();
            DontDestroyOnLoad(go);
            Debug.Log("[GameDataBridge] Auto-created SaveManager.");
        }
        
        Debug.Log($"[GameDataBridge] SaveManager found: {saveManager != null}");
    }
    
    // === CURRENCY ===
    public int GetGems() => saveManager != null ? saveManager.Gems : 9999;
    public int GetGold() => saveManager != null ? saveManager.Gold : 1500000;
    public int GetStamina() => saveManager != null ? saveManager.CurrentStamina : 120;
    public int GetMaxStamina() => saveManager != null ? saveManager.MaxStamina : 120;
    
    // === PROGRESS ===
    public int GetCurrentFloor() => saveManager != null ? saveManager.CurrentFloor : 1;
    public int GetHighestFloor() => saveManager != null ? saveManager.HighestFloor : 1;
    
    // === HEROES ===
    public RuntimeHero GetShowcaseHero()
    {
        if (saveManager == null || saveManager.ActiveRoster == null || saveManager.ActiveRoster.Count == 0)
            return null;
        return saveManager.ActiveRoster[0];
    }
    
    public List<RuntimeHero> GetActiveRoster()
    {
        if (saveManager == null || saveManager.ActiveRoster == null)
            return new List<RuntimeHero>();
        return saveManager.ActiveRoster;
    }
    
    // === SAVE/LOAD ===
    public void SaveGame() => saveManager?.SaveGame();
    public void LoadGame() => saveManager?.LoadGame();
    
    // === HELPERS ===
    private List<GameObject> DontDestroyOnLoadSceneObjects()
    {
        var result = new List<GameObject>();
        var temp = new GameObject("Temp_DDOL_Finder");
        DontDestroyOnLoad(temp);
        var scene = temp.scene;
        Destroy(temp);
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name != "Temp_DDOL_Finder")
                result.Add(root);
        }
        return result;
    }
}
