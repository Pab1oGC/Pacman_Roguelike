using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetworkAdapter : NetworkBehaviour
{
    [Header("Opcional, asigna si quieres forzar refs")]
    public Movement movement;          // tu script Movement (puede quedar en null)
    public Camera playerCamera;        // la cámara del jugador
    public AudioListener audioListener;// su AudioListener

    void Awake()
    {
        // Resolve perezoso
        if (!movement) movement = GetComponent<Movement>();
        if (!playerCamera)
        {
            var ctx = GetComponent<PlayerContext>();
            if (ctx) playerCamera = ctx.MainCamera;
        }
        if (!audioListener && playerCamera) audioListener = playerCamera.GetComponent<AudioListener>();
    }

    public override void OnStartLocalPlayer()
    {
        if (movement) movement.enabled = true;
        if (playerCamera) playerCamera.enabled = true;
        if (audioListener) audioListener.enabled = true;
    }

    public override void OnStartClient()
    {
        if (!isLocalPlayer)
        {
            if (movement) movement.enabled = false;
            if (playerCamera) playerCamera.enabled = false;
            if (audioListener) audioListener.enabled = false;
        }
    }
}
