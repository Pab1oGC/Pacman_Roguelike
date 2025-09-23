using UnityEngine;

public class RadialShooterMovementSOLID : IMovementSOLID
{
    private float moveSpeed;
    private float rotationSpeed;
    private float minDistance;
    private Transform player;

    public RadialShooterMovementSOLID(Transform player, float moveSpeed, float rotationSpeed, float minDistance)
    {
        this.player = player;
        this.moveSpeed = moveSpeed;
        this.rotationSpeed = rotationSpeed;
        this.minDistance = minDistance;
    }

    public void Move(Transform self, Vector3 unusedTarget, float unusedSpeed, bool canAct)
    {
        if (!canAct || player == null) return;

        Vector3 direction = player.position - self.position;
        float distance = direction.magnitude;

        // Girar hacia el jugador solo en Y
        Vector3 lookDirection = new Vector3(direction.x, 0, direction.z);
        if (lookDirection != Vector3.zero)
            self.rotation = Quaternion.Slerp(self.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * rotationSpeed);

        // Mantener distancia mínima
        if (distance > minDistance)
        {
            Vector3 targetPos = player.position - direction.normalized * minDistance;
            self.position = Vector3.MoveTowards(self.position, targetPos, moveSpeed * Time.deltaTime);
        }
    }
}
