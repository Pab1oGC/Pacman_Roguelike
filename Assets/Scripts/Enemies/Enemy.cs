using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : NetworkBehaviour
{
    [SyncVar] public float health = 3f;
    [SyncVar] public bool canAct = false;

    [Header("Daño por contacto")]
    [SerializeField] private bool dealContactDamage = true;
    [SerializeField] private float contactDamage = 1f;
    [SerializeField] private float contactCooldown = 0.5f;
    [SerializeField] private bool selfDestructOnHit = false;
    [SerializeField] private string playerTag = "Player";
    private float _lastContactTime = -999f;

    [Header("Knockback al Player")]
    [SerializeField] private bool doKnockback = true;
    [SerializeField] private float knockbackForce = 1f;
    [SerializeField] private float knockbackUpForce = 1f;
    [SerializeField] private float movementLock = 0.15f;

    [Header("Muerte & Loot")]
    [SerializeField] private GameObject deathVfxPrefab;
    [SerializeField] private float deathVfxLifetime = 2f;

    [SerializeField, Range(0f, 1f)] private float dropChance = 0.5f;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Vector2Int coinAmountRange = new Vector2Int(1, 3);
    [SerializeField] private float coinScatter = 0.25f;
    [SerializeField] private float coinSpawnForward = 0.6f;
    [SerializeField] private float coinSpawnUp = 0.1f;

    [SerializeField] private float destroyDelay = 0.1f;

    protected bool isDead = false;

    // --- API que llama CombatRoom ---
    [Server] public void EnableActServer() { canAct = true; }
    [Server] public void DisableActServer() { canAct = false; }

    // --- Daño / muerte (solo servidor) ---
    [Server]
    public void TakeDamage(float dmg)
    {
        if (isDead) return;
        health -= dmg;
        if (health <= 0f) DieServer();
    }

    [Server]
    protected virtual void DieServer()
    {
        if (isDead) return;
        isDead = true;
        canAct = false;

        // VFX (opcional: si quieres sincronizarlo, dale NetworkIdentity y Spawn)
        if (deathVfxPrefab)
        {
            var vfx = Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
            // Si quieres que lo vean los clientes, descomenta siguiente línea y asegúrate que el prefab es spawnable:
            // NetworkServer.Spawn(vfx);
            Destroy(vfx, deathVfxLifetime);
        }

        DropCoinsServer();

        // Destruye al enemigo para todos
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    private void DropCoinsServer()
    {
        if (!coinPrefab) return;
        if (Random.value > dropChance) return;

        Vector3 basePos = transform.position + transform.forward * coinSpawnForward + Vector3.up * coinSpawnUp;
        int amount = Random.Range(coinAmountRange.x, coinAmountRange.y + 1);

        for (int i = 0; i < amount; i++)
        {
            Vector2 circle = Random.insideUnitCircle * coinScatter;
            Vector3 spawnPos = basePos + new Vector3(circle.x, 0f, circle.y);

            var coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            NetworkServer.Spawn(coin);

            if (coin.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 dir = (spawnPos - basePos);
                dir.y = 0f;
                if (dir.sqrMagnitude < 1e-4f) dir = transform.forward;
                dir = dir.normalized + Vector3.up * 0.5f;
                rb.AddForce(dir * 2f, ForceMode.Impulse);
            }
        }
    }

    protected Transform GetClosestPlayerServer()
    {
        Transform best = null;
        float bestSqr = float.MaxValue;

        foreach (var kv in NetworkServer.connections)
        {
            var id = kv.Value?.identity;
            if (!id) continue;
            var t = id.transform;
            float d = (t.position - transform.position).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = t; }
        }
        return best;
    }

    // Daño de contacto (solo servidor)
    [ServerCallback] private void OnCollisionEnter(Collision other) => TryDealContactDamage(other.collider);
    [ServerCallback] private void OnTriggerEnter(Collider other) => TryDealContactDamage(other);

    [Server]
    private void TryDealContactDamage(Collider other)
    {
        if (!dealContactDamage || !enabled) return;
        if (!other || !other.CompareTag(playerTag)) return;
        if (Time.time - _lastContactTime < contactCooldown) return;

        if (other.TryGetComponent<Health>(out var hp))
        {
            hp.ApplyDamageServer(contactDamage);
            _lastContactTime = Time.time;

            if (doKnockback)
            {
                Vector3 dir = other.transform.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude < 1e-4f) dir = transform.forward;
                dir.Normalize();

                if (other.TryGetComponent<PlayerKnockback>(out var kb))
                {
                    kb.ApplyKnockback(dir, knockbackForce, knockbackUpForce, movementLock);
                }
                else if (other.attachedRigidbody != null)
                {
                    Vector3 impulse = dir * knockbackForce + Vector3.up * knockbackUpForce;
                    other.attachedRigidbody.AddForce(impulse, ForceMode.Impulse);
                }
            }

            if (selfDestructOnHit) DieServer();
        }
    }
}
