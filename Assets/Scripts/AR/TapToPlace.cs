using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TapToPlace : MonoBehaviour
{
    [Header("Visuales (opcional)")]
    [SerializeField] private Material planeMaterial;

    [Header("Separación extra sobre el plano (m)")]
    [SerializeField] private float extraClearance = 0.01f;

    // Managers AR (asígnalos en el Inspector o pon este script en AR Session Origin)
    private ARRaycastManager _raycaster;
    private ARPlaneManager _planeMgr;
    private ARAnchorManager _anchorMgr;

    // Estado
    private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();
    private bool _locked;
    private ARPlane _selectedPlane;
    private ARAnchor _anchor;
    private GameObject _spawned;

    // ==================== DUNGEON ====================
    [Header("Dungeon (procedural)")]
    [Tooltip("Prefab del mapa que contiene DungeonGenerator (autoGenerate=false)")]
    [SerializeField] private GameObject dungeonPrefab;

    [Tooltip("Tamaño de UNA sala/celda en metros (X = ancho, Z = largo)")]
    [SerializeField] private float tileSizeX = 9.77f;

    [SerializeField] private float tileSizeZ = 5.92f;

    [Tooltip("Margen a recortar del tamaño del plano por cada lado (m)")]
    [SerializeField] private float planePadding = 0.10f;

    [Tooltip("Mínimos y máximos del grid del dungeon")]
    [SerializeField] private int minCols = 1, minRows = 1;

    [SerializeField] private int maxCols = 20, maxRows = 20;

    [SerializeField] private float zNudgeMeters = 0.10f;
    // =================================================

    private void Awake()
    {
        _raycaster = GetComponent<ARRaycastManager>();
        _planeMgr = GetComponent<ARPlaneManager>();
        _anchorMgr = GetComponent<ARAnchorManager>();

        if (_planeMgr != null)
            _planeMgr.requestedDetectionMode = PlaneDetectionMode.Horizontal;
    }

    private void Update()
    {
        if (_locked) return;

        // Input: touch o mouse (Editor con XR Simulation)
        if (!TryGetPointerDown(out var screenPos)) return;

        // Raycast a planos
        if (_raycaster == null || !_raycaster.Raycast(screenPos, Hits, TrackableType.PlaneWithinPolygon))
            return;

        var hit = Hits[0];
        if (_planeMgr == null) return;

        var plane = _planeMgr.GetPlane(hit.trackableId);
        if (plane == null || plane.trackingState != TrackingState.Tracking || plane.alignment != PlaneAlignment.HorizontalUp)
            return;

        // ===================== TAMAÑO DEL PLANO =====================
        // ARPlane.size devuelve metros: x = ancho, y = largo (ya está alineado a HorizontalUp)
        Vector2 planeSize = plane.size;

        // Recortamos bordes con padding en ambos lados
        float usableX = Mathf.Max(0f, planeSize.x - planePadding * 2f);
        float usableZ = Mathf.Max(0f, planeSize.y - planePadding * 2f);

        if (tileSizeX <= 0.01f || tileSizeZ <= 0.01f)
        {
            Debug.LogWarning("[TapToPlace] Tile inválido. Define tileSizeX/tileSizeZ reales de tu sala.");
            return;
        }

        int cols = Mathf.FloorToInt(usableX / tileSizeX);
        int rows = Mathf.FloorToInt(usableZ / tileSizeZ);
        cols = Mathf.Clamp(cols, minCols, maxCols);
        rows = Mathf.Clamp(rows, minRows, maxRows);

        if (cols < minCols || rows < minRows)
        {
            Debug.Log("[TapToPlace] Plano útil pequeño para los mínimos configurados. Mueve un poco la cámara o baja minCols/minRows.");
            return;
        }

        // ===================== POSICIÓN / ANCHOR =====================
        // ARPlane.center está en espacio local del plano
        Vector3 worldCenter = plane.transform.TransformPoint(new Vector3(plane.center.x, 0f, plane.center.y));
        var pose = new Pose(worldCenter + Vector3.up * extraClearance, plane.transform.rotation);

        _anchor = EnsureAnchor(plane, pose, _anchorMgr, _anchor);
        if (_anchor == null) return;

        // ===================== INSTANCIAR DUNGEON =====================
        if (dungeonPrefab == null)
        {
            Debug.LogError("[TapToPlace] dungeonPrefab no asignado.");
            return;
        }

        if (dungeonPrefab == null)
        {
            Debug.LogError("[TapToPlace] dungeonPrefab no asignado.");
            return;
        }

        // Instancia como hijo del anchor, usando el pose (centro del plano)
        // --- Instanciar anclado al centro del plano ---
        var dungeonGO = Instantiate(dungeonPrefab, _anchor.transform);
        dungeonGO.transform.position = pose.position;           // centro del plano
        dungeonGO.transform.rotation = Quaternion.identity;     // horizontal

        var gen = dungeonGO.GetComponent<DungeonGenerator>();
        if (gen == null) { Debug.LogError("DungeonGenerator faltante"); Destroy(dungeonGO); return; }
        gen.autoGenerate = false;
        gen.Run(new Vector2Int(cols, rows), new Vector2(tileSizeX, tileSizeZ));

        // --- 1) Obtener bounds del contenido ya generado (mundo) ---
        bool TryGetWorldBounds(GameObject root, out Bounds b)
        {
            b = default;
            var rends = root.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
                return true;
            }
            // fallback con colliders si no hay renderers
            var cols = root.GetComponentsInChildren<Collider>();
            if (cols.Length > 0)
            {
                b = cols[0].bounds;
                for (int i = 1; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);
                return true;
            }
            return false;
        }

        if (TryGetWorldBounds(dungeonGO, out var bb))
        {
            // --- 2) Ajuste vertical: apoyar en el plano ---
            float planeY = pose.position.y;
            float deltaY = (planeY + extraClearance) - bb.min.y;
            dungeonGO.transform.position += new Vector3(0f, deltaY, 0f);

            // Recalcular bounds tras mover en Y (opcional pero fino)
            TryGetWorldBounds(dungeonGO, out bb);

            // --- 3) Centrando en XZ respecto al centro del plano ---
            Vector3 targetXZ = new Vector3(pose.position.x, dungeonGO.transform.position.y, pose.position.z);
            Vector3 deltaXZ = new Vector3(targetXZ.x - bb.center.x, 0f, targetXZ.z - bb.center.z);
            dungeonGO.transform.position += deltaXZ;

            Vector3 fwd = plane.transform.forward;
            fwd.y = 0f;                // proyección horizontal
            if (fwd.sqrMagnitude > 0f) fwd.Normalize();
            dungeonGO.transform.position += fwd * zNudgeMeters;
        }
        else
        {
            // Si no encontramos bounds, al menos súbelo un pelín
            dungeonGO.transform.position += Vector3.up * extraClearance;
        }

        Debug.Log($"[TapToPlace] Dungeon generado {cols}x{rows} tiles | tile=({tileSizeX}m, {tileSizeZ}m) | planeSize=({planeSize.x:F2},{planeSize.y:F2})");

        // (Opcional) bloquear tracking/planos si quieres “congelar” la escena
        LockScanning(plane);

        _selectedPlane = plane;
        _locked = true;
    }

    // Mouse o touch
    private bool TryGetPointerDown(out Vector2 screenPos)
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            screenPos = Input.mousePosition;
            return true;
        }
#endif
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            screenPos = Input.GetTouch(0).position;
            return true;
        }
        screenPos = default;
        return false;
    }

    private static ARAnchor EnsureAnchor(ARPlane plane, Pose pose, ARAnchorManager mgr, ARAnchor current)
    {
        if (current != null) Object.Destroy(current);

        ARAnchor anchor = null;
        if (mgr != null)
            anchor = mgr.AttachAnchor(plane, pose);

        if (anchor == null)
        {
            // Fallback: anclar directamente sobre el GO del plano
            anchor = plane.gameObject.AddComponent<ARAnchor>();
            anchor.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
        return anchor;
    }

    private void LockScanning(ARPlane keepPlane)
    {
        if (_planeMgr == null) return;

        // Oculta TODOS los planos, incluso el seleccionado
        foreach (var p in _planeMgr.trackables)
        {
            p.gameObject.SetActive(false);
        }

        // Desactiva el plane manager
        _planeMgr.enabled = false;
    }
}
