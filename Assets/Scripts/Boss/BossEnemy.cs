using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : Enemy
{
    [Header("Boss Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1.6f;     // rango de golpe
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float meleeDamage = 1f;       // daño por golpe

    [Header("Animator Params")]
    [SerializeField] private string runBool = "Run";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string dieTrigger = "Die";

    // (opcional) si le pones NetworkHealth al propio boss, lo usamos para morir bonito
    [Header("Vida (opcional)")]
    [SerializeField] private Health bossHealth;
    [SerializeField] private float destroyAfterDeathSeconds = 5f;

    Animator _anim;
    NetworkAnimator _netAnim;
    float _lastAttackTime;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _netAnim = GetComponent<NetworkAnimator>();
        if (!bossHealth) bossHealth = GetComponent<Health>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        // El CombatRoom te pondrá canAct=true cuando toque
        if (bossHealth != null)
        {
            // Si tienes vida en el boss, escucha su muerte para animar y destruir
            bossHealth.OnDied += OnBossDied_ServerCallback;
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (bossHealth != null)
            bossHealth.OnDied -= OnBossDied_ServerCallback;
    }

    // === IA SOLO EN SERVIDOR ===
    [ServerCallback]
    void Update()
    {
        if (!isServer || !canAct) return;

        // 1) Objetivo: jugador más cercano
        var target = GetClosestPlayerServer();
        if (!target)
        {
            // nadie cerca -> idle
            SetRun(false);
            return;
        }

        // 2) Girar hacia el jugador (solo Y)
        Vector3 to = target.position - transform.position;
        to.y = 0f;
        float dist = to.magnitude;
        if (to.sqrMagnitude > 1e-6f)
        {
            var look = Quaternion.LookRotation(to, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 5f);
        }

        // 3) Mover o atacar
        if (dist > attackRange)
        {
            // caminar hacia el target
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            SetRun(true);
        }
        else
        {
            SetRun(false);

            if (Time.time - _lastAttackTime >= attackCooldown)
            {
                _lastAttackTime = Time.time;
                // Disparamos el trigger EN EL SERVER; NetworkAnimator lo replica a todos
                if (_netAnim) _netAnim.SetTrigger(attackTrigger);
                else if (_anim) _anim.SetTrigger(attackTrigger);
            }
        }
    }

    // === Animation Event ===
    // Pon un evento en el clip de ataque que llame EXACTAMENTE a "AnimEvent_DealHit".
    // Este método sólo hace daño en el SERVIDOR.
    public void AnimEvent_DealHit()
    {
        if (!isServer || meleeDamage <= 0f) return;

        // Golpea a cualquier Player dentro del rango (puedes afinar con un ángulo, etc.)
        // Aquí usamos un OverlapSphere sencillo alrededor del boss.
        const float hitRadius = 1.8f; // un poquito más que attackRange para tolerancia
        var hits = Physics.OverlapSphere(transform.position, hitRadius, ~0, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            if (!h.CompareTag("Player")) continue;

            // Aplicar daño al jugador golpeado
            if (h.TryGetComponent<Health>(out var hp))
            {
                hp.ApplyDamageServer(meleeDamage);
            }
        }
    }

    // === Muerte del Boss (si usas NetworkHealth en el boss) ===
    [Server]
    void OnBossDied_ServerCallback()
    {
        canAct = false; // ya no actúa

        // Trigger de muerte replicado
        if (_netAnim) _netAnim.SetTrigger(dieTrigger);
        else if (_anim) _anim.SetTrigger(dieTrigger);

        // Destruye el boss tras la animación
        Invoke(nameof(ServerDestroySelf), Mathf.Max(0.1f, destroyAfterDeathSeconds));
    }

    [Server]
    void ServerDestroySelf()
    {
        // Usa la utilidad del Enemy base si quieres VFX/loot; si no, destruye directo
        // DieServer(); // <- si tu Enemy.base ya hace VFX/loot
        NetworkServer.Destroy(gameObject);
    }

    // ===== utilidades Animator =====
    void SetRun(bool on)
    {
        // Cambia el bool en el server; NetworkAnimator replica el cambio a todos
        if (_anim) _anim.SetBool(runBool, on);
    }

#if UNITY_EDITOR
    // Gizmo para ver el rango de ataque
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, attackRange);
    }
#endif
}