using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartRoom : MonoBehaviour
{
    [SerializeField] private Transform spawnPlayer;
    public static StartRoom Instance { get; private set; }
    public Transform SpawnPoint => spawnPlayer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
