using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class DPadArrowButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] public DPadDirection direction;
    public event Action<DPadDirection, bool, int> OnPressChanged; // (dir, pressed, pointerId)

    int _activePointerId = int.MinValue;
    bool _pressed;

    public void OnPointerDown(PointerEventData e)
    {
        if (_pressed) return;
        _pressed = true;
        _activePointerId = e.pointerId;
        OnPressChanged?.Invoke(direction, true, _activePointerId);
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (!_pressed || e.pointerId != _activePointerId) return;
        _pressed = false;
        OnPressChanged?.Invoke(direction, false, _activePointerId);
        _activePointerId = int.MinValue;
    }

    // Si el dedo se sale del botón, liberamos (opcional pero útil)
    public void OnPointerExit(PointerEventData e)
    {
        if (!_pressed || e.pointerId != _activePointerId) return;
        _pressed = false;
        OnPressChanged?.Invoke(direction, false, _activePointerId);
        _activePointerId = int.MinValue;
    }
}

public enum DPadDirection { None, Up, Down, Left, Right }
