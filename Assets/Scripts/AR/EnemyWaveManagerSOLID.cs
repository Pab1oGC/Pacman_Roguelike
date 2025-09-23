using UnityEngine;

public class EnemyWaveManagerSOLID
{
    private int enemiesPerWave;
    private int currentWave;

    public EnemyWaveManagerSOLID(int enemiesPerWave)
    {
        this.enemiesPerWave = enemiesPerWave;
        currentWave = 0;
    }

    public int GetEnemiesForNextWave()
    {
        currentWave++;
        return enemiesPerWave * currentWave;
    }
}
