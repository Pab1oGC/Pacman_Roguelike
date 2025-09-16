using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    [Header("Opcional: desactivar control durante el knockback")]
    [SerializeField] private MonoBehaviour[] movementToDisable; // ej: tu script "Movement"

    public Rigidbody rb;

    [Header("Dirección")]
    [Tooltip("Si está activo, ignora la dirección que envía el enemigo y empuja hacia -player.forward.")]
    [SerializeField] private bool forceBackwardsFromFacing = true;

    [Header("Tuning")]
    [SerializeField] private bool useVelocityChange = true;   // más predecible que Impulse
    [SerializeField] private float maxHorizontalSpeed = 5f;   // tope visual del deslizamiento
    [SerializeField] private float maxVerticalSpeed = 3f;
    [SerializeField] private float damping = 10f;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        // Suele ayudar en top-down:
        rb.freezeRotation = true; // o congela X/Z en Constraints desde el inspector
    }

    /// <summary>
    /// Aplica un impulso de knockback y bloquea el control por 'lockDuration' segundos.
    /// </summary>
    public void ApplyKnockback(Vector3 horizontalDir, float force, float upForce, float lockDuration = 0f)
    {
        // 1) Dirección final
        Vector3 dir = horizontalDir;
        if (forceBackwardsFromFacing)
        {
            dir = -transform.forward; // empuja "para atrás" del player
        }
        dir.y = 0f;
        if (dir.sqrMagnitude > 0f) dir.Normalize();

        // 2) Preparar estado: corta subida previa para que no acumule saltos
        rb.velocity = new Vector3(rb.velocity.x, Mathf.Min(rb.velocity.y, 0f), rb.velocity.z);

        // 3) Aplicar impulso
        Vector3 deltaV = dir * Mathf.Max(0f, force) + Vector3.up * Mathf.Max(0f, upForce);
        if (useVelocityChange) 
            rb.AddForce(deltaV, ForceMode.VelocityChange);
        else 
            rb.AddForce(deltaV, ForceMode.Impulse);

        // 4) Clamp inmediato (tope de seguridad)
        ClampVelocity();


        // 6) Bloquear control + amortiguar el deslizamiento para que se “vea” el empujón
        if (lockDuration > 0f)
        {
            if (TryGetComponent<Movement>(out var mv))
                mv.LockMovementFor(lockDuration);

            StopAllCoroutines();
            StartCoroutine(DampenWhileLocked(lockDuration));
        }
    }

    private IEnumerator DampenWhileLocked(float duration)
    {
        float t = 0f;
        // amortiguación exponencial por FixedUpdate
        while (t < duration)
        {
            yield return new WaitForFixedUpdate();
            t += Time.fixedDeltaTime;

            // amortiguar sólo el plano XZ (que se vea el empujón pero se frene)
            Vector3 v = rb.velocity;
            Vector2 vXZ = new Vector2(v.x, v.z);
            float k = Mathf.Exp(-damping * Time.fixedDeltaTime); // 0..1
            vXZ *= k;

            // mantener un mínimo para que no se corte “en seco”
            if (vXZ.magnitude < 0.05f) vXZ = Vector2.zero;

            rb.velocity = new Vector3(vXZ.x, Mathf.Clamp(v.y, -maxVerticalSpeed, maxVerticalSpeed), vXZ.y);
            ClampVelocity(); // por si algo se disparó
        }
    }

    private void ClampVelocity()
    {
        Vector3 v = rb.velocity;
        Vector2 vXZ = new Vector2(v.x, v.z);
        if (vXZ.magnitude > maxHorizontalSpeed)
            vXZ = vXZ.normalized * maxHorizontalSpeed;

        float vy = Mathf.Clamp(v.y, -maxVerticalSpeed, maxVerticalSpeed);
        rb.velocity = new Vector3(vXZ.x, vy, vXZ.y);
    }
}
