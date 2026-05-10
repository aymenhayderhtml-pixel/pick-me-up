using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

namespace PickMeUp.Editor
{
    public static class DungeonSceneBuilder
    {
        [MenuItem("Tools/PickMeUp/Build Dungeon Scene")]
        public static void BuildDungeonScene()
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camObj = new GameObject("Main Camera");
            var camera = camObj.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.05f, 0.1f);

            // Light
            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.6f;

            // Canvas
            var canvasObj = new GameObject("DungeonCanvas");
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

            // ===== HEADER =====
            var header = CreatePanel(root.transform, "Header", themeConfig?.panelDark ?? new Color(0.1f, 0.1f, 0.14f, 0.9f));
            var headerRT = header.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0, 0.9f);
            headerRT.anchorMax = new Vector2(1, 1f);
            headerRT.offsetMin = new Vector2(20, 10);
            headerRT.offsetMax = new Vector2(-20, -10);

            var title = CreateText(header.transform, "Title", "DUNGEONS", 44, TextAnchor.MiddleCenter);
            var titleRT = title.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.2f, 0);
            titleRT.anchorMax = new Vector2(0.8f, 1);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;

            var staminaText = CreateText(header.transform, "StaminaText", "Stamina: 100", 20, TextAnchor.MiddleLeft);
            var stamRT = staminaText.GetComponent<RectTransform>();
            stamRT.anchorMin = new Vector2(0.02f, 0);
            stamRT.anchorMax = new Vector2(0.3f, 1);
            stamRT.offsetMin = Vector2.zero;
            stamRT.offsetMax = Vector2.zero;

            var goldText = CreateText(header.transform, "GoldText", "Gold: 0", 20, TextAnchor.MiddleRight);
            var goldRT = goldText.GetComponent<RectTransform>();
            goldRT.anchorMin = new Vector2(0.7f, 0);
            goldRT.anchorMax = new Vector2(0.98f, 1);
            goldRT.offsetMin = Vector2.zero;
            goldRT.offsetMax = Vector2.zero;

            // ===== LEFT: DUNGEON LIST =====
            var listPanel = CreatePanel(root.transform, "ListPanel", themeConfig?.panelDark ?? new Color(0.08f, 0.08f, 0.12f, 0.85f));
            var listRT = listPanel.GetComponent<RectTransform>();
            listRT.anchorMin = new Vector2(0.02f, 0.05f);
            listRT.anchorMax = new Vector2(0.38f, 0.88f);
            listRT.offsetMin = Vector2.zero;
            listRT.offsetMax = Vector2.zero;

            var listScroll = new GameObject("ListScroll");
            listScroll.transform.SetParent(listPanel.transform, false);
            var listScrollRT = listScroll.AddComponent<RectTransform>();
            listScrollRT.anchorMin = new Vector2(0, 0);
            listScrollRT.anchorMax = new Vector2(1, 1);
            listScrollRT.offsetMin = new Vector2(10, 10);
            listScrollRT.offsetMax = new Vector2(-10, -10);
            var listSR = listScroll.AddComponent<ScrollRect>();

            var listVP = new GameObject("Viewport");
            listVP.transform.SetParent(listScroll.transform, false);
            var listVPRT = listVP.AddComponent<RectTransform>();
            listVPRT.anchorMin = Vector2.zero;
            listVPRT.anchorMax = Vector2.one;
            listVPRT.offsetMin = Vector2.zero;
            listVPRT.offsetMax = Vector2.zero;
            listVP.AddComponent<Image>().color = Color.clear;
            listVP.AddComponent<Mask>().showMaskGraphic = false;

            var listContent = new GameObject("Content");
            listContent.transform.SetParent(listVP.transform, false);
            var listContentRT = listContent.AddComponent<RectTransform>();
            listContentRT.anchorMin = new Vector2(0, 1);
            listContentRT.anchorMax = Vector2.one;
            listContentRT.pivot = new Vector2(0.5f, 1f);
            listContentRT.sizeDelta = new Vector2(0, 600);

            var listVLG = listContent.AddComponent<VerticalLayoutGroup>();
            listVLG.padding = new RectOffset(10, 10, 10, 10);
            listVLG.spacing = 10;
            listVLG.childAlignment = TextAnchor.UpperCenter;
            listVLG.childControlWidth = true;
            listVLG.childForceExpandWidth = true;
            listVLG.childForceExpandHeight = false;
            listContent.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            listSR.viewport = listVPRT;
            listSR.content = listContentRT;
            listSR.vertical = true;
            listSR.horizontal = false;

            // ===== RIGHT: DETAIL PANEL =====
            var detailPanel = CreatePanel(root.transform, "DetailPanel", themeConfig?.panelDark ?? new Color(0.1f, 0.1f, 0.14f, 0.9f));
            var detailRT = detailPanel.GetComponent<RectTransform>();
            detailRT.anchorMin = new Vector2(0.4f, 0.05f);
            detailRT.anchorMax = new Vector2(0.98f, 0.88f);
            detailRT.offsetMin = Vector2.zero;
            detailRT.offsetMax = Vector2.zero;

            var detailVLG = detailPanel.AddComponent<VerticalLayoutGroup>();
            detailVLG.padding = new RectOffset(20, 20, 20, 20);
            detailVLG.spacing = 12;
            detailVLG.childAlignment = TextAnchor.UpperCenter;
            detailVLG.childControlWidth = true;
            detailVLG.childControlHeight = false;

            // Icon
            var iconObj = new GameObject("DungeonIcon");
            iconObj.transform.SetParent(detailPanel.transform, false);
            var iconImg = iconObj.AddComponent<Image>();
            iconImg.color = Color.clear;
            var iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(100, 100);

            var dungeonName = CreateText(detailPanel.transform, "DungeonName", "Select a Dungeon", 30, TextAnchor.MiddleCenter);
            var dungeonType = CreateText(detailPanel.transform, "DungeonType", "", 18, TextAnchor.MiddleCenter);
            var dungeonDesc = CreateText(detailPanel.transform, "DungeonDesc", "Choose a dungeon from the list to view details.", 16, TextAnchor.UpperCenter);

            // Info row
            var infoPanel = CreatePanel(detailPanel.transform, "InfoPanel", Color.clear);
            var infoRT = infoPanel.GetComponent<RectTransform>();
            infoRT.sizeDelta = new Vector2(0, 30);
            var infoHLG = infoPanel.AddComponent<HorizontalLayoutGroup>();
            infoHLG.spacing = 20;
            infoHLG.childAlignment = TextAnchor.MiddleCenter;
            infoHLG.childControlWidth = false;

            var attemptsText = CreateText(infoPanel.transform, "AttemptsText", "Attempts: 0/3", 18, TextAnchor.MiddleLeft);
            var resetText = CreateText(infoPanel.transform, "ResetText", "Resets in: 12h", 18, TextAnchor.MiddleRight);
            var staminaCost = CreateText(infoPanel.transform, "StaminaCost", "Stamina: 10", 18, TextAnchor.MiddleCenter);

            // Difficulty selection
            var diffTitle = CreateText(detailPanel.transform, "DiffTitle", "DIFFICULTY", 20, TextAnchor.MiddleCenter);

            var diffPanel = CreatePanel(detailPanel.transform, "DiffPanel", Color.clear);
            var diffRT = diffPanel.GetComponent<RectTransform>();
            diffRT.sizeDelta = new Vector2(0, 50);
            var diffHLG = diffPanel.AddComponent<HorizontalLayoutGroup>();
            diffHLG.spacing = 10;
            diffHLG.childAlignment = TextAnchor.MiddleCenter;
            diffHLG.childControlWidth = false;
            diffHLG.childControlHeight = false;

            // Rewards
            var rewardTitle = CreateText(detailPanel.transform, "RewardTitle", "REWARD PREVIEW", 20, TextAnchor.MiddleCenter);
            var rewardText = CreateText(detailPanel.transform, "RewardText", "", 16, TextAnchor.UpperLeft);

            // Lock reason
            var lockText = CreateText(detailPanel.transform, "LockReason", "", 16, TextAnchor.MiddleCenter);

            // Action buttons
            var btnPanel = CreatePanel(detailPanel.transform, "BtnPanel", Color.clear);
            var btnRT = btnPanel.GetComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(0, 60);
            var btnHLG = btnPanel.AddComponent<HorizontalLayoutGroup>();
            btnHLG.spacing = 20;
            btnHLG.childAlignment = TextAnchor.MiddleCenter;
            btnHLG.childControlWidth = false;

            var enterBtn = CreateButton(btnPanel.transform, "EnterBtn", "ENTER DUNGEON", themeConfig?.accentGold ?? new Color(0.9f, 0.7f, 0.2f));
            var enterRT = enterBtn.GetComponent<RectTransform>();
            enterRT.sizeDelta = new Vector2(200, 55);

            var backBtn = CreateButton(btnPanel.transform, "BackBtn", "BACK", themeConfig?.backgroundDark ?? new Color(0.3f, 0.3f, 0.35f));
            var backRT = backBtn.GetComponent<RectTransform>();
            backRT.sizeDelta = new Vector2(120, 55);

            detailPanel.SetActive(false);

            // ===== PREFABS =====
            // Dungeon Card Prefab
            var cardPrefab = new GameObject("DungeonCardPrefab");
            cardPrefab.SetActive(false);
            var cardRT = cardPrefab.AddComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(0, 100);
            var cardImg = cardPrefab.AddComponent<Image>();
            cardImg.color = themeConfig?.panelDark ?? new Color(0.12f, 0.12f, 0.16f, 0.9f);
            cardImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            cardImg.type = Image.Type.Sliced;
            var cardBtn = cardPrefab.AddComponent<Button>();

            var cardIcon = new GameObject("Icon");
            cardIcon.transform.SetParent(cardPrefab.transform, false);
            var cardIconImg = cardIcon.AddComponent<Image>();
            cardIconImg.color = Color.clear;
            var cardIconRT = cardIcon.GetComponent<RectTransform>();
            cardIconRT.anchorMin = new Vector2(0.02f, 0.1f);
            cardIconRT.anchorMax = new Vector2(0.22f, 0.9f);
            cardIconRT.offsetMin = Vector2.zero;
            cardIconRT.offsetMax = Vector2.zero;

            var cardName = CreateText(cardPrefab.transform, "Name", "Dungeon Name", 20, TextAnchor.MiddleLeft);
            var cardNameRT = cardName.GetComponent<RectTransform>();
            cardNameRT.anchorMin = new Vector2(0.24f, 0.5f);
            cardNameRT.anchorMax = new Vector2(0.75f, 0.9f);
            cardNameRT.offsetMin = Vector2.zero;
            cardNameRT.offsetMax = Vector2.zero;

            var cardType = CreateText(cardPrefab.transform, "Type", "DAILY", 14, TextAnchor.MiddleLeft);
            var cardTypeRT = cardType.GetComponent<RectTransform>();
            cardTypeRT.anchorMin = new Vector2(0.24f, 0.2f);
            cardTypeRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardTypeRT.offsetMin = Vector2.zero;
            cardTypeRT.offsetMax = Vector2.zero;

            var cardAttempts = CreateText(cardPrefab.transform, "Attempts", "3/3", 14, TextAnchor.MiddleRight);
            var cardAttemptsRT = cardAttempts.GetComponent<RectTransform>();
            cardAttemptsRT.anchorMin = new Vector2(0.7f, 0.2f);
            cardAttemptsRT.anchorMax = new Vector2(0.98f, 0.5f);
            cardAttemptsRT.offsetMin = Vector2.zero;
            cardAttemptsRT.offsetMax = new Vector2(-5, 0);

            string cardPrefabPath = "Assets/Prefabs/UI/DungeonCardPrefab.prefab";
            if (!System.IO.Directory.Exists("Assets/Prefabs/UI"))
                System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");
            PrefabUtility.SaveAsPrefabAsset(cardPrefab, cardPrefabPath);

            // Difficulty Button Prefab
            var diffPrefab = new GameObject("DiffButtonPrefab");
            diffPrefab.SetActive(false);
            var diffPrefabRT = diffPrefab.AddComponent<RectTransform>();
            diffPrefabRT.sizeDelta = new Vector2(90, 45);
            var diffPrefabImg = diffPrefab.AddComponent<Image>();
            diffPrefabImg.color = themeConfig?.panelDark ?? new Color(0.15f, 0.15f, 0.2f);
            diffPrefabImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            diffPrefabImg.type = Image.Type.Sliced;
            var diffPrefabBtn = diffPrefab.AddComponent<Button>();

            var diffPrefabText = CreateText(diffPrefab.transform, "Text", "EASY", 16, TextAnchor.MiddleCenter);
            var diffPrefabTextRT = diffPrefabText.GetComponent<RectTransform>();
            diffPrefabTextRT.anchorMin = Vector2.zero;
            diffPrefabTextRT.anchorMax = Vector2.one;
            diffPrefabTextRT.offsetMin = new Vector2(5, 3);
            diffPrefabTextRT.offsetMax = new Vector2(-5, -3);

            string diffPrefabPath = "Assets/Prefabs/UI/DiffButtonPrefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(diffPrefab, diffPrefabPath);

            // ===== MANAGERS =====
            var dungeonMgrObj = new GameObject("DungeonManager");
            var dungeonMgr = dungeonMgrObj.AddComponent<PickMeUp.Dungeon.DungeonManager>();

            // ===== UI CONTROLLER =====
            var dungeonUI = canvasObj.AddComponent<PickMeUp.UI.DungeonUIController>();
            dungeonUI.dungeonListContainer = listContentRT;
            dungeonUI.dungeonCardPrefab = cardPrefab;
            dungeonUI.detailPanel = detailPanel;
            dungeonUI.dungeonIcon = iconImg;
            dungeonUI.dungeonNameText = dungeonName.GetComponent<TextMeshProUGUI>();
            dungeonUI.dungeonTypeText = dungeonType.GetComponent<TextMeshProUGUI>();
            dungeonUI.dungeonDescText = dungeonDesc.GetComponent<TextMeshProUGUI>();
            dungeonUI.attemptsText = attemptsText.GetComponent<TextMeshProUGUI>();
            dungeonUI.resetTimerText = resetText.GetComponent<TextMeshProUGUI>();
            dungeonUI.staminaCostText = staminaCost.GetComponent<TextMeshProUGUI>();
            dungeonUI.rewardPreviewText = rewardText.GetComponent<TextMeshProUGUI>();
            dungeonUI.difficultyContainer = diffPanel.transform;
            dungeonUI.difficultyButtonPrefab = diffPrefab;
            dungeonUI.enterButton = enterBtn.GetComponent<Button>();
            dungeonUI.backButton = backBtn.GetComponent<Button>();
            dungeonUI.enterButtonText = enterBtn.GetComponentInChildren<TextMeshProUGUI>();
            dungeonUI.lockReasonText = lockText.GetComponent<TextMeshProUGUI>();
            dungeonUI.staminaText = staminaText.GetComponent<TextMeshProUGUI>();
            dungeonUI.goldText = goldText.GetComponent<TextMeshProUGUI>();
            dungeonUI.theme = themeConfig;

            // Save scene
            string scenePath = "Assets/Scenes/Dungeon.unity";
            if (!System.IO.Directory.Exists("Assets/Scenes"))
                System.IO.Directory.CreateDirectory("Assets/Scenes");

            EditorSceneManager.SaveScene(newScene, scenePath);

            // Cleanup
            Object.DestroyImmediate(cardPrefab);
            Object.DestroyImmediate(diffPrefab);

            Debug.Log($"[DungeonSceneBuilder] Dungeon scene built at {scenePath}");

            EditorUtility.DisplayDialog("Dungeon Scene Built",
                "Dungeon scene created successfully!",
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
            rt.sizeDelta = new Vector2(140, 50);

            var txtObj = CreateText(obj.transform, "Text", text, 18, TextAnchor.MiddleCenter);
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
