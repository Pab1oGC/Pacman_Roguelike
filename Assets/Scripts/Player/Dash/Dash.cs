using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : MonoBehaviour, IDashController
{
    [Header("Refs")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Invulnerability invulnerability;
    [SerializeField] private DashConfig config;

    [Header("Opcional (para leer input de dir)")]
    [SerializeField] private Movement movementController; // si lo tienes

    public bool IsDashing { get; private set; }
    public bool IsOnCooldown => _cooldownTimer > 0f;

    // Dependencias (DIP)
    IDashDirectionProvider _dirProvider;
    IDashMotor _motor;

    float _cooldownTimer;

    public void SetDependencies(IDashDirectionProvider dirProvider, IDashMotor motor)
    {
        _dirProvider = dirProvider ?? _dirProvider;
        _motor = motor ?? _motor;
    }

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!invulnerability) invulnerability = GetComponent<Invulnerability>();
        if (!config)
        {
            // fallback sencillo
            config = ScriptableObject.CreateInstance<DashConfig>();
        }

        // Dirección: usa input del MovementController si se asigna
        System.Func<Vector2> getter = (movementController != null)
            ? (System.Func<Vector2>)(() => movementController.LatestInput) // expón LatestInput en tu MovementController (Vector2)
            : null;

        _dirProvider = new DefaultDashDirectionProvider(transform, getter);
        _motor = new RigidbodyDashMotor();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) TryDash();
        if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
    }

    /// <summary>Llama desde tu UI/Button o input: intenta iniciar dash.</summary>
    public void TryDash()
    {
        if (IsDashing || IsOnCooldown || !rb) return;
        StartCoroutine(CoDash());
    }

    IEnumerator CoDash()
    {
        IsDashing = true;
        _cooldownTimer = config.cooldown;

        Vector3 dir = _dirProvider.ResolveDashDirection();
        float totalDist = Mathf.Max(0.01f, config.distance);
        float dur = Mathf.Max(0.01f, config.duration);

        // I-frames
        if (invulnerability && config.iFrameDuration > 0f)
            invulnerability.SetInvulnerable(config.iFrameDuration);

        // Desplazamiento por curva
        float t = 0f;
        Vector3 lastPos = rb.position;
        while (t < dur)
        {
            float norm = t / dur;
            float speedFactor = config.speedCurve.Evaluate(norm); // metros/seg relativo
            float stepDist = (totalDist / dur) * speedFactor * Time.deltaTime;

            Vector3 delta = dir * stepDist;
            _motor.Move(rb, delta);

            t += Time.deltaTime;
            lastPos = rb.position;
            yield return null;
        }

        // Asegurar distancia total (pequeño “snap” final)
        Vector3 finalPos = lastPos + dir * Mathf.Max(0f, totalDist - Vector3.Distance(lastPos, rb.position));
        rb.MovePosition(finalPos);

        IsDashing = false;
    }

    public float CooldownRemaining => Mathf.Max(0f, _cooldownTimer);
    public float CooldownDuration => config ? Mathf.Max(0f, config.cooldown) : 0f;


}
