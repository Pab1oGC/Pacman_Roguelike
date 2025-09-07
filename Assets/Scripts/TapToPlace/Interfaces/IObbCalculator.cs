using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public interface IObbCalculator 
{
    bool TryComputeObb(ARPlane plane, out Obb2D obb);
}
