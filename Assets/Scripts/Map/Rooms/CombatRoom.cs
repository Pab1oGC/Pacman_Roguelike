using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatRoom : NetworkBehaviour
{
    [Header("Enemigos")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int minEnemies = 1;
    [SerializeField] private int maxEnemies = 3;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Puertas/Paredes")]
    [SerializeField] private RoomBehaviour room;

    [Header("Lógica de inicio")]
    [Tooltip("Jugadores necesarios dentro para iniciar el combate")]
    [SerializeField] private int minPlayersToStart = 1;

    [Tooltip("Si > 0, al entrar el primero se abre ventana para que otros entren; al expirar, inicia si se cumple el mínimo.")]
    [SerializeField] private float joinWindowSeconds = 0f;

    [Tooltip("Inicia en cuanto llega el mínimo, sin esperar ventana")]
    [SerializeField] private bool startImmediatelyOnMinReached = true;

    [Header("Recompensa (opcional)")]
    public GameObject buffon;
    public Transform buffonSpawn;

    // Estado
    private readonly HashSet<uint> _playersInside = new HashSet<uint>();
    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();

    [SyncVar] private bool cleared = false;
    [SyncVar] private bool inCombat = false;

    private Coroutine _joinWindowCoro;

    // ----------------------------------------------------------------------

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
        if (!room) room = GetComponent<RoomBehaviour>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (!room) room = GetComponent<RoomBehaviour>();
        if (spawnPoints == null || spawnPoints.Length == 0)
            Debug.LogWarning("[CombatRoom] No hay spawnPoints asignados.", this);
    }

    // -------------------- Triggers SOLO en servidor ------------------------

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!enabled || cleared) return;
        if (!other || !other.gameObject.activeInHierarchy) return;
        if (!other.CompareTag("Player")) return;

        var ni = other.GetComponentInParent<NetworkIdentity>();
        if (!ni && other.attachedRigidbody)
            ni = other.attachedRigidbody.GetComponent<NetworkIdentity>();
        if (!ni) return;

        _playersInside.Add(ni.netId);
        //Debug.Log($"[Room] Player {ni.netId} ENTER ({_playersInside.Count})");

        TryStartJoinWindowOrCombat();
    }

    [ServerCallback]
    private void OnTriggerExit(Collider other)
    {
        if (!enabled || cleared) return;
        if (!other || !other.gameObject.activeInHierarchy) return;
        if (!other.CompareTag("Player")) return;

        var ni = other.GetComponentInParent<NetworkIdentity>();
        if (!ni) return;

        _playersInside.Remove(ni.netId);
        //Debug.Log($"[Room] Player {ni.netId} EXIT ({_playersInside.Count})");
    }

    // ----------------------- Arranque del combate --------------------------

    [Server]
    private void TryStartJoinWindowOrCombat()
    {
        if (inCombat || cleared) return;

        int inside = _playersInside.Count;
        int minNeeded = Mathf.Max(1, minPlayersToStart);

        if (startImmediatelyOnMinReached && inside >= minNeeded)
        {
            if (_joinWindowCoro != null) StopCoroutine(_joinWindowCoro);
            _joinWindowCoro = null;
            StartCoroutine(StartCombat());
            return;
        }

        if (joinWindowSeconds > 0f && _joinWindowCoro == null)
            _joinWindowCoro = StartCoroutine(JoinWindowCountdown(joinWindowSeconds));
    }

    [Server]
    private IEnumerator JoinWindowCountdown(float seconds)
    {
        float t = 0f;
        while (t < seconds && !inCombat && !cleared)
        {
            t += Time.deltaTime;
            yield return null;
        }
        _joinWindowCoro = null;

        if (!inCombat && !cleared && _playersInside.Count >= Mathf.Max(1, minPlayersToStart))
            StartCoroutine(StartCombat());
    }

    // -------------------------- Combate ------------------------------------

    [Server]
    private IEnumerator StartCombat()
    {
        inCombat = true;

        RpcSetRoomLocked(true);

        int count = Random.Range(minEnemies, maxEnemies + 1);
        spawnedEnemies.Clear();

        for (int i = 0; i < count; i++)
        {
            var p = spawnPoints[Random.Range(0, spawnPoints.Length)];
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            var enemy = SpawnEnemy(prefab, p.position, Quaternion.identity);
            spawnedEnemies.Add(enemy);
        }

        // Espera un poco y habilita AI
        yield return new WaitForSeconds(0.5f);
        foreach (var e in spawnedEnemies)
            if (e) e.GetComponent<Enemy>()?.EnableActServer();

        // Espera hasta que mueran todos
        while (spawnedEnemies.Exists(e => e != null))
            yield return null;

        cleared = true;
        inCombat = false;

        RpcSetRoomLocked(false);

        // Recompensa/NPC
        if (buffon && buffonSpawn)
        {
            var b = Instantiate(buffon, buffonSpawn.position, buffonSpawn.rotation);
            NetworkServer.Spawn(b);
        }
    }

    // ---------------------- Visual (todos los clientes) --------------------

    [ClientRpc]
    private void RpcSetRoomLocked(bool locked)
    {
        if (!room) return;
        for (int i = 0; i < room.doors.Length; i++)
        {
            if (room.doors[i]) room.doors[i].SetActive(!locked);
            if (room.walls[i]) room.walls[i].SetActive(locked);
        }
    }

    // ---------------------- Util: spawn en servidor ------------------------

    [Server]
    private GameObject SpawnEnemy(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        var go = Instantiate(prefab, pos, rot);
        NetworkServer.Spawn(go); // <- imprescindible para que llegue al cliente
        return go;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (TryGetComponent<BoxCollider>(out var box))
        {
            Gizmos.color = new Color(1f, .6f, 0f, .25f);
            var m = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix = m;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(1f, .6f, 0f, .9f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
#endif
}
