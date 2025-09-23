using UnityEngine;

[RequireComponent(typeof(EnemySOLID))]
public class RadialShooterEnemySOLID : MonoBehaviour
{
    [Header("Disparo")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int bulletsPerWave = 8;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private float spawnDistance = 0.9f;
    [SerializeField] private float shootHeightOffset = 0f;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 1.2f;
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

        movement = new RadialShooterMovementSOLID(player, moveSpeed, rotationSpeed, minDistance);
        shooter = new RadialShooterSOLID(bulletPrefab, bulletsPerWave, spawnDistance, shootHeightOffset);
    }

    private void Update()
    {
        if (!enemyBase.CanAct) return;

        movement.Move(transform, Vector3.zero, 0, enemyBase.CanAct);

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            shooter.Shoot(shootOrigin);
            fireCooldown = 1f / fireRate;
        }
    }
}
