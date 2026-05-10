using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using PickMeUp.Data;

namespace PickMeUp.Editor
{
    public static class InventorySceneBuilder
    {
        [MenuItem("Tools/PickMeUp/Build Inventory Scene")]
        public static void BuildInventoryScene()
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camObj = new GameObject("Main Camera");
            var camera = camObj.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.06f, 0.1f);

            // Light
            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.6f;

            // Canvas
            var canvasObj = new GameObject("InventoryCanvas");
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
            var header = CreatePanel(root.transform, "Header", themeConfig?.panelDark ?? new Color(0.12f, 0.12f, 0.16f, 0.9f));
            var headerRT = header.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0, 0.9f);
            headerRT.anchorMax = new Vector2(1, 1f);
            headerRT.offsetMin = new Vector2(20, 10);
            headerRT.offsetMax = new Vector2(-20, -10);

            var title = CreateText(header.transform, "Title", "HEROES & INVENTORY", 40, TextAnchor.MiddleCenter);
            var titleRT = title.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.2f, 0);
            titleRT.anchorMax = new Vector2(0.8f, 1);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;

            var goldText = CreateText(header.transform, "GoldText", "Gold: 0", 20, TextAnchor.MiddleRight);
            var goldRT = goldText.GetComponent<RectTransform>();
            goldRT.anchorMin = new Vector2(0.8f, 0);
            goldRT.anchorMax = new Vector2(0.98f, 1);
            goldRT.offsetMin = Vector2.zero;
            goldRT.offsetMax = Vector2.zero;

            // ===== LEFT: ROSTER LIST =====
            var rosterPanel = CreatePanel(root.transform, "RosterPanel", themeConfig?.panelDark ?? new Color(0.1f, 0.1f, 0.14f, 0.85f));
            var rosterRT = rosterPanel.GetComponent<RectTransform>();
            rosterRT.anchorMin = new Vector2(0.02f, 0.05f);
            rosterRT.anchorMax = new Vector2(0.28f, 0.88f);
            rosterRT.offsetMin = Vector2.zero;
            rosterRT.offsetMax = Vector2.zero;

            var rosterTitle = CreateText(rosterPanel.transform, "RosterTitle", "ROSTER", 22, TextAnchor.MiddleCenter);
            var rosterTitleRT = rosterTitle.GetComponent<RectTransform>();
            rosterTitleRT.anchorMin = new Vector2(0, 0.9f);
            rosterTitleRT.anchorMax = new Vector2(1, 1f);
            rosterTitleRT.offsetMin = Vector2.zero;
            rosterTitleRT.offsetMax = Vector2.zero;

            var rosterScroll = new GameObject("RosterScroll");
            rosterScroll.transform.SetParent(rosterPanel.transform, false);
            var rosterScrollRT = rosterScroll.AddComponent<RectTransform>();
            rosterScrollRT.anchorMin = new Vector2(0, 0);
            rosterScrollRT.anchorMax = new Vector2(1, 0.9f);
            rosterScrollRT.offsetMin = new Vector2(10, 10);
            rosterScrollRT.offsetMax = new Vector2(-10, -5);
            var rosterSR = rosterScroll.AddComponent<ScrollRect>();

            var rosterVP = new GameObject("Viewport");
            rosterVP.transform.SetParent(rosterScroll.transform, false);
            var rosterVPRT = rosterVP.AddComponent<RectTransform>();
            rosterVPRT.anchorMin = Vector2.zero;
            rosterVPRT.anchorMax = Vector2.one;
            rosterVPRT.offsetMin = Vector2.zero;
            rosterVPRT.offsetMax = Vector2.zero;
            rosterVP.AddComponent<Image>().color = Color.clear;
            rosterVP.AddComponent<Mask>().showMaskGraphic = false;

            var rosterContent = new GameObject("Content");
            rosterContent.transform.SetParent(rosterVP.transform, false);
            var rosterContentRT = rosterContent.AddComponent<RectTransform>();
            rosterContentRT.anchorMin = new Vector2(0, 1);
            rosterContentRT.anchorMax = Vector2.one;
            rosterContentRT.pivot = new Vector2(0.5f, 1f);
            rosterContentRT.sizeDelta = new Vector2(0, 400);

            var rosterVLG = rosterContent.AddComponent<VerticalLayoutGroup>();
            rosterVLG.padding = new RectOffset(8, 8, 8, 8);
            rosterVLG.spacing = 6;
            rosterVLG.childAlignment = TextAnchor.UpperCenter;
            rosterVLG.childControlWidth = true;
            rosterVLG.childForceExpandWidth = true;
            rosterVLG.childForceExpandHeight = false;
            rosterContent.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            rosterSR.viewport = rosterVPRT;
            rosterSR.content = rosterContentRT;
            rosterSR.vertical = true;
            rosterSR.horizontal = false;

            // ===== CENTER: HERO DETAIL =====
            var detailPanel = CreatePanel(root.transform, "DetailPanel", themeConfig?.panelDark ?? new Color(0.12f, 0.12f, 0.16f, 0.9f));
            var detailRT = detailPanel.GetComponent<RectTransform>();
            detailRT.anchorMin = new Vector2(0.3f, 0.05f);
            detailRT.anchorMax = new Vector2(0.62f, 0.88f);
            detailRT.offsetMin = Vector2.zero;
            detailRT.offsetMax = Vector2.zero;

            var detailVLG = detailPanel.AddComponent<VerticalLayoutGroup>();
            detailVLG.padding = new RectOffset(15, 15, 15, 15);
            detailVLG.spacing = 10;
            detailVLG.childAlignment = TextAnchor.UpperCenter;
            detailVLG.childControlWidth = true;
            detailVLG.childControlHeight = false;

            var heroName = CreateText(detailPanel.transform, "HeroName", "Select a Hero", 28, TextAnchor.MiddleCenter);
            var heroRarity = CreateText(detailPanel.transform, "HeroRarity", "", 20, TextAnchor.MiddleCenter);
            var heroLevel = CreateText(detailPanel.transform, "HeroLevel", "", 22, TextAnchor.MiddleCenter);

            // EXP bar
            var expObj = new GameObject("ExpBar");
            expObj.transform.SetParent(detailPanel.transform, false);
            var expRT = expObj.AddComponent<RectTransform>();
            expRT.sizeDelta = new Vector2(0, 20);
            var expSlider = expObj.AddComponent<Slider>();
            var expBg = new GameObject("Background");
            expBg.transform.SetParent(expObj.transform, false);
            var expBgImg = expBg.AddComponent<Image>();
            expBgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            var expBgRT = expBg.GetComponent<RectTransform>();
            expBgRT.anchorMin = Vector2.zero;
            expBgRT.anchorMax = Vector2.one;
            expBgRT.offsetMin = Vector2.zero;
            expBgRT.offsetMax = Vector2.zero;

            var expFill = new GameObject("Fill");
            expFill.transform.SetParent(expObj.transform, false);
            var expFillImg = expFill.AddComponent<Image>();
            expFillImg.color = themeConfig?.accentGold ?? new Color(0.9f, 0.7f, 0.2f);
            var expFillRT = expFill.GetComponent<RectTransform>();
            expFillRT.anchorMin = Vector2.zero;
            expFillRT.anchorMax = Vector2.one;
            expFillRT.offsetMin = Vector2.zero;
            expFillRT.offsetMax = Vector2.zero;

            expSlider.fillRect = expFillRT;
            expSlider.targetGraphic = expFillImg;

            var expText = CreateText(detailPanel.transform, "ExpText", "EXP: 0 / 100", 16, TextAnchor.MiddleCenter);

            // Stats grid
            var statsPanel = CreatePanel(detailPanel.transform, "StatsPanel", Color.clear);
            var statsRT = statsPanel.GetComponent<RectTransform>();
            statsRT.sizeDelta = new Vector2(0, 150);
            var statsGLG = statsPanel.AddComponent<GridLayoutGroup>();
            statsGLG.cellSize = new Vector2(140, 35);
            statsGLG.spacing = new Vector2(10, 8);
            statsGLG.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            statsGLG.constraintCount = 2;

            var hpText = CreateText(statsPanel.transform, "HPText", "HP: 100", 18, TextAnchor.MiddleLeft);
            var atkText = CreateText(statsPanel.transform, "ATKText", "ATK: 10", 18, TextAnchor.MiddleLeft);
            var defText = CreateText(statsPanel.transform, "DEFText", "DEF: 5", 18, TextAnchor.MiddleLeft);
            var spdText = CreateText(statsPanel.transform, "SPDText", "SPD: 10", 18, TextAnchor.MiddleLeft);
            var critText = CreateText(statsPanel.transform, "CRITText", "CRIT: 5%", 18, TextAnchor.MiddleLeft);

            // Equipment slots
            var equipTitle = CreateText(detailPanel.transform, "EquipTitle", "EQUIPMENT", 20, TextAnchor.MiddleCenter);

            var equipPanel = CreatePanel(detailPanel.transform, "EquipPanel", Color.clear);
            var equipRT = equipPanel.GetComponent<RectTransform>();
            equipRT.sizeDelta = new Vector2(0, 120);
            var equipHLG = equipPanel.AddComponent<HorizontalLayoutGroup>();
            equipHLG.spacing = 15;
            equipHLG.childAlignment = TextAnchor.MiddleCenter;
            equipHLG.childControlWidth = false;
            equipHLG.childControlHeight = false;

            var weaponSlot = CreateButton(equipPanel.transform, "WeaponSlot", "[Weapon]", new Color(0.3f, 0.2f, 0.15f));
            var armorSlot = CreateButton(equipPanel.transform, "ArmorSlot", "[Armor]", new Color(0.15f, 0.25f, 0.3f));
            var accessorySlot = CreateButton(equipPanel.transform, "AccessorySlot", "[Accessory]", new Color(0.25f, 0.15f, 0.3f));

            foreach (var slot in new[] { weaponSlot, armorSlot, accessorySlot })
            {
                var rt = slot.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100, 80);
            }

            // Back button
            var backBtn = CreateButton(detailPanel.transform, "BackButton", "BACK TO LOBBY", themeConfig?.backgroundDark ?? new Color(0.3f, 0.3f, 0.35f));
            var backRT = backBtn.GetComponent<RectTransform>();
            backRT.sizeDelta = new Vector2(200, 50);

            // ===== RIGHT: INVENTORY =====
            var invPanel = CreatePanel(root.transform, "InventoryPanel", themeConfig?.panelDark ?? new Color(0.1f, 0.1f, 0.14f, 0.85f));
            var invRT = invPanel.GetComponent<RectTransform>();
            invRT.anchorMin = new Vector2(0.64f, 0.05f);
            invRT.anchorMax = new Vector2(0.98f, 0.88f);
            invRT.offsetMin = Vector2.zero;
            invRT.offsetMax = Vector2.zero;

            // Filter tabs
            var tabPanel = CreatePanel(invPanel.transform, "TabPanel", Color.clear);
            var tabRT = tabPanel.GetComponent<RectTransform>();
            tabRT.anchorMin = new Vector2(0, 0.92f);
            tabRT.anchorMax = new Vector2(1, 1f);
            tabRT.offsetMin = new Vector2(5, 0);
            tabRT.offsetMax = new Vector2(-5, -2);
            var tabHLG = tabPanel.AddComponent<HorizontalLayoutGroup>();
            tabHLG.spacing = 4;
            tabHLG.childAlignment = TextAnchor.MiddleCenter;
            tabHLG.childControlWidth = true;
            tabHLG.childControlHeight = true;
            tabHLG.childForceExpandWidth = true;

            var allTab = CreateButton(tabPanel.transform, "AllTab", "ALL", themeConfig?.accentGold ?? new Color(0.9f, 0.7f, 0.2f));
            var wepTab = CreateButton(tabPanel.transform, "WepTab", "WPN", new Color(0.2f, 0.2f, 0.25f));
            var armTab = CreateButton(tabPanel.transform, "ArmTab", "ARM", new Color(0.2f, 0.2f, 0.25f));
            var accTab = CreateButton(tabPanel.transform, "AccTab", "ACC", new Color(0.2f, 0.2f, 0.25f));
            var matTab = CreateButton(tabPanel.transform, "MatTab", "MAT", new Color(0.2f, 0.2f, 0.25f));

            // Item count
            var countText = CreateText(invPanel.transform, "ItemCount", "Items: 0", 16, TextAnchor.MiddleLeft);
            var countRT = countText.GetComponent<RectTransform>();
            countRT.anchorMin = new Vector2(0.02f, 0.87f);
            countRT.anchorMax = new Vector2(0.5f, 0.92f);
            countRT.offsetMin = Vector2.zero;
            countRT.offsetMax = Vector2.zero;

            // Inventory grid
            var invScroll = new GameObject("InvScroll");
            invScroll.transform.SetParent(invPanel.transform, false);
            var invScrollRT = invScroll.AddComponent<RectTransform>();
            invScrollRT.anchorMin = new Vector2(0, 0.32f);
            invScrollRT.anchorMax = new Vector2(1, 0.87f);
            invScrollRT.offsetMin = new Vector2(10, 5);
            invScrollRT.offsetMax = new Vector2(-10, -5);
            var invSR = invScroll.AddComponent<ScrollRect>();

            var invVP = new GameObject("Viewport");
            invVP.transform.SetParent(invScroll.transform, false);
            var invVPRT = invVP.AddComponent<RectTransform>();
            invVPRT.anchorMin = Vector2.zero;
            invVPRT.anchorMax = Vector2.one;
            invVPRT.offsetMin = Vector2.zero;
            invVPRT.offsetMax = Vector2.zero;
            invVP.AddComponent<Image>().color = Color.clear;
            invVP.AddComponent<Mask>().showMaskGraphic = false;

            var invContent = new GameObject("Content");
            invContent.transform.SetParent(invVP.transform, false);
            var invContentRT = invContent.AddComponent<RectTransform>();
            invContentRT.anchorMin = new Vector2(0, 1);
            invContentRT.anchorMax = Vector2.one;
            invContentRT.pivot = new Vector2(0.5f, 1f);
            invContentRT.sizeDelta = new Vector2(0, 500);

            var invGLG = invContent.AddComponent<GridLayoutGroup>();
            invGLG.cellSize = new Vector2(80, 100);
            invGLG.spacing = new Vector2(8, 8);
            invGLG.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            invGLG.constraintCount = 4;
            invGLG.padding = new RectOffset(8, 8, 8, 8);

            invContent.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            invSR.viewport = invVPRT;
            invSR.content = invContentRT;
            invSR.vertical = true;
            invSR.horizontal = false;

            // Item Detail (bottom right)
            var itemDetail = CreatePanel(invPanel.transform, "ItemDetail", themeConfig?.panelDark ?? new Color(0.08f, 0.08f, 0.12f, 0.9f));
            var itemDetailRT = itemDetail.GetComponent<RectTransform>();
            itemDetailRT.anchorMin = new Vector2(0, 0);
            itemDetailRT.anchorMax = new Vector2(1, 0.31f);
            itemDetailRT.offsetMin = new Vector2(10, 10);
            itemDetailRT.offsetMax = new Vector2(-10, -5);

            var itemDetailVLG = itemDetail.AddComponent<VerticalLayoutGroup>();
            itemDetailVLG.padding = new RectOffset(10, 10, 10, 10);
            itemDetailVLG.spacing = 4;
            itemDetailVLG.childAlignment = TextAnchor.UpperLeft;
            itemDetailVLG.childControlWidth = true;
            itemDetailVLG.childControlHeight = false;

            var itemName = CreateText(itemDetail.transform, "ItemName", "Select an item", 20, TextAnchor.MiddleLeft);
            var itemRarity = CreateText(itemDetail.transform, "ItemRarity", "", 16, TextAnchor.MiddleLeft);
            var itemType = CreateText(itemDetail.transform, "ItemType", "", 16, TextAnchor.MiddleLeft);
            var itemDesc = CreateText(itemDetail.transform, "ItemDesc", "", 14, TextAnchor.UpperLeft);
            var itemStats = CreateText(itemDetail.transform, "ItemStats", "", 14, TextAnchor.UpperLeft);
            var itemUpgrade = CreateText(itemDetail.transform, "ItemUpgrade", "", 14, TextAnchor.MiddleLeft);

            var actionPanel = CreatePanel(itemDetail.transform, "ActionPanel", Color.clear);
            var actionRT = actionPanel.GetComponent<RectTransform>();
            actionRT.sizeDelta = new Vector2(0, 40);
            var actionHLG = actionPanel.AddComponent<HorizontalLayoutGroup>();
            actionHLG.spacing = 10;
            actionHLG.childAlignment = TextAnchor.MiddleCenter;
            actionHLG.childControlWidth = false;

            var equipBtn = CreateButton(actionPanel.transform, "EquipBtn", "EQUIP", new Color(0.2f, 0.7f, 0.3f));
            var unequipBtn = CreateButton(actionPanel.transform, "UnequipBtn", "UNEQUIP", new Color(0.8f, 0.5f, 0.2f));
            var upgBtn = CreateButton(actionPanel.transform, "UpgradeBtn", "UPGRADE", new Color(0.2f, 0.5f, 0.9f));
            var sellBtn = CreateButton(actionPanel.transform, "SellBtn", "SELL", new Color(0.8f, 0.2f, 0.2f));

            foreach (var btn in new[] { equipBtn, unequipBtn, upgBtn, sellBtn })
            {
                var rt = btn.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(80, 35);
            }

            itemDetail.SetActive(false);

            // ===== PREFABS =====
            // Hero Button Prefab
            var heroPrefab = new GameObject("HeroButtonPrefab");
            heroPrefab.SetActive(false);
            var heroRT = heroPrefab.AddComponent<RectTransform>();
            heroRT.sizeDelta = new Vector2(0, 50);
            var heroImg = heroPrefab.AddComponent<Image>();
            heroImg.color = themeConfig?.panelDark ?? new Color(0.15f, 0.15f, 0.2f, 0.9f);
            heroImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            heroImg.type = Image.Type.Sliced;
            var heroBtn = heroPrefab.AddComponent<Button>();

            var heroNameTxt = CreateText(heroPrefab.transform, "Name", "Hero Name", 18, TextAnchor.MiddleCenter);
            var heroNameRT = heroNameTxt.GetComponent<RectTransform>();
            heroNameRT.anchorMin = Vector2.zero;
            heroNameRT.anchorMax = Vector2.one;
            heroNameRT.offsetMin = new Vector2(10, 5);
            heroNameRT.offsetMax = new Vector2(-10, -5);

            string heroPrefabPath = "Assets/Prefabs/UI/HeroButtonPrefab.prefab";
            if (!System.IO.Directory.Exists("Assets/Prefabs/UI"))
                System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");
            PrefabUtility.SaveAsPrefabAsset(heroPrefab, heroPrefabPath);

            // Item Slot Prefab
            var itemPrefab = new GameObject("ItemSlotPrefab");
            itemPrefab.SetActive(false);
            var itemRT = itemPrefab.AddComponent<RectTransform>();
            itemRT.sizeDelta = new Vector2(80, 100);
            var itemImg = itemPrefab.AddComponent<Image>();
            itemImg.color = themeConfig?.panelDark ?? new Color(0.12f, 0.12f, 0.16f, 0.9f);
            itemImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            itemImg.type = Image.Type.Sliced;
            var itemBtn = itemPrefab.AddComponent<Button>();

            var rarityBorder = new GameObject("RarityBorder");
            rarityBorder.transform.SetParent(itemPrefab.transform, false);
            var rbImg = rarityBorder.AddComponent<Image>();
            rbImg.color = Color.white;
            var rbRT = rarityBorder.GetComponent<RectTransform>();
            rbRT.anchorMin = Vector2.zero;
            rbRT.anchorMax = Vector2.one;
            rbRT.offsetMin = Vector2.zero;
            rbRT.offsetMax = Vector2.zero;

            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(itemPrefab.transform, false);
            var iconImg = iconObj.AddComponent<Image>();
            iconImg.color = Color.clear;
            var iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.1f, 0.3f);
            iconRT.anchorMax = new Vector2(0.9f, 0.85f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;

            var itemNameObj = CreateText(itemPrefab.transform, "Name", "Item", 14, TextAnchor.MiddleCenter);
            var itemNameRT = itemNameObj.GetComponent<RectTransform>();
            itemNameRT.anchorMin = new Vector2(0, 0.15f);
            itemNameRT.anchorMax = new Vector2(1, 0.3f);
            itemNameRT.offsetMin = Vector2.zero;
            itemNameRT.offsetMax = Vector2.zero;

            var qtyObj = CreateText(itemPrefab.transform, "Quantity", "", 12, TextAnchor.MiddleRight);
            var qtyRT = qtyObj.GetComponent<RectTransform>();
            qtyRT.anchorMin = new Vector2(0.6f, 0);
            qtyRT.anchorMax = new Vector2(1, 0.15f);
            qtyRT.offsetMin = Vector2.zero;
            qtyRT.offsetMax = new Vector2(-2, 0);

            var upgObj = CreateText(itemPrefab.transform, "UpgradeLevel", "", 12, TextAnchor.MiddleLeft);
            var upgRT = upgObj.GetComponent<RectTransform>();
            upgRT.anchorMin = new Vector2(0, 0);
            upgRT.anchorMax = new Vector2(0.4f, 0.15f);
            upgRT.offsetMin = new Vector2(2, 0);
            upgRT.offsetMax = Vector2.zero;

            var eqIcon = new GameObject("EquippedIcon");
            eqIcon.transform.SetParent(itemPrefab.transform, false);
            var eqImg = eqIcon.AddComponent<Image>();
            eqImg.color = new Color(0.2f, 1f, 0.2f, 0.8f);
            var eqRT = eqIcon.GetComponent<RectTransform>();
            eqRT.anchorMin = new Vector2(0.7f, 0.7f);
            eqRT.anchorMax = new Vector2(1, 1);
            eqRT.offsetMin = Vector2.zero;
            eqRT.offsetMax = Vector2.zero;
            eqIcon.SetActive(false);

            string itemPrefabPath = "Assets/Prefabs/UI/ItemSlotPrefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(itemPrefab, itemPrefabPath);

            // ===== MANAGERS =====
            var invMgrObj = new GameObject("InventoryManager");
            var invMgr = invMgrObj.AddComponent<PickMeUp.Inventory.InventoryManager>();

            // ===== UI CONTROLLERS =====
            var heroUI = canvasObj.AddComponent<PickMeUp.UI.HeroRosterUIController>();
            heroUI.rosterContainer = rosterContentRT;
            heroUI.heroButtonPrefab = heroPrefab;
            heroUI.detailPanel = detailPanel;
            heroUI.heroNameText = heroName.GetComponent<TextMeshProUGUI>();
            heroUI.heroRarityText = heroRarity.GetComponent<TextMeshProUGUI>();
            heroUI.heroLevelText = heroLevel.GetComponent<TextMeshProUGUI>();
            heroUI.expText = expText.GetComponent<TextMeshProUGUI>();
            heroUI.expSlider = expSlider;
            heroUI.hpText = hpText.GetComponent<TextMeshProUGUI>();
            heroUI.attackText = atkText.GetComponent<TextMeshProUGUI>();
            heroUI.defenseText = defText.GetComponent<TextMeshProUGUI>();
            heroUI.speedText = spdText.GetComponent<TextMeshProUGUI>();
            heroUI.critText = critText.GetComponent<TextMeshProUGUI>();
            heroUI.weaponSlot = weaponSlot.GetComponent<Button>();
            heroUI.armorSlot = armorSlot.GetComponent<Button>();
            heroUI.accessorySlot = accessorySlot.GetComponent<Button>();
            heroUI.weaponText = weaponSlot.GetComponentInChildren<TextMeshProUGUI>();
            heroUI.armorText = armorSlot.GetComponentInChildren<TextMeshProUGUI>();
            heroUI.accessoryText = accessorySlot.GetComponentInChildren<TextMeshProUGUI>();
            heroUI.backButton = backBtn.GetComponent<Button>();
            heroUI.inventoryPanel = invPanel;
            heroUI.theme = themeConfig;

            var invUI = canvasObj.AddComponent<PickMeUp.UI.InventoryUIController>();
            invUI.inventoryGrid = invContentRT;
            invUI.itemSlotPrefab = itemPrefab;
            invUI.detailPanel = itemDetail;
            invUI.detailIcon = iconImg;
            invUI.detailName = itemName.GetComponent<TextMeshProUGUI>();
            invUI.detailRarity = itemRarity.GetComponent<TextMeshProUGUI>();
            invUI.detailType = itemType.GetComponent<TextMeshProUGUI>();
            invUI.detailDescription = itemDesc.GetComponent<TextMeshProUGUI>();
            invUI.detailStats = itemStats.GetComponent<TextMeshProUGUI>();
            invUI.detailUpgradeLevel = itemUpgrade.GetComponent<TextMeshProUGUI>();
            invUI.equipButton = equipBtn.GetComponent<Button>();
            invUI.unequipButton = unequipBtn.GetComponent<Button>();
            invUI.upgradeButton = upgBtn.GetComponent<Button>();
            invUI.sellButton = sellBtn.GetComponent<Button>();
            invUI.allTab = allTab.GetComponent<Button>();
            invUI.weaponTab = wepTab.GetComponent<Button>();
            invUI.armorTab = armTab.GetComponent<Button>();
            invUI.accessoryTab = accTab.GetComponent<Button>();
            invUI.materialTab = matTab.GetComponent<Button>();
            invUI.goldText = goldText.GetComponent<TextMeshProUGUI>();
            invUI.itemCountText = countText.GetComponent<TextMeshProUGUI>();
            invUI.theme = themeConfig;

            heroUI.inventoryUI = invUI;

            // Save scene
            string scenePath = "Assets/Scenes/Inventory.unity";
            if (!System.IO.Directory.Exists("Assets/Scenes"))
                System.IO.Directory.CreateDirectory("Assets/Scenes");

            EditorSceneManager.SaveScene(newScene, scenePath);

            // Cleanup
            Object.DestroyImmediate(heroPrefab);
            Object.DestroyImmediate(itemPrefab);

            Debug.Log($"[InventorySceneBuilder] Inventory scene built at {scenePath}");

            EditorUtility.DisplayDialog("Inventory Scene Built",
                "Inventory/Hero Management scene created successfully!",
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
            rt.sizeDelta = new Vector2(100, 40);

            var txtObj = CreateText(obj.transform, "Text", text, 16, TextAnchor.MiddleCenter);
            var txtRT = txtObj.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = new Vector2(8, 4);
            txtRT.offsetMax = new Vector2(-8, -4);

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
