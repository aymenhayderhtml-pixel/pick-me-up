using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
public class SummonSceneBuilder
{
    [MenuItem("Tools/PickMeUp/Build Summon UI")]
    public static void BuildSummonUI()
    {
        GameObject existing = GameObject.Find("SummonCanvas");
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Rebuild?", "SummonCanvas exists. Rebuild?", "Yes", "Cancel"))
                return;
            Undo.DestroyObjectImmediate(existing);
        }

        // === CANVAS ===
        GameObject canvasGO = new GameObject("SummonCanvas");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Summon Canvas");
        
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2340f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<SafeAreaFitter>();

        // === CONTROLLER ===
        GameObject ctrlGO = new GameObject("SummonManager");
        ctrlGO.transform.SetParent(canvasGO.transform, false);
        SummonUIController controller = ctrlGO.AddComponent<SummonUIController>();

        // === TOP BAR ===
        Color topColor = ThemeConfig.Instance != null ? ThemeConfig.Instance.panelDark : new Color(0.06f, 0.06f, 0.12f);
        GameObject topBar = CreatePanel(canvasGO.transform, "TopBar", 
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -100), new Vector2(0, 0), topColor);
        
        GameObject gemsText = CreateText(topBar.transform, "GemsText", "Gems: 1000", 
            new Vector2(0.85f, 0.5f), new Vector2(0.85f, 0.5f), Vector2.zero, new Vector2(400, 60));
        
        GameObject backBtnGO = CreateButton(topBar.transform, "Btn_Back", "BACK",
            new Vector2(0.05f, 0.5f), new Vector2(0.05f, 0.5f), Vector2.zero, new Vector2(200, 60), 
            new Color(0.3f, 0.3f, 0.3f));
        Button backBtn = backBtnGO.GetComponent<Button>();
        backBtn.onClick.RemoveAllListeners();
        backBtn.onClick.AddListener(controller.GoBack);

        // === PORTAL AREA ===
        GameObject portal = CreatePanel(canvasGO.transform, "PortalArea",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-400, -200), new Vector2(400, 400),
            new Color(0.15f, 0.05f, 0.3f)); // Purple theme
        
        // Add a "Glow" or decorative text
        CreateText(portal.transform, "PortalLabel", "SUMMON PORTAL",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 100))
            .GetComponent<TextMeshProUGUI>().fontSize = 50;

        // === BUTTONS ===
        GameObject btn1x = CreateButton(canvasGO.transform, "Btn_Summon1x", "Summon 1x\n(100 Gems)",
            new Vector2(0.35f, 0.2f), new Vector2(0.35f, 0.2f), Vector2.zero, new Vector2(400, 120),
            new Color(0.5f, 0.1f, 0.8f)); // Purple
        Button s1xBtn = btn1x.GetComponent<Button>();
        s1xBtn.onClick.RemoveAllListeners();
        s1xBtn.onClick.AddListener(controller.Summon1x);

        GameObject btn10x = CreateButton(canvasGO.transform, "Btn_Summon10x", "Summon 10x\n(1000 Gems)",
            new Vector2(0.65f, 0.2f), new Vector2(0.65f, 0.2f), Vector2.zero, new Vector2(400, 120),
            new Color(0.8f, 0.6f, 0.1f)); // Gold
        Button s10xBtn = btn10x.GetComponent<Button>();
        s10xBtn.onClick.RemoveAllListeners();
        s10xBtn.onClick.AddListener(controller.Summon10x);

        // === RESULTS PANEL ===
        GameObject results = CreatePanel(canvasGO.transform, "ResultsPanel",
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
            new Color(0, 0, 0, 0.9f));
        
        GameObject resContent = CreateText(results.transform, "ResultsText", "Results Here...",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1800, 800));
        resContent.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        
        GameObject closeBtn = CreateButton(results.transform, "Btn_Close", "CONTINUE",
            new Vector2(0.5f, 0.1f), new Vector2(0.5f, 0.1f), Vector2.zero, new Vector2(400, 80),
            new Color(0.2f, 0.6f, 0.2f));
        closeBtn.GetComponent<Button>().onClick.AddListener(controller.CloseResults);

        // === WIRE CONTROLLER ===
        SetPrivateField(controller, "gemsText", gemsText.GetComponent<TextMeshProUGUI>());
        SetPrivateField(controller, "resultsPanel", results);
        SetPrivateField(controller, "resultsContentText", resContent.GetComponent<TextMeshProUGUI>());

        results.SetActive(false);

        EditorUtility.DisplayDialog("Success", "Summon UI built!", "OK");
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

    static GameObject CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, Vector2 pos, Vector2 size, Color c)
    {
        GameObject btnGO = CreatePanel(parent, name, min, max, Vector2.zero, size, c);
        btnGO.GetComponent<RectTransform>().anchoredPosition = pos;
        btnGO.AddComponent<Button>();
        CreateText(btnGO.transform, "Label", label, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        return btnGO;
    }

    static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(target, value);
    }
}
#endif
