using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KamikazeEnemy : Enemy
{
    public float speed = 3f;
    private Transform target;

    public override void OnStartServer()
    {
        base.OnStartServer();
        target = FindNearestPlayer();
    }

    [ServerCallback]
    private void Update()
    {
        if (!canAct || isDead || target == null) return;

        // perseguir
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        Vector3 dir = (target.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 1e-6f) transform.forward = dir.normalized;
    }

    // Util
    private Transform FindNearestPlayer()
    {
        float best = float.MaxValue; Transform bestT = null;
        foreach (var kv in NetworkServer.connections)
        {
            var id = kv.Value?.identity;
            if (!id) continue;
            float d = (id.transform.position - transform.position).sqrMagnitude;
            if (d < best) { best = d; bestT = id.transform; }
        }
        return bestT;
    }
}
