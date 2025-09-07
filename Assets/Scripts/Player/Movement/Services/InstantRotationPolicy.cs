using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantRotationPolicy : IRotationPolicy
{
    public Quaternion Compute(Quaternion current, Vector3 desiredDir, float turnSpeed, float dt)
        => desiredDir == Vector3.zero ? current : Quaternion.LookRotation(desiredDir, Vector3.up);
}
