using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator; // Arrastra el Animator del hijo desde el Inspector
    [SerializeField] private Bullet fireballPrefab;
    [SerializeField] private Transform firePoint;

    private bool isAttacking = false;
    private float lifeTimeBullet = 1;

    private void Awake()
    {
        // Si no lo asignaste en el inspector, búscalo en los hijos
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("No se encontró un Animator en los hijos de " + gameObject.name);
        }
    }

    public void SpawnFireball()
    {
        if (fireballPrefab != null && firePoint != null)
        {
            Bullet b = Instantiate(fireballPrefab, firePoint.position, firePoint.rotation);
            b.timeToDestroy = lifeTimeBullet;
        }
        else
        {
            Debug.LogWarning("Faltan referencias en AttackController");
        }
    }

    public void Attack()
    {
        if (isAttacking) return;
        isAttacking = true;
        animator.SetTrigger("Attack");
        animator.speed = 3.5f;
    }

    public void NoAttack()
    {
        isAttacking = false;
        animator.speed = 1f;
    }

    public bool IsAttacking => isAttacking;

    public void IncrementBulletLifetime(float increment)
    {
        lifeTimeBullet += increment;
    }

    public void DecrementBulletLifetime(float decrease)
    {
        if(lifeTimeBullet <= 0.1) return;
        lifeTimeBullet -= decrease;
    }

    [Server]
    public void AttackServer(Vector3 worldDir)
    {
        if (!fireballPrefab || !firePoint) return;

        // Rotación orientada a la dirección (fallback al forward del firePoint)
        Quaternion rot = firePoint.rotation;
        if (worldDir.sqrMagnitude > 1e-6f) rot = Quaternion.LookRotation(worldDir.normalized, Vector3.up);

        // Instancia en server
        Bullet b = Instantiate(fireballPrefab, firePoint.position, rot);
        b.timeToDestroy = lifeTimeBullet;

        // Si tu Bullet se mueve por sí mismo, no hace falta RB aquí.
        // Si usas Rigidbody en la bala y quieres empujarla:
        // var rb = b.GetComponent<Rigidbody>();
        // if (rb) rb.velocity = (worldDir.sqrMagnitude > 1e-6f ? worldDir.normalized : firePoint.forward) * 12f;

        // IMPORTANTE: el prefab de Bullet debe tener NetworkIdentity y estar en Spawnable Prefabs
        NetworkServer.Spawn(b.gameObject);
    }
}
