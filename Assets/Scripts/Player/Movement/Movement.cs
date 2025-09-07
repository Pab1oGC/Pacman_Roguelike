using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SimpleJoystick joystick;  // auto-resolve si null
    [SerializeField] private Rigidbody rb;             // auto-resolve si null
    [SerializeField] private Camera arCamera;          // usa Camera.main si null

    [Header("Tuning (fallback si no hay ScriptableObject)")]
    [SerializeField, Min(0f)] private float moveSpeed = 0.5f;
    [SerializeField, Min(0f)] private float turnSpeed = 540f;
    [SerializeField] private bool faceInstantly = false;
    [SerializeField] private MovementConfig config;    // opcional

    [Header("Integraciones")]
    [Tooltip("Componente que implementa ISpeedProvider (ej: SpeedComponent)")]
    [SerializeField] private MonoBehaviour speedSource;
    [Tooltip("Componente que implementa IDashController (ej: DashComponent)")]
    [SerializeField] private MonoBehaviour dashSource;

    // Dependencias (DIP)
    private IMoveInputSource _inputSource;
    private IDirectionProvider _dirProvider;
    private IRotationPolicy _rotationPolicy;
    private IPhysicsMotor _motor;
    private ISpeedProvider _speed;
    private IDashController _dash;

    // Estado
    private Vector2 _input;
    public Vector2 LatestInput => _input; // <- Dash leerá esto

    // ---- Inyección opcional desde código ----
    public void SetSpeedProvider(ISpeedProvider sp) => _speed = sp;
    public void SetDashController(IDashController dc) => _dash = dc;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!arCamera) arCamera = Camera.main;

        if (!joystick)
        {
#if UNITY_2022_2_OR_NEWER
            joystick = FindFirstObjectByType<SimpleJoystick>(FindObjectsInactive.Include);
#else
            joystick = FindObjectOfType<SimpleJoystick>(true);
#endif
        }

        if (rb) rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Dependencias por defecto
        _inputSource = new JoystickInputSource(joystick);
        _dirProvider = new CameraRelativeProvider(arCamera);
        _rotationPolicy = UseCfgFaceInstantly() ? (IRotationPolicy)new InstantRotationPolicy()
                                                : (IRotationPolicy)new SmoothRotationPolicy();
        _motor = new RigidbodyMotor();

        // Speed Provider
        _speed = speedSource as ISpeedProvider ?? new StaticSpeedProvider(UseCfgMoveSpeed());

        // Dash Controller
        _dash = dashSource as IDashController; // puede quedar null si aún no lo agregaste
    }

    private void Update()
    {
        _input = _inputSource?.GetMoveInput() ?? Vector2.zero;

        // Si el flag cambia en runtime, refresca la política sin allocs
        _rotationPolicy = UseCfgFaceInstantly() ? (IRotationPolicy)new InstantRotationPolicy()
                                               : new SmoothRotationPolicy();
    }

    private void FixedUpdate()
    {
        if (!rb) return;

        // Si está dashing, no aplicar locomoción normal
        if (_dash != null && (_dash.IsDashing || _dash.IsOnCooldown && _input.sqrMagnitude < 1e-6f))
            return;

        if (_input.sqrMagnitude < 1e-6f) return;

        Vector3 worldDir = _dirProvider.ResolveDirection(_input);
        if (worldDir == Vector3.zero) return;

        float dt = Time.fixedDeltaTime;
        float baseSpeed = _speed?.CurrentSpeed ?? UseCfgMoveSpeed();
        float speed = baseSpeed * Mathf.Clamp01(_input.magnitude);

        Quaternion newRot = _rotationPolicy.Compute(rb.rotation, worldDir, UseCfgTurnSpeed(), dt);
        _motor.Move(rb, newRot, speed, dt);
    }

    private float UseCfgMoveSpeed() => config ? config.moveSpeed : moveSpeed;
    private float UseCfgTurnSpeed() => config ? config.turnSpeed : turnSpeed;
    private bool UseCfgFaceInstantly() => config ? config.faceInstantly : faceInstantly;

    // ---- Implementaciones auxiliares ----
    private sealed class StaticSpeedProvider : ISpeedProvider
    {
        public float CurrentSpeed { get; private set; }
        public StaticSpeedProvider(float v) { CurrentSpeed = Mathf.Max(0f, v); }
    }
}
