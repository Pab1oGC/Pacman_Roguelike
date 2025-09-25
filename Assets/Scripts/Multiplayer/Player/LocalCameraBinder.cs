using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class LocalCameraBinder : NetworkBehaviour
{
    Camera cam; AudioListener lst;

    public override void OnStartLocalPlayer()
    {
        StartCoroutine(ResolveAndEnableCamera());
    }

    IEnumerator ResolveAndEnableCamera()
    {
        // hasta 3s de reintentos por si otros scripts tocan la cámara al inicio
        float end = Time.time + 3f;
        while (Time.time < end)
        {
            // 1) Camera.main (si la AR Camera tiene tag MainCamera)
            if (!cam) cam = Camera.main;

            // 2) Buscar dentro de ARSessionOrigin
            if (!cam)
            {
                var origin = FindObjectOfType<ARSessionOrigin>();
                if (origin) cam = origin.GetComponentInChildren<Camera>(true);
            }

            // 3) Cualquier cámara en escena
            if (!cam)
            {
                var cams = FindObjectsOfType<Camera>(true);
                if (cams.Length > 0) cam = cams[0];
            }

            if (cam) break;
            yield return null;
        }

        // 4) Si no hay ninguna, crea una DebugCamera para no quedar sin render
        if (!cam)
        {
            Debug.LogError("[Binder] No se encontró ninguna Camera. Creando DebugCamera.");
            var go = new GameObject("DebugCamera");
            cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.nearClipPlane = 0.1f; cam.farClipPlane = 1000f;
        }

        // Suelta parent (la AR camera NUNCA debe ser hija del Player)
        if (cam.transform.parent) cam.transform.SetParent(null, true);

        // Fuerza configuración mínima
        cam.targetDisplay = 1;
        cam.depth = 10;
        cam.enabled = true;
        cam.gameObject.SetActive(true);

        lst = cam.GetComponent<AudioListener>();
        if (lst) lst.enabled = true;

        Debug.Log("[Binder] Camera activa: " + cam.name);
        // Apaga cámaras de players remotos (si las hubiera por error)
        DisableRemoteCameras();
        // Mantén viva la cámara los primeros frames por si alguien intenta apagarla
        yield return StartCoroutine(KeepAliveFor(1.5f));
    }

    IEnumerator KeepAliveFor(float seconds)
    {
        var end = Time.time + seconds;
        while (Time.time < end)
        {
            if (cam && !cam.enabled) { cam.enabled = true; Debug.LogWarning("[Binder] Re-enabled camera."); }
            yield return null;
        }
    }

    void DisableRemoteCameras()
    {
        var all = FindObjectsOfType<Camera>(true);
        foreach (var c in all)
        {
            if (c == cam) continue;
            // si la cámara pertenece a un player que NO es local, apágala
            var id = c.GetComponentInParent<NetworkIdentity>();
            if (id != null && !id.isLocalPlayer) c.enabled = false;
        }
    }

    public override void OnStopLocalPlayer()
    {
        if (cam) cam.enabled = false;
        if (lst) lst.enabled = false;
    }
}
