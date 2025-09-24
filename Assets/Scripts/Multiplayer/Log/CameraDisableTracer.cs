using UnityEngine;
using System.Diagnostics;

public class CameraDisableTracer : MonoBehaviour
{
    Camera cam;

    void Awake() { cam = GetComponent<Camera>(); }

    void OnEnable()
    {
        UnityEngine.Debug.Log("[CameraDisableTracer] ENABLED " + name);
    }

    void OnDisable()
    {
        // Se llama cuando el componente Camera se desactiva o el GO se desactiva
        var trace = new StackTrace(true);
        UnityEngine.Debug.LogError($"[CameraDisableTracer] DISABLED {name}\n{trace}");
    }

    void LateUpdate()
    {
        // Log en runtime si alguien cambió enabled a false sin desactivar el GO
        if (cam != null && !cam.enabled)
        {
            var trace = new StackTrace(true);
            UnityEngine.Debug.LogError($"[CameraDisableTracer] cam.enabled == FALSE en {name}\n{trace}");
        }
    }

}
