using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterEnemy : Enemy
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    private float fireCooldown = 0f;

    public float moveRadius = 2f;
    public float moveSpeed = 1.5f;
    private Vector3 targetPos;
    public float minDistance = 3f;

    private Transform player;
    public Rigidbody rb;

    private float rotationSpeed = 2f;

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        PickNewTarget();
    }

    void Update()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        // Girar hacia el jugador (solo eje Y)
        Vector3 lookDirection = new Vector3(direction.x, 0, direction.z);
        if (lookDirection != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * rotationSpeed);

        // Mantener distancia mínima
        if (distance > minDistance)
        {
            Vector3 targetPos = player.position - direction.normalized * minDistance;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }


        // Disparar
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / fireRate;
        }
    }

    public void Shoot()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    void PickNewTarget()
    {
        Vector2 rand = Random.insideUnitCircle * moveRadius;
        targetPos = new Vector3(transform.position.x + rand.x, transform.position.y, transform.position.z + rand.y);
    }
}
