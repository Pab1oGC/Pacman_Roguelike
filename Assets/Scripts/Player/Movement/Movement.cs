using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Movement : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DPadInputSource dpadOverride;
    [SerializeField] private SimpleJoystick joystick;  // auto-resolve si null
    [SerializeField] private Rigidbody rb;             // auto-resolve si null
    [SerializeField] private Camera arCamera;          // usa Camera.main si null
    [SerializeField] private AttackController attackController;

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

    public Animator animator;

    // Estado
    private Vector2 _input;
    public Vector2 LatestInput => _input; // <- Dash leerá esto

    // ---- Inyección opcional desde código ----
    public void SetSpeedProvider(ISpeedProvider sp) => _speed = sp;
    public void SetDashController(IDashController dc) => _dash = dc;

    private bool _movementLocked = false;
    private Coroutine _unlockCoro;

    public bool IsMovementLocked => _movementLocked;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!arCamera) arCamera = Camera.main;
        

        // --- INPUT SOURCE: D-PAD PRIMERO ---
        joystick = null
                   ?? GetComponentInChildren<SimpleJoystick>(true)
#if UNITY_2022_2_OR_NEWER
               ?? FindFirstObjectByType<SimpleJoystick>(FindObjectsInactive.Include);
#else
                   ?? FindObjectOfType<SimpleJoystick>(true);
#endif

        /*if (dpad != null)
        {
            _inputSource = dpad;              // <- usa el D-pad
            //Debug.Log("[Movement] Input = DPad");
        }*/
        if (joystick != null)
        {
            _inputSource = new JoystickInputSource(joystick);
            //Debug.Log("[Movement] Input = Joystick");
        }
        else
        {
            _inputSource = new StaticZeroInput();
            //Debug.LogWarning("[Movement] Sin input source");
        }

        // resto igual...
        _dirProvider = new CameraRelativeProvider(arCamera);
        _rotationPolicy = UseCfgFaceInstantly() ? (IRotationPolicy)new InstantRotationPolicy()
                                               : new SmoothRotationPolicy();
        _motor = new RigidbodyMotor();
        _speed = speedSource as ISpeedProvider ?? new StaticSpeedProvider(UseCfgMoveSpeed());
        _dash = dashSource as IDashController;
    }

    private sealed class StaticZeroInput : IMoveInputSource { public Vector2 GetMoveInput() => Vector2.zero; }

    private void Update()
    {
        _input = _inputSource?.GetMoveInput() ?? Vector2.zero;

        // Si el flag cambia en runtime, refresca la política sin allocs
        _rotationPolicy = UseCfgFaceInstantly() ? (IRotationPolicy)new InstantRotationPolicy()
                                               : new SmoothRotationPolicy();
    }

    private void FixedUpdate()
    {

        if (_movementLocked)
        {
            if (animator) animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
            return; // no movemos ni rotamos: deja que la física haga el knockback
        }

        if (attackController != null && attackController.IsAttacking)
        {
            // No mover mientras ataca
            animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
            return;
        }

        float baseSpeed = _speed?.CurrentSpeed ?? UseCfgMoveSpeed();
        float speed = baseSpeed * Mathf.Clamp01(_input.magnitude);
        animator.SetFloat("Speed", speed * 2.5f, 0.1f, Time.deltaTime);

        if (_dash != null && (_dash.IsDashing || _dash.IsOnCooldown && _input.sqrMagnitude < 1e-6f))
            return;

        if (_input.sqrMagnitude < 1e-6f) return;

        Vector3 worldDir = _dirProvider.ResolveDirection(_input);
        if (worldDir == Vector3.zero) return;

        float dt = Time.fixedDeltaTime;
        Vector3 moveStep = worldDir.normalized * speed * dt;
        rb.MovePosition(rb.position + moveStep);

        Quaternion targetRot = Quaternion.LookRotation(worldDir, Vector3.up);
        Quaternion newRot = Quaternion.RotateTowards(rb.rotation, targetRot, UseCfgTurnSpeed() * dt);
        rb.MoveRotation(newRot);

        if (transform.childCount > 0)
        {
            Rigidbody childRb = transform.GetChild(0).GetComponent<Rigidbody>();
            if (childRb != null)
            {
                childRb.MovePosition(rb.position);
                childRb.MoveRotation(newRot);
            }
        }
    }

    public void SetMovementLocked(bool locked)
    {
        _movementLocked = locked;
        if (locked && animator) animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
    }

    public void LockMovementFor(float seconds)
    {
        SetMovementLocked(true);
        if (_unlockCoro != null) StopCoroutine(_unlockCoro);
        _unlockCoro = StartCoroutine(UnlockAfter(seconds));
    }

    private IEnumerator UnlockAfter(float t)
    {
        yield return new WaitForSeconds(t);
        SetMovementLocked(false);
        _unlockCoro = null;
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
    public void SetInputSource(IMoveInputSource src) => _inputSource = src;

    public void IncrementSpeed(float amount)
    {
        config.moveSpeed += amount;
    }

    public void DecrementSpeed(float amount) { if (config.moveSpeed <= 1) return; config.moveSpeed -= amount; }
}
