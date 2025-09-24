using UnityEngine;
using Mirror;                       // <— IMPORTANTE

public class PlayerCameraController : NetworkBehaviour  // <— hereda de NetworkBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private AudioListener listener; // opcional

    void Awake()
    {
        if (!cam) cam = GetComponentInChildren<Camera>(true);
        if (!listener) listener = GetComponentInChildren<AudioListener>(true);

        // Apaga por defecto para evitar que las cámaras de remotos rendericen
        if (cam) cam.enabled = false;
        if (listener) listener.enabled = false;
    }

    public override void OnStartLocalPlayer()
    {
        // Solo el jugador local enciende su cámara
        if (cam)
        {
            cam.enabled = true;
            cam.tag = "MainCamera";               // útil si usas Camera.main
            cam.depth = 10;                       // por si hay otra cámara residual
        }
        if (listener) listener.enabled = true;    // solo un AudioListener activo
    }

    public override void OnStopClient()
    {
        if (cam) cam.enabled = false;
        if (listener) listener.enabled = false;
    }
}