using UnityEngine;

public class EnemyFactorySOLID : IEnemyFactorySOLID
{
    private GameObject enemyPrefab;

    public EnemyFactorySOLID(GameObject prefab)
    {
        enemyPrefab = prefab;
    }

    public GameObject CreateEnemy(Vector3 position)
    {
        return Object.Instantiate(enemyPrefab, position, Quaternion.identity);
    }
}
