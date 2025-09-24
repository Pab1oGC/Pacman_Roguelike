using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackedImageMultiplayer : NetworkBehaviour
{
    [Header("Dungeon")]
    public GameObject dungeonPrefab;
    public Vector2Int gridSize = new Vector2Int(6, 4);
    public Vector2 tileSize = new Vector2(6f, 6f);
    public Vector3 playerSpawnLocal = new Vector3(0, 0, 0.5f);

    ARTrackedImageManager _imgMgr;
    bool _built;

    public bool useEditorDebug = true;
    public Transform editorOrigin;
    public KeyCode buildKey = KeyCode.Space;

    [SerializeField] ARAnchorManager _anchorMgr; // arrástralo en el Inspector
    [SerializeField] Transform _contentRoot;

    // --- NEW: guard para no disparar varias veces el TP ---
    bool _placing = false;

    void Awake() 
    {
        _imgMgr = GetComponent<ARTrackedImageManager>();
        if (!_anchorMgr) _anchorMgr = GetComponent<ARAnchorManager>();
    } 

    void OnEnable() => _imgMgr.trackedImagesChanged += OnChanged;
    void OnDisable() => _imgMgr.trackedImagesChanged -= OnChanged;

    void OnChanged(ARTrackedImagesChangedEventArgs e)
    {
        if (_built) return;
        foreach (var img in e.added) if (TryUse(img)) return;
        foreach (var img in e.updated) if (TryUse(img)) return;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (useEditorDebug && !Application.isPlaying) return;
        if (useEditorDebug && isServer && !_built && Input.GetKeyDown(buildKey) && editorOrigin)
        {
            int seed = UnityEngine.Random.Range(0, 1_000_000);
            RpcBuildDungeon(gridSize.x, gridSize.y, tileSize.x, tileSize.y, seed);
            BuildLocal(editorOrigin, gridSize, tileSize, seed);

            // --- SOLO server y solo una vez ---
            if (!_placing) StartCoroutine(PlaceAllPlayersAtStartDelayed());
            _built = true;
        }
#endif
    }

    bool TryUse(ARTrackedImage img)
    {
        if (_built || img.trackingState != TrackingState.Tracking) return false;

        if (isServer)
        {
            int seed = UnityEngine.Random.Range(0, 1_000_000);
            RpcBuildDungeon(gridSize.x, gridSize.y, tileSize.x, tileSize.y, seed);
            BuildLocal(img.transform, gridSize, tileSize, seed);

            if (!_placing) StartCoroutine(PlaceAllPlayersAtStartDelayed());
            _built = true;
        }
        return _built;
    }

    [ClientRpc]
    void RpcBuildDungeon(int cols, int rows, float tileX, float tileZ, int seed)
    {
        if (_built) return;
        StartCoroutine(WaitAndBuild(new Vector2Int(cols, rows), new Vector2(tileX, tileZ), seed));
    }

    IEnumerator WaitAndBuild(Vector2Int size, Vector2 tile, int seed)
    {
        // Espera a que este cliente tenga el marcador en Tracking
        float t = 0f;
        while (t < 10f)
        {
            var tr = FindTrackedImageTransform();
            if (tr != null)
            {
                BuildLocal(tr, size, tile, seed);

                // 🔴 IMPORTANTE: NO LLAMES PlaceAllPlayers... AQUÍ (esto es CLIENTE)
                // Antes lo hacías y no corría (método [Server])

                _built = true;
                yield break;
            }
            t += Time.deltaTime;
            yield return null;
        }
        Debug.LogWarning("[TrackedImageMP] Timeout esperando marker local.");
    }

    Transform FindTrackedImageTransform()
    {
#if UNITY_EDITOR
        if (useEditorDebug && editorOrigin) return editorOrigin;
#endif
        foreach (var img in _imgMgr.trackables)
            if (img.trackingState == TrackingState.Tracking)
                return img.transform;
        return null;
    }

    void BuildLocal(Transform origin, Vector2Int size, Vector2 tile, int seed)
    {
        if (!dungeonPrefab) { Debug.LogError("dungeonPrefab no asignado"); return; }

        var go = Instantiate(dungeonPrefab, origin);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        var gen = go.GetComponent<DungeonGenerator>();
        if (!gen) { Debug.LogError("DungeonGenerator faltante"); Destroy(go); return; }

        gen.autoGenerate = false;
        gen.Run(size, tile, seed);
        Debug.Log($"[MP] Dungeon {size.x}x{size.y} @ {tile.x}x{tile.y} seed={seed}");
    }

    [Server]
    IEnumerator PlaceAllPlayersAtStartDelayed()
    {
        // si alguien lo llamó desde cliente por error, aborta
        if (!isServer) yield break;

        _placing = true;

        // 1) Espera a que StartRoom/SpawnPoint existan
        float t = 0f;
        while (StartRoom.Instance == null || StartRoom.Instance.SpawnPoint == null)
        {
            if (t > 2f) { Debug.LogWarning("[MP] SpawnPoint no encontrado tras 2s."); _placing = false; yield break; }
            t += Time.deltaTime;
            yield return null;
        }

        // 2) Espera 1 paso de física + sync transforms (colliders listos)
        yield return new WaitForFixedUpdate();
        Physics.SyncTransforms();

        var start = StartRoom.Instance.SpawnPoint;
        int i = 0;
        foreach (var kv in NetworkServer.connections)
        {
            var conn = kv.Value;
            if (conn?.identity == null) continue;

            Vector3 localOffset = new Vector3(0.5f * i, 0f, 0f);
            Vector3 worldPos = start.TransformPoint(localOffset);
            Quaternion worldRot = start.rotation;

            conn.identity.transform.SetPositionAndRotation(worldPos, worldRot);
            TargetSnap(conn, worldPos, worldRot);
            i++;
        }

        _placing = false;
    }

    [TargetRpc]
    void TargetSnap(NetworkConnection conn, Vector3 pos, Quaternion rot)
    {
        var id = NetworkClient.localPlayer;
        if (id == null) return;

        var rb = id.GetComponent<Rigidbody>();
        var mv = id.GetComponent<Movement>();

        if (mv) mv.enabled = false;

        if (rb)
        {
            var prevGrav = rb.useGravity;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.position = pos;
            rb.rotation = rot;
            rb.MovePosition(pos);
            rb.MoveRotation(rot);

            id.StartCoroutine(ReenableNextFrame(rb, prevGrav, mv));
        }
        else
        {
            id.transform.SetPositionAndRotation(pos, rot);
            id.StartCoroutine(ReenableNextFrame(null, true, mv));
        }
    }

    IEnumerator ReenableNextFrame(Rigidbody rb, bool prevGrav, Movement mv)
    {
        yield return null;
        yield return new WaitForFixedUpdate();
        if (rb) rb.useGravity = prevGrav;
        if (mv) mv.enabled = true;
    }
}
