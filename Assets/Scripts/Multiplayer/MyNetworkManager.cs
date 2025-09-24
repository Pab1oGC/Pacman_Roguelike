using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{
    // Deja Auto Create Player = ON en el inspector
    // (as� se crea el player al conectar y aqu� lo teletransportamos si corresponde)

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Crea el player del due�o
        base.OnServerAddPlayer(conn);

        // Si el mapa YA est�, teletransporta al SpawnPoint
        /*if (TrackedImageMultiplayer.MapReady && StartRoom.Instance != null && StartRoom.Instance.SpawnPoint != null)
        {
            Transform sp = StartRoom.Instance.SpawnPoint;

            // Solo-yaw para evitar inclinaciones raras
            Quaternion rot = YawOnly(sp.rotation);
            Vector3 pos = sp.position;

            // 1) mueve tambi�n el objeto del servidor
            conn.identity.transform.SetPositionAndRotation(pos, rot);

            // 2) pide al due�o que aplique local (evita pelea con NetworkTransform)
            var snap = conn.identity.GetComponent<PlayerSpawnSnap>();
            if (snap != null) snap.TargetSnap(conn, pos, rot);
        }*/
        // Si el mapa NO est� listo a�n, no hacemos nada (nacer� donde el prefab).
        // Cuando el host construya, tambi�n podr�as teletransportarlo si quieres (opcional).
    }

    static Quaternion YawOnly(Quaternion q)
    {
        Vector3 fwd = Vector3.ProjectOnPlane(q * Vector3.forward, Vector3.up);
        if (fwd.sqrMagnitude < 1e-6f) fwd = Vector3.forward;
        return Quaternion.LookRotation(fwd, Vector3.up);
    }
}
