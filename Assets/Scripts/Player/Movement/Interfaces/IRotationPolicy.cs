using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRotationPolicy 
{
    Quaternion Compute(Quaternion current, Vector3 desiredDir, float turnSpeedDegPerSec, float dt);
}
