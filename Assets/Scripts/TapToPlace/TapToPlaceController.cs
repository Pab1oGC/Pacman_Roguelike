using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
[RequireComponent(typeof(ARAnchorManager))]
public class TapToPlaceController : MonoBehaviour
{
    [Header("Visuales")]
    [SerializeField] private Material planeMaterial;

    [Header("Prefab a instanciar")]
    [SerializeField] private GameObject prefab;

    [Tooltip("Separación extra sobre el plano (m)")]
    [SerializeField] private float extraClearance = 0.01f;

    // Managers AR
    private ARRaycastManager _raycaster;
    private ARPlaneManager _planeMgr;
    private ARAnchorManager _anchorMgr;

    // Servicios (intercambiables: DIP)
    private IInputSource _input;
    private IArRaycastService _rayService;
    private IPlaneSelectionPolicy _planePolicy;
    private IObbCalculator _obbCalc;
    private IRectifiedQuadFactory _quadFactory;
    private IPrefabPlacer _placer;
    private IScanLocker _scanLocker;

    // Estado
    private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();
    private bool _locked;
    private ARPlane _selectedPlane;
    private ARAnchor _anchor;
    private GameObject _rectifiedGO;
    private GameObject _spawned;

    private void Awake()
    {
        _raycaster = GetComponent<ARRaycastManager>();
        _planeMgr = GetComponent<ARPlaneManager>();
        _anchorMgr = GetComponent<ARAnchorManager>();

        _planeMgr.requestedDetectionMode = PlaneDetectionMode.Horizontal;

        // Implementaciones por defecto (puedes inyectar otras)

        _input = new TouchMouseInput();
        _rayService = new ArRaycastService(_raycaster);
        _planePolicy = new HorizontalUpPlaneSelectionPolicy(_planeMgr);
        _obbCalc = new ObbCalculator();
        _quadFactory = new RectifiedQuadFactory(planeMaterial);
        _placer = new PrefabPlacer(extraClearance);
        _scanLocker = new ScanLocker();
    }

    private void Update()
    {
        if (_locked || prefab == null) return;

        if (!_input.TryGetTap(out var screenPos)) return;

        if (!_rayService.TryRaycastPlanes(screenPos, Hits, TrackableType.PlaneWithinPolygon))
            return;

        foreach (var hit in Hits)
        {
            var plane = _planePolicy.Resolve(hit);
            if (plane == null) continue;

            if (!_obbCalc.TryComputeObb(plane, out var obb)) continue;

            // Centro del rectángulo en espacio de mundo
            Vector3 worldCenter = plane.transform.TransformPoint(new Vector3(obb.Center.x, 0f, obb.Center.y));
            var pose = new Pose(worldCenter, plane.transform.rotation);

            _anchor = EnsureAnchor(plane, pose, _anchorMgr, _anchor);
            if (_anchor == null) return;

            // Quad rectificado (visual)
            _rectifiedGO = _quadFactory.EnsureQuad(_rectifiedGO, _anchor.transform);
            _quadFactory.UpdateQuadTransform(_rectifiedGO.transform, obb);

            // Instanciar/ubicar prefab
            _spawned = _placer.Place(prefab, _spawned, _anchor.transform);

            // Conjunto final: bloquear escaneo, guardar plano y terminar
            _scanLocker.Lock(_planeMgr, _raycaster, plane);
            _selectedPlane = plane;
            _locked = true;
            return;
        }
    }

    private static ARAnchor EnsureAnchor(ARPlane plane, Pose pose, ARAnchorManager mgr, ARAnchor current)
    {
        if (current != null) Destroy(current);

        var anchor = mgr.AttachAnchor(plane, pose);
        if (anchor == null)
        {
            // fallback: componente ARAnchor sobre el plane GO
            anchor = plane.gameObject.AddComponent<ARAnchor>();
            anchor.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
        return anchor;
    }

}
