using UnityEngine;

public interface IMovementSOLID
{
    // self = transform del enemigo
    // unusedTarget / unusedSpeed = par�metros para compatibilidad (no usados aqu�)
    // canAct = si el enemigo puede moverse
    void Move(Transform self, Vector3 unusedTarget, float unusedSpeed, bool canAct);
}
