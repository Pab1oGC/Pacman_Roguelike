using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(PlayerTargetProvider))]
[RequireComponent(typeof(BossAttack))]
public class BossBrain : MonoBehaviour
{
    [SerializeField] float detectionRange = 20f;
    [SerializeField] float turnSpeed = 540f;

    NavMeshAgent agent; EnemyHealth hp; PlayerTargetProvider tp; BossAttack atk;
    Animator anim; // del hijo

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        hp = GetComponent<EnemyHealth>();
        tp = GetComponent<PlayerTargetProvider>();
        atk = GetComponent<BossAttack>();
        anim = GetComponentInChildren<Animator>(); // importante
        if (anim) anim.applyRootMotion = false;
    }

    void OnEnable() { hp.OnDied.AddListener(OnDeath); }
    void OnDisable() { hp.OnDied.RemoveListener(OnDeath); }

    void Update()
    {
        if (hp.IsDead) return;

        // Actualiza parámetro Speed para Idle/Run
        if (anim) anim.SetFloat("Speed", agent ? agent.velocity.magnitude : 0f);

        var target = tp.GetTarget();
        if (!target) { agent.ResetPath(); return; }

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > detectionRange) { agent.ResetPath(); return; }

        if (dist > atk.AttackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }
        else
        {
            agent.isStopped = true;
            Face(target.position);
            if (atk.CanAttack) atk.TryAttack(target); // esto pone el Trigger "Attack"
        }
    }

    void Face(Vector3 pos)
    {
        var dir = pos - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        var look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);
    }

    void OnDeath()
    {
        if (anim)
        {
            anim.ResetTrigger("Die");
            anim.SetTrigger("Die"); // entra en Dying
        }
        if (agent) { agent.isStopped = true; agent.enabled = false; }
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        Destroy(gameObject, 4f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectionRange);
        var a = GetComponent<BossAttack>(); if (a) { Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, a.AttackRange); }
    }
}
