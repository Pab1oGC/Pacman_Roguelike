using UnityEngine;

public interface IMovementSOLID
{
    // self = transform del enemigo
    // unusedTarget / unusedSpeed = parámetros para compatibilidad (no usados aquí)
    // canAct = si el enemigo puede moverse
    void Move(Transform self, Vector3 unusedTarget, float unusedSpeed, bool canAct);
}
