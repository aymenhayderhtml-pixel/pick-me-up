using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PickMeUp.Tower;
using PickMeUp.Data;
using PickMeUp.Dungeon;
using PickMeUp.Achievement;

namespace PickMeUp.Combat
{
    public enum CombatPhase { Setup, PlayerTurn, EnemyTurn, Victory, Defeat, Reward }

    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        [Header("Phase")]
        public CombatPhase currentPhase = CombatPhase.Setup;

        [Header("Units")]
        public List<RuntimeCombatUnit> playerUnits = new List<RuntimeCombatUnit>();
        public List<RuntimeCombatUnit> enemyUnits = new List<RuntimeCombatUnit>();
        public RuntimeCombatUnit activeUnit;

        [Header("Configuration")]
        public GameObject heroPrefab;
        public GameObject enemyPrefab;
        public Transform playerSpawnParent;
        public Transform enemySpawnParent;

        [Header("Floor Data")]
        public TowerFloorData currentFloorData;

        [Header("Dungeon Context")]
        public bool isDungeonMode = false;
        public DungeonData currentDungeonData;
        public DungeonDifficulty currentDungeonDifficulty;

        [Header("Events")]
        public System.Action<CombatPhase> OnPhaseChanged;
        public System.Action<RuntimeCombatUnit> OnUnitTurnStarted;
        public System.Action<RuntimeCombatUnit, RuntimeCombatUnit, int, bool> OnDamageDealt; // attacker, target, damage, isCrit
        public System.Action OnCombatEnded;

        private List<RuntimeCombatUnit> turnOrder = new List<RuntimeCombatUnit>();
        private int turnIndex = 0;
        private bool isProcessing = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Check if we came from DungeonManager
            if (DungeonManager.Instance != null && DungeonManager.Instance.selectedDungeon != null)
            {
                isDungeonMode = true;
                currentDungeonData = DungeonManager.Instance.selectedDungeon;
                currentDungeonDifficulty = DungeonManager.Instance.selectedDifficulty;
            }

