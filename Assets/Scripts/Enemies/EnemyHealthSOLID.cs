using UnityEngine;

[RequireComponent(typeof(EnemySOLID))]
public class EnemyHealthSOLID : MonoBehaviour
{
    [SerializeField] private float maxHealth = 3f;
    private float currentHealth;

    private EnemySOLID _enemy;

    private void Awake()
    {
        currentHealth = maxHealth;
        _enemy = GetComponent<EnemySOLID>();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0f)
        {
            _enemy.Die();
        }
    }

    public float GetHealth() => currentHealth;
}
