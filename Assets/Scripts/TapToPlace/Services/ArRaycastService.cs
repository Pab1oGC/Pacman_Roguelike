using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ArRaycastService : IArRaycastService
{
    private readonly ARRaycastManager _raycast;

    public ArRaycastService(ARRaycastManager raycast) => _raycast = raycast;

    public bool TryRaycastPlanes(Vector2 screenPos, List<ARRaycastHit> hits, TrackableType mask)
        => _raycast.Raycast(screenPos, hits, mask);

}
