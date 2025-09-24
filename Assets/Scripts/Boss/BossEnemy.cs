using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : Enemy
{
    [Header("Boss Settings")]
    public float moveSpeed = 2f;
    public float attackRange = 0.1f;
    public float attackCooldown = 3f;
    private float lastAttackTime = 0f;

    private Transform player;
    private Animator animator;

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        canAct = false; // se activará desde CombatRoom
    }

    void Update()
    {
        if (!canAct || player == null || isDead) return;

        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        // Girar hacia el jugador (solo eje Y)
        Vector3 lookDir = new Vector3(direction.x, 0, direction.z).normalized;
        if (lookDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);

        // Movimiento
        if (distance > attackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            if (animator) animator.SetBool("Run", true);
        }
        else
        {
            if (animator) animator.SetBool("Run", false);

            // Ataque cuerpo a cuerpo
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    private void Attack()
    {
        if (animator) animator.SetTrigger("Attack");

        if (player.TryGetComponent<Health>(out var hp))
        {
            hp.ApplyDamage(1f); // daño ejemplo
        }
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;
        canAct = false;

        if (animator) animator.SetTrigger("Die");

        // Terminar juego
        //if (GameManager.Instance != null)
        //    GameManager.Instance.GameOver();

        Destroy(gameObject, 5f); // tiempo para animación de muerte
    }
}