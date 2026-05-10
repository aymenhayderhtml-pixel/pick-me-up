using UnityEngine;
using System;
using PickMeUp.Systems;

namespace PickMeUp.Systems
{
    public class StaminaManager : MonoBehaviour
    {
        public static StaminaManager Instance { get; private set; }

        [Header("Regen Settings")]
        public int maxStamina = 120;
        public int regenRateMinutes = 5; // 1 stamina per X minutes
        public int regenAmount = 1;

        [Header("Overflow")]
        public bool allowOverflow = true; // from items/purchases
        public int overflowCap = 200;

        [Header("Events")]
        public System.Action<int, int> OnStaminaChanged; // current, max
        public System.Action OnStaminaFull;

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
            ProcessOfflineRegen();
            StartCoroutine(RegenLoop());
        }

        private System.Collections.IEnumerator RegenLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(60f); // Check every minute
                ProcessRegen();
            }
        }

        private void ProcessOfflineRegen()
        {
            if (SaveManager.Instance == null) return;

            if (string.IsNullOrEmpty(SaveManager.Instance.LastStaminaTimestamp))
            {
                SaveManager.Instance.LastStaminaTimestamp = DateTime.Now.ToString("O");
                SaveManager.Instance.SaveGame();
                return;
            }

            DateTime lastUpdate;
            if (!DateTime.TryParse(SaveManager.Instance.LastStaminaTimestamp, out lastUpdate))
            {
                SaveManager.Instance.LastStaminaTimestamp = DateTime.Now.ToString("O");
                return;
            }

            TimeSpan offline = DateTime.Now - lastUpdate;
            int minutesPassed = (int)offline.TotalMinutes;
            int staminaToAdd = (minutesPassed / regenRateMinutes) * regenAmount;

            if (staminaToAdd > 0)
            {
                int oldStamina = SaveManager.Instance.CurrentStamina;
                int effectiveMax = allowOverflow ? overflowCap : maxStamina;
                
                SaveManager.Instance.CurrentStamina = Mathf.Min(effectiveMax, SaveManager.Instance.CurrentStamina + staminaToAdd);
                int actualAdded = SaveManager.Instance.CurrentStamina - oldStamina;

                if (actualAdded > 0)
                {
                    Debug.Log($"[StaminaManager] Offline regen: +{actualAdded} stamina ({minutesPassed} min offline)");
                    OnStaminaChanged?.Invoke(SaveManager.Instance.CurrentStamina, maxStamina);

                    if (SaveManager.Instance.CurrentStamina >= maxStamina)
                        OnStaminaFull?.Invoke();
                }
            }

            SaveManager.Instance.LastStaminaTimestamp = DateTime.Now.ToString("O");
            SaveManager.Instance.SaveGame();
        }

        private void ProcessRegen()
        {
            if (SaveManager.Instance == null) return;

            int effectiveMax = allowOverflow ? overflowCap : maxStamina;
            if (SaveManager.Instance.CurrentStamina >= effectiveMax) return;

            DateTime lastUpdate;
            if (string.IsNullOrEmpty(SaveManager.Instance.LastStaminaTimestamp) || 
                !DateTime.TryParse(SaveManager.Instance.LastStaminaTimestamp, out lastUpdate))
            {
                lastUpdate = DateTime.Now;
            }

            TimeSpan elapsed = DateTime.Now - lastUpdate;
            int intervals = (int)(elapsed.TotalMinutes / regenRateMinutes);

            if (intervals > 0)
            {
                int oldStamina = SaveManager.Instance.CurrentStamina;
                SaveManager.Instance.CurrentStamina = Mathf.Min(effectiveMax, SaveManager.Instance.CurrentStamina + (intervals * regenAmount));
                int actualAdded = SaveManager.Instance.CurrentStamina - oldStamina;

                if (actualAdded > 0)
                {
                    OnStaminaChanged?.Invoke(SaveManager.Instance.CurrentStamina, maxStamina);

                    if (SaveManager.Instance.CurrentStamina >= maxStamina)
                        OnStaminaFull?.Invoke();
                }
            }

            SaveManager.Instance.LastStaminaTimestamp = DateTime.Now.ToString("O");
            SaveManager.Instance.SaveGame();
        }

        public bool UseStamina(int amount)
        {
            if (SaveManager.Instance == null) return false;

            if (SaveManager.Instance.CurrentStamina < amount) return false;

            SaveManager.Instance.CurrentStamina -= amount;
            SaveManager.Instance.LastStaminaTimestamp = DateTime.Now.ToString("O");
            SaveManager.Instance.SaveGame();

            OnStaminaChanged?.Invoke(SaveManager.Instance.CurrentStamina, maxStamina);
            return true;
        }

        public void AddStamina(int amount, bool allowAboveMax = false)
        {
            if (SaveManager.Instance == null) return;

            int effectiveMax = (allowAboveMax && allowOverflow) ? overflowCap : maxStamina;

            SaveManager.Instance.CurrentStamina = Mathf.Min(effectiveMax, SaveManager.Instance.CurrentStamina + amount);
            SaveManager.Instance.LastStaminaTimestamp = DateTime.Now.ToString("O");
            SaveManager.Instance.SaveGame();

            OnStaminaChanged?.Invoke(SaveManager.Instance.CurrentStamina, maxStamina);
        }

        public void RefillToMax()
        {
            if (SaveManager.Instance == null) return;

            SaveManager.Instance.CurrentStamina = maxStamina;
            SaveManager.Instance.LastStaminaTimestamp = DateTime.Now.ToString("O");
            SaveManager.Instance.SaveGame();

            OnStaminaChanged?.Invoke(SaveManager.Instance.CurrentStamina, maxStamina);
        }

        public int GetCurrentStamina()
        {
            if (SaveManager.Instance == null) return 0;
            return SaveManager.Instance.CurrentStamina;
        }

        public int GetMaxStamina() => maxStamina;

        public TimeSpan GetTimeUntilNextStamina()
        {
            if (SaveManager.Instance == null) return TimeSpan.Zero;

            int effectiveMax = allowOverflow ? overflowCap : maxStamina;
            if (SaveManager.Instance.CurrentStamina >= effectiveMax) return TimeSpan.Zero;

            DateTime lastUpdate;
            if (string.IsNullOrEmpty(SaveManager.Instance.LastStaminaTimestamp) || 
                !DateTime.TryParse(SaveManager.Instance.LastStaminaTimestamp, out lastUpdate))
            {
                return TimeSpan.Zero;
            }

            DateTime nextRegen = lastUpdate.AddMinutes(regenRateMinutes);
            TimeSpan remaining = nextRegen - DateTime.Now;

            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public string GetRegenCountdownText()
        {
            var time = GetTimeUntilNextStamina();
            if (time == TimeSpan.Zero) return "FULL";
            return $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        public float GetStaminaRatio()
        {
            if (SaveManager.Instance == null) return 0f;
            return (float)SaveManager.Instance.CurrentStamina / maxStamina;
        }
    }
}
