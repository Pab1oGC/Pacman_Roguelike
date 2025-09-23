using UnityEngine;

[RequireComponent(typeof(EnemySOLID))]
public class KamikazeEnemySOLID : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    private Transform player;
    private IKamikazeMovementSOLID movement;
    private EnemySOLID enemyBase;

    private void Awake()
    {
        movement = new KamikazeMovementSOLID();
        enemyBase = GetComponent<EnemySOLID>();
    }

    private void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) player = playerGO.transform;
    }

    private void Update()
    {
        movement.MoveTowardsTarget(transform, player, speed, enemyBase.CanAct);
    }
}
