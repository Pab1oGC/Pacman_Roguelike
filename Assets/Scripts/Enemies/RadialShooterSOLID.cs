using UnityEngine;

public class RadialShooterSOLID : IShooterSOLID
{
    private GameObject bulletPrefab;
    private int bulletsPerWave;
    private float spawnDistance;
    private float shootHeightOffset;

    public RadialShooterSOLID(GameObject bulletPrefab, int bulletsPerWave, float spawnDistance, float shootHeightOffset)
    {
        this.bulletPrefab = bulletPrefab;
        this.bulletsPerWave = bulletsPerWave;
        this.spawnDistance = spawnDistance;
        this.shootHeightOffset = shootHeightOffset;
    }

    public void Shoot(Transform origin)
    {
        if (!bulletPrefab) return;

        float angleStep = 360f / Mathf.Max(1, bulletsPerWave);
        Quaternion baseRot = origin.rotation;
        Vector3 basePos = origin.position + Vector3.up * shootHeightOffset;

        for (int i = 0; i < bulletsPerWave; i++)
        {
            float angle = i * angleStep;
            Quaternion rot = baseRot * Quaternion.Euler(0f, angle, 0f);
            Vector3 localForward = rot * Vector3.forward;
            Vector3 spawnPos = basePos + localForward * spawnDistance;
            Object.Instantiate(bulletPrefab, spawnPos, rot);
        }
    }
}
