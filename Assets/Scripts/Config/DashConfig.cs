using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CyberAverno/DashConfig")]
public sealed class DashConfig : ScriptableObject
{
    [Min(0.1f)] public float distance = 5f;        // metros
    [Min(0.05f)] public float duration = 0.15f;    // segundos
    [Min(0f)] public float cooldown = 0.8f;        // segundos
    [Min(0f)] public float iFrameDuration = 0.20f; // segundos

    // curva de velocidad relativa (0..1 tiempo → factor)
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
}
