using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct DamageInfo
{
    public readonly float Amount;
    public readonly Vector3? HitPoint;
    public readonly Object Source;

    public DamageInfo(float amount, Object source = null, Vector3? hitPoint = null)
    {
        Amount = Mathf.Max(0f, amount);
        Source = source;
        HitPoint = hitPoint;
    }
}
