using UnityEngine;

[ExecuteAlways]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform rt;
    private Rect last;

    void OnEnable()
    {
        rt = GetComponent<RectTransform>();
        Apply();
    }

    void Update()
    {
        if (Screen.safeArea != last) Apply();
    }

    void Apply()
    {
        last = Screen.safeArea;
        Vector2 anchorMin = last.position;
        Vector2 anchorMax = last.position + last.size;
        anchorMin.x /= Screen.width; anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width; anchorMax.y /= Screen.height;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
    }
}
