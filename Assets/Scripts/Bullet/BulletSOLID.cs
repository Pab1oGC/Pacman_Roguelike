using UnityEngine;

public class BulletSOLID : MonoBehaviour
{
    [SerializeField] private float timeToDestroy = 1f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private bool isBulletPlayer;
    [SerializeField] private float bulletSpeed = 0.24f;

    private IBulletMovementSOLID bulletMovement;

    private void Awake()
    {
        bulletMovement = new BulletForwardMovementSOLID(bulletSpeed);
        Destroy(gameObject, timeToDestroy);
    }

    private void Update()
    {
        bulletMovement.Move(transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isBulletPlayer && other.CompareTag("Enemy"))
        {
            IDamageableSOLID enemy = other.GetComponent<IDamageableSOLID>();
            enemy?.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (!isBulletPlayer && other.CompareTag("Player"))
        {
            IDamageableSOLID player = other.GetComponent<IDamageableSOLID>();
            player?.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
