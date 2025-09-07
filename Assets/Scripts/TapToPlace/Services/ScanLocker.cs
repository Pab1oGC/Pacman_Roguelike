using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ScanLocker : IScanLocker
{
    public void Lock(ARPlaneManager planeMgr, ARRaycastManager rayMgr, ARPlane keep)
    {
        if (planeMgr != null)
        {
            foreach (var p in planeMgr.trackables)
                p.gameObject.SetActive(p.trackableId == keep.trackableId);

            planeMgr.enabled = false;
            planeMgr.requestedDetectionMode = PlaneDetectionMode.None;

            var vis = keep.GetComponent<ARPlaneMeshVisualizer>();
            if (vis) vis.enabled = false;
        }

        if (rayMgr != null) rayMgr.enabled = false;
    }
}
