using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArmor
{
    float Current { get; }

    event Action<float> OnArmorChanged; // current

    /// <summary>Absorbe daño. Devuelve el residual que pasa a la salud.</summary>
    float Absorb(float amount);
    void Set(float amount);
    void Add(float amount);
}
