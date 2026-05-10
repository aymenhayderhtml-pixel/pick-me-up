#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class LobbySceneBuilder
{
    [MenuItem("Tools/PickMeUp/Build Lobby UI")]
    public static void BuildLobbyUI()
    {
        GameObject existing = GameObject.Find("LobbyCanvas");
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Rebuild?", "LobbyCanvas exists. Rebuild?", "Yes", "Cancel"))
                return;
            Undo.DestroyObjectImmediate(existing);
        }

        // === CANVAS ===
        GameObject canvasGO = new GameObject("LobbyCanvas");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
        
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2340f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<SafeAreaFitter>();

        // === EVENT SYSTEM ===
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // === LOBBY MANAGER ===
        GameObject mgr = new GameObject("LobbyManager");
        Undo.RegisterCreatedObjectUndo(mgr, "Create Manager");
        mgr.transform.SetParent(canvasGO.transform, false);
        LobbyUIAdapter adapter = mgr.AddComponent<LobbyUIAdapter>();

        // === TOP BAR ===
        Color topBarColor = ThemeConfig.Instance != null ? ThemeConfig.Instance.panelDark : new Color(0.06f, 0.06f, 0.12f);
        GameObject topBar = CreatePanel(canvasGO.transform, "TopBar", 
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -100), new Vector2(0, 0), 
            topBarColor);
        
        GameObject gemsObj = CreateText(topBar.transform, "GemsText", "Gems: 9999", 
            new Vector2(0.15f, 0.5f), new Vector2(0.15f, 0.5f), Vector2.zero, new Vector2(300, 50));
        GameObject goldObj = CreateText(topBar.transform, "GoldText", "Gold: 1.5M", 
            new Vector2(0.4f, 0.5f), new Vector2(0.4f, 0.5f), Vector2.zero, new Vector2(300, 50));
        GameObject staminaObj = CreateText(topBar.transform, "StaminaText", "Stamina: 120/120", 
            new Vector2(0.65f, 0.5f), new Vector2(0.65f, 0.5f), Vector2.zero, new Vector2(300, 50));
        GameObject floorObj = CreateText(topBar.transform, "FloorText", "Floor: 1", 
            new Vector2(0.9f, 0.5f), new Vector2(0.9f, 0.5f), Vector2.zero, new Vector2(200, 50));

        ResourceDisplay resDisplay = topBar.AddComponent<ResourceDisplay>();
        SetPrivateField(resDisplay, "gemsText", gemsObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(resDisplay, "goldText", goldObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(resDisplay, "staminaText", staminaObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(resDisplay, "floorText", floorObj.GetComponent<TextMeshProUGUI>());

        // === HERO SHOWCASE (Left 40%) ===
        Color showcaseColor = ThemeConfig.Instance != null ? ThemeConfig.Instance.backgroundDark : new Color(0.04f, 0.04f, 0.08f);
        GameObject showcase = CreatePanel(canvasGO.transform, "HeroShowcasePanel",
            new Vector2(0, 0), new Vector2(0.4f, 1), new Vector2(30, 110), new Vector2(-15, -130), 
            showcaseColor);
        
        GameObject portrait = new GameObject("Portrait");
        Undo.RegisterCreatedObjectUndo(portrait, "Create Portrait");
        portrait.transform.SetParent(showcase.transform, false);
        Image pImg = portrait.AddComponent<Image>();
        pImg.color = new Color(0.1f, 0.1f, 0.2f);
        RectTransform prt = portrait.GetComponent<RectTransform>();
        prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.6f);
        prt.anchoredPosition = Vector2.zero;
        prt.sizeDelta = new Vector2(400, 400);

        GameObject nameObj = CreateText(showcase.transform, "NameText", "Hero Name",
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), Vector2.zero, new Vector2(800, 60));
        
        if (ThemeConfig.Instance != null)
            nameObj.GetComponent<TextMeshProUGUI>().color = ThemeConfig.Instance.accentGold;

        GameObject starsObj = CreateText(showcase.transform, "StarsText", "*****",
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), Vector2.zero, new Vector2(800, 50));
        GameObject statsObj = CreateText(showcase.transform, "StatsText", "ATK: -- | DEF: -- | HP: --",
            new Vector2(0.5f, 0.14f), new Vector2(0.5f, 0.14f), Vector2.zero, new Vector2(800, 40));
        GameObject lvlObj = CreateText(showcase.transform, "LevelText", "Level 1",
            new Vector2(0.5f, 0.06f), new Vector2(0.5f, 0.06f), Vector2.zero, new Vector2(800, 40));

        HeroShowcasePanel hsp = showcase.AddComponent<HeroShowcasePanel>();
        SetPrivateField(hsp, "heroPortrait", pImg);
        SetPrivateField(hsp, "nameText", nameObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(hsp, "starsText", starsObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(hsp, "statsText", statsObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(hsp, "levelText", lvlObj.GetComponent<TextMeshProUGUI>());

        // === MENU GRID (Right 60%) ===
        GameObject gridParent = CreatePanel(canvasGO.transform, "MenuGrid",
            new Vector2(0.4f, 0), new Vector2(1, 1), new Vector2(15, 110), new Vector2(-30, -130), 
            new Color(0, 0, 0, 0));
        
        GridLayoutGroup grid = gridParent.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(420, 180);
        grid.spacing = new Vector2(30, 25);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.MiddleCenter;

        string[] features = new[]
        {
            "Summon", "Hero", "Inventory", "Training", "Tower", "Dungeon",
            "Memorial", "Tavern", "Blacksmith", "Research", "Synthesis", "Settings"
        };

        foreach (string f in features)
        {
            GameObject btnGO = new GameObject("Btn_" + f);
            Undo.RegisterCreatedObjectUndo(btnGO, "Create " + f);
            btnGO.transform.SetParent(gridParent.transform, false);
            
            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = GetFeatureColor(f);
            
            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            
            LobbyMenuButton lmb = btnGO.AddComponent<LobbyMenuButton>();
            
            GameObject lbl = CreateText(btnGO.transform, "Label", f,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 60));
            
            SetPrivateField(lmb, "button", btn);
            SetPrivateField(lmb, "labelText", lbl.GetComponent<TextMeshProUGUI>());
            
            lmb.Setup(f, f, null, btnImg.color);
        }

        // === BOTTOM BAR ===
        GameObject botBar = CreatePanel(canvasGO.transform, "BottomBar",
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 80), 
            new Color(0.06f, 0.06f, 0.12f));

        string[] bottoms = { "Quests", "Mail", "Stats", "WorldMap" };
        for (int i = 0; i < bottoms.Length; i++)
        {
            float anchorPct = 0.125f + (i * 0.25f);
            GameObject btnGO = new GameObject("Btn_" + bottoms[i]);
            Undo.RegisterCreatedObjectUndo(btnGO, "Create " + bottoms[i]);
            btnGO.transform.SetParent(botBar.transform, false);
            
            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.15f, 0.15f, 0.25f);
            
            RectTransform brt = btnGO.GetComponent<RectTransform>();
            brt.anchorMin = brt.anchorMax = new Vector2(anchorPct, 0.5f);
            brt.anchoredPosition = Vector2.zero;
            brt.sizeDelta = new Vector2(300, 60);
            
            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            
            CreateText(btnGO.transform, "Label", bottoms[i],
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280, 50));
            
            string cap = bottoms[i];
            btn.onClick.AddListener(() => Debug.Log("[Lobby] Bottom: " + cap));
        }

        // Wire adapter
        SetPrivateField(adapter, "showcasePanel", hsp);
        SetPrivateField(adapter, "resourceDisplay", resDisplay);

        EditorUtility.DisplayDialog("Success", "Full Lobby UI built!", "OK");
    }

    static GameObject CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax, Color c)
    {
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = c;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = min; rt.anchorMax = max;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        return go;
    }

    static GameObject CreateText(Transform parent, string name, string text, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 28; tmp.color = Color.white;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = min; rt.anchorMax = max;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        return go;
    }

    static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(target, value);
    }

    static Color GetFeatureColor(string f)
    {
        switch (f)
        {
            case "Summon": return new Color(0.6f, 0.1f, 0.8f);
            case "Tower": return new Color(0.9f, 0.4f, 0f);
            case "Synthesis": return new Color(0.5f, 0.5f, 0.1f);
            case "Dungeon": return new Color(0.2f, 0.1f, 0.6f);
            case "Memorial": return new Color(0.1f, 0.4f, 0.2f);
            case "Tavern": return new Color(0.4f, 0.25f, 0.15f);
            case "Blacksmith": return new Color(0.2f, 0.25f, 0.3f);
            case "Research": return new Color(0f, 0.4f, 0.4f);
            case "Training": return new Color(0.4f, 0.1f, 0.1f);
            case "Hero": return new Color(0.1f, 0.15f, 0.5f);
            case "Inventory": return new Color(0.05f, 0.3f, 0.2f);
            default: return new Color(0.15f, 0.15f, 0.2f);
        }
    }
}
#endif
