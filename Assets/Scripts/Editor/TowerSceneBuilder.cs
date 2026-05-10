using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace PickMeUp.Editor
{
    public static class TowerSceneBuilder
    {
        [MenuItem("Tools/PickMeUp/Build Tower Scene")]
        public static void BuildTowerScene()
        {
            // Create new scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Setup camera
            var camera = Camera.main;
            if (camera == null)
            {
                var camObj = new GameObject("Main Camera");
                camera = camObj.AddComponent<Camera>();
                camera.tag = "MainCamera";
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.08f, 0.08f, 0.12f); // Dark blue-black
            }

            // Setup lighting
            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.8f;

            // Create Canvas
            var canvasObj = new GameObject("TowerCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create EventSystem
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Theme config (create or find)
            var theme = AssetDatabase.FindAssets("t:ThemeConfig");
            ThemeConfig themeConfig = null;
            if (theme.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(theme[0]);
                themeConfig = AssetDatabase.LoadAssetAtPath<ThemeConfig>(path);
            }

            // Build UI Hierarchy
            var rootPanel = CreatePanel(canvasObj.transform, "RootPanel", Color.clear);
            var rt = rootPanel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Title
            var titleObj = CreateText(rootPanel.transform, "TowerTitle", "THE TOWER", 48, TextAnchor.MiddleCenter);
            var titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.9f);
            titleRT.anchorMax = new Vector2(1, 1f);
            titleRT.offsetMin = new Vector2(20, 10);
            titleRT.offsetMax = new Vector2(-20, -10);

            // Info Bar
            var infoBar = CreatePanel(rootPanel.transform, "InfoBar", themeConfig?.panelDark ?? new Color(0.15f, 0.15f, 0.2f, 0.9f));
            var infoRT = infoBar.GetComponent<RectTransform>();
            infoRT.anchorMin = new Vector2(0, 0.85f);
            infoRT.anchorMax = new Vector2(1, 0.9f);
            infoRT.offsetMin = new Vector2(20, 0);
            infoRT.offsetMax = new Vector2(-20, -5);

            var currentFloorText = CreateText(infoBar.transform, "CurrentFloor", "Current: 1", 20, TextAnchor.MiddleLeft);
            var currentRT = currentFloorText.GetComponent<RectTransform>();
            currentRT.anchorMin = new Vector2(0.02f, 0);
            currentRT.anchorMax = new Vector2(0.33f, 1);
            currentRT.offsetMin = Vector2.zero;
            currentRT.offsetMax = Vector2.zero;

            var maxFloorText = CreateText(infoBar.transform, "MaxFloor", "Highest: 0", 20, TextAnchor.MiddleCenter);
            var maxRT = maxFloorText.GetComponent<RectTransform>();
            maxRT.anchorMin = new Vector2(0.34f, 0);
            maxRT.anchorMax = new Vector2(0.66f, 1);
            maxRT.offsetMin = Vector2.zero;
            maxRT.offsetMax = Vector2.zero;

            var staminaText = CreateText(infoBar.transform, "StaminaText", "Stamina: 100", 20, TextAnchor.MiddleRight);
            var stamRT = staminaText.GetComponent<RectTransform>();
            stamRT.anchorMin = new Vector2(0.67f, 0);
            stamRT.anchorMax = new Vector2(0.98f, 1);
            stamRT.offsetMin = Vector2.zero;
            stamRT.offsetMax = Vector2.zero;

            // Floor List (Scrollable)
            var scrollObj = new GameObject("FloorScroll");
            scrollObj.transform.SetParent(rootPanel.transform, false);
            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            var scrollRT = scrollObj.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 0.3f);
            scrollRT.anchorMax = new Vector2(0.4f, 0.84f);
            scrollRT.offsetMin = new Vector2(20, 10);
            scrollRT.offsetMax = new Vector2(-10, -10);

            var scrollBg = scrollObj.AddComponent<Image>();
            scrollBg.color = themeConfig?.panelDark ?? new Color(0.12f, 0.12f, 0.16f, 0.8f);

            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);
            var vpRT = viewport.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;
            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = Vector2.one;
            contentRT.pivot = new Vector2(0.5f, 1f);
            contentRT.sizeDelta = new Vector2(0, 500);

            var contentVLG = content.AddComponent<VerticalLayoutGroup>();
            contentVLG.padding = new RectOffset(10, 10, 10, 10);
            contentVLG.spacing = 8;
            contentVLG.childAlignment = TextAnchor.UpperCenter;
            contentVLG.childControlWidth = true;
            contentVLG.childForceExpandWidth = true;
            contentVLG.childForceExpandHeight = false;

            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = vpRT;
            scrollRect.content = contentRT;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            // Detail Panel
            var detailPanel = CreatePanel(rootPanel.transform, "DetailPanel", themeConfig?.panelDark ?? new Color(0.15f, 0.15f, 0.2f, 0.95f));
            var detailRT = detailPanel.GetComponent<RectTransform>();
            detailRT.anchorMin = new Vector2(0.42f, 0.3f);
            detailRT.anchorMax = new Vector2(1, 0.84f);
            detailRT.offsetMin = new Vector2(10, 10);
            detailRT.offsetMax = new Vector2(-20, -10);

            var detailVLG = detailPanel.AddComponent<VerticalLayoutGroup>();
            detailVLG.padding = new RectOffset(20, 20, 20, 20);
            detailVLG.spacing = 12;
            detailVLG.childAlignment = TextAnchor.UpperLeft;
            detailVLG.childControlWidth = true;
            detailVLG.childControlHeight = false;
            detailVLG.childForceExpandWidth = true;

            var floorName = CreateText(detailPanel.transform, "FloorName", "Select a Floor", 28, TextAnchor.UpperLeft);
            var floorDesc = CreateText(detailPanel.transform, "FloorDesc", "Choose a floor from the list to view details and enter combat.", 18, TextAnchor.UpperLeft);
            var recLevel = CreateText(detailPanel.transform, "RecLevel", "", 18, TextAnchor.UpperLeft);
            var staminaCost = CreateText(detailPanel.transform, "StaminaCost", "", 18, TextAnchor.UpperLeft);
            var rewards = CreateText(detailPanel.transform, "Rewards", "", 18, TextAnchor.UpperLeft);
            var enemies = CreateText(detailPanel.transform, "Enemies", "", 18, TextAnchor.UpperLeft);

            // Action Buttons
            var btnPanel = CreatePanel(rootPanel.transform, "ButtonPanel", Color.clear);
            var btnRT = btnPanel.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0, 0.05f);
            btnRT.anchorMax = new Vector2(1, 0.28f);
            btnRT.offsetMin = new Vector2(20, 0);
            btnRT.offsetMax = new Vector2(-20, 0);

            var btnHLG = btnPanel.AddComponent<HorizontalLayoutGroup>();
            btnHLG.spacing = 20;
            btnHLG.childAlignment = TextAnchor.MiddleCenter;
            btnHLG.childControlWidth = false;
            btnHLG.childControlHeight = false;
            btnHLG.childForceExpandWidth = false;

            var enterBtn = CreateButton(btnPanel.transform, "EnterButton", "ENTER FLOOR", themeConfig?.accentGold ?? new Color(0.9f, 0.7f, 0.2f));
            var enterBtnRT = enterBtn.GetComponent<RectTransform>();
            enterBtnRT.sizeDelta = new Vector2(200, 60);

            var backBtn = CreateButton(btnPanel.transform, "BackButton", "BACK TO LOBBY", themeConfig?.backgroundDark ?? new Color(0.3f, 0.3f, 0.35f));
            var backBtnRT = backBtn.GetComponent<RectTransform>();
            backBtnRT.sizeDelta = new Vector2(200, 60);

            // Floor Button Prefab (for runtime instantiation)
            var prefabObj = new GameObject("FloorButtonPrefab");
            prefabObj.SetActive(false);
            var pfRT = prefabObj.AddComponent<RectTransform>();
            pfRT.sizeDelta = new Vector2(300, 50);

            var pfBtn = prefabObj.AddComponent<Button>();
            var pfImg = prefabObj.AddComponent<Image>();
            pfImg.color = themeConfig?.panelDark ?? new Color(0.2f, 0.2f, 0.25f);
            pfImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            pfImg.type = Image.Type.Sliced;

            var pfText = CreateText(prefabObj.transform, "FloorButtonText", "Floor 1", 20, TextAnchor.MiddleCenter);
            var pfTextRT = pfText.GetComponent<RectTransform>();
            pfTextRT.anchorMin = Vector2.zero;
            pfTextRT.anchorMax = Vector2.one;
            pfTextRT.offsetMin = new Vector2(10, 5);
            pfTextRT.offsetMax = new Vector2(-10, -5);

            // Save prefab
            string prefabPath = "Assets/Prefabs/UI/FloorButtonPrefab.prefab";
            if (!System.IO.Directory.Exists("Assets/Prefabs/UI"))
                System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");
            PrefabUtility.SaveAsPrefabAsset(prefabObj, prefabPath);

            // Setup TowerManager
            var towerMgrObj = new GameObject("TowerManager");
            var towerMgr = towerMgrObj.AddComponent<PickMeUp.Tower.TowerManager>();

            // Setup TowerUIController
            var towerUI = canvasObj.AddComponent<PickMeUp.UI.TowerUIController>();
            towerUI.floorListContainer = contentRT;
            towerUI.floorButtonPrefab = prefabObj;
            towerUI.detailPanel = detailPanel;
            towerUI.floorNameText = floorName.GetComponent<TextMeshProUGUI>();
            towerUI.floorDescriptionText = floorDesc.GetComponent<TextMeshProUGUI>();
            towerUI.recommendedLevelText = recLevel.GetComponent<TextMeshProUGUI>();
            towerUI.staminaCostText = staminaCost.GetComponent<TextMeshProUGUI>();
            towerUI.rewardText = rewards.GetComponent<TextMeshProUGUI>();
            towerUI.enemyInfoText = enemies.GetComponent<TextMeshProUGUI>();
            towerUI.enterButton = enterBtn.GetComponent<Button>();
            towerUI.backButton = backBtn.GetComponent<Button>();
            towerUI.currentFloorText = currentFloorText.GetComponent<TextMeshProUGUI>();
            towerUI.maxFloorText = maxFloorText.GetComponent<TextMeshProUGUI>();
            towerUI.staminaText = staminaText.GetComponent<TextMeshProUGUI>();
            towerUI.theme = themeConfig;

            // Save scene
            string scenePath = "Assets/Scenes/Tower.unity";
            if (!System.IO.Directory.Exists("Assets/Scenes"))
                System.IO.Directory.CreateDirectory("Assets/Scenes");

            EditorSceneManager.SaveScene(newScene, scenePath);

            // Cleanup temp prefab from scene
            Object.DestroyImmediate(prefabObj);

            Debug.Log($"[TowerSceneBuilder] Tower scene built successfully at {scenePath}");
            Debug.Log($"[TowerSceneBuilder] FloorButtonPrefab saved to {prefabPath}");

            EditorUtility.DisplayDialog("Tower Scene Built", 
                "Tower scene created successfully!\n\nNext steps:\n1. Add 'Tower' to Build Settings\n2. Create TowerFloorData ScriptableObjects\n3. Assign floorDatabase in TowerManager", "OK");
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
            colors.disabledColor = new Color(color.r, color.g, color.b, 0.5f);
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
