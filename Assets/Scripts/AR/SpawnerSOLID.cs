using UnityEngine;

public class SpawnerSOLID : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int enemiesPerWave = 5;

    private IEnemyFactorySOLID enemyFactory;
    private EnemyWaveManagerSOLID waveManager;

    private void Start()
    {
        enemyFactory = new EnemyFactorySOLID(enemyPrefab);
        waveManager = new EnemyWaveManagerSOLID(enemiesPerWave);

        SpawnWave();
    }

    private void SpawnWave()
    {
        int enemiesToSpawn = waveManager.GetEnemiesForNextWave();

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            enemyFactory.CreateEnemy(spawnPoint.position);
        }
    }
}
