using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialShooterEnemy : Enemy
{
    public GameObject bulletPrefab;
    public int bulletsPerWave = 8;
    public float fireRate = 2f;
    private float fireCooldown = 0f;

    public float moveRadius = 2f;
    public float moveSpeed = 1.2f;
    private Transform player;       // referencia al jugador
    public float minDistance = 3f;
    private float rotationSpeed = 2f;
    public Rigidbody rb;

    [Header("Disparo")]
    [SerializeField] private Transform shootOrigin;     // ← ASÍGNALO en el prefab al centro/altura del pecho
    [SerializeField] private float spawnDistance = 0.9f; // antes estaba hardcodeado en el método
    [SerializeField] private float shootHeightOffset = 0f;

    protected override void Start()
    {
        base.Start();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        //PickNewTarget();
    }

    void Update()
    {
        if (!canAct) return;

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

        // Movimiento ligero hacia target aleatorio



        // Disparo radial
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            ShootRadial();
            fireCooldown = 1f / fireRate;
        }
    }

    void ShootRadial()
    {
        if (!bulletPrefab) return;

        float angleStep = 360f / Mathf.Max(1, bulletsPerWave);

        // Base en LOCAL del enemigo/origen
        Quaternion baseRot = shootOrigin.rotation;
        Vector3 basePos = shootOrigin.position + Vector3.up * shootHeightOffset;

        for (int i = 0; i < bulletsPerWave; i++)
        {
            float angle = i * angleStep;

            // Rotación final = rotación del enemigo/origen * giro radial
            Quaternion rot = baseRot * Quaternion.Euler(0f, angle, 0f);

            // Offset radial en el plano local del origen, convertido a mundo
            Vector3 localForward = rot * Vector3.forward;              // ya en mundo
            Vector3 spawnPos = basePos + localForward * spawnDistance; // a cierta distancia del modelo

            Instantiate(bulletPrefab, spawnPos, rot);
        }
    }

    /*void PickNewTarget()
    {
        Vector2 rand = Random.insideUnitCircle * moveRadius;
        targetPos = new Vector3(transform.position.x + rand.x, transform.position.y, transform.position.z + rand.y);
    }*/
}
