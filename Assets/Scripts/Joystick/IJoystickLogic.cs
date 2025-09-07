using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IJoystickLogic 
{
    Vector2 ComputeHandleDelta(Vector2 localPoint, Vector2 center, float maxRadius);
    Vector2 ComputeValue(Vector2 handleDelta, float maxRadius, float deadZone);
}
