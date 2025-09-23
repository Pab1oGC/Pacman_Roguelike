using UnityEngine;

public class EnemyLootDropSOLID : MonoBehaviour
{
    [Header("Loot")]
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.5f;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Vector2Int coinAmountRange = new Vector2Int(1, 3);
    [SerializeField] private float scatterImpulse = 2f;

    [Header("Posición de spawn")]
    [SerializeField] private float coinSpawnForward = 0.6f;
    [SerializeField] private float coinSpawnUp = 0.1f;
    [SerializeField] private float coinScatter = 0.25f;

    public void DropLoot()
    {
        if (!coinPrefab || Random.value > dropChance) return;

        Vector3 basePos = transform.position + transform.forward * coinSpawnForward + Vector3.up * coinSpawnUp;
        int amount = Random.Range(coinAmountRange.x, coinAmountRange.y + 1);

        for (int i = 0; i < amount; i++)
        {
            Vector2 circle = Random.insideUnitCircle * coinScatter;
            Vector3 spawnPos = basePos + new Vector3(circle.x, 0f, circle.y);
            var coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);

            if (coin.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 dir = (spawnPos - basePos);
                dir.y = 0f;
                if (dir.sqrMagnitude < 1e-4f) dir = transform.forward;
                dir = dir.normalized + Vector3.up * 0.5f;
                rb.AddForce(dir * scatterImpulse, ForceMode.Impulse);
            }
        }
    }
}
