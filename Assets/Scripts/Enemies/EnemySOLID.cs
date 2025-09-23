using UnityEngine;

[RequireComponent(typeof(EnemyHealthSOLID))]
[RequireComponent(typeof(EnemyContactDamageSOLID))]
[RequireComponent(typeof(EnemyDeathSOLID))]
public class EnemySOLID : MonoBehaviour
{
    public bool CanAct { get; set; } = false;
    private bool _isDead = false;

    public void Die()
    {
        if (_isDead) return;
        _isDead = true;
        CanAct = false;

        var deathHandler = GetComponent<EnemyDeathSOLID>();
        deathHandler?.HandleDeath();
    }
}
