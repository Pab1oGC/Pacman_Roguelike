using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkBulletSpawner : NetworkBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject bulletPrefab;  // Debe tener NetworkIdentity y estar en Spawnable Prefabs
    [SerializeField] private Transform firePoint;

    [Header("Tuning")]
    [SerializeField] private float bulletSpeed = 12f;
    [SerializeField] private float lifeTimeBullet = 1f;

    // === Llamado por el Animation Event ===
    // En el Animator, pon el evento a este método (en lugar de AttackController.SpawnFireball)
    public void AnimEvent_SpawnFireball()
    {
        // ✅ Dedicated server: no hay cliente local que envíe Cmd, así que spawnea aquí
        if (isServer && !isClient)
        {
            ServerSpawn();
            return;
        }

        // ✅ Host o cliente normal: SOLO el jugador local manda el Cmd
        if (isLocalPlayer)
        {
            CmdSpawn();
        }

        // ❌ OJO: si estás en el host y este evento se ejecuta en la "copia server" del jugador remoto,
        // isServer==true pero isClient==true, por lo que NO entrará al ServerSpawn aquí.
        // Ese remoto ya habrá enviado su Cmd desde su propio dispositivo.
    }

    [Command]
    private void CmdSpawn()
    {
        ServerSpawn();
    }

    [Server]
    private void ServerSpawn()
    {
        if (!bulletPrefab || !firePoint) return;

        var go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Debug.Log($"[ServerSpawn] {name} fired on server t={Time.frameCount}");

        // Si tu Bullet usa timeToDestroy:
        var b = go.GetComponent<Bullet>();
        if (b) b.timeToDestroy = lifeTimeBullet;

        // Empuje opcional
        var rb = go.GetComponent<Rigidbody>();
        if (rb) rb.velocity = firePoint.forward * bulletSpeed;

        NetworkServer.Spawn(go);
    }
}
