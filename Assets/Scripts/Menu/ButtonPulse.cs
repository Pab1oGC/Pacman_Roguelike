using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPulse : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public float pressScale = 0.95f;
    public float hoverScale = 1.05f;
    public float lerp = 12f;

    private Vector3 baseScale;
    private float targetScale;

    void Awake()
    {
        baseScale = transform.localScale;
        targetScale = baseScale.x;
    }

    void Update()
    {
        float s = Mathf.Lerp(transform.localScale.x, targetScale, Time.deltaTime * lerp);
        transform.localScale = new Vector3(s, s, 1f);
    }

    public void OnPointerDown(PointerEventData eventData) { targetScale = baseScale.x * pressScale; }
    public void OnPointerUp(PointerEventData eventData) { targetScale = baseScale.x; }
    public void OnPointerEnter(PointerEventData eventData) { targetScale = baseScale.x * hoverScale; }
    public void OnPointerExit(PointerEventData eventData) { targetScale = baseScale.x; }
}
