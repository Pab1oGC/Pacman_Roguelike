using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlayerSpawnApplier : NetworkBehaviour
{
    public Movement movement;
    public Rigidbody rb;

    void Awake()
    {
        if (!movement) movement = GetComponent<Movement>();
        if (!rb) rb = GetComponent<Rigidbody>();
    }

    [TargetRpc]
    public void TargetApplySpawnLocal(NetworkConnection conn, Vector3 localPos, Quaternion localRot)
    {
        StartCoroutine(WaitOriginAndApply(localPos, localRot));
    }

    IEnumerator WaitOriginAndApply(Vector3 localPos, Quaternion localRot)
    {
        Transform origin = s_editorOrigin;
        if (origin == null)
        {
            var mgr = FindObjectOfType<ARTrackedImageManager>();
            float t = 0f;
            while (t < 10f && origin == null)
            {
                if (mgr != null)
                    foreach (var ti in mgr.trackables)
                        if (ti.trackingState == TrackingState.Tracking) { origin = ti.transform; break; }
                if (origin) break;
                t += Time.deltaTime; yield return null;
            }
        }

        Vector3 worldPos; Quaternion worldRot;
        if (origin != null)
        {
            worldPos = origin.TransformPoint(localPos);
            worldRot = YawOnly(origin.rotation) * localRot; // solo yaw
        }
        else { worldPos = localPos; worldRot = localRot; }

        if (movement) movement.enabled = false;

        if (rb)
        {
            rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero;
            rb.position = worldPos; rb.rotation = worldRot;
            rb.MovePosition(worldPos); rb.MoveRotation(worldRot);
        }
        else transform.SetPositionAndRotation(worldPos, worldRot);

        yield return null;
        if (movement) movement.enabled = true;
    }

    static Quaternion YawOnly(Quaternion q)
    {
        Vector3 fwd = Vector3.ProjectOnPlane(q * Vector3.forward, Vector3.up);
        if (fwd.sqrMagnitude < 1e-6f) fwd = Vector3.forward;
        return Quaternion.LookRotation(fwd, Vector3.up);
    }

    // Editor bridge
    static Transform s_editorOrigin;
    public static void SetEditorOrigin(Transform t) => s_editorOrigin = t;
}
