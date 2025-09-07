using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularJoystickLogic : IJoystickLogic
{
    public Vector2 ComputeHandleDelta(Vector2 localPoint, Vector2 center, float maxRadius)
    {
        Vector2 delta = localPoint - center;
        float mag = delta.magnitude;
        if (mag > maxRadius && mag > 0f)
            delta = (delta / mag) * maxRadius; // clamp circular
        return delta;
    }

    public Vector2 ComputeValue(Vector2 handleDelta, float maxRadius, float deadZone)
    {
        if (maxRadius <= 0f) return Vector2.zero;
        Vector2 norm = handleDelta / maxRadius; // -1..1
        return (norm.magnitude < Mathf.Clamp01(deadZone)) ? Vector2.zero : norm;
    }
}
