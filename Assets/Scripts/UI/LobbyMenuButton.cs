using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyMenuButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject notificationBadge;
    
    public string FeatureId { get; private set; }
    
    public void Setup(string featureId, string label, Sprite icon, Color bgColor)
    {
        FeatureId = featureId.Trim();
        labelText.text = label;
        iconImage.sprite = icon;
        backgroundImage.color = bgColor;
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }
    
    private void OnClicked()
    {
        // Fire event to existing MainMenuNavigation or new adapter
        LobbyUIAdapter.Instance?.OpenFeature(FeatureId);
    }
    
    public void SetNotification(bool active)
    {
        if (notificationBadge != null)
            notificationBadge.SetActive(active);
    }
}
