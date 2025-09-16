using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    [Header("Opcional: desactivar control durante el knockback")]
    [SerializeField] private MonoBehaviour[] movementToDisable; // ej: tu script "Movement"

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Aplica un impulso de knockback y (opcional) bloquea control por 'lockDuration' segundos.
    /// </summary>
    public void ApplyKnockback(Vector3 horizontalDir, float force, float upForce, float lockDuration = 0f)
    {
        horizontalDir.y = 0f;
        if (horizontalDir.sqrMagnitude > 0f) horizontalDir.Normalize();

        // Reducir un poco la Y previa para evitar “saltar” de más si ya ibas hacia arriba
        rb.velocity = new Vector3(rb.velocity.x, Mathf.Min(rb.velocity.y, 0f), rb.velocity.z);

        Vector3 impulse = horizontalDir * force + Vector3.up * upForce;
        rb.AddForce(impulse, ForceMode.Impulse);

        if (lockDuration > 0f && movementToDisable != null && movementToDisable.Length > 0)
            StartCoroutine(LockControl(lockDuration));
    }

    private IEnumerator LockControl(float t)
    {
        foreach (var m in movementToDisable) if (m) m.enabled = false;
        yield return new WaitForSeconds(t);
        foreach (var m in movementToDisable) if (m) m.enabled = true;
    }
}
