using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class UIAimInputSOLID : MonoBehaviour, IAimInputSOLID
{
    private RectTransform pad;
    private Camera cam;
    private bool aiming;
    private Vector2 pressPos;
    private Vector2 aimDirUI;

    public Action<Vector2> OnAimChanged;
    public Action OnAimEnded;
    public Action OnAimStarted;

    public void Initialize(RectTransform pad, Camera cam)
    {
        this.pad = pad;
        this.cam = cam;
    }

    public Vector2 GetAimDirection() => aimDirUI;
    public bool IsAiming() => aiming;

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(pad, eventData.position, cam, out pressPos);
        aiming = true;
        OnAimStarted?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!aiming) return;
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(pad, eventData.position, cam, out localPos);
        aimDirUI = (localPos - pressPos).normalized;
        OnAimChanged?.Invoke(aimDirUI);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        aiming = false;
        aimDirUI = Vector2.zero;
        OnAimEnded?.Invoke();
    }
}
