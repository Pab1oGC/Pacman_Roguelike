using UnityEngine;

public class BulletForwardMovementSOLID : IBulletMovementSOLID
{
    private float speed;

    public BulletForwardMovementSOLID(float speed)
    {
        this.speed = speed;
    }

    public void Move(Transform bulletTransform)
    {
        bulletTransform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
