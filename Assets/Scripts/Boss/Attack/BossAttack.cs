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
    }
}
