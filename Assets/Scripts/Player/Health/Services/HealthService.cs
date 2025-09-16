using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthService : IHealth
{
    public float Max { get; private set; }
    public float Current { get; private set; }
    public bool IsDead => Current <= 0f;

    public event Action<float, float> OnChanged;
    public event Action<DamageInfo> OnDamaged;
    public event Action<float> OnHealed;
    public event Action OnDied;

    public HealthService(float maxHealth)
    {
        SetMax(Mathf.Max(1f, maxHealth), clampCurrent: false);
        ResetFull();
    }

    public void SetMax(float max, bool clampCurrent = true)
    {
        Max = Mathf.Max(1f, max);
        if (clampCurrent)
        {
            Current = Mathf.Clamp(Current, 0f, Max);
            OnChanged?.Invoke(Current, Max);
        }
    }

    public void ResetFull()
    {
        Current = Max;
        OnChanged?.Invoke(Current, Max);
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        float before = Current;
        Current = Mathf.Min(Max, Current + Mathf.Max(0f, amount));
        float gained = Current - before;
        if (gained > 0f)
        {
            OnHealed?.Invoke(gained);
            OnChanged?.Invoke(Current, Max);
        }
    }

    public void Kill()
    {
        if (IsDead) return;
        Current = 0f;
        OnChanged?.Invoke(Current, Max);
        OnDied?.Invoke();
    }

    public float TakeDamage(float amount, DamageInfo info)
    {
        if (IsDead || amount <= 0f) return 0f;

        float before = Current;
        Current = Mathf.Max(0f, Current - amount);
        float dealt = before - Current;

        // Siempre disparar OnChanged
        OnChanged?.Invoke(Current, Max);

        // Solo disparar OnDamaged si hubo daño real
        if (dealt > 0f)
            OnDamaged?.Invoke(info);

        // Disparar OnDied si murió
        if (IsDead)
            OnDied?.Invoke();

        return amount - dealt;
    }
}
