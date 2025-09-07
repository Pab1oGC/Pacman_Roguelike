using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ArmorService : IArmor
{
    public float Current { get; private set; }
    public event Action<float> OnArmorChanged;

    public ArmorService(float start)
    {
        Current = Math.Max(0f, start);
    }

    public float Absorb(float amount)
    {
        if (amount <= 0f) return 0f;
        float absorbed = Math.Min(Current, amount);
        Current -= absorbed;
        OnArmorChanged?.Invoke(Current);
        return amount - absorbed; // residual
    }

    public void Set(float amount)
    {
        Current = Math.Max(0f, amount);
        OnArmorChanged?.Invoke(Current);
    }

    public void Add(float amount)
    {
        if (amount <= 0f) return;
        Current += amount;
        OnArmorChanged?.Invoke(Current);
    }
}
