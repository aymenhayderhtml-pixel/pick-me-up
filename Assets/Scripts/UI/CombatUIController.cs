using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using PickMeUp.Combat;
using PickMeUp.Data;

namespace PickMeUp.UI
{
    public class CombatUIController : MonoBehaviour
    {
        [Header("Unit Panels")]
        public Transform playerPanelContainer;
        public Transform enemyPanelContainer;
        public GameObject unitPanelPrefab;

        [Header("Action Panel")]
        public GameObject actionPanel;
        public Button attackButton;
        public Button skillButton;
        public Button defendButton;

        [Header("Turn Indicator")]
        public TextMeshProUGUI turnText;
        public GameObject turnIndicator;

        [Header("Result Overlay")]
        public GameObject resultOverlay;
        public TextMeshProUGUI resultTitle;
        public TextMeshProUGUI resultDetails;
        public Button resultButton;

        [Header("Damage Number")]
        public GameObject damageNumberPrefab;
        public Transform damageNumberContainer;

        [Header("Theme")]
        public ThemeConfig theme;

        private Dictionary<RuntimeCombatUnit, UnitUI> unitUIMap = new Dictionary<RuntimeCombatUnit, UnitUI>();
        private RuntimeCombatUnit selectedTarget;
        private bool waitingForTarget = false;
        private bool isSkillAttack = false;

        private class UnitUI
        {
            public GameObject root;
            public Image hpBar;
            public TextMeshProUGUI nameText;
            public TextMeshProUGUI hpText;
            public TextMeshProUGUI levelText;
            public GameObject turnIndicator;
            public GameObject deadOverlay;
            public Button selectButton;
        }

        private void Start()
        {
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.OnPhaseChanged += OnPhaseChanged;
                CombatManager.Instance.OnUnitTurnStarted += OnUnitTurnStarted;
                CombatManager.Instance.OnDamageDealt += OnDamageDealt;
                CombatManager.Instance.OnCombatEnded += OnCombatEnded;
            }

            if (attackButton != null)
                attackButton.onClick.AddListener(OnAttackClicked);
            if (skillButton != null)
                skillButton.onClick.AddListener(OnSkillClicked);
            if (defendButton != null)
                defendButton.onClick.AddListener(OnDefendClicked);

            if (resultButton != null)
                resultButton.onClick.AddListener(OnResultClicked);

            if (actionPanel != null)
                actionPanel.SetActive(false);
            if (resultOverlay != null)
                resultOverlay.SetActive(false);

            // Build UI after a short delay to let CombatManager spawn units
            Invoke(nameof(BuildUnitPanels), 0.3f);
        }

