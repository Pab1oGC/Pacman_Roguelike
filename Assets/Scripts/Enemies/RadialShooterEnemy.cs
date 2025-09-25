using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialShooterEnemy : Enemy
{
    public GameObject bulletPrefab;        // prefab con NetworkIdentity
    public int bulletsPerWave = 8;
    public float fireRate = 2f;
    private float fireCooldown = 0f;

    [Header("Disparo")]
    [SerializeField] private Transform shootOrigin; // centro/pecho
    [SerializeField] private float spawnDistance = 0.9f;
    [SerializeField] private float shootHeightOffset = 0f;

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    [ServerCallback]
    private void Update()
    {
        if (!canAct || isDead) return;

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            ServerShootRadial();
            fireCooldown = 1f / Mathf.Max(0.01f, fireRate);
        }
    }

    [Server]
    private void ServerShootRadial()
    {
        if (!bulletPrefab || !shootOrigin) return;

        float step = 360f / Mathf.Max(1, bulletsPerWave);
        Quaternion baseRot = shootOrigin.rotation;
        Vector3 basePos = shootOrigin.position + Vector3.up * shootHeightOffset;

        for (int i = 0; i < bulletsPerWave; i++)
        {
            Quaternion rot = baseRot * Quaternion.Euler(0f, i * step, 0f);
            Vector3 dir = (rot * Vector3.forward);
            Vector3 pos = basePos + dir * spawnDistance;

            var go = Object.Instantiate(bulletPrefab, pos, rot);
            NetworkServer.Spawn(go);
        }
    }
}
