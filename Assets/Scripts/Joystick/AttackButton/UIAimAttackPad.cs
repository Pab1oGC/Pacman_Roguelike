using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIAimAttackPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Refs")]
    [SerializeField] private RectTransform padArea;        // este mismo RectTransform si lo dejas vacío
    [SerializeField] private RectTransform arrowUI;        // flecha UI (pivot en (0.5,0), apunta hacia arriba)
    [SerializeField] private RectTransform knob;           // opcional: puntito que sigue el dedo

    [Header("Ajustes de input")]
    [SerializeField] private float maxRadius = 140f;       // px de arrastre máximo
    [SerializeField] private float deadZone = 12f;         // px mínimos para considerar dirección
    [SerializeField] private bool requireHoldToShowArrow = false;

    [Header("Flecha sobre el Player (opcional)")]
    [SerializeField] private RectTransform playerArrow;    // UI Image encima del player (en Canvas overlay)
    [SerializeField] private float playerArrowLenMin = 60f;
    [SerializeField] private float playerArrowLenMax = 180f;

    [Header("Aim en mundo (LineRenderer)")]
    [SerializeField] private LineRenderer worldLine;       // opcional; si está vacío lo creo en runtime
    [SerializeField] private float worldLineMaxDistance = 12f;
    [SerializeField] private LayerMask worldLineMask = ~0; // todo por defecto
    [SerializeField] private float worldLineHeight = 1.0f; // altura base sobre el player (m)
    [SerializeField] private Gradient worldLineColor;      // opcional: colores/alpha de la línea
    [SerializeField] private float worldLineWidth = 0.06f; // grosor base (m)
    [SerializeField] private GameObject hitDotPrefab;

    [Header("Player binding")]
    [SerializeField] private string playerTag = "Player";

    public Camera _cam;                                    // ARCamera u otra
    private Canvas _canvas;                                // Canvas que contiene el pad
    private bool _canvasIsOverlay;                         // si es Overlay => camera=null en RectTransformUtility

    private RectTransform _rt;
    private Vector2 _pressPos;
    private int _activePointerId = -100;
    private bool _aiming;
    private Vector2 _aimDirUI;            // dirección 2D en espacio UI (eje Y = arriba en pantalla)
    private AimAttackRelay _relay;        // en el Player
    private Transform _player;            // world
    private bool _hadDirection;
    private Vector2 _knobHome;
    private GameObject _hitDot;

    private void Awake()
    {
        // Rect del pad
        _rt = padArea ? padArea : (RectTransform)transform;

        // Canvas contenedor y modo
        _canvas = GetComponentInParent<Canvas>();
        _canvasIsOverlay = _canvas && _canvas.renderMode == RenderMode.ScreenSpaceOverlay;

        // Cámara a usar para mundo y UI
        if (!_cam) _cam = Camera.main;
        if (!_cam) _cam = FindObjectOfType<Camera>();

        // Crear/Configurar línea desde ya
        SetupWorldLine();
        

        // Bind al player en cuanto aparezca
        StartCoroutine(WaitAndBindPlayer());

        SetWorldLineActive(false);

        // UI inicial
        ShowArrow(false);
        if (knob) _knobHome = knob.anchoredPosition;
        if (playerArrow) playerArrow.gameObject.SetActive(false);
    }

    private IEnumerator WaitAndBindPlayer()
    {
        // espera a que exista y esté listo el player local
        while (NetworkClient.localPlayer == null)
            yield return null;

        var localId = NetworkClient.localPlayer;
        _player = localId.transform;

        // Relay (en el mismo prefab del player)
        _relay = localId.GetComponentInChildren<AimAttackRelay>();
        if (_relay == null) _relay = localId.gameObject.AddComponent<AimAttackRelay>();

        // Re-parent de la línea al player (si la tenías creada antes)
        if (worldLine) worldLine.transform.SetParent(_player, false);
    }

    // Cámara correcta para RectTransformUtility según modo del Canvas
    private Camera GetCanvasCamera()
    {
        if (_canvasIsOverlay) return null;                    // Overlay: pasar null
        if (_canvas && _canvas.worldCamera) return _canvas.worldCamera;
        return _cam;                                          // fallback
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_activePointerId != -100) return; // ya hay otro dedo activo en esta UI
        _activePointerId = eventData.pointerId;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, eventData.position, GetCanvasCamera(), out _pressPos);
        _aiming = true;
        _hadDirection = false;

        if (!requireHoldToShowArrow) ShowArrow(true);
        if (knob) knob.anchoredPosition = _pressPos;
        if (playerArrow) playerArrow.gameObject.SetActive(true);

        SetWorldLineActive(true);
        UpdateWorldAimVisual(Vector3.zero);
        UpdateVisuals(Vector2.zero);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != _activePointerId || !_aiming) return;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, eventData.position, GetCanvasCamera(), out localPos);

        Vector2 delta = localPos - _pressPos;
        float mag = delta.magnitude;

        // dead zone
        if (mag < deadZone)
        {
            _aimDirUI = Vector2.zero;
            _hadDirection = false;
            UpdateVisuals(Vector2.zero);
            UpdateWorldAimVisual(Vector3.zero);
            return;
        }

        // clamp radio
        float clampedMag = Mathf.Min(mag, maxRadius);
        Vector2 dir = delta / (mag > 1e-5f ? mag : 1f); // seguro
        Vector2 clamped = dir * clampedMag;

        _aimDirUI = dir;
        _hadDirection = true;

        if (knob) knob.anchoredPosition = _pressPos + clamped;
        ShowArrow(true);
        UpdateVisuals(dir * (clampedMag / maxRadius)); // 0..1 para escala

        Vector3 worldDir = UIToWorldDir(_aimDirUI);
        UpdateWorldAimVisual(worldDir);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != _activePointerId) return;
        _activePointerId = -100;

        // Fire si hubo dirección válida
        if (_hadDirection && _relay != null && _player != null)
        {
            Vector3 worldDir = UIToWorldDir(_aimDirUI);
            _relay.Fire(worldDir);
        }

        _aiming = false;
        _aimDirUI = Vector2.zero;
        ShowArrow(false);
        if (playerArrow) playerArrow.gameObject.SetActive(false);
        if (knob) knob.anchoredPosition = _knobHome;

        SetWorldLineActive(false);
        UpdateWorldAimVisual(Vector3.zero);
    }

    private void ShowArrow(bool show)
    {
        if (arrowUI) arrowUI.gameObject.SetActive(show);
    }

    private void Update()
    {
        // Actualiza flecha sobre el player mientras apuntas
        if (_aiming && playerArrow && _player && _cam)
        {
            Vector3 screen = _cam.WorldToScreenPoint(_player.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)playerArrow.parent, screen, GetCanvasCamera(), out var local);

            playerArrow.anchoredPosition = local;

            // rotación y escala por magnitud del arrastre
            float ang = Mathf.Atan2(_aimDirUI.x, _aimDirUI.y) * Mathf.Rad2Deg; // up = 0°
            playerArrow.localRotation = Quaternion.Euler(0, 0, -ang);          // UI Z rota antihoraria

            float t = Mathf.Clamp01((_aimDirUI == Vector2.zero ? 0f : 1f));    // simple ON/OFF por ahora
            float len = Mathf.Lerp(playerArrowLenMin, playerArrowLenMax, t);
            playerArrow.sizeDelta = new Vector2(playerArrow.sizeDelta.x, len);  // asume pivot (0.5,0)
        }
    }

    private void UpdateVisuals(Vector2 scaledDir01)
    {
        if (!arrowUI) return;

        // ángulo (eje up = 0°, atan2(y, x) pero queremos up, así invertimos)
        float ang = Mathf.Atan2(_aimDirUI.x, _aimDirUI.y) * Mathf.Rad2Deg;
        arrowUI.localRotation = Quaternion.Euler(0, 0, -ang);

        // escala de longitud según arrastre
        float length = Mathf.Lerp(50f, 140f, Mathf.Clamp01(scaledDir01.magnitude));
        arrowUI.sizeDelta = new Vector2(arrowUI.sizeDelta.x, length); // pivot (0.5,0) → crece hacia arriba
    }

    // Convierte la dirección 2D de pantalla a un vector mundo XZ relativo a la cámara (como tu Movement)
    private Vector3 UIToWorldDir(Vector2 uiDir)
    {
        if (uiDir.sqrMagnitude < 1e-6f) return Vector3.zero;

        // 1) Elige cámara: inspector → Main → cualquier Camera en escena
        Camera cam = _cam;
        if (!cam) cam = Camera.main;
        if (!cam) cam = FindObjectOfType<Camera>(); // incluye la activa si no hay Main

        Vector3 world;
        if (cam)
        {
            // forward/right de la cámara, solo en plano XZ
            Vector3 camF = cam.transform.forward; camF.y = 0f;
            Vector3 camR = cam.transform.right; camR.y = 0f;
            if (camF.sqrMagnitude < 1e-6f) camF = Vector3.forward;
            if (camR.sqrMagnitude < 1e-6f) camR = Vector3.right;

            world = camR * uiDir.x + camF * uiDir.y;
        }
        else
        {
            // Fallback: ejes del mundo (útil si tu juego es top-down sin dependencia de cámara)
            world = new Vector3(uiDir.x, 0f, uiDir.y);
            Debug.LogWarning("[UIAimAttackPad] No se encontró cámara. Usando ejes de mundo como fallback.");
        }

        world.y = 0f;
        return world.sqrMagnitude > 0f ? world.normalized : Vector3.forward;
    }

    private void SetupWorldLine()
    {
        if (worldLine == null)
        {
            var go = new GameObject("AimLine", typeof(LineRenderer));
            worldLine = go.GetComponent<LineRenderer>();
        }

        worldLine.useWorldSpace = true;
        worldLine.positionCount = 2;
        worldLine.numCornerVertices = 2;
        worldLine.numCapVertices = 2;
        worldLine.alignment = LineAlignment.View;

        if (worldLine.material == null)
        {
            worldLine.material = new Material(Shader.Find("Sprites/Default"));
            worldLine.material.renderQueue = 3000; // transparente
        }
        if (worldLineColor != null) worldLine.colorGradient = worldLineColor;

        // ancho base: lo ajustaremos por escala del player en UpdateWorldAimVisual
        worldLine.widthMultiplier = worldLineWidth;

        if (hitDotPrefab && _hitDot == null)
        {
            _hitDot = Instantiate(hitDotPrefab);
            _hitDot.SetActive(false);
        }
        // Si ya tenemos player, parent aquí; si no, se hará al bind
        if (_player) worldLine.transform.SetParent(_player, false);

        var p = Vector3.zero;
        worldLine.SetPosition(0, p);
        worldLine.SetPosition(1, p);
        worldLine.enabled = false;
    }

    private void SetWorldLineActive(bool on)
    {
        if (!worldLine) return;
        worldLine.enabled = on;

        if (!on)
        {
            // colapsa para que no se vea ni un frame
            var p = worldLine.positionCount > 0 ? worldLine.GetPosition(0) : Vector3.zero;
            worldLine.SetPosition(0, p);
            worldLine.SetPosition(1, p);
        }

        if (_hitDot) _hitDot.SetActive(on);
    }

    private void UpdateWorldAimVisual(Vector3 worldDir)
    {
        if (!worldLine) return;

        // Origen: player + altura (compensa escala del player si cambió)
        float scaleY = (_player ? _player.lossyScale.y : 1f);
        Vector3 origin = (_player ? _player.position : Vector3.zero) + Vector3.up * (worldLineHeight * scaleY);

        // Ajusta grosor según escala Y del player (opcional, pero útil si reescalaste todo)
        worldLine.widthMultiplier = worldLineWidth * scaleY;

        if (_player == null || worldDir.sqrMagnitude < 1e-6f)
        {
            worldLine.SetPosition(0, origin);
            worldLine.SetPosition(1, origin);
            if (_hitDot) _hitDot.SetActive(false);
            return;
        }

        Vector3 end = origin + worldDir.normalized * worldLineMaxDistance;
        if (Physics.Raycast(origin, worldDir, out var hit, worldLineMaxDistance, worldLineMask, QueryTriggerInteraction.Ignore))
        {
            end = hit.point;
            if (_hitDot)
            {
                _hitDot.SetActive(true);
                _hitDot.transform.position = hit.point + hit.normal * 0.01f;
                _hitDot.transform.rotation = Quaternion.LookRotation(hit.normal);
            }
        }
        else if (_hitDot) _hitDot.SetActive(false);

        worldLine.SetPosition(0, origin);
        worldLine.SetPosition(1, end);
    }
}