        private void OnDestroy()
        {
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.OnPhaseChanged -= OnPhaseChanged;
                CombatManager.Instance.OnUnitTurnStarted -= OnUnitTurnStarted;
                CombatManager.Instance.OnDamageDealt -= OnDamageDealt;
                CombatManager.Instance.OnCombatEnded -= OnCombatEnded;
            }
        }

        private void BuildUnitPanels()
        {
            if (CombatManager.Instance == null) return;

            // Player panels
            foreach (var unit in CombatManager.Instance.playerUnits)
            {
                var ui = CreateUnitPanel(unit, playerPanelContainer);
                unitUIMap[unit] = ui;

                unit.OnHPChanged += (hp) => UpdateHPBar(ui, unit);
                unit.OnDeath += () => OnUnitDeath(ui);
            }

            // Enemy panels
            foreach (var unit in CombatManager.Instance.enemyUnits)
            {
                var ui = CreateUnitPanel(unit, enemyPanelContainer);
                unitUIMap[unit] = ui;

                unit.OnHPChanged += (hp) => UpdateHPBar(ui, unit);
                unit.OnDeath += () => OnUnitDeath(ui);

                if (ui.selectButton != null)
                {
                    var capture = unit; // closure
                    ui.selectButton.onClick.AddListener(() => OnTargetSelected(capture));
                }
            }
        }

        private UnitUI CreateUnitPanel(RuntimeCombatUnit unit, Transform parent)
        {
            GameObject panel;
            if (unitPanelPrefab != null)
            {
                panel = Instantiate(unitPanelPrefab, parent);
            }
            else
            {
                panel = new GameObject($"UnitPanel_{unit.displayName}");
                panel.transform.SetParent(parent, false);
                var img = panel.AddComponent<Image>();
                img.color = theme?.panelDark ?? new Color(0.08f, 0.08f, 0.15f);
                var rt = panel.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(180, 100);
            }

            var ui = new UnitUI();
            ui.root = panel;

            // Find or create child elements
            ui.nameText = panel.GetComponentInChildren<TextMeshProUGUI>();
            if (ui.nameText == null)
            {
                var nameObj = new GameObject("Name");
                nameObj.transform.SetParent(panel.transform, false);
                ui.nameText = nameObj.AddComponent<TextMeshProUGUI>();
                ui.nameText.fontSize = 18;
                ui.nameText.color = Color.white;
                var nrt = nameObj.GetComponent<RectTransform>();
                nrt.anchorMin = new Vector2(0, 0.7f);
                nrt.anchorMax = new Vector2(1, 1);
                nrt.offsetMin = new Vector2(5, 0);
                nrt.offsetMax = new Vector2(-5, 0);
            }
            ui.nameText.text = unit.displayName;

            // HP Bar background
            var hpBg = panel.transform.Find("HPBarBg");
            if (hpBg == null)
            {
                var bgObj = new GameObject("HPBarBg");
                bgObj.transform.SetParent(panel.transform, false);
                var bgImg = bgObj.AddComponent<Image>();
                bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
                var bgrt = bgObj.GetComponent<RectTransform>();
                bgrt.anchorMin = new Vector2(0.05f, 0.45f);
                bgrt.anchorMax = new Vector2(0.95f, 0.6f);
                bgrt.offsetMin = Vector2.zero;
                bgrt.offsetMax = Vector2.zero;
                hpBg = bgObj.transform;
            }

            // HP Bar fill
            var hpFill = panel.transform.Find("HPBarFill");
            if (hpFill == null)
            {
                var fillObj = new GameObject("HPBarFill");
                fillObj.transform.SetParent(hpBg, false);
                ui.hpBar = fillObj.AddComponent<Image>();
                ui.hpBar.color = unit.team == UnitTeam.Player ? new Color(0.2f, 0.8f, 0.3f) : new Color(0.9f, 0.2f, 0.2f);
                var frt = fillObj.GetComponent<RectTransform>();
                frt.anchorMin = Vector2.zero;
                frt.anchorMax = Vector2.one;
                frt.offsetMin = Vector2.zero;
                frt.offsetMax = Vector2.zero;
            }
            else
            {
                ui.hpBar = hpFill.GetComponent<Image>();
            }

            // HP Text
            var hpTxt = panel.transform.Find("HPText");
            if (hpTxt == null)
            {
                var hptObj = new GameObject("HPText");
                hptObj.transform.SetParent(panel.transform, false);
                ui.hpText = hptObj.AddComponent<TextMeshProUGUI>();
                ui.hpText.fontSize = 14;
                ui.hpText.color = Color.white;
                var hrt = hptObj.GetComponent<RectTransform>();
                hrt.anchorMin = new Vector2(0, 0.25f);
                hrt.anchorMax = new Vector2(1, 0.45f);
                hrt.offsetMin = new Vector2(5, 0);
                hrt.offsetMax = new Vector2(-5, 0);
            }
            else
            {
                ui.hpText = hpTxt.GetComponent<TextMeshProUGUI>();
            }

            // Level text
            var lvlTxt = panel.transform.Find("LevelText");
            if (lvlTxt == null)
            {
                var lObj = new GameObject("LevelText");
                lObj.transform.SetParent(panel.transform, false);
                ui.levelText = lObj.AddComponent<TextMeshProUGUI>();
                ui.levelText.fontSize = 14;
                ui.levelText.color = new Color(0.9f, 0.7f, 0.2f);
                var lrt = lObj.GetComponent<RectTransform>();
                lrt.anchorMin = new Vector2(0.7f, 0.7f);
                lrt.anchorMax = new Vector2(1, 1);
                lrt.offsetMin = Vector2.zero;
                lrt.offsetMax = new Vector2(-5, 0);
            }
            else
            {
                ui.levelText = lvlTxt.GetComponent<TextMeshProUGUI>();
            }
            ui.levelText.text = $"Lv.{unit.level}";

            // Select button for enemies
            if (unit.team == UnitTeam.Enemy)
            {
                var btn = panel.GetComponent<Button>();
                if (btn == null) btn = panel.AddComponent<Button>();
                ui.selectButton = btn;
                var colors = btn.colors;
                colors.highlightedColor = new Color(1f, 1f, 1f, 0.3f);
                colors.pressedColor = new Color(1f, 1f, 1f, 0.5f);
                btn.colors = colors;
                btn.targetGraphic = panel.GetComponent<Image>();
            }

            // Dead overlay
            var deadObj = panel.transform.Find("DeadOverlay");
            if (deadObj == null)
            {
                var dObj = new GameObject("DeadOverlay");
                dObj.transform.SetParent(panel.transform, false);
                var dImg = dObj.AddComponent<Image>();
                dImg.color = new Color(0, 0, 0, 0.6f);
                var drt = dObj.GetComponent<RectTransform>();
                drt.anchorMin = Vector2.zero;
                drt.anchorMax = Vector2.one;
                drt.offsetMin = Vector2.zero;
                drt.offsetMax = Vector2.zero;
                dObj.SetActive(false);
                ui.deadOverlay = dObj;
            }
            else
            {
                ui.deadOverlay = deadObj.gameObject;
            }

            UpdateHPBar(ui, unit);
            return ui;
        }

        private void UpdateHPBar(UnitUI ui, RuntimeCombatUnit unit)
        {
            if (ui == null || unit == null) return;

            float ratio = unit.GetHPRatio();
            if (ui.hpBar != null)
            {
                ui.hpBar.fillAmount = ratio;
                // Color shift: green → yellow → red
                if (ratio > 0.5f)
                    ui.hpBar.color = Color.Lerp(new Color(0.9f, 0.8f, 0.1f), new Color(0.2f, 0.8f, 0.3f), (ratio - 0.5f) * 2);
                else
                    ui.hpBar.color = Color.Lerp(new Color(0.9f, 0.1f, 0.1f), new Color(0.9f, 0.8f, 0.1f), ratio * 2);
            }

            if (ui.hpText != null)
                ui.hpText.text = $"{unit.currentHP}/{unit.maxHP}";
        }

        private void OnUnitDeath(UnitUI ui)
        {
            if (ui?.deadOverlay != null)
                ui.deadOverlay.SetActive(true);
            if (ui?.nameText != null)
                ui.nameText.color = Color.gray;
        }

        private void OnPhaseChanged(CombatPhase phase)
        {
            if (turnText != null)
            {
                switch (phase)
                {
                    case CombatPhase.Setup: turnText.text = "Preparing..."; break;
                    case CombatPhase.PlayerTurn: turnText.text = "Your Turn"; break;
                    case CombatPhase.EnemyTurn: turnText.text = "Enemy Turn"; break;
                    case CombatPhase.Victory: turnText.text = "VICTORY!"; break;
                    case CombatPhase.Defeat: turnText.text = "DEFEAT..."; break;
                }
            }

            if (actionPanel != null)
                actionPanel.SetActive(phase == CombatPhase.PlayerTurn);
        }

        private void OnUnitTurnStarted(RuntimeCombatUnit unit)
        {
            // Highlight active unit
            foreach (var kvp in unitUIMap)
            {
                var img = kvp.Value.root.GetComponent<Image>();
                if (img != null)
                {
                    img.color = (kvp.Key == unit) 
                        ? (theme?.accentGold ?? new Color(0.3f, 0.25f, 0.15f, 0.95f))
                        : (theme?.panelDark ?? new Color(0.08f, 0.08f, 0.15f));
                }
            }

            // Update skill button
            if (skillButton != null && unit != null)
            {
                skillButton.interactable = unit.CanUseSkill();
                var skillText = skillButton.GetComponentInChildren<TextMeshProUGUI>();
                if (skillText != null)
                {
                    string cdText = unit.skillCurrentCooldown > 0 ? $" ({unit.skillCurrentCooldown})" : "";
                    skillText.text = $"{unit.skillName}{cdText}";
                }
            }

            // Show target selection hint
            if (unit?.team == UnitTeam.Player)
            {
                waitingForTarget = false;
                selectedTarget = null;
                isSkillAttack = false;
            }
        }

        private void OnAttackClicked()
        {
            if (CombatManager.Instance?.activeUnit?.team != UnitTeam.Player) return;

            isSkillAttack = false;
            waitingForTarget = true;

            if (turnText != null)
                turnText.text = "Select Target...";

            // Highlight enemy panels
            foreach (var unit in CombatManager.Instance.enemyUnits)
            {
                if (!unit.isDead && unitUIMap.TryGetValue(unit, out var ui))
                {
                    var img = ui.root.GetComponent<Image>();
                    if (img != null) img.color = new Color(0.9f, 0.3f, 0.3f, 0.8f);
                }
            }
        }

        private void OnSkillClicked()
        {
            if (CombatManager.Instance?.activeUnit?.team != UnitTeam.Player) return;
            if (!CombatManager.Instance.activeUnit.CanUseSkill()) return;

            isSkillAttack = true;
            waitingForTarget = true;

            if (turnText != null)
                turnText.text = $"Select Target for {CombatManager.Instance.activeUnit.skillName}...";

            // Highlight enemy panels
            foreach (var unit in CombatManager.Instance.enemyUnits)
            {
                if (!unit.isDead && unitUIMap.TryGetValue(unit, out var ui))
                {
                    var img = ui.root.GetComponent<Image>();
                    if (img != null) img.color = new Color(0.9f, 0.5f, 0.1f, 0.8f);
                }
            }
        }

        private void OnDefendClicked()
        {
            CombatManager.Instance?.PlayerDefend();
        }

        private void OnTargetSelected(RuntimeCombatUnit target)
        {
            if (!waitingForTarget || target == null || target.isDead) return;

            waitingForTarget = false;

            if (isSkillAttack)
                CombatManager.Instance?.PlayerUseSkill(target);
            else
                CombatManager.Instance?.PlayerAttack(target);

            // Reset panel colors
            OnUnitTurnStarted(CombatManager.Instance?.activeUnit);
        }

        private void OnDamageDealt(RuntimeCombatUnit attacker, RuntimeCombatUnit target, int damage, bool isCrit)
        {
            if (damageNumberPrefab != null && target != null && unitUIMap.TryGetValue(target, out var ui))
            {
                ShowDamageNumber(damage, isCrit, ui.root.transform.position);
            }
        }

        private void ShowDamageNumber(int damage, bool isCrit, Vector3 worldPos)
        {
            var obj = Instantiate(damageNumberPrefab, damageNumberContainer);
            var txt = obj.GetComponent<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.text = damage.ToString();
                txt.color = isCrit ? new Color(1f, 0.8f, 0f) : Color.white;
                txt.fontSize = isCrit ? 42 : 32;
                if (isCrit) txt.text = $"CRIT! {damage}";
            }

            // Convert world pos to screen pos
            var rt = obj.GetComponent<RectTransform>();
            rt.position = worldPos + new Vector3(0, 30, 0);

            // Animate up and fade
            StartCoroutine(AnimateDamageNumber(rt));
        }

        private System.Collections.IEnumerator AnimateDamageNumber(RectTransform rt)
        {
            float t = 0;
            Vector3 start = rt.position;
            var txt = rt.GetComponent<TextMeshProUGUI>();

            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                rt.position = start + new Vector3(0, t * 50, 0);
                if (txt != null)
                    txt.alpha = 1f - t;
                yield return null;
            }

            Destroy(rt.gameObject);
        }

        private void OnCombatEnded()
        {
            if (CombatManager.Instance == null) return;

            bool isVictory = CombatManager.Instance.currentPhase == CombatPhase.Victory;

            if (resultOverlay != null)
                resultOverlay.SetActive(true);

            if (resultTitle != null)
            {
                resultTitle.text = isVictory ? "VICTORY!" : "DEFEAT";
                resultTitle.color = isVictory ? new Color(0.9f, 0.7f, 0.2f) : new Color(0.7f, 0.2f, 0.2f);
            }

            if (resultDetails != null)
            {
                if (isVictory && CombatManager.Instance.currentFloorData != null)
                {
                    var data = CombatManager.Instance.currentFloorData;
                    resultDetails.text = $"Rewards:\n{data.goldReward} Gold\n{data.expReward} EXP per hero";
                }
                else
                {
                    resultDetails.text = "Your heroes have fallen...\nRest and try again.";
                }
            }

            if (resultButton != null)
            {
                var btnText = resultButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                    btnText.text = isVictory ? "CONTINUE" : "RETURN";
            }
        }

        private void OnResultClicked()
        {
            if (CombatManager.Instance == null) return;

            if (CombatManager.Instance.currentPhase == CombatPhase.Victory)
            {
                CombatManager.Instance.ReturnToTower();
            }
            else
            {
                CombatManager.Instance.ReturnToLobby();
            }
        }
    }
}
