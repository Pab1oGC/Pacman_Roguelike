using UnityEngine;

public class ShooterShootingSOLID : IShooterSOLID
{
    private GameObject bulletPrefab;
    private Transform firePoint;

    public ShooterShootingSOLID(GameObject bulletPrefab, Transform firePoint)
    {
        this.bulletPrefab = bulletPrefab;
        this.firePoint = firePoint;
    }

    public void Shoot(Transform origin)
    {
        if (bulletPrefab && firePoint)
        {
            Object.Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}
