using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CyberAverno/MovementConfig")]
public class MovementConfig : ScriptableObject
{
    [Min(0f)] public float moveSpeed = 0.5f;   // m/s a palanca máxima
    [Min(0f)] public float turnSpeed = 540f;   // °/s
    public bool faceInstantly = false;
}
