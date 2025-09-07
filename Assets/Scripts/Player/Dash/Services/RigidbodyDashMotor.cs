using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyDashMotor : IDashMotor
{
    public void Move(Rigidbody rb, Vector3 delta)
    {
        if (!rb) return;
        rb.MovePosition(rb.position + delta);
    }
}