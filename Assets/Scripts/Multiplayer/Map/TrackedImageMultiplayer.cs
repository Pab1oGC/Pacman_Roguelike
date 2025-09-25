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

    [Header("Spawns")]
    [SerializeField] Transform fixedSpawn;              // ⬅️ ASIGNA EN INSPECTOR (pos fija del mapa)
    public Vector3 playerSpawnLocal = new Vector3(0, 0, 0.5f);

    ARTrackedImageManager _imgMgr;
    [SerializeField] ARAnchorManager _anchorMgr;
    [SerializeField] Transform _contentRoot;

    public bool useEditorDebug = true;
    public Transform editorOrigin;
    public KeyCode buildKey = KeyCode.Space;
    [SerializeField] public bool useSafetyFloor = false;

    public enum NormalAxis { Forward, Up }
    [SerializeField] NormalAxis normalAxis = NormalAxis.Forward;
    [SerializeField] float heightOffset = 0.01f;
    [SerializeField] bool fitToImageSize = false;       // ⬅️ sin auto-escala

    Dictionary<ARTrackedImage, int> _stableFrames = new Dictionary<ARTrackedImage, int>();
    bool _placing = false;

    [SyncVar] bool sBuilt;  // marca en server para idempotencia
    bool _built;            // marca local

    void Awake()
    {
        if (!_imgMgr) _imgMgr = FindObjectOfType<ARTrackedImageManager>(true);
        if (!_anchorMgr) _anchorMgr = FindObjectOfType<ARAnchorManager>(true);
    }

    void OnEnable()
    {
        if (_imgMgr != null) _imgMgr.trackedImagesChanged += OnChanged;
        else Debug.LogError("[TrackedImageMP] ARTrackedImageManager no encontrado en escena.");
    }
    void OnDisable()
    {
        if (_imgMgr != null) _imgMgr.trackedImagesChanged -= OnChanged;
    }

    void Update()
    {
#if UNITY_EDITOR
        // Editor: QuickBuild con Space sin requerir isServer
        if (useEditorDebug && Application.isPlaying && !_built && Input.GetKeyDown(buildKey))
            EditorQuickBuildNow();
#endif
    }

#if UNITY_EDITOR
    [ContextMenu("EDITOR: Quick Build Now")]
    public void EditorQuickBuildNow()
    {
        if (_built) return;

        // Pose para el Editor (editorOrigin -> fixedSpawn -> este GO)
        Transform origin = editorOrigin ? editorOrigin : (fixedSpawn ? fixedSpawn : transform);
        var pose = new Pose(origin.position, origin.rotation);
        int seed = UnityEngine.Random.Range(0, 1_000_000);

        if (isServer)
        {
            // 1) Construye en el Host
            BuildAtPose(pose, gridSize, tileSize, seed);
            _built = true; sBuilt = true;

            if (_imgMgr) { _imgMgr.trackedImagesChanged -= OnChanged; _imgMgr.enabled = false; }

            // 2) Manda construir a TODOS los clientes de Editor
            Rpc_EDITOR_BuildAtPose(pose.position, pose.rotation, seed);

            // 3) TP en server (para que todos queden colocados)
            if (!_placing) StartCoroutine(PlaceAllPlayersAtStartDelayed());
        }
        else
        {
            // Si soy Client de Editor, pido al Server que haga el broadcast
            Cmd_EDITOR_RequestBuild(pose.position, pose.rotation, seed);
        }

        Debug.Log("[Editor] QuickBuild: host + broadcast listo.");
    }
#endif

#if UNITY_EDITOR
    [Command(requiresAuthority = false)]
    void Cmd_EDITOR_RequestBuild(Vector3 pos, Quaternion rot, int seed)
    {
        if (!isServer) return;

        // Construye también en el Server (por si el comando vino del client)
        if (!_built)
        {
            BuildAtPose(new Pose(pos, rot), gridSize, tileSize, seed);
            _built = true; sBuilt = true;
            if (_imgMgr) { _imgMgr.trackedImagesChanged -= OnChanged; _imgMgr.enabled = false; }
        }

        // Ordena a todos los clientes del Editor construir en la misma pose/seed
        Rpc_EDITOR_BuildAtPose(pos, rot, seed);

        // Y coloca a los players
        if (!_placing) StartCoroutine(PlaceAllPlayersAtStartDelayed());
    }

    [ClientRpc]
    void Rpc_EDITOR_BuildAtPose(Vector3 pos, Quaternion rot, int seed)
    {
        // Este RPC solo afecta al Editor; en móvil no se llama
        if (!Application.isPlaying || _built) return;

        BuildAtPose(new Pose(pos, rot), gridSize, tileSize, seed);
        _built = true;

        if (_imgMgr) { _imgMgr.trackedImagesChanged -= OnChanged; _imgMgr.enabled = false; }

        Debug.Log("[Editor] Cliente construyó por RPC en la misma pose/seed.");
    }