            StartCoroutine(SetupCombat());
        }

        private IEnumerator SetupCombat()
        {
            currentPhase = CombatPhase.Setup;
            OnPhaseChanged?.Invoke(currentPhase);

            // Get floor data from TowerManager
            if (TowerManager.Instance != null)
            {
                currentFloorData = TowerManager.Instance.GetFloorData(TowerManager.Instance.currentSelectedFloor);
            }

            yield return StartCoroutine(SpawnPlayerUnits());
            yield return StartCoroutine(SpawnEnemyUnits());
            yield return new WaitForSeconds(0.5f);

            // Build turn order by speed
            turnOrder = playerUnits.Concat(enemyUnits)
                .Where(u => !u.isDead)
                .OrderByDescending(u => u.speed)
                .ToList();

            turnIndex = 0;
            StartNextTurn();
        }

        private IEnumerator SpawnPlayerUnits()
        {
            if (SaveManager.Instance == null) yield break;

            var roster = SaveManager.Instance.ActiveRoster;
            if (roster == null || roster.Count == 0)
            {
                Debug.LogError("[CombatManager] No heroes in active roster!");
                yield break;
            }

            for (int i = 0; i < roster.Count && i < 4; i++) // Max 4 heroes
            {
                var heroData = roster[i];
                var unit = CreateUnit(heroPrefab, playerSpawnParent, i, 4);
                if (unit == null) continue;

                unit.Initialize(
                    heroData.heroId,
                    heroData.heroName,
                    UnitTeam.Player,
                    heroData.level,
                    heroData.maxHP,
                    heroData.attack,
                    heroData.defense,
                    heroData.speed
                );
                unit.critRate = heroData.critRate;
                unit.skillName = heroData.skillName;
                unit.skillMultiplier = heroData.skillMultiplier;
                unit.skillCooldown = heroData.skillCooldown;

                playerUnits.Add(unit);
                yield return new WaitForSeconds(0.1f);
            }
        }

        private IEnumerator SpawnEnemyUnits()
        {
            if (isDungeonMode && currentDungeonData != null)
            {
                float diffMult = currentDungeonData.GetDifficultyMultiplier(currentDungeonDifficulty);

                for (int i = 0; i < currentDungeonData.enemyIds.Length; i++)
                {
                    for (int j = 0; j < currentDungeonData.enemyCounts[i]; j++)
                    {
                        var unit = CreateUnit(enemyPrefab, enemySpawnParent, enemyUnits.Count, 6);
                        if (unit == null) continue;

                        int lvl = Mathf.FloorToInt(currentDungeonData.enemyLevels[i] * diffMult);

                        unit.Initialize(
                            currentDungeonData.enemyIds[i],
                            currentDungeonData.enemyIds[i],
                            UnitTeam.Enemy,
                            lvl,
                            Mathf.FloorToInt(80 * diffMult * (1 + lvl * 0.1f)),
                            Mathf.FloorToInt(12 * diffMult * (1 + lvl * 0.05f)),
                            Mathf.FloorToInt(4 * diffMult),
                            Mathf.FloorToInt(8 * diffMult)
                        );

                        // Track death for achievements
                        var capturedUnit = unit;
                        unit.OnDeath += () => AchievementManager.Instance?.TrackKill(capturedUnit.unitId);

                        enemyUnits.Add(unit);
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                yield break;
            }

            if (currentFloorData == null)
            {
                Debug.LogWarning("[CombatManager] No floor data — spawning test enemies.");
                SpawnTestEnemies();
                yield break;
            }

            for (int i = 0; i < currentFloorData.enemyIds.Length; i++)
            {
                for (int j = 0; j < currentFloorData.enemyCounts[i]; j++)
                {
                    var unit = CreateUnit(enemyPrefab, enemySpawnParent, enemyUnits.Count, 6);
                    if (unit == null) continue;

                    int enemyLevel = currentFloorData.enemyLevels[i];
                    float diff = currentFloorData.difficultyMultiplier;

                    unit.Initialize(
                        currentFloorData.enemyIds[i],
                        currentFloorData.enemyIds[i],
                        UnitTeam.Enemy,
                        enemyLevel,
                        Mathf.FloorToInt(80 * diff * (1 + enemyLevel * 0.1f)),
                        Mathf.FloorToInt(12 * diff * (1 + enemyLevel * 0.05f)),
                        Mathf.FloorToInt(4 * diff),
                        Mathf.FloorToInt(8 * diff)
                    );

                    // Track death for achievements
                    var capturedUnit = unit;
                    unit.OnDeath += () => AchievementManager.Instance?.TrackKill(capturedUnit.unitId);

                    enemyUnits.Add(unit);
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        private void SpawnTestEnemies()
        {
            for (int i = 0; i < 3; i++)
            {
                var unit = CreateUnit(enemyPrefab, enemySpawnParent, i, 6);
                if (unit == null) continue;
                unit.Initialize($"Enemy_{i}", $"Goblin {i+1}", UnitTeam.Enemy, 1, 60, 10, 3, 7);
                enemyUnits.Add(unit);
            }
        }

        private RuntimeCombatUnit CreateUnit(GameObject prefab, Transform parent, int index, int maxSlots)
        {
            if (prefab == null)
            {
                var obj = new GameObject("Unit");
                obj.transform.SetParent(parent);
                return obj.AddComponent<RuntimeCombatUnit>();
            }

            var instance = Instantiate(prefab, parent);
            var unit = instance.GetComponent<RuntimeCombatUnit>();
            if (unit == null) unit = instance.AddComponent<RuntimeCombatUnit>();

            // Position in formation
            float spacing = 120f;
            float offset = (index - (maxSlots - 1) * 0.5f) * spacing;
            var rt = instance.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(offset, 0);
            }

            return unit;
        }

        private void StartNextTurn()
        {
            // Remove dead units from turn order
            turnOrder = turnOrder.Where(u => !u.isDead).ToList();

            if (turnOrder.Count == 0)
            {
                CheckCombatEnd();
                return;
            }

            if (turnIndex >= turnOrder.Count)
                turnIndex = 0;

            activeUnit = turnOrder[turnIndex];
            activeUnit.StartTurn();

            if (activeUnit.team == UnitTeam.Player)
            {
                currentPhase = CombatPhase.PlayerTurn;
            }
            else
            {
                currentPhase = CombatPhase.EnemyTurn;
                StartCoroutine(ProcessEnemyTurn());
            }

            OnPhaseChanged?.Invoke(currentPhase);
            OnUnitTurnStarted?.Invoke(activeUnit);
        }

        public void EndCurrentTurn()
        {
            if (activeUnit != null)
                activeUnit.EndTurn();

            turnIndex++;
            StartNextTurn();
        }

        // Called by UI when player clicks Attack
        public void PlayerAttack(RuntimeCombatUnit target)
        {
            if (currentPhase != CombatPhase.PlayerTurn || activeUnit == null || activeUnit.team != UnitTeam.Player)
                return;

            if (target == null || target.isDead) return;

            int damage = activeUnit.CalculateDamage(target);
            bool isCrit = Random.Range(0, 100) < activeUnit.critRate;
            if (isCrit) damage = Mathf.FloorToInt(damage * 1.5f);

            target.TakeDamage(damage);
            OnDamageDealt?.Invoke(activeUnit, target, damage, isCrit);

            activeUnit.state = UnitState.Attacking;

            CheckCombatEnd();
            if (currentPhase != CombatPhase.Victory && currentPhase != CombatPhase.Defeat)
            {
                EndCurrentTurn();
            }
        }

        // Called by UI when player clicks Skill
        public void PlayerUseSkill(RuntimeCombatUnit target)
        {
            if (currentPhase != CombatPhase.PlayerTurn || activeUnit == null || !activeUnit.CanUseSkill())
                return;

            if (target == null || target.isDead) return;

            activeUnit.UseSkill();
            int damage = activeUnit.CalculateDamage(target, true);
            bool isCrit = Random.Range(0, 100) < activeUnit.critRate;
            if (isCrit) damage = Mathf.FloorToInt(damage * 1.5f);

            target.TakeDamage(damage);
            OnDamageDealt?.Invoke(activeUnit, target, damage, isCrit);

            CheckCombatEnd();
            if (currentPhase != CombatPhase.Victory && currentPhase != CombatPhase.Defeat)
            {
                EndCurrentTurn();
            }
        }

        // Called by UI when player clicks Defend
        public void PlayerDefend()
        {
            if (currentPhase != CombatPhase.PlayerTurn || activeUnit == null)
                return;

            activeUnit.SetDefending();
            EndCurrentTurn();
        }

        private IEnumerator ProcessEnemyTurn()
        {
            yield return new WaitForSeconds(0.8f);

            if (activeUnit == null || activeUnit.isDead)
            {
                EndCurrentTurn();
                yield break;
            }

            // Simple AI: attack lowest HP player
            var target = playerUnits.Where(u => !u.isDead).OrderBy(u => u.GetHPRatio()).FirstOrDefault();

            if (target == null)
            {
                EndCurrentTurn();
                yield break;
            }

            // 20% chance to use skill if available
            bool useSkill = activeUnit.CanUseSkill() && Random.value < 0.2f;

            yield return new WaitForSeconds(0.3f);

            if (useSkill)
            {
                activeUnit.UseSkill();
                int damage = activeUnit.CalculateDamage(target, true);
                target.TakeDamage(damage);
                OnDamageDealt?.Invoke(activeUnit, target, damage, false);
            }
            else
            {
                int damage = activeUnit.CalculateDamage(target);
                target.TakeDamage(damage);
                OnDamageDealt?.Invoke(activeUnit, target, damage, false);
            }

            yield return new WaitForSeconds(0.5f);

            CheckCombatEnd();
            if (currentPhase != CombatPhase.Victory && currentPhase != CombatPhase.Defeat)
            {
                EndCurrentTurn();
            }
        }

        private void CheckCombatEnd()
        {
            bool allEnemiesDead = enemyUnits.All(u => u.isDead);
            bool allPlayersDead = playerUnits.All(u => u.isDead);

            if (allEnemiesDead)
            {
                StartCoroutine(CombatVictory());
            }
            else if (allPlayersDead)
            {
                StartCoroutine(CombatDefeat());
            }
        }

        private IEnumerator CombatVictory()
        {
            currentPhase = CombatPhase.Victory;
            OnPhaseChanged?.Invoke(currentPhase);

            yield return new WaitForSeconds(1.5f);

            if (isDungeonMode && currentDungeonData != null)
            {
                // Dungeon rewards
                DungeonManager.Instance?.DistributeRewards(currentDungeonData, currentDungeonDifficulty);
            }
            else if (currentFloorData != null && SaveManager.Instance != null)
            {
                SaveManager.Instance.Gold += currentFloorData.goldReward;

                // Give EXP to surviving heroes
                foreach (var hero in playerUnits.Where(u => !u.isDead))
                {
                    var runtimeHero = SaveManager.Instance.ActiveRoster
                        .FirstOrDefault(h => h.heroId == hero.unitId);
                    if (runtimeHero != null)
                    {
                        runtimeHero.exp += currentFloorData.expReward;
                        // Simple level up check
                        while (runtimeHero.exp >= runtimeHero.expToNextLevel)
                        {
                            runtimeHero.exp -= runtimeHero.expToNextLevel;
                            runtimeHero.LevelUp();
                        }
                    }
                }

                SaveManager.Instance.SaveGame();
            }

            // Mark floor as cleared
            if (TowerManager.Instance != null && currentFloorData != null && !isDungeonMode)
            {
                TowerManager.Instance.ClearFloor(currentFloorData.floorNumber);
                AchievementManager.Instance?.TrackFloorClear(currentFloorData.floorNumber);
            }

            // Track win streak (simulated here - you might want a persistent streak in SaveManager)
            AchievementManager.Instance?.TrackWinStreak(1); 

            OnCombatEnded?.Invoke();
        }

        private IEnumerator CombatDefeat()
        {
            currentPhase = CombatPhase.Defeat;
            OnPhaseChanged?.Invoke(currentPhase);

            yield return new WaitForSeconds(2f);

            OnCombatEnded?.Invoke();
        }

        public void ReturnToTower()
        {
            if (isDungeonMode)
                UnityEngine.SceneManagement.SceneManager.LoadScene("Dungeon");
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("Tower");
        }

        public void ReturnToLobby()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }
    }
}
