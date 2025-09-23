using UnityEngine;

public interface IAimVisualSOLID
{
    void UpdateVisual(Vector2 aimDir01);
    void UpdateWorldVisual(Vector3 worldDir);
    void ShowArrow(bool show);
    void SetWorldLineActive(bool active);
}
