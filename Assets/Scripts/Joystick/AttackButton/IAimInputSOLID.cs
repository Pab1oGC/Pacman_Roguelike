using UnityEngine;
using UnityEngine.EventSystems;

public interface IAimInputSOLID : IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    Vector2 GetAimDirection();
    bool IsAiming();
}
