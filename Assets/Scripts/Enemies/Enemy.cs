using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private bool isDead = false;


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

        int amount = Random.Range(coinAmountRange.x, coinAmountRange.y + 1);
        for (int i = 0; i < amount; i++)
        {
            var offset = Random.insideUnitSphere * scatterRadius;
            offset.y = Mathf.Abs(offset.y); // evita aparecer bajo el piso

            var coin = Instantiate(coinPrefab, transform.position + offset, Quaternion.identity);

            if (coin.TryGetComponent<Rigidbody>(out var rb))
            {
                var dir = (coin.transform.position - transform.position).normalized + Vector3.up * 0.5f;
                rb.AddForce(dir * scatterImpulse, ForceMode.Impulse);
            }
        }
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
