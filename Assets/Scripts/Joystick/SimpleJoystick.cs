using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class SimpleJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Referencias UI")]
    [SerializeField] private RectTransform handle; // knob

    [Header("Ajustes")]
    [Min(1f)][SerializeField] private float maxRadius = 80f;     // px
    [Range(0f, 1f)][SerializeField] private float deadZone = 0.1f;
    [Tooltip("Si está activo, el centro se fija donde toques (joystick flotante).")]
    [SerializeField] private bool centerOnPointerDown = false;

    [Header("Eventos")]
    public UnityEvent<Vector2> OnValueChanged; // notifica (-1..1, -1..1)

    public Vector2 Value { get; private set; }   // salida normalizada
    public bool IsPressed => _pressed;

    RectTransform _rt;
    IJoystickLogic _logic;

    Vector2 _centerLocal; // centro en coord. locales del RectTransform
    bool _pressed;

    // Permite inyectar otra lógica en runtime (DIP)
    public void SetLogic(IJoystickLogic logic) => _logic = logic ?? new CircularJoystickLogic();

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _logic = _logic ?? new CircularJoystickLogic();
        // Por defecto, centro en el origen local del rect (pivot centrado recomendado)
        _centerLocal = Vector2.zero;
        ResetHandle();
    }

    public void OnPointerDown(PointerEventData e)
    {
        _pressed = true;
        if (TryLocalPoint(e, out var local))
        {
            if (centerOnPointerDown)
                _centerLocal = local; // joystick flotante: reubica centro
            UpdateFromLocal(local);
        }
    }

    public void OnDrag(PointerEventData e)
    {
        if (!_pressed) return;
        if (TryLocalPoint(e, out var local))
            UpdateFromLocal(local);
    }

    public void OnPointerUp(PointerEventData e)
    {
        _pressed = false;
        Value = Vector2.zero;
        ResetHandle();

        // Notificar cambio a cero (opcional según preferencia)
        OnValueChanged?.Invoke(Value);

        // Si el centro era flotante, opcionalmente regresa al origen:
        if (centerOnPointerDown) _centerLocal = Vector2.zero;
    }

    void UpdateFromLocal(Vector2 localPoint)
    {
        // 1) Delta clamp (px)
        var delta = _logic.ComputeHandleDelta(localPoint, _centerLocal, maxRadius);

        // 2) Posicionar knob
        if (handle) handle.anchoredPosition = delta;

        // 3) Calcular salida normalizada con deadzone
        var newValue = _logic.ComputeValue(delta, maxRadius, deadZone);

        if (newValue != Value)
        {
            Value = newValue;
            OnValueChanged?.Invoke(Value);
        }
    }

    void ResetHandle()
    {
        if (handle) handle.anchoredPosition = Vector2.zero;
    }

    bool TryLocalPoint(PointerEventData e, out Vector2 local)
        => RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, e.position, e.pressEventCamera, out local);
}
