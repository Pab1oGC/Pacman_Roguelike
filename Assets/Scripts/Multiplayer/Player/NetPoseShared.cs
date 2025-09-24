using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPoseShared : NetworkBehaviour
{
    [SyncVar] Vector3 hostPos;
    [SyncVar] Quaternion hostRot;

    void Update()
    {
        if (isServer)
        {
            hostPos = transform.position;
            hostRot = transform.rotation;
        }
        else
        {
            if (SharedAlignment.has)
            {
                Vector3 p = SharedAlignment.MapPos_HostToClient(hostPos);
                Quaternion r = SharedAlignment.MapRot_HostToClient(hostRot);
                transform.SetPositionAndRotation(p, r);
            }
        }
    }
}
