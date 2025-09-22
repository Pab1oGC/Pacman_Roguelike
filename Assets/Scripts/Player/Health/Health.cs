using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.LegacyInputHelpers;

public class Health : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 6f;
    public float currentHealth;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        // Emite estado actual cuando el componente se habilita, así nuevas UIs se sincronizan
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ApplyDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
            OnDied?.Invoke();
    }

    public void Heal(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void IncrementHealth(float amount)
    {
        if(currentHealth>=maxHealth) return;

        currentHealth += amount;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void DecrementHealth(float amount) 
    { if (currentHealth <= 0f) return; 
        ApplyDamage(amount);
    }
}
