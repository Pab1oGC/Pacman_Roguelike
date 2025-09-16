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
            GameObject buffon = GameObject.FindGameObjectWithTag("Buffon");
            Destroy(buffon);
        }
    }

    private IEnumerator StartCombat()
    {
        inCombat = true;

        // Activar muros overlapeados, bloquear puertas temporalmente
        for (int i = 0; i < room.doors.Length; i++)
        {
            room.doors[i].SetActive(false);
            room.walls[i].SetActive(true);
        }

        // Spawnear enemigos
        int count = Random.Range(minEnemies, maxEnemies + 1);
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;

            GameObject enemy = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], pos, Quaternion.identity, transform);
            spawnedEnemies.Add(enemy);
        }

        yield return new WaitForSeconds(0.5f);

        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null) enemy.GetComponent<Enemy>().canAct = true;
        }

        // Esperar a que todos mueran
        while (spawnedEnemies.Exists(e => e != null))
            yield return null;

        cleared = true;
        inCombat = false;

        // Restaurar todo el estado original de la sala (doors + walls)
        room.RestoreRoom();

        Instantiate(buffon, buffonSpawn.position, buffonSpawn.rotation);
    }
}
