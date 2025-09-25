using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [Header("Tuning")]
    public float timeToDestroy = 1f;
    public float damage = 1f;
    public float speed = 0.24f;               // era tu 0.24f en Translate

    [Header("Ownership")]
    [SyncVar] public bool isBulletPlayer;     // quién disparó (para lógica de daño)
    [SyncVar] public NetworkIdentity owner;   // opcional: para ignorar al tirador

    Rigidbody rb;
    Collider col;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    // VIDA y MOVIMIENTO se controlan en el SERVIDOR
    public override void OnStartServer()
    {
        // vida autoritativa
        StartCoroutine(Life());

        // si tienes Rigidbody, úsalo; si no, movemos por Translate en Update server
        if (rb)
        {
            rb.useGravity = false; // quítalo si quieres caída
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.velocity = transform.forward * speed; // velocidad constante
        }
    }

    IEnumerator Life()
    {
        yield return new WaitForSeconds(timeToDestroy);
        if (isServer) NetworkServer.Destroy(gameObject);
    }

    void Update()
    {
        // si NO hay rigidbody, movemos vía Translate pero SOLO en server
        if (isServer && !rb)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        }
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        // Evita pegarle al dueño (si lo seteaste en el spawner)
        if (owner)
        {
            var otherId = other.GetComponentInParent<NetworkIdentity>();
            if (otherId && otherId == owner) return;
        }

        // Lógica de daño (server)
        if (other.CompareTag("Enemy") && isBulletPlayer)
        {
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null) enemy.TakeDamage(damage);
            NetworkServer.Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Player") && !isBulletPlayer)
        {
            var hp = other.GetComponent<Health>();
            if (hp != null) hp.ApplyDamageServer(damage);
            NetworkServer.Destroy(gameObject);
            return;
        }

        // Pared u otros
        if (other.CompareTag("Wall"))
        {
            NetworkServer.Destroy(gameObject);
        }
    }

}
