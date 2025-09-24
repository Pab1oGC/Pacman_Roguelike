using UnityEngine;
using UnityEngine.AI;

public class BossMovement
{
    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private float chaseRange;
    private float attackRange;

    public BossMovement(NavMeshAgent agent, Animator animator, Transform player, float chaseRange, float attackRange)
    {
        this.agent = agent;
        this.animator = animator;
        this.player = player;
        this.chaseRange = chaseRange;
        this.attackRange = attackRange;
    }

    public void HandleMovement()
    {
        if (!player) return;

        float distance = Vector3.Distance(agent.transform.position, player.position);

        if (distance <= attackRange)
        {
            agent.isStopped = true;
            animator.SetBool("isRunning", false);
        }
        else if (distance <= chaseRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool("isRunning", true);
        }
        else
        {
            agent.isStopped = true;
            animator.SetBool("isRunning", false);
        }
    }

    public void Stop()
    {
        agent.isStopped = true;
        animator.SetBool("isRunning", false);
    }
}
