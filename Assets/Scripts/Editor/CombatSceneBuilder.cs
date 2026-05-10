using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

namespace PickMeUp.Editor
{
    public static class CombatSceneBuilder
    {
        [MenuItem("Tools/PickMeUp/Build Combat Scene")]
        public static void BuildCombatScene()
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camObj = new GameObject("Main Camera");
            var camera = camObj.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.05f, 0.08f);

            // Light
            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.6f;

            // Canvas
            var canvasObj = new GameObject("CombatCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Theme
            var theme = AssetDatabase.FindAssets("t:ThemeConfig");
            ThemeConfig themeConfig = null;
            if (theme.Length > 0)
            {
                themeConfig = AssetDatabase.LoadAssetAtPath<ThemeConfig>(AssetDatabase.GUIDToAssetPath(theme[0]));
            }

            // Root
            var root = CreatePanel(canvasObj.transform, "Root", Color.clear);
            var rootRT = root.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // Title
            var title = CreateText(root.transform, "CombatTitle", "COMBAT", 48, TextAnchor.MiddleCenter);
            var titleRT = title.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.9f);
            titleRT.anchorMax = new Vector2(1, 1f);
            titleRT.offsetMin = new Vector2(20, 10);
            titleRT.offsetMax = new Vector2(-20, -10);

            // Turn Indicator
            var turnObj = CreateText(root.transform, "TurnText", "Preparing...", 24, TextAnchor.MiddleCenter);
            var turnRT = turnObj.GetComponent<RectTransform>();
            turnRT.anchorMin = new Vector2(0.3f, 0.82f);
            turnRT.anchorMax = new Vector2(0.7f, 0.9f);
            turnRT.offsetMin = Vector2.zero;
            turnRT.offsetMax = Vector2.zero;

            // Enemy Area (top)
            var enemyArea = CreatePanel(root.transform, "EnemyArea", new Color(0.1f, 0.08f, 0.08f, 0.3f));
            var enemyRT = enemyArea.GetComponent<RectTransform>();
            enemyRT.anchorMin = new Vector2(0.05f, 0.55f);
            enemyRT.anchorMax = new Vector2(0.95f, 0.8f);
            enemyRT.offsetMin = Vector2.zero;
            enemyRT.offsetMax = Vector2.zero;

            var enemyHLG = enemyArea.AddComponent<HorizontalLayoutGroup>();
            enemyHLG.spacing = 20;
            enemyHLG.childAlignment = TextAnchor.MiddleCenter;
            enemyHLG.childControlWidth = false;
            enemyHLG.childControlHeight = false;
            enemyHLG.childForceExpandWidth = false;

            // Player Area (bottom)
            var playerArea = CreatePanel(root.transform, "PlayerArea", new Color(0.08f, 0.1f, 0.08f, 0.3f));
            var playerRT = playerArea.GetComponent<RectTransform>();
            playerRT.anchorMin = new Vector2(0.05f, 0.3f);
            playerRT.anchorMax = new Vector2(0.95f, 0.52f);
            playerRT.offsetMin = Vector2.zero;
            playerRT.offsetMax = Vector2.zero;

            var playerHLG = playerArea.AddComponent<HorizontalLayoutGroup>();
            playerHLG.spacing = 20;
            playerHLG.childAlignment = TextAnchor.MiddleCenter;
            playerHLG.childControlWidth = false;
            playerHLG.childControlHeight = false;
            playerHLG.childForceExpandWidth = false;

            // Action Panel
            var actionPanel = CreatePanel(root.transform, "ActionPanel", themeConfig?.panelDark ?? new Color(0.15f, 0.15f, 0.2f, 0.95f));
            var actionRT = actionPanel.GetComponent<RectTransform>();
            actionRT.anchorMin = new Vector2(0.2f, 0.05f);
            actionRT.anchorMax = new Vector2(0.8f, 0.27f);
            actionRT.offsetMin = Vector2.zero;
            actionRT.offsetMax = Vector2.zero;

            var actionHLG = actionPanel.AddComponent<HorizontalLayoutGroup>();
            actionHLG.spacing = 30;
            actionHLG.padding = new RectOffset(30, 30, 20, 20);
            actionHLG.childAlignment = TextAnchor.MiddleCenter;
            actionHLG.childControlWidth = false;
            actionHLG.childControlHeight = false;
            actionHLG.childForceExpandWidth = false;

            var attackBtn = CreateButton(actionPanel.transform, "AttackBtn", "ATTACK", new Color(0.8f, 0.2f, 0.2f));
            var skillBtn = CreateButton(actionPanel.transform, "SkillBtn", "SKILL", new Color(0.2f, 0.5f, 0.9f));
            var defendBtn = CreateButton(actionPanel.transform, "DefendBtn", "DEFEND", new Color(0.2f, 0.7f, 0.3f));

            foreach (var btn in new[] { attackBtn, skillBtn, defendBtn })
            {
                var rt = btn.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(160, 70);
            }

            actionPanel.SetActive(false);

