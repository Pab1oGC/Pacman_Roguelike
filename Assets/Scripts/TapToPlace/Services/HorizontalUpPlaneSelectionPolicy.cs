using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class HorizontalUpPlaneSelectionPolicy : IPlaneSelectionPolicy
{
    private readonly ARPlaneManager _planeMgr;

    public HorizontalUpPlaneSelectionPolicy(ARPlaneManager planeMgr) => _planeMgr = planeMgr;

    public ARPlane Resolve(ARRaycastHit hit)
    {
        var plane = _planeMgr.GetPlane(hit.trackableId);
        if (plane == null) return null;
        if (plane.alignment != PlaneAlignment.HorizontalUp) return null;
        return plane;
    }
}
