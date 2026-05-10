using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    [SerializeField] private int leftPadding = 60;
    [SerializeField] private int rightPadding = 60;
    
    private void Awake()
    {
        ApplySafeArea();
    }
    
    private void ApplySafeArea()
    {
        RectTransform rect = GetComponent<RectTransform>();
        Rect safeArea = Screen.safeArea;
        
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        
        // Add manual padding for A34 curved edges
        anchorMin.x += leftPadding;
        anchorMax.x -= rightPadding;
        
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
    }
}
