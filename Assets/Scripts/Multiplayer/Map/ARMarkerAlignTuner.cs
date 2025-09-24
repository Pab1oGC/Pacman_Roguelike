using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARMarkerAlignTuner : MonoBehaviour
{
    public Transform marker;                       // el ARTrackedImage.transform
    public bool liveUpdate = true;                 // recalcular cada frame
    public bool snapToPlane = true;                // pegar contra el plano del marker
    [Range(0f, 0.05f)] public float snapEpsilon = 0.01f; // separación mínima (m)

    [Header("Offsets en espacio LOCAL del marker (m)")]
    public Vector3 localOffset = new Vector3(0f, 0.0f, 0f); // X=Right, Y=Up, Z=Forward del marker

    [Header("Rotación adicional")]
    public Vector3 localEulerOffset = Vector3.zero; // rotación extra (deg) en ejes del marker

    [Header("Escala")]
    public bool fitToImageWidth = false;           // ajusta ancho del mapa al ancho físico del marker
    public float uniformScale = 1f;                // multiplicador adicional

    // cache
    Bounds _bounds;
    bool _hasBounds;

    void Awake()
    {
        RecalculateBounds();
    }

    public void RecalculateBounds()
    {
        var rends = GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) { _hasBounds = false; return; }
        _bounds = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) _bounds.Encapsulate(rends[i].bounds);
        _hasBounds = true;
    }

    void LateUpdate()
    {
        if (!liveUpdate) return;
        Apply();
    }

    [ContextMenu("Apply Now")]
    public void Apply()
    {
        if (!marker) return;

        // 1) Pose base = plano del marker
        transform.position = marker.position;
        transform.rotation = marker.rotation;

        // 2) Escala
        float scale = Mathf.Max(0.0001f, uniformScale);
        if (fitToImageWidth)
        {
            var img = marker.GetComponent<ARTrackedImage>() ?? marker.GetComponentInParent<ARTrackedImage>();
            if (img)
            {
                // ancho físico del marker
                float markerWidth = img.size.x;
                // ancho actual del contenido (bounds en X a escala 1)
                if (!_hasBounds) RecalculateBounds();
                float contentWidth = Mathf.Max(0.0001f, _bounds.size.x);
                scale *= markerWidth / contentWidth;
            }
        }
        transform.localScale = Vector3.one * scale;

        // 3) Rotación adicional en ejes locales del marker
        transform.rotation *= Quaternion.Euler(localEulerOffset);

        // 4) Offset en el espacio local del marker (Right/Up/Forward del marker)
        transform.position += marker.right * localOffset.x
                            + marker.up * localOffset.y
                            + marker.forward * localOffset.z;

        // 5) Snap al plano del marker (sube media altura + epsilon en dirección "Up" del marker)
        if (snapToPlane)
        {
            if (!_hasBounds) RecalculateBounds();
            // bounds a mundo después de escala/rot
            var rends = GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
                float halfH = b.extents.y;
                transform.position += marker.up * (halfH + snapEpsilon);
            }
        }
    }

    // gizmos para ver el plano y ejes del marker
    void OnDrawGizmosSelected()
    {
        if (!marker) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(marker.position, marker.position + marker.right * 0.2f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(marker.position, marker.position + marker.up * 0.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(marker.position, marker.position + marker.forward * 0.2f);

        // plano del marker
        Gizmos.color = new Color(1, 1, 0, 0.25f);
        var p = marker.position;
        var r = marker.rotation;
        var right = r * Vector3.right;
        var forward = r * Vector3.forward;
        float s = 0.15f;
        Vector3 a = p + right * s + forward * s;
        Vector3 b = p + right * s - forward * s;
        Vector3 c = p - right * s - forward * s;
        Vector3 d = p - right * s + forward * s;
        Gizmos.DrawLine(a, b); Gizmos.DrawLine(b, c); Gizmos.DrawLine(c, d); Gizmos.DrawLine(d, a);
    }
}
