using UnityEngine;
using PickMeUp.Data;
using PickMeUp.Systems;
using PickMeUp.Achievement;

namespace PickMeUp.Tower
{
    public class TowerManager : MonoBehaviour
    {
        public static TowerManager Instance { get; private set; }

        [Header("Configuration")]
        public TowerFloorData[] floorDatabase;
        public int maxUnlockedFloor = 1;
        public int currentSelectedFloor = 1;

        [Header("Events")]
        public System.Action<int> OnFloorSelected;
        public System.Action<int> OnFloorCleared;
        public System.Action OnTowerStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadProgress();
        }

        public void LoadProgress()
        {
            if (SaveManager.Instance != null)
            {
                maxUnlockedFloor = SaveManager.Instance.HighestFloor + 1;
                currentSelectedFloor = Mathf.Clamp(SaveManager.Instance.CurrentFloor, 1, maxUnlockedFloor);
            }
            OnTowerStateChanged?.Invoke();
        }

        public void SelectFloor(int floorNumber)
        {
            if (floorNumber < 1 || floorNumber > maxUnlockedFloor)
            {
                Debug.LogWarning($"[TowerManager] Floor {floorNumber} is not unlocked. Max: {maxUnlockedFloor}");
                return;
            }

            currentSelectedFloor = floorNumber;
            OnFloorSelected?.Invoke(floorNumber);

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.CurrentFloor = floorNumber;
                SaveManager.Instance.SaveGame();
            }
        }

        public TowerFloorData GetFloorData(int floorNumber)
        {
            if (floorDatabase == null || floorDatabase.Length == 0) return null;

            foreach (var floor in floorDatabase)
            {
                if (floor.floorNumber == floorNumber)
                    return floor;
            }
            return null;
        }

        public bool CanEnterFloor(int floorNumber)
        {
            var data = GetFloorData(floorNumber);
            if (data == null) return false;

            if (SaveManager.Instance == null) return false;

            return StaminaManager.Instance != null && 
                   StaminaManager.Instance.GetCurrentStamina() >= data.staminaCost && 
                   floorNumber <= maxUnlockedFloor;
        }

        public void EnterFloor(int floorNumber)
        {
            if (!CanEnterFloor(floorNumber))
            {
                Debug.LogWarning($"[TowerManager] Cannot enter floor {floorNumber}. Check stamina or unlock status.");
                return;
            }

            var data = GetFloorData(floorNumber);
            if (data == null) return;

            // Deduct stamina
            if (StaminaManager.Instance != null)
                StaminaManager.Instance.UseStamina(data.staminaCost);
            else
            {
                SaveManager.Instance.CurrentStamina -= data.staminaCost;
                SaveManager.Instance.SaveGame();
            }

            Debug.Log($"[TowerManager] Entering {data.GetDisplayName()}...");

            UnityEngine.SceneManagement.SceneManager.LoadScene("Combat");
        }

        public void ClearFloor(int floorNumber)
        {
            if (floorNumber >= maxUnlockedFloor)
            {
                maxUnlockedFloor = floorNumber + 1;

                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.HighestFloor = floorNumber;
                    SaveManager.Instance.SaveGame();
                }
            }

            AchievementManager.Instance?.TrackFloorClear(floorNumber);

            OnFloorCleared?.Invoke(floorNumber);
            OnTowerStateChanged?.Invoke();

            Debug.Log($"[TowerManager] Floor {floorNumber} cleared! New max: {maxUnlockedFloor}");
        }

        public bool IsFloorUnlocked(int floorNumber)
        {
            return floorNumber <= maxUnlockedFloor;
        }

        public bool IsFloorCompleted(int floorNumber)
        {
            return floorNumber < maxUnlockedFloor;
        }
    }
}
