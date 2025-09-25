using UnityEngine;

public class BackgroundBreathing : MonoBehaviour
{
    [Range(0f, 0.02f)] public float scaleAmp = 0.01f;
    [Range(0f, 2f)] public float scaleSpeed = 0.6f;
    [Range(0f, 12f)] public float parallaxAmp = 6f;
    [Range(0f, 5f)] public float parallaxSmooth = 2f;

    private RectTransform rt;
    private Vector2 targetOffset;
    private Vector2 currentOffset;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Breathing (zoom sutil)
        float s = 1f + Mathf.Sin(Time.time * scaleSpeed) * scaleAmp;
        rt.localScale = new Vector3(s, s, 1f);

        // Parallax leve con posición del puntero/último toque
        Vector2 mouse = Input.mousePosition;
        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 dir = (mouse - center) / Mathf.Max(Screen.width, Screen.height);

        targetOffset = dir * parallaxAmp;
        currentOffset = Vector2.Lerp(currentOffset, targetOffset, Time.deltaTime * parallaxSmooth);
        rt.anchoredPosition = currentOffset;
    }
}
