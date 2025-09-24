using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{
    // Deja Auto Create Player = ON en el inspector
    // (así se crea el player al conectar y aquí lo teletransportamos si corresponde)

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Crea el player del dueño
        base.OnServerAddPlayer(conn);

        // Si el mapa YA está, teletransporta al SpawnPoint
        /*if (TrackedImageMultiplayer.MapReady && StartRoom.Instance != null && StartRoom.Instance.SpawnPoint != null)
        {
            Transform sp = StartRoom.Instance.SpawnPoint;

            // Solo-yaw para evitar inclinaciones raras
            Quaternion rot = YawOnly(sp.rotation);
            Vector3 pos = sp.position;

            // 1) mueve también el objeto del servidor
            conn.identity.transform.SetPositionAndRotation(pos, rot);

            // 2) pide al dueño que aplique local (evita pelea con NetworkTransform)
            var snap = conn.identity.GetComponent<PlayerSpawnSnap>();
            if (snap != null) snap.TargetSnap(conn, pos, rot);
        }*/
        // Si el mapa NO está listo aún, no hacemos nada (nacerá donde el prefab).
        // Cuando el host construya, también podrías teletransportarlo si quieres (opcional).
    }

    static Quaternion YawOnly(Quaternion q)
    {
        Vector3 fwd = Vector3.ProjectOnPlane(q * Vector3.forward, Vector3.up);
        if (fwd.sqrMagnitude < 1e-6f) fwd = Vector3.forward;
        return Quaternion.LookRotation(fwd, Vector3.up);
    }
}
