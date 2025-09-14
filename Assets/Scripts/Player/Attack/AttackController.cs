using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator; // Arrastra el Animator del hijo desde el Inspector
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform firePoint;

    private bool isAttacking = false;

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
            Instantiate(fireballPrefab, firePoint.position, firePoint.rotation);
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
}