            // Result Overlay
            var resultOverlay = CreatePanel(root.transform, "ResultOverlay", new Color(0, 0, 0, 0.85f));
            var resultRT = resultOverlay.GetComponent<RectTransform>();
            resultRT.anchorMin = Vector2.zero;
            resultRT.anchorMax = Vector2.one;
            resultRT.offsetMin = Vector2.zero;
            resultRT.offsetMax = Vector2.zero;
            resultOverlay.SetActive(false);

            var resultVLG = resultOverlay.AddComponent<VerticalLayoutGroup>();
            resultVLG.spacing = 20;
            resultVLG.padding = new RectOffset(50, 50, 100, 100);
            resultVLG.childAlignment = TextAnchor.MiddleCenter;
            resultVLG.childControlWidth = true;
            resultVLG.childControlHeight = false;

            var resultTitle = CreateText(resultOverlay.transform, "ResultTitle", "VICTORY!", 64, TextAnchor.MiddleCenter);
            var resultDetails = CreateText(resultOverlay.transform, "ResultDetails", "Rewards:\n100 Gold\n50 EXP", 28, TextAnchor.MiddleCenter);
            var resultBtn = CreateButton(resultOverlay.transform, "ResultBtn", "CONTINUE", themeConfig?.accentGold ?? new Color(0.9f, 0.7f, 0.2f));
            var resultBtnRT = resultBtn.GetComponent<RectTransform>();
            resultBtnRT.sizeDelta = new Vector2(220, 70);

            // Damage Number Container
            var dmgContainer = new GameObject("DamageNumbers");
            dmgContainer.transform.SetParent(root.transform, false);
            var dmgRT = dmgContainer.AddComponent<RectTransform>();
            dmgRT.anchorMin = Vector2.zero;
            dmgRT.anchorMax = Vector2.one;
            dmgRT.offsetMin = Vector2.zero;
            dmgRT.offsetMax = Vector2.zero;

            // Damage Number Prefab
            var dmgPrefab = new GameObject("DamageNumberPrefab");
            dmgPrefab.SetActive(false);
            var dmgPrefabRT = dmgPrefab.AddComponent<RectTransform>();
            dmgPrefabRT.sizeDelta = new Vector2(150, 60);
            var dmgTxt = dmgPrefab.AddComponent<TextMeshProUGUI>();
            dmgTxt.fontSize = 36;
            dmgTxt.color = Color.white;
            dmgTxt.alignment = TextAlignmentOptions.Center;
            dmgTxt.font = TMP_Settings.defaultFontAsset;

            string dmgPrefabPath = "Assets/Prefabs/UI/DamageNumberPrefab.prefab";
            if (!System.IO.Directory.Exists("Assets/Prefabs/UI"))
                System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");
            PrefabUtility.SaveAsPrefabAsset(dmgPrefab, dmgPrefabPath);

            // Unit Panel Prefab
            var unitPrefab = new GameObject("UnitPanelPrefab");
            unitPrefab.SetActive(false);
            var upRT = unitPrefab.AddComponent<RectTransform>();
            upRT.sizeDelta = new Vector2(160, 120);
            var upImg = unitPrefab.AddComponent<Image>();
            upImg.color = themeConfig?.panelDark ?? new Color(0.15f, 0.15f, 0.2f, 0.9f);
            upImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            upImg.type = Image.Type.Sliced;

            // Name
            var upName = CreateText(unitPrefab.transform, "Name", "Hero", 18, TextAnchor.UpperLeft);
            var upNameRT = upName.GetComponent<RectTransform>();
            upNameRT.anchorMin = new Vector2(0, 0.7f);
            upNameRT.anchorMax = new Vector2(1, 1);
            upNameRT.offsetMin = new Vector2(8, 0);
            upNameRT.offsetMax = new Vector2(-8, -2);

            // HP Bar Bg
            var hpBg = new GameObject("HPBarBg");
            hpBg.transform.SetParent(unitPrefab.transform, false);
            var hpBgImg = hpBg.AddComponent<Image>();
            hpBgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            var hpBgRT = hpBg.GetComponent<RectTransform>();
            hpBgRT.anchorMin = new Vector2(0.05f, 0.45f);
            hpBgRT.anchorMax = new Vector2(0.95f, 0.6f);
            hpBgRT.offsetMin = Vector2.zero;
            hpBgRT.offsetMax = Vector2.zero;

            // HP Bar Fill
            var hpFill = new GameObject("HPBarFill");
            hpFill.transform.SetParent(hpBg.transform, false);
            var hpFillImg = hpFill.AddComponent<Image>();
            hpFillImg.color = new Color(0.2f, 0.8f, 0.3f);
            var hpFillRT = hpFill.GetComponent<RectTransform>();
            hpFillRT.anchorMin = Vector2.zero;
            hpFillRT.anchorMax = Vector2.one;
            hpFillRT.offsetMin = Vector2.zero;
            hpFillRT.offsetMax = Vector2.zero;

