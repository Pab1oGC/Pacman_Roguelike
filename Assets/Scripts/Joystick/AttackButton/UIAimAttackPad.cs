using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIAimAttackPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Refs")]
    [SerializeField] private RectTransform padArea;        // este mismo RectTransform si lo dejas vacío
    [SerializeField] private RectTransform arrowUI;        // flecha UI (pivot en (0.5,0), apunta hacia arriba)
    [SerializeField] private RectTransform knob;                   // opcional: puntito que sigue el dedo

    [Header("Ajustes de input")]
    [SerializeField] private float maxRadius = 140f;       // px de arrastre máximo
    [SerializeField] private float deadZone = 12f;         // px mínimos para considerar dirección
    [SerializeField] private bool requireHoldToShowArrow = false;

    [Header("Flecha sobre el Player (opcional)")]
    [SerializeField] private RectTransform playerArrow;    // UI Image encima del player (en Canvas overlay)
    [SerializeField] private float playerArrowLenMin = 60f;
    [SerializeField] private float playerArrowLenMax = 180f;

    [Header("Aim en mundo (LineRenderer)")]
    [SerializeField] private LineRenderer worldLine;     // opcional; si está vacío lo creo en runtime
    [SerializeField] private float worldLineMaxDistance = 12f;
    [SerializeField] private LayerMask worldLineMask = ~0; // todo por defecto
    [SerializeField] private float worldLineHeight = 1.0f; // altura del origen (y) sobre el player
    [SerializeField] private Gradient worldLineColor;       // opcional: colores/alpha de la línea
    [SerializeField] private float worldLineWidth = 0.06f;  // grosor
    [SerializeField] private GameObject hitDotPrefab;

    [Header("Player binding")]
    [SerializeField] private string playerTag = "Player";

    public Camera _cam;
    private RectTransform _rt;
    private Vector2 _pressPos;
    private int _activePointerId = -100;
    private bool _aiming;
    private Vector2 _aimDirUI;            // dirección 2D en espacio UI (eje Y = arriba en pantalla)
    private AimAttackRelay _relay;        // en el Player
    private Transform _player;            // world
    private bool _hadDirection;
    private Vector2 _knobHome;

    private void Awake()
    {
        _rt = padArea ? padArea : (RectTransform)transform;
        _cam = Camera.main;
        StartCoroutine(WaitAndBindPlayer());
        ShowArrow(false);
        if (knob) _knobHome = knob.anchoredPosition;
        if (playerArrow) playerArrow.gameObject.SetActive(false);
    }

    private IEnumerator WaitAndBindPlayer()
    {
        GameObject go = null;
        while (go == null)
        {
            go = GameObject.FindGameObjectWithTag(playerTag);
            yield return null;
        }
        _player = go.transform;
        _relay = go.GetComponentInChildren<AimAttackRelay>();
        if (_relay == null)
        {
            // Si no existe, lo añadimos para no romper flujo
            _relay = go.AddComponent<AimAttackRelay>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_activePointerId != -100) return; // ya hay otro dedo activo en esta UI
        _activePointerId = eventData.pointerId;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, eventData.position, _cam, out _pressPos);
        _aiming = true;
        _hadDirection = false;

        if (!requireHoldToShowArrow) ShowArrow(true);
        if (knob) { knob.anchoredPosition = _pressPos; }
        if (playerArrow) playerArrow.gameObject.SetActive(true);
        SetWorldLineActive(true);
        UpdateWorldAimVisual(Vector3.zero);
        UpdateVisuals(Vector2.zero);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != _activePointerId || !_aiming) return;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, eventData.position, _cam, out localPos);

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
        Vector2 dir = delta / mag; // normaliza
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
                (RectTransform)playerArrow.parent, screen, _cam, out var local);

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
            // parented al player si ya lo tenemos; si no, quedará suelto y lo moveremos cada frame
            if (_player) go.transform.SetParent(_player, false);
        }

        worldLine.useWorldSpace = true;
        worldLine.positionCount = 2;
        worldLine.widthMultiplier = worldLineWidth;
        worldLine.numCornerVertices = 2;
        worldLine.numCapVertices = 2;
        worldLine.alignment = LineAlignment.View;

        // Material y color
        if (worldLine.material == null)
        {
            // material simple sin iluminación
            worldLine.material = new Material(Shader.Find("Sprites/Default"));
            worldLine.material.renderQueue = 3000; // transparente
        }
        if (worldLineColor != null)
            worldLine.colorGradient = worldLineColor;
        else
            worldLine.startColor = worldLine.endColor = new Color(1f, 1f, 1f, 0.9f);
    }

    private void SetWorldLineActive(bool on)
    {
        if (worldLine) worldLine.enabled = on;

        if (hitDotPrefab)
        {
            if (on)
            {
                if (!_hitDot && hitDotPrefab) _hitDot = Instantiate(hitDotPrefab);
                if (_hitDot) _hitDot.SetActive(true);
            }
            else
            {
                if (_hitDot) _hitDot.SetActive(false);
            }
        }
    }
    private GameObject _hitDot;

    private void UpdateWorldAimVisual(Vector3 worldDir)
    {
        if (!worldLine) return;

        // Si no hay player o no hay dirección, colapsa la línea en el origen
        Vector3 origin = (_player ? _player.position : Vector3.zero) + Vector3.up * worldLineHeight;

        if (_player == null || worldDir.sqrMagnitude < 1e-6f)
        {
            worldLine.SetPosition(0, origin);
            worldLine.SetPosition(1, origin);
            if (_hitDot) _hitDot.SetActive(false);
            return;
        }

        // Raycast para fin de línea
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
        else if (_hitDot)
        {
            _hitDot.SetActive(false);
        }

        worldLine.SetPosition(0, origin);
        worldLine.SetPosition(1, end);
    }
}
