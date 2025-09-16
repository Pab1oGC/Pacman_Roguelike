using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartsUI : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform heartsContainer;

    private readonly List<GameObject> hearts = new List<GameObject>();

    private void OnEnable()
    {
        // Si ya está asignado, nos suscribimos; si no, esperamos a que spawnee
        if (playerHealth != null) Hook(playerHealth);
        else StartCoroutine(WaitAndBindByTag());
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHearts;
    }

    private IEnumerator WaitAndBindByTag()
    {
        while (playerHealth == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
            {
                var hp = go.GetComponent<Health>();
                if (hp != null) Hook(hp);
                break;
            }
            yield return null; // espera al siguiente frame
        }
    }

    private void Hook(Health hp)
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHearts;

        playerHealth = hp;
        playerHealth.OnHealthChanged += UpdateHearts;

        GenerateHearts(playerHealth.maxHealth);
        UpdateHearts(playerHealth.currentHealth, playerHealth.maxHealth);
    }

    private void GenerateHearts(float max)
    {
        foreach (var h in hearts) Destroy(h);
        hearts.Clear();

        int count = Mathf.RoundToInt(max);
        for (int i = 0; i < count; i++)
            hearts.Add(Instantiate(heartPrefab, heartsContainer));
    }

    private void UpdateHearts(float current, float max)
    {
        int activeHearts = Mathf.Clamp(Mathf.FloorToInt(current), 0, hearts.Count);
        for (int i = 0; i < hearts.Count; i++)
            hearts[i].SetActive(i < activeHearts);
    }
}
