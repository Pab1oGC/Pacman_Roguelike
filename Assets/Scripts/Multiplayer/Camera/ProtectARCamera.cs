using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectARCamera : MonoBehaviour
{
    Camera cam;
    AudioListener lst;

    void Awake()
    {
        cam = GetComponent<Camera>();
        lst = GetComponent<AudioListener>();
    }

    void LateUpdate()
    {
        // Si alguien la desactiva, la volvemos a encender ese mismo frame
        if (cam && !cam.enabled)
        {
            Debug.LogWarning("[ProtectARCamera] Rehabilitando AR Camera (alguien la apagó).");
            cam.enabled = true;
            if (lst) lst.enabled = true;
        }
    }
}
