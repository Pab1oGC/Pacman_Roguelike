using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterEnemy : Enemy
{
    public GameObject bulletPrefab;      // prefab con NetworkIdentity
    public Transform firePoint;
    public float fireRate = 1f;
    private float fireCooldown = 0f;

    public float minDistance = 3f;
    public float moveSpeed = 1.5f;
    public float rotationSpeed = 2f;

    private Transform target;

    Animator _anim;
    NetworkAnimator _netAnim;
    float _cd;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _netAnim = GetComponent<NetworkAnimator>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        target = FindNearestPlayer();
    }

    [ServerCallback]
    void Update()
    {
        if (!isServer || !canAct) return;

        // 1) Buscar/encarar al jugador más cercano
        var target = GetClosestPlayerServer();
        if (target)
        {
            Vector3 dir = target.position - transform.position; dir.y = 0f;
            if (dir.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 4f);

            float dist = dir.magnitude;
            if (dist > minDistance)
                transform.position += dir.normalized * moveSpeed * Time.deltaTime;
        }

        // 2) Reloj de disparo y lanzar animación
        _cd -= Time.deltaTime;
        if (_cd <= 0f)
        {
            _cd = 1f / Mathf.Max(0.01f, fireRate);

            // Dispara el trigger EN EL SERVIDOR. NetworkAnimator lo replica a todos.
            /*if (_netAnim) _netAnim.SetTrigger(shootTrigger);
            else if (_anim) _anim.SetTrigger(shootTrigger);*/
        }
    }

    public void AnimEvent_Shoot()
    {
        if (!isServer) return;                    // seguridad: sólo server crea la bala
        if (!bulletPrefab || !firePoint) return;

        var go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Si tu Bullet tiene flags (ej. para diferenciar daño al jugador):
        var b = go.GetComponent<Bullet>();
        if (b) b.isBulletPlayer = false;          // bala enemiga

        NetworkServer.Spawn(go);                  // replica a todos los clientes
    }

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
