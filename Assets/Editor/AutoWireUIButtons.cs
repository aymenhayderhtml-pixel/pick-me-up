using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.UI;
using UnityEngine.Events;

#if UNITY_EDITOR
public class AutoWireUIButtons
{
    [MenuItem("Tools/PickMeUp/Auto-Wire All Buttons")]
    public static void AutoWireAll()
    {
        int lobbyCount = 0;
        int summonCount = 0;

        // === LOBBY CANVAS ===
        GameObject lobbyCanvas = GameObject.Find("LobbyCanvas");
        if (lobbyCanvas != null)
        {
            LobbyUIAdapter adapter = lobbyCanvas.GetComponentInChildren<LobbyUIAdapter>();
            if (adapter == null)
            {
                Debug.LogWarning("[AutoWire] LobbyCanvas found but LobbyUIAdapter is missing!");
            }
            else
            {
                LobbyMenuButton[] buttons = lobbyCanvas.GetComponentsInChildren<LobbyMenuButton>(true);
                foreach (var lmb in buttons)
                {
                    Button btn = lmb.GetComponent<Button>();
                    if (btn != null)
                    {
                        string fid = lmb.FeatureId;
                        if (string.IsNullOrEmpty(fid)) fid = lmb.gameObject.name.Replace("Btn_", "");
                        
                        btn.onClick.RemoveAllListeners();
                        // Note: Using AddListener here for dynamic routing since OpenFeature takes a string parameter
                        btn.onClick.AddListener(() => adapter.OpenFeature(fid));
                        lobbyCount++;
                        EditorUtility.SetDirty(btn);
                    }
                }
            }
        }

        // === SUMMON CANVAS ===
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        GameObject summonCanvas = null;
        foreach (var root in roots)
        {
            if (root.name == "SummonCanvas") { summonCanvas = root; break; }
            var found = FindInChildren(root, "SummonCanvas");
            if (found != null) { summonCanvas = found; break; }
        }

        if (summonCanvas != null)
        {
            SummonUIController controller = summonCanvas.GetComponentInChildren<SummonUIController>(true);
            if (controller == null)
            {
                Debug.LogWarning("[AutoWire] SummonUIController missing!");
            }
            else
            {
                Button[] allButtons = summonCanvas.GetComponentsInChildren<Button>(true);
                foreach (var btn in allButtons)
                {
                    string n = btn.gameObject.name;
                    bool wired = false;

                    if (n.Contains("Summon1x")) { WireButton(btn, controller.Summon1x); wired = true; }
                    else if (n.Contains("Summon10x")) { WireButton(btn, controller.Summon10x); wired = true; }
                    else if (n.Contains("Back")) { WireButton(btn, controller.GoBack); wired = true; }
                    else if (n.Contains("Close") || n.Contains("Continue")) { WireButton(btn, controller.CloseResults); wired = true; }

                    if (wired) summonCount++;
                }
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Auto-Wire Results",
            $"Wired {lobbyCount} Lobby buttons and {summonCount} Summon buttons.", "OK");
    }

    static void WireButton(Button btn, UnityAction action)
    {
        // Clear ALL persistent listeners first by index
        int count = btn.onClick.GetPersistentEventCount();
        for (int i = count - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(btn.onClick, i);

        btn.onClick.RemoveAllListeners(); // clears runtime

        UnityEventTools.AddPersistentListener(btn.onClick, action);
        EditorUtility.SetDirty(btn);
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
}
#endif
