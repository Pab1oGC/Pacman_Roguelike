using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public interface IPlaneSelectionPolicy 
{
    ARPlane Resolve(ARRaycastHit hit);
}
