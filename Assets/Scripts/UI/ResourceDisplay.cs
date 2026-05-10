using UnityEngine;
using TMPro;

public class ResourceDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI floorText;
    
    private void Start()
    {
        Refresh();
    }
    
    public void Refresh()
    {
        Debug.Log("[ResourceDisplay] Refresh called");
        if (GameDataBridge.Instance != null)
        {
            gemsText.text = GameDataBridge.Instance.GetGems().ToString("N0");
            goldText.text = GameDataBridge.Instance.GetGold().ToString("N0");
            staminaText.text = $"{GameDataBridge.Instance.GetStamina()}/{GameDataBridge.Instance.GetMaxStamina()}";
            floorText.text = $"F: {GameDataBridge.Instance.GetCurrentFloor()}";
        }
        else
        {
            Debug.LogError("GameDataBridge not found!");
            // Placeholder values until wired:
            gemsText.text = "9,999";
            goldText.text = "1.5M";
            staminaText.text = "120/120";
            floorText.text = "F: 42";
        }
    }
}
