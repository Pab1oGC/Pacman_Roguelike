using UnityEngine;

public class KamikazeMovementSOLID : IKamikazeMovementSOLID
{
    public void MoveTowardsTarget(Transform self, Transform target, float speed, bool canAct)
    {
        if (!canAct || target == null) return;

        self.position = Vector3.MoveTowards(self.position, target.position, speed * Time.deltaTime);

        Vector3 dir = (target.position - self.position).normalized;
        if (dir.sqrMagnitude > 0f)
            self.forward = dir;
    }
}
