using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public float health = 3f;
    public bool canAct = false;

    [Header("Daño por contacto")]
    [SerializeField] private bool dealContactDamage = true;
    [SerializeField] private float contactDamage = 1f;
    [SerializeField] private float contactCooldown = 0.5f;
    [SerializeField] private bool selfDestructOnHit = false;
    [SerializeField] private string playerTag = "Player";
    private float _lastContactTime = -999f;

    [Header("Knockback al Player")]
    [SerializeField] private bool doKnockback = true;
    [SerializeField] private float knockbackForce = 1f;   // fuerza horizontal
    [SerializeField] private float knockbackUpForce = 1f; // empujón vertical
    [SerializeField] private float movementLock = 0.15f;

    [Header("Muerte & Loot")]
    [SerializeField] private GameObject deathVfxPrefab;
    [SerializeField] private float deathVfxLifetime = 2f;

    [SerializeField, Range(0f, 1f)] private float dropChance = 0.5f;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Vector2Int coinAmountRange = new Vector2Int(1, 3);
    [SerializeField] private float scatterRadius = 0.5f;
    [SerializeField] private float scatterImpulse = 2f;

    [SerializeField] private float destroyDelay = 0.1f;

    [SerializeField] private float coinSpawnForward = 0.6f; // distancia adelante del enemigo
    [SerializeField] private float coinSpawnUp = 0.1f;      // un pelín arriba
    [SerializeField] private float coinScatter = 0.25f;

    protected bool isDead = false;



    protected virtual void Start()
    {

    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        canAct = false;

        // 1) VFX de muerte
        if (deathVfxPrefab)
        {
            var vfx = Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
            if (deathVfxLifetime > 0f) Destroy(vfx, deathVfxLifetime);
        }

        // 2) Drop de monedas
        DropCoins();

        Destroy(gameObject, destroyDelay);
    }

    private void DropCoins()
    {
        if (!coinPrefab) return;
        if (Random.value > dropChance) return;

        // Punto base: frente al enemigo + un pelín arriba
        Vector3 basePos = transform.position
                        + transform.forward * coinSpawnForward
                        + Vector3.up * coinSpawnUp;

        int amount = Random.Range(coinAmountRange.x, coinAmountRange.y + 1);
        for (int i = 0; i < amount; i++)
        {
            // Pequeña dispersión plana alrededor del punto base
            Vector2 circle = Random.insideUnitCircle * coinScatter;
            Vector3 spawnPos = basePos + new Vector3(circle.x, 0f, circle.y);

            var coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);

            if (coin.TryGetComponent<Rigidbody>(out var rb))
            {
                // Impulso radial + un poquito hacia arriba
                Vector3 dir = (spawnPos - basePos);
                dir.y = 0f;
                if (dir.sqrMagnitude < 1e-4f) dir = transform.forward; // por si sale en el centro
                dir = dir.normalized + Vector3.up * 0.5f;
                rb.AddForce(dir * scatterImpulse, ForceMode.Impulse);
            }
        }
    }

    private Vector3 GetDeathSpawnPoint()
    {
        // Tomamos el centro real (con escala aplicada)
        Vector3 center = transform.position;
        if (TryGetComponent<Collider>(out var c)) center = c.bounds.center;
        else if (TryGetComponent<Renderer>(out var r)) center = r.bounds.center;

        // Intento A: raycast hacia abajo (si el piso tiene collider)
        Vector3 origin = center + Vector3.up * 2f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 50f, ~0, QueryTriggerInteraction.Ignore))
            return hit.point + Vector3.up * 0.06f;

        // Intento C: base del propio bounds (por si acaso)
        if (TryGetComponent<Collider>(out var col2))
            return new Vector3(center.x, col2.bounds.min.y + 0.06f, center.z);
        if (TryGetComponent<Renderer>(out var r2))
            return new Vector3(center.x, r2.bounds.min.y + 0.06f, center.z);

        return transform.position;
    }



    private void OnCollisionEnter(Collision other)
    {
        TryDealContactDamage(other.collider);
    }

    // Triggers
    private void OnTriggerEnter(Collider other)
    {
        TryDealContactDamage(other);
    }

    private void TryDealContactDamage(Collider other)
    {
        if (!dealContactDamage) return;
        if (!enabled) return;
        if (!other.CompareTag(playerTag)) return;
        if (Time.time - _lastContactTime < contactCooldown) return;

        // 1) Daño
        if (other.TryGetComponent<Health>(out var hp))
        {
            hp.ApplyDamage(contactDamage);
            _lastContactTime = Time.time;

            // 2) Knockback
            if (doKnockback)
            {
                // dirección desde el enemigo hacia el jugador (solo plano XZ)
                Vector3 dir = other.transform.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.0001f) dir = transform.forward; // por si están superpuestos
                dir.Normalize();

                if (other.TryGetComponent<PlayerKnockback>(out var kb))
                {
                    kb.ApplyKnockback(dir, knockbackForce, knockbackUpForce, movementLock);
                }
                else if (other.attachedRigidbody != null)
                {
                    // Fallback directo por física
                    Vector3 impulse = dir * knockbackForce + Vector3.up * knockbackUpForce;
                    other.attachedRigidbody.AddForce(impulse, ForceMode.Impulse);
                }
            }

            // 3) Kamikaze / auto-destrucción
            if (selfDestructOnHit)
                Die(); // respeta VFX y drop
        }
    }
}
