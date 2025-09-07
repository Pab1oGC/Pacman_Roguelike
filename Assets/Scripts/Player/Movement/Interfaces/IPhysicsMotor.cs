using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPhysicsMotor 
{
    void Move(Rigidbody rb, Quaternion newRotation, float speed, float dt);
}