            // HP Text
            var hpText = CreateText(unitPrefab.transform, "HPText", "100/100", 14, TextAnchor.MiddleLeft);
            var hpTextRT = hpText.GetComponent<RectTransform>();
            hpTextRT.anchorMin = new Vector2(0, 0.25f);
            hpTextRT.anchorMax = new Vector2(1, 0.45f);
            hpTextRT.offsetMin = new Vector2(8, 0);
            hpTextRT.offsetMax = new Vector2(-8, 0);

            // Level
            var lvlText = CreateText(unitPrefab.transform, "LevelText", "Lv.1", 14, TextAnchor.UpperRight);
            var lvlRT = lvlText.GetComponent<RectTransform>();
            lvlRT.anchorMin = new Vector2(0.7f, 0.7f);
            lvlRT.anchorMax = new Vector2(1, 1);
            lvlRT.offsetMin = new Vector2(0, 0);
            lvlRT.offsetMax = new Vector2(-8, -2);

            // Dead Overlay
            var deadOverlay = new GameObject("DeadOverlay");
            deadOverlay.transform.SetParent(unitPrefab.transform, false);
            var deadImg = deadOverlay.AddComponent<Image>();
            deadImg.color = new Color(0, 0, 0, 0.6f);
            var deadRT = deadOverlay.GetComponent<RectTransform>();
            deadRT.anchorMin = Vector2.zero;
            deadRT.anchorMax = Vector2.one;
            deadRT.offsetMin = Vector2.zero;
            deadRT.offsetMax = Vector2.zero;
            deadOverlay.SetActive(false);

            string unitPrefabPath = "Assets/Prefabs/UI/UnitPanelPrefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(unitPrefab, unitPrefabPath);

            // CombatManager
            var mgrObj = new GameObject("CombatManager");
            var combatMgr = mgrObj.AddComponent<PickMeUp.Combat.CombatManager>();
            combatMgr.playerSpawnParent = playerArea.transform;
            combatMgr.enemySpawnParent = enemyArea.transform;

            // CombatUIController
            var combatUI = canvasObj.AddComponent<PickMeUp.UI.CombatUIController>();
            combatUI.playerPanelContainer = playerArea.transform;
            combatUI.enemyPanelContainer = enemyArea.transform;
            combatUI.unitPanelPrefab = unitPrefab;
            combatUI.actionPanel = actionPanel;
            combatUI.attackButton = attackBtn.GetComponent<Button>();
            combatUI.skillButton = skillBtn.GetComponent<Button>();
            combatUI.defendButton = defendBtn.GetComponent<Button>();
            combatUI.turnText = turnObj.GetComponent<TextMeshProUGUI>();
            combatUI.resultOverlay = resultOverlay;
            combatUI.resultTitle = resultTitle.GetComponent<TextMeshProUGUI>();
            combatUI.resultDetails = resultDetails.GetComponent<TextMeshProUGUI>();
            combatUI.resultButton = resultBtn.GetComponent<Button>();
            combatUI.damageNumberPrefab = dmgPrefab;
            combatUI.damageNumberContainer = dmgContainer.transform;
            combatUI.theme = themeConfig;

            // Save scene
            string scenePath = "Assets/Scenes/Combat.unity";
            if (!System.IO.Directory.Exists("Assets/Scenes"))
                System.IO.Directory.CreateDirectory("Assets/Scenes");

            EditorSceneManager.SaveScene(newScene, scenePath);

            // Cleanup temp objects from scene
            Object.DestroyImmediate(dmgPrefab);
            Object.DestroyImmediate(unitPrefab);

            Debug.Log($"[CombatSceneBuilder] Combat scene built at {scenePath}");
            Debug.Log($"[CombatSceneBuilder] Remember to: 1) Add 'Combat' to Build Settings, 2) TowerManager.EnterFloor() will auto-transition here");

            EditorUtility.DisplayDialog("Combat Scene Built",
                "Combat scene created successfully!\n\nNext steps:\n1. Add 'Combat' to Build Settings\n2. Ensure Tower scene routes to Combat on Enter Floor",
                "OK");
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var img = obj.AddComponent<Image>();
            img.color = color;
            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return obj;
        }

        private static GameObject CreateText(Transform parent, string name, string text, int fontSize, TextAnchor alignment)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = GetTMPAlignment(alignment);
            tmp.color = Color.white;
            tmp.font = TMP_Settings.defaultFontAsset;
            return obj;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var btn = obj.AddComponent<Button>();
            var img = obj.AddComponent<Image>();
            img.color = color;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            var rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160, 50);

            var txtObj = CreateText(obj.transform, "Text", text, 20, TextAnchor.MiddleCenter);
            var txtRT = txtObj.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = new Vector2(10, 5);
            txtRT.offsetMax = new Vector2(-10, -5);

            var colors = btn.colors;
            colors.highlightedColor = color * 1.2f;
            colors.pressedColor = color * 0.8f;
            colors.disabledColor = new Color(color.r, color.g, color.b, 0.4f);
            btn.colors = colors;

            return obj;
        }

        private static TextAlignmentOptions GetTMPAlignment(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Center;
            }
        }
    }
}
