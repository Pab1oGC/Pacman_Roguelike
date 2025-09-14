using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyMotor : IPhysicsMotor
{
    public void Move(Rigidbody rb, Quaternion newRotation, float speed, float dt)
    {
        if (!rb) return;

        // Rotar primero
        rb.MoveRotation(newRotation);

        // Avanzar en el forward **actual** del personaje, no del newRotation
        Vector3 forward = rb.transform.forward; // <--- usar transform.forward
        forward.y = 0f;
        Vector3 step = forward * (Mathf.Max(0f, speed) * dt);
        rb.MovePosition(rb.position + step);
    }
}
