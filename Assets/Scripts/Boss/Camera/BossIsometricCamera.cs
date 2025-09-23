using UnityEngine;
using UnityEngine.AI;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class BossIsometricCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;              // Asigna tu Player; si está vacío, se autodescubre
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.0f, 0f);

    [Header("Isométrica")]
    [SerializeField] private float yaw = 45f;               // giro horizontal (0–360)
    [SerializeField] private float pitch = 35f;             // inclinación (20–45 típico iso)

    [Header("Distancia / Zoom")]
    [SerializeField] private bool useOrthographic = true;   // isométrica real = ortho
    [SerializeField] private float distance = 8f;           // si usas perspectiva
    [SerializeField] private float orthographicSize = 6f;   // si usas ortho (más chico = más cerca)
    [SerializeField] private float fov = 50f;               // si usas perspectiva (más chico = “zoom in”)

    [Header("Seguimiento")]
    [SerializeField] private float followSmooth = 10f;      // suavizado (más alto = más responsive)
    [SerializeField] private bool lockExactRotation = true; // si false, mira al target con LookAt

    [Header("Anti-clipping (opcional)")]
    [SerializeField] private float collisionRadius = 0.25f; // radio del spherecast
    [SerializeField] private LayerMask collisionMask = ~0;  // capas a considerar para colisión
    [SerializeField] private float minDistance = 2.5f;      // distancia mínima al target

    private Camera cam;
    private Vector3 vel; // para SmoothDamp

    void Awake()
    {
        cam = GetComponent<Camera>();
        ConfigureProjection();

        // Si en esta escena quedó el otro script de cámara, lo apagamos
        var old = GetComponent<CameraController>();
        if (old) old.enabled = false;
    }

    void Start()
    {
        AutoFindTargetIfNull();
        // Posición inicial correcta
        if (Application.isPlaying) SnapNow();
    }

    void OnValidate()
    {
        if (!cam) cam = GetComponent<Camera>();
        ConfigureProjection();
    }

    void LateUpdate()
    {
        if (!target) { AutoFindTargetIfNull(); if (!target) return; }

        // Punto a seguir (centro del jugador + offset)
        Vector3 focus = target.position + targetOffset;

        // Rotación isométrica
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        // Offset según distancia (para perspectiva). En ortho no cambia el tamaño por distancia,
        // pero mantenemos un offset para el framing.
        Vector3 desiredOffset = rot * (Vector3.back * Mathf.Max(distance, minDistance));
        Vector3 desiredPos = focus + desiredOffset;

        // Anti-clipping sencillo: encoge la distancia si choca con paredes
        if (collisionRadius > 0f)
        {
            var dir = (desiredPos - focus);
            float d = dir.magnitude;
            if (d > 0.001f)
            {
                dir /= d;
                if (Physics.SphereCast(focus, collisionRadius, dir, out var hit, d, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    float safe = Mathf.Max(hit.distance - 0.1f, minDistance);
                    desiredPos = focus + dir * safe;
                }
            }
        }

        // Suavizado de posición
        float smoothTime = 1f / Mathf.Max(0.01f, followSmooth);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref vel, smoothTime);

        // Rotación
        if (lockExactRotation) transform.rotation = rot;
        else transform.rotation = Quaternion.LookRotation((focus - transform.position).normalized, Vector3.up);
    }

    // --------------------------------------------------------

    public void SetTarget(Transform t) => target = t;

    public void SetYawPitch(float newYaw, float newPitch)
    {
        yaw = newYaw; pitch = newPitch;
    }

    public void SetZoom(float newDistanceOrSize)
    {
        if (useOrthographic) orthographicSize = Mathf.Max(0.1f, newDistanceOrSize);
        else distance = Mathf.Max(minDistance, newDistanceOrSize);
        ConfigureProjection();
    }

    private void ConfigureProjection()
    {
        if (!cam) return;
        cam.orthographic = useOrthographic;
        if (cam.orthographic) cam.orthographicSize = Mathf.Max(0.1f, orthographicSize);
        else cam.fieldOfView = Mathf.Clamp(fov, 10f, 100f);
    }

    private void AutoFindTargetIfNull()
    {
        if (target) return;
        // Intenta PlayerContext, luego tag "Player"
        var pc = FindObjectOfType<PlayerContext>();
        if (pc) { target = pc.transform; return; }
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go) target = go.transform;
    }

    // Coloca inmediatamente la cámara en su lugar (sin suavizado)
    private void SnapNow()
    {
        if (!target) return;
        Vector3 focus = target.position + targetOffset;
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredOffset = rot * (Vector3.back * Mathf.Max(distance, minDistance));
        transform.position = focus + desiredOffset;
        transform.rotation = rot;
    }
}
