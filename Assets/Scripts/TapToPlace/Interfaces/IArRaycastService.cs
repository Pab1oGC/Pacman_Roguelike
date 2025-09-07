using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public interface IArRaycastService 
{
    bool TryRaycastPlanes(Vector2 screenPos, List<ARRaycastHit> hits, TrackableType mask);
}
