using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothRotationPolicy : IRotationPolicy
{
    public Quaternion Compute(Quaternion current, Vector3 desiredDir, float turnSpeedDegPerSec, float dt)
    {
        if (desiredDir == Vector3.zero) return current;
        var target = Quaternion.LookRotation(desiredDir, Vector3.up);
        return Quaternion.RotateTowards(current, target, turnSpeedDegPerSec * dt);
    }
}
