using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealth 
{
    float Max { get; }
    float Current { get; }
    bool IsDead { get; }

    event Action<float, float> OnChanged;     // (current, max)
    event Action<DamageInfo> OnDamaged;
    event Action<float> OnHealed;
    event Action OnDied;

    void SetMax(float max, bool clampCurrent = true);
    void ResetFull();
    void Heal(float amount);
    void Kill();
    /// <summary>Reduce salud. Devuelve el daño que NO pudo ser absorbido (normalmente 0).</summary>
    float TakeDamage(float amount, DamageInfo info);
}
