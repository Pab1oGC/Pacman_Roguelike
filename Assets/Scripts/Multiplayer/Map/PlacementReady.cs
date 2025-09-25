using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementReady : NetworkBehaviour
{
    public static readonly HashSet<int> ReadyConnIds = new HashSet<int>();

    [Command(requiresAuthority = false)]
    public void CmdIAmReady()
    {
        if (connectionToClient != null)
        {
            ReadyConnIds.Add(connectionToClient.connectionId);
            Debug.Log($"[Ready] Conn {connectionToClient.connectionId} READY");
        }
    }

    public static bool IsAllReadyOrTimeout(float startedAt, float timeoutSec)
    {
        foreach (var kv in NetworkServer.connections)
        {
            var conn = kv.Value;
            if (conn == null || conn.identity == null) continue;
            if (!ReadyConnIds.Contains(conn.connectionId))
            {
                if (Time.time - startedAt > timeoutSec) return true; // salir por timeout
                return false;
            }
        }
        return true;
    }
}
