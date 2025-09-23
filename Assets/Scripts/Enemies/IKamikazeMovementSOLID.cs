using UnityEngine;

public interface IKamikazeMovementSOLID
{
    void MoveTowardsTarget(Transform self, Transform target, float speed, bool canAct);
}
