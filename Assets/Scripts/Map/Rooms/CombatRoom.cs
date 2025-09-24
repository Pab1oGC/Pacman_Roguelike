using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatRoom : MonoBehaviour
{
    [Header("Enemigos")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int minEnemies = 1;
    [SerializeField] private int maxEnemies = 3;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Boss (Opcional)")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private bool isBossRoom = false; // Marcar la sala como Boss

    private RoomBehaviour room;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool cleared = false;
    private bool inCombat = false;

    public GameObject buffon;
    public Transform buffonSpawn;

    private void Awake()
    {
        room = GetComponent<RoomBehaviour>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || cleared || inCombat) return;

        StartCoroutine(StartCombat());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject buffonObj = GameObject.FindGameObjectWithTag("Buffon");
            if (buffonObj) Destroy(buffonObj);
        }
    }

    private IEnumerator StartCombat()
    {
        inCombat = true;

        // Bloquear puertas temporalmente
        for (int i = 0; i < room.doors.Length; i++)
        {
            room.doors[i].SetActive(false);
            room.walls[i].SetActive(true);
        }

        // Spawnear enemigos o Boss
        if (isBossRoom && bossPrefab != null && bossSpawnPoint != null)
        {
            // Sala con Boss
            GameObject boss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation, transform);
            spawnedEnemies.Add(boss);
        }
        else
        {
            // Sala normal con enemigos
            int count = Random.Range(minEnemies, maxEnemies + 1);
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
                GameObject enemy = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], pos, Quaternion.identity, transform);
                spawnedEnemies.Add(enemy);
            }
        }

        // Pequeña espera para que se instancien
        yield return new WaitForSeconds(0.5f);

        // Activar la acción de todos los enemigos
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null && enemy.TryGetComponent<Enemy>(out var e))
            {
                e.canAct = true;
            }
        }

        // Esperar a que todos mueran
        while (spawnedEnemies.Exists(e => e != null))
        {
            yield return null;
        }

        cleared = true;
        inCombat = false;

        // Restaurar estado original de la sala
        room.RestoreRoom();

        // Instanciar buffon
        if (buffon != null && buffonSpawn != null)
        {
            Instantiate(buffon, buffonSpawn.position, buffonSpawn.rotation);
        }
    }
}
