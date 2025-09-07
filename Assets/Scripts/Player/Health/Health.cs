using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.LegacyInputHelpers;

public class Health : MonoBehaviour
{
    [Header("Vida / Armadura inicial")]
    [Min(1f)] public float maxHealth = 6f;
    [Min(0f)] public float startArmor = 0f;

    [Header("Opcional")]
    [SerializeField] private Invulnerability invulnerability;

    public IHealth health { get; private set; }
    public IArmor armor { get; private set; }

    // Eventos simples para UI (también puedes suscribirte a los del modelo)
    public event Action<float, float> OnHealthChanged;
    public event Action<float> OnArmorChanged;
    public event Action OnDied;

    void Awake()
    {
        health = new HealthService(maxHealth);
        armor = new ArmorService(startArmor);

        if (!invulnerability) invulnerability = GetComponent<Invulnerability>();

        health.OnChanged += (c, m) => OnHealthChanged?.Invoke(c, m);
        health.OnDied += () => OnDied?.Invoke();
        armor.OnArmorChanged += a => OnArmorChanged?.Invoke(a);
    }

    public void ApplyDamage(DamageInfo info)
    {
        if (health.IsDead) return;
        if (invulnerability && invulnerability.IsInvulnerable) return;

        float residual = armor.Absorb(info.Amount);
        if (residual > 0f) health.TakeDamage(residual, info);
    }

    public void Heal(float amount) => health.Heal(amount);
    public void AddArmor(float amount) => armor.Add(amount);

    public void Kill() => health.Kill();
}
