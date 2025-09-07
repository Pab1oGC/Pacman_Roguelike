using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public interface IScanLocker 
{
    void Lock(ARPlaneManager planeMgr, ARRaycastManager rayMgr, ARPlane keep);
}
