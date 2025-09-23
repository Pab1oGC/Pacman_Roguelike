using UnityEngine;

public class EnemyDeathSOLID : MonoBehaviour
{
    [Header("Muerte & Loot")]
    [SerializeField] private GameObject deathVfxPrefab;
    [SerializeField] private float deathVfxLifetime = 2f;
    [SerializeField] private float destroyDelay = 0.1f;

    private EnemyLootDropSOLID _lootDrop;

    private void Awake() => _lootDrop = GetComponent<EnemyLootDropSOLID>();

    public void HandleDeath()
    {
        // VFX
        if (deathVfxPrefab)
        {
            var vfx = Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
            if (deathVfxLifetime > 0f) Destroy(vfx, deathVfxLifetime);
        }

        // Loot
        _lootDrop?.DropLoot();

        Destroy(gameObject, destroyDelay);
    }
}
