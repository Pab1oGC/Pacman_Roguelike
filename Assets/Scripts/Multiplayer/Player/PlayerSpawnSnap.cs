using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnSnap : NetworkBehaviour
{
    [TargetRpc]  // se ejecuta en el CLIENTE dueño
    public void TargetSnap(NetworkConnection conn, Vector3 pos, Quaternion rot)
    {
        var rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Set y Move para evitar jitter del primer frame
            rb.position = pos;
            rb.rotation = rot;
            rb.MovePosition(pos);
            rb.MoveRotation(rot);
        }
        else
        {
            transform.SetPositionAndRotation(pos, rot);
        }
    }
}
