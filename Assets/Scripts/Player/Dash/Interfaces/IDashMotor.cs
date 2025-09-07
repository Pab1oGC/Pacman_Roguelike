using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDashMotor
{
    void Move(Rigidbody rb, Vector3 delta);
}