#endif

    void OnChanged(ARTrackedImagesChangedEventArgs e)
    {
        if (AlreadyBuilt()) return;
        foreach (var img in e.added) if (TryUse(img)) return;
        foreach (var img in e.updated) if (TryUse(img)) return;
    }

    bool AlreadyBuilt() => sBuilt || _built;

    bool TryUse(ARTrackedImage img)
    {
        if (AlreadyBuilt()) return false;
        if (img.trackingState != TrackingState.Tracking)
        {
            _stableFrames[img] = 0;
            return false;
        }

        if (!_stableFrames.ContainsKey(img)) _stableFrames[img] = 0;
        _stableFrames[img]++;
        if (_stableFrames[img] < 6) return false; // ~100ms

        if (isServer)
        {
            if (!fixedSpawn)
            {
                Debug.LogError("[MP] fixedSpawn no asignado en Host.");
                return false;
            }

            int seed = UnityEngine.Random.Range(0, 1_000_000);

            // Pose del marcador del HOST (para alinear clientes)
            var hostMarkerPos = img.transform.position;
            var hostMarkerRot = img.transform.rotation;

            // 1) Host construye en su spawn fijo
            BuildAtPose(new Pose(fixedSpawn.position, fixedSpawn.rotation), gridSize, tileSize, seed);

            // 2) RPC: manda tamaño + pose FIJA + pose del marcador del host
            RpcBuildAtFixedPoseWithAlignment(
                gridSize.x, gridSize.y, tileSize.x, tileSize.y, seed,
                fixedSpawn.position, fixedSpawn.rotation,     // F_h
                hostMarkerPos, hostMarkerRot                  // S
            );

            if (!_placing) StartCoroutine(PlaceAllPlayersAtStartDelayed());
            _built = true; sBuilt = true;

            // congelar tracking en host
            if (_imgMgr) { _imgMgr.trackedImagesChanged -= OnChanged; _imgMgr.enabled = false; }
        }
        return true;
    }

    [ClientRpc]
    void RpcBuildAtFixedPoseWithAlignment(
        int cols, int rows, float tileX, float tileZ, int seed,
        Vector3 hostSpawnPos, Quaternion hostSpawnRot,  // F_h
        Vector3 hostMarkerPos, Quaternion hostMarkerRot // S
    )
    {
        if (_built) return;
        StartCoroutine(AlignAndBuildAtFixed(
            new Vector2Int(cols, rows), new Vector2(tileX, tileZ), seed,
            hostSpawnPos, hostSpawnRot,
            hostMarkerPos, hostMarkerRot
        ));
    }

    IEnumerator AlignAndBuildAtFixed(
        Vector2Int size, Vector2 tile, int seed,
        Vector3 hostSpawnPos, Quaternion hostSpawnRot, // F_h
        Vector3 hostMarkerPos, Quaternion hostMarkerRot // S
    )
    {
        // 1) Espera a tener el marcador local (L)
        float t = 0f;
        Transform localMarker = null;
        while (t < 10f)
        {
            foreach (var track in _imgMgr.trackables)
                if (track.trackingState == TrackingState.Tracking) { localMarker = track.transform; break; }
            if (localMarker) break;
            t += Time.deltaTime; yield return null;
        }
        if (!localMarker) { Debug.LogWarning("[Align] Timeout marker local."); yield break; }

        // 2) Matrices S (host) y L (client)
        var S = Matrix4x4.TRS(hostMarkerPos, hostMarkerRot, Vector3.one);
        var L = Matrix4x4.TRS(localMarker.position, localMarker.rotation, Vector3.one);

        // 3) Mapeo HOST->CLIENT
        var hostToClient = L * S.inverse;
        SharedAlignment.hostToClient = hostToClient;
        SharedAlignment.has = true;

        // 4) Transforma pose fija del host al espacio del cliente: F_c = H2C * F_h
        var Fh = Matrix4x4.TRS(hostSpawnPos, hostSpawnRot, Vector3.one);
        var Fc = hostToClient * Fh;

        var pos = Fc.GetColumn(3);
        var fwd = Fc.GetColumn(2); var up = Fc.GetColumn(1);
        var rot = Quaternion.LookRotation(fwd, up);

        BuildAtPose(new Pose(pos, rot), size, tile, seed);

        // Congelar tracking en cliente (evita re-updates que muevan todo)
        if (_imgMgr) { _imgMgr.trackedImagesChanged -= OnChanged; _imgMgr.enabled = false; }

        _built = true;

        // Avisar READY al server para autorización de TP
        var local = NetworkClient.localPlayer;
        if (local)
        {
            var ready = local.GetComponent<PlacementReady>();
            if (!ready) ready = local.gameObject.AddComponent<PlacementReady>();
            ready.CmdIAmReady();
        }
    }

    // ===== Construcción en pose fija (no pegado a la imagen) =====
    void BuildAtPose(Pose pose, Vector2Int size, Vector2 tile, int seed)
    {
        if (!dungeonPrefab) { Debug.LogError("dungeonPrefab no asignado"); return; }

        var root = new GameObject("DungeonRoot");
        root.transform.SetPositionAndRotation(pose.position, pose.rotation);
        if (_contentRoot) root.transform.SetParent(_contentRoot, true);

        var go = GameObject.Instantiate(dungeonPrefab, root.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        var gen = go.GetComponent<DungeonGenerator>();
        if (!gen) { Debug.LogError("DungeonGenerator faltante"); Destroy(root); return; }

        gen.autoGenerate = false;
        gen.Run(size, tile, seed);

        // pequeño epsilon para no z-fight
        root.transform.position += root.transform.up * Mathf.Max(0.002f, heightOffset);

        // Piso de seguridad temporal (evita caídas infinitas)
        if (useSafetyFloor) AddSafetyFloor(root.transform);
    }

    void AddSafetyFloor(Transform root)
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "SafetyFloor_TEMP";
        floor.transform.SetParent(root, false);
        floor.transform.localScale = new Vector3(100f, 0.1f, 100f);
        floor.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        var rb = floor.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    // ===== Utilidades previas (si las necesitas para otros casos) =====
    Bounds CalculateBounds(GameObject go)
    {
        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        var b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        return b;
    }
    Vector3[] GetBoundsCorners(Bounds b)
    {
        Vector3 c = b.center;
        Vector3 e = b.extents;
        return new[]
        {
            c + new Vector3(+e.x,+e.y,+e.z),
            c + new Vector3(+e.x,+e.y,-e.z),
            c + new Vector3(+e.x,-e.y,+e.z),
            c + new Vector3(+e.x,-e.y,-e.z),
            c + new Vector3(-e.x,+e.y,+e.z),
            c + new Vector3(-e.x,+e.y,-e.z),
            c + new Vector3(-e.x,-e.y,+e.z),
            c + new Vector3(-e.x,-e.y,-e.z),
        };
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

    // ====== Server: TP cuando todos están READY ======
    [Server]
    IEnumerator PlaceAllPlayersAtStartDelayed()
    {
        if (!isServer) yield break;
        _placing = true;

        // 1) Espera SpawnPoint
        float t = 0f;
        while (StartRoom.Instance == null || StartRoom.Instance.SpawnPoint == null)
        {
            if (t > 2f) { Debug.LogWarning("[MP] SpawnPoint no encontrado tras 2s."); _placing = false; yield break; }
            t += Time.deltaTime;
            yield return null;
        }

        // 2) Espera READY (o timeout)
        float started = Time.time;
        while (!PlacementReady.IsAllReadyOrTimeout(started, 6f))
            yield return null;

        // 3) Un frame de física + sync
        yield return new WaitForFixedUpdate();
        Physics.SyncTransforms();

        // 4) TP con offsets
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

    // ===== Client: TP robusto con reintentos / snap a suelo =====
    [TargetRpc]
    void TargetSnap(NetworkConnection conn, Vector3 pos, Quaternion rot)
    {
        var lp = NetworkClient.localPlayer;
        if (lp) lp.StartCoroutine(TrySnapLoop(pos, rot));
    }

    IEnumerator TrySnapLoop(Vector3 hostPos, Quaternion hostRot)
    {
        var id = NetworkClient.localPlayer;
        while (!id) { yield return null; id = NetworkClient.localPlayer; }

        var go = id.gameObject;
        var rb = go.GetComponent<Rigidbody>();
        var mv = go.GetComponent<Movement>();

        // Espera mundo listo
        float t = 0f; while (!(_built && SharedAlignment.has) && t < 5f) { t += Time.deltaTime; yield return null; }

        // Apaga control y caída
        if (mv) mv.enabled = false;
        bool prevGrav = rb && rb.useGravity;
        if (rb) { rb.useGravity = false; rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

        Vector3 pos = SharedAlignment.has ? SharedAlignment.MapPos_HostToClient(hostPos) : hostPos;
        Quaternion rot = SharedAlignment.has ? SharedAlignment.MapRot_HostToClient(hostRot) : hostRot;

        float end = Time.time + 5f;
        while (Time.time < end)
        {
            SetPose(go, rb, pos, rot);
            yield return new WaitForFixedUpdate();
            Physics.SyncTransforms();

            TryGroundSnap(ref pos, go);
            SetPose(go, rb, pos, rot);

            if (!IsFalling(rb)) break;
            yield return null;
        }

        if (rb) rb.useGravity = prevGrav;
        if (mv) mv.enabled = true;
    }

    void SetPose(GameObject go, Rigidbody rb, Vector3 p, Quaternion r)
    {
        if (rb) { rb.position = p; rb.rotation = r; rb.MovePosition(p); rb.MoveRotation(r); }
        else go.transform.SetPositionAndRotation(p, r);
    }

    bool TryGroundSnap(ref Vector3 pos, GameObject player)
    {
        float standHeight = 1.0f;
        var col = player.GetComponent<Collider>();
        if (col) standHeight = Mathf.Max(0.1f, col.bounds.extents.y);

        RaycastHit hit;
        Vector3 rayStart = pos + Vector3.up * 2f;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 10f, ~0, QueryTriggerInteraction.Ignore))
        {
            pos = new Vector3(pos.x, hit.point.y + standHeight, pos.z);
            return true;
        }
        return false;
    }

    bool IsFalling(Rigidbody rb) => rb && rb.useGravity && rb.velocity.y < -0.1f;
}
