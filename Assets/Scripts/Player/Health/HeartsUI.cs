using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartsUI : MonoBehaviour
{
    [Header("Refs (opcional)")]
    [SerializeField] private Health playerHealth;  // si lo dejas vacío, se auto-bindea al localPlayer
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform heartsContainer;

    private readonly List<GameObject> hearts = new List<GameObject>();

    void OnEnable()
    {
        if (playerHealth != null) Hook(playerHealth);
        else StartCoroutine(BindToLocalPlayerHealth());
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= OnHealthChanged;
    }

    IEnumerator BindToLocalPlayerHealth()
    {
        while (NetworkClient.localPlayer == null)
            yield return null;

        var lp = NetworkClient.localPlayer;
        var hp = lp.GetComponentInChildren<Health>(true);
        while (hp == null)
        {
            yield return null;
            hp = lp.GetComponentInChildren<Health>(true);
        }

        Hook(hp);
    }

    void Hook(Health hp)
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= OnHealthChanged;

        playerHealth = hp;
        playerHealth.OnHealthChanged += OnHealthChanged;

        // Genera corazones y sincroniza estado inicial
        RegenerateHearts(Mathf.RoundToInt(playerHealth.MaxHealth));
        OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    void RegenerateHearts(int max)
    {
        foreach (var h in hearts) Destroy(h);
        hearts.Clear();

        for (int i = 0; i < max; i++)
            hearts.Add(Instantiate(heartPrefab, heartsContainer));
    }

    // Hook que llega cada vez que cambia la SyncVar en NetworkHealth
    void OnHealthChanged(float current, float max)
    {
        // Si algún día modificas max en runtime, reconstituye la UI:
        int maxInt = Mathf.RoundToInt(max);
        if (maxInt != hearts.Count)
            RegenerateHearts(maxInt);

        int active = Mathf.Clamp(Mathf.FloorToInt(current), 0, hearts.Count);
        for (int i = 0; i < hearts.Count; i++)
            hearts[i].SetActive(i < active);
    }
}
