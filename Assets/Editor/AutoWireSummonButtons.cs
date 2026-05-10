using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AutoWireSummonButtons
{
    [MenuItem("Tools/PickMeUp/Wire Summon Buttons")]
    public static void WireSummonButtons()
    {
        GameObject canvas = null;

        // Find including inactive objects
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in roots)
        {
            if (root.name == "SummonCanvas") { canvas = root; break; }
            var found = FindInChildren(root, "SummonCanvas");
            if (found != null) { canvas = found; break; }
        }

        if (canvas == null) { EditorUtility.DisplayDialog("Error", "SummonCanvas not found!", "OK"); return; }

        var controller = canvas.GetComponentInChildren<SummonUIController>(true);
        if (controller == null) controller = canvas.AddComponent<SummonUIController>();

        var btn1x  = FindButton(canvas, "Summon1x",  "Btn_Summon1x", "1x");
        var btn10x = FindButton(canvas, "Summon10x", "Btn_Summon10x", "10x");
        var btnBack = FindButton(canvas, "Back",     "Btn_Back", "BACK");

        if (btn1x != null)  { btn1x.onClick.RemoveAllListeners();  btn1x.onClick.AddListener(controller.Summon1x); }
        if (btn10x != null) { btn10x.onClick.RemoveAllListeners(); btn10x.onClick.AddListener(controller.Summon10x); }
        if (btnBack != null){ btnBack.onClick.RemoveAllListeners(); btnBack.onClick.AddListener(controller.GoBack); }

        // Mark dirty so Unity saves the wiring
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Done", $"Wired: 1x={btn1x != null}, 10x={btn10x != null}, Back={btnBack != null}", "OK");
    }

    static GameObject FindInChildren(GameObject parent, string targetName)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.name == targetName) return child.gameObject;
            var found = FindInChildren(child.gameObject, targetName);
            if (found != null) return found;
        }
        return null;
    }

    static Button FindButton(GameObject parent, params string[] names)
    {
        var allTransforms = parent.GetComponentsInChildren<Transform>(true); // true = include inactive
        foreach (var name in names)
            foreach (var t in allTransforms)
                if (t.name == name && t.TryGetComponent<Button>(out var btn)) return btn;
        return null;
    }
}
