using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerSpawnDebug : NetworkBehaviour
{
    GameObject debugCube;

    public override void OnStartClient()
    {
        Debug.Log($"[PlayerSpawnDebug] OnStartClient netId={netId} isLocal={isLocalPlayer} conn={(connectionToServer != null ? 1 : -1)}");
        MakeCube();
    }

    public override void OnStartLocalPlayer()
    {
        Debug.Log("[PlayerSpawnDebug] OnStartLocalPlayer");
    }

    void MakeCube()
    {
        debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugCube.transform.SetParent(transform, false);
        debugCube.transform.localScale = Vector3.one * 0.3f;
        debugCube.transform.localPosition = Vector3.up * 0.5f;
        var r = debugCube.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Standard"));
        r.material.color = isLocalPlayer ? Color.green : Color.red;
        debugCube.name = "DebugCube_Player";
    }
}
