using UnityEngine;

public class UIAimVisualSOLID : IAimVisualSOLID
{
    private RectTransform arrowUI;
    private RectTransform knob;
    private LineRenderer worldLine;
    private GameObject hitDot;

    public UIAimVisualSOLID(RectTransform arrowUI, RectTransform knob, LineRenderer worldLine, GameObject hitDot)
    {
        this.arrowUI = arrowUI;
        this.knob = knob;
        this.worldLine = worldLine;
        this.hitDot = hitDot;
    }

    public void ShowArrow(bool show)
    {
        if (arrowUI) arrowUI.gameObject.SetActive(show);
        if (knob) knob.gameObject.SetActive(show);
    }

    public void SetWorldLineActive(bool active)
    {
        if (worldLine) worldLine.enabled = active;
        if (hitDot) hitDot.SetActive(active);
    }

    public void UpdateVisual(Vector2 aimDir01)
    {
        if (!arrowUI) return;
        float ang = Mathf.Atan2(aimDir01.x, aimDir01.y) * Mathf.Rad2Deg;
        arrowUI.localRotation = Quaternion.Euler(0, 0, -ang);
    }

    public void UpdateWorldVisual(Vector3 worldDir)
    {
        if (!worldLine) return;
        Vector3 origin = Vector3.zero; // aquí se puede bindear al player
        worldLine.SetPosition(0, origin);
        worldLine.SetPosition(1, origin + worldDir * 5f);
    }
}
