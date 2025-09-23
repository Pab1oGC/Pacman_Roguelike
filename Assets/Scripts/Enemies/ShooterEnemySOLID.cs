using UnityEngine;

[RequireComponent(typeof(EnemySOLID))]
public class ShooterEnemySOLID : MonoBehaviour
{
    [Header("Disparo")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float minDistance = 3f;

    private float fireCooldown = 0f;
    private EnemySOLID enemyBase;
    private IMovementSOLID movement;
    private IShooterSOLID shooter;
    private Transform player;

    private void Awake()
    {
        enemyBase = GetComponent<EnemySOLID>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        movement = new ShooterMovementSOLID(player, moveSpeed, rotationSpeed, minDistance);
        shooter = new ShooterShootingSOLID(bulletPrefab, firePoint);
    }

    private void Update()
    {
        if (!enemyBase.CanAct) return;

        movement.Move(transform, Vector3.zero, 0, enemyBase.CanAct);

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            shooter.Shoot(transform);
            fireCooldown = 1f / fireRate;
        }
    }
}
