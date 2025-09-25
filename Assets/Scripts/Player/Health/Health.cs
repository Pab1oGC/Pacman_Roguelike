using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.LegacyInputHelpers;

public class Health : NetworkBehaviour
{
    [Header("Vida")]
    [SerializeField] private float maxHealth = 6f;

    // Se replica a todos; el hook dispara eventos locales para UI
    [SyncVar(hook = nameof(OnHealthSync))]
    private float currentHealth;

    // Eventos SOLO locales (para UI/HUD de cada cliente)
    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;

    #region Lifecycle
    public override void OnStartServer()
    {
        base.OnStartServer();
        currentHealth = Mathf.Clamp(currentHealth <= 0f ? maxHealth : currentHealth, 0f, maxHealth);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Al entrar un cliente, emite el estado actual para “sincronizar” HUD
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (IsDead) OnDied?.Invoke();
    }
    #endregion

    #region Hooks
    // Este hook se ejecuta en TODOS los clientes cuando cambia la SyncVar
    void OnHealthSync(float oldValue, float newValue)
    {
        OnHealthChanged?.Invoke(newValue, maxHealth);
        if (newValue <= 0f && oldValue > 0f)
            OnDied?.Invoke();
    }
    #endregion

    #region API (SERVER authoritative)
    [Server]
    public void ApplyDamageServer(float amount)
    {
        if (amount <= 0f || IsDead) return;
        currentHealth = Mathf.Max(0f, currentHealth - amount);
    }

    [Server]
    public void HealServer(float amount)
    {
        if (amount <= 0f || IsDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    [Server]
    public void SetMaxHealthServer(float newMax, bool clampToNewMax = true)
    {
        maxHealth = Mathf.Max(0.01f, newMax);
        if (clampToNewMax && currentHealth > maxHealth)
            currentHealth = maxHealth;
        // Dispara hook para refrescar UI
        OnHealthSync(currentHealth, currentHealth);
    }
    #endregion

    #region Opcional: peticiones del cliente (si quieres botones de “curar” locales)
    // Si te interesa que el propio jugador pida acciones (p.ej. usar poción),
    // puedes exponer estos Commands. Usa requiresAuthority según tu diseño.

    [Command(requiresAuthority = true)]
    public void CmdRequestHeal(float amount)
    {
        HealServer(amount);
    }

    [Command(requiresAuthority = true)]
    public void CmdRequestDamage(float amount)
    {
        ApplyDamageServer(amount);
    }
    #endregion
}
