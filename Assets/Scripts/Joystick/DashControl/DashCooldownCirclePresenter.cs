using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashCooldownCirclePresenter : MonoBehaviour
{
    [Header("Fuente (IDashController)")]
    [SerializeField] private MonoBehaviour dashSource; // Debe implementar IDashController

    [Header("Sprites")]
    [SerializeField] private Image contour;               // Tu sprite de contorno (anillo)
    [SerializeField] private RectTransform dot;           // Tu sprite de punto (RectTransform)

    [Header("Geometría")]
    [SerializeField] private RectTransform center;        // Centro del círculo; si null, usa este transform
    [Min(0f)] public float radius = 48f;                  // radio en px
    [Tooltip("Ángulo en grados donde empieza el indicador (0 = derecha, 90 = arriba).")]
    public float startAngleDeg = 90f;
    public bool clockwise = true;

    [Header("Colores opcionales")]
    public Color readyColor = Color.white;
    public Color cooldownColor = new Color(1f, 1f, 1f, 0.5f);

    private IDashController _dash;

    void Awake()
    {
        _dash = dashSource as IDashController;
        //if (!_dash) Debug.LogError("[DashCooldownCirclePresenter] 'dashSource' no implementa IDashController", this);
        if (!center) center = (RectTransform)transform;
        if (!contour) contour = GetComponentInChildren<Image>();
        if (!dot) Debug.LogWarning("[DashCooldownCirclePresenter] 'dot' no asignado", this);
    }

    void Update()
    {
        if (_dash == null || dot == null) return;

        float dur = Mathf.Max(0.0001f, _dash.CooldownDuration);
        // Progreso de cooldown: 1 al empezar, 0 cuando está listo
        float cdNorm = Mathf.Clamp01(_dash.CooldownRemaining / dur);

        // Ángulo del punto (recorre el círculo durante el cooldown)
        float angle = startAngleDeg + (clockwise ? -360f * cdNorm : 360f * cdNorm);
        float rad = angle * Mathf.Deg2Rad;
        Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

        dot.anchoredPosition = pos; // relativo a 'center'

        // Tint del contorno (opcional)
        if (contour)
            contour.color = (_dash.IsOnCooldown || _dash.IsDashing) ? cooldownColor : readyColor;
    }
}
