<<<<<<< Updated upstream
﻿using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BossAttack : MonoBehaviour
{
    [SerializeField] int damage = 15;
    [SerializeField] float attackRange = 2.0f;
    [SerializeField] float cooldown = 1.0f;

    private bool _canAttack = true;
    private Transform _currentTarget;   // 👈 EL MISMO NOMBRE EN TODO EL SCRIPT
    private Animator _anim;             // Animator del hijo "Model"

    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
        if (!_anim) Debug.LogError("[BossAttack] No se encontró Animator en hijos.");
        else _anim.applyRootMotion = false;
    }

    public float AttackRange => attackRange;
    public bool CanAttack => _canAttack;

    // Llamado por BossBrain cuando está en rango
    public void TryAttack(Transform target)
    {
        if (!_canAttack || !target) return;

        _currentTarget = target;   // 👈 guardamos el objetivo actual

        if (_anim)
        {
            _anim.ResetTrigger("Attack");
            _anim.SetTrigger("Attack");
        }

        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        _canAttack = false;
        yield return new WaitForSeconds(cooldown);
        _canAttack = true;
    }

    // Llamado por AnimationEventRelay (en el hijo con Animator)
    public void AE_DealDamage()
    {
        if (_currentTarget == null) return;

        // Por si el evento cae tarde: no pegues si ya está lejos
        if (Vector3.Distance(transform.position, _currentTarget.position) > attackRange + 0.3f)
            return;

        // 1) Tu Player usa Health.ApplyDamage(DamageInfo)
        var health = _currentTarget.GetComponentInParent<Health>();
        if (health != null)
        {
            // Asegúrate de tener el using/namespace correcto de DamageInfo si no compila
            var info = new DamageInfo(damage, this, _currentTarget.position);
            health.ApplyDamage(info);
            return;
        }

        // 2) Alternativa: contrato genérico
        var dmg = _currentTarget.GetComponentInParent<IDamageable>();
        if (dmg != null) { dmg.ApplyDamage(damage); }
=======
using UnityEngine;

public class BossAttack
{
    private Animator animator;
    private Transform player;
    private float attackRange;
    private float cooldown;
    private float lastAttackTime = 0f;

    public BossAttack(Animator animator, Transform player, float attackRange, float cooldown)
    {
        this.animator = animator;
        this.player = player;
        this.attackRange = attackRange;
        this.cooldown = cooldown;
    }

    public void HandleAttack()
    {
        if (!player) return;

        float distance = Vector3.Distance(animator.transform.position, player.position);
        if (distance <= attackRange && Time.time > lastAttackTime + cooldown)
        {
            animator.SetTrigger("Attack");
            lastAttackTime = Time.time;
        }
    }

    // Se llama desde un Animation Event en el frame del golpe
    public void DealDamage(float damage)
    {
        if (!player) return;

        float distance = Vector3.Distance(animator.transform.position, player.position);
        if (distance <= attackRange && player.TryGetComponent<Health>(out var hp))
        {
            hp.ApplyDamage(damage);
        }
>>>>>>> Stashed changes
    }
}
