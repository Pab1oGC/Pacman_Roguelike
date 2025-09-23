using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public class Cell
    {
        public bool visited = false;
        public bool[] status = new bool[4]; // 0=up,1=down,2=right,3=left
    }

    [System.Serializable]
    public class Rule
    {
        public GameObject room;
        public Vector2Int minPosition;
        public Vector2Int maxPosition;
        public bool obligatory;

        public int ProbabilityOfSpawning(int x, int y)
        {
            if (x >= minPosition.x && x <= maxPosition.x && y >= minPosition.y && y <= maxPosition.y)
                return obligatory ? 2 : 1;
            return 0;
        }
    }

    [Header("Config")]
    public Vector2Int size = new Vector2Int(5, 3); // columnas x filas
    public Vector2 offset = new Vector2(6f, 6f);   // metros por tile (X/Z)
    public int startPos = 0;
    public Rule[] rooms;

    [Header("Start room")]
    public GameObject spawnRoomPrefab;   // <- NUEVO
    private bool _spawnPlaced = false;   // <- NUEVO

    [Tooltip("Si true, genera en Start con size/offset actuales (útil en editor). En AR lo pondremos en false.")]
    public bool autoGenerate = true;

    private List<Cell> board;

    void Start()
    {
        if (autoGenerate) Run(size, offset);
    }

    public void Run(Vector2Int gridSize, Vector2 tileOffset)
    {
        size = new Vector2Int(Mathf.Max(1, gridSize.x), Mathf.Max(1, gridSize.y));
        offset = new Vector2(Mathf.Max(0.01f, tileOffset.x), Mathf.Max(0.01f, tileOffset.y));

        // reset del flag de spawn para regeneraciones
        _spawnPlaced = false;

        for (int i = transform.childCount - 1; i >= 0; i--)
#if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(i).gameObject);
#else
            Destroy(transform.GetChild(i).gameObject);
#endif

        MazeGenerator();
    }

    void GenerateDungeon()
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Cell currentCell = board[i + j * size.x];
                if (!currentCell.visited) continue;

                // === OVERRIDE: spawn en (0,0) una sola vez ===
                if (i == 0 && j == 0 && !_spawnPlaced && spawnRoomPrefab != null)
                {
                    var startRoom = Instantiate(
                        spawnRoomPrefab,
                        new Vector3(0f, 0f, 0f),            // (0,0) de la grilla
                        Quaternion.identity,
                        transform
                    ).GetComponent<RoomBehaviour>();

                    if (startRoom != null)
                        startRoom.UpdateRoom(currentCell.status);

                    startRoom.name = "StartRoom 0-0";
                    _spawnPlaced = true;
                    continue; // no elijas otra room para (0,0)
                }
                // =============================================

                int randomRoom = -1;
                List<int> availableRooms = new List<int>();

                for (int k = 0; k < rooms.Length; k++)
                {
                    int p = rooms[k].ProbabilityOfSpawning(i, j);
                    if (p == 2) { randomRoom = k; break; }
                    else if (p == 1) availableRooms.Add(k);
                }

                if (randomRoom == -1)
                    randomRoom = (availableRooms.Count > 0) ? availableRooms[Random.Range(0, availableRooms.Count)] : 0;

                var newRoom = Instantiate(
                    rooms[randomRoom].room,
                    new Vector3(i * offset.x, 0, -j * offset.y),
                    Quaternion.identity,
                    transform
                ).GetComponent<RoomBehaviour>();

                newRoom.UpdateRoom(currentCell.status);
                newRoom.name += $" {i}-{j}";
            }
        }
    }

    void MazeGenerator()
    {
        board = new List<Cell>(size.x * size.y);
        for (int i = 0; i < size.x; i++)
            for (int j = 0; j < size.y; j++)
                board.Add(new Cell());

        int currentCell = startPos; // ya parte en 0
        Stack<int> path = new Stack<int>();
        int k = 0;

        while (k < 1000)
        {
            k++;
            board[currentCell].visited = true;
            if (currentCell == board.Count - 1) break;

            List<int> neighbors = CheckNeighbors(currentCell);

            if (neighbors.Count == 0)
            {
                if (path.Count == 0) break;
                currentCell = path.Pop();
            }
            else
            {
                path.Push(currentCell);
                int newCell = neighbors[Random.Range(0, neighbors.Count)];

                if (newCell > currentCell)
                {
                    if (newCell - 1 == currentCell) { board[currentCell].status[2] = true; currentCell = newCell; board[currentCell].status[3] = true; }
                    else { board[currentCell].status[1] = true; currentCell = newCell; board[currentCell].status[0] = true; }
                }
                else
                {
                    if (newCell + 1 == currentCell) { board[currentCell].status[3] = true; currentCell = newCell; board[currentCell].status[2] = true; }
                    else { board[currentCell].status[0] = true; currentCell = newCell; board[currentCell].status[1] = true; }
                }
            }
        }

        GenerateDungeon();
    }

    List<int> CheckNeighbors(int cell)
    {
        List<int> neighbors = new List<int>();

        if (cell - size.x >= 0 && !board[cell - size.x].visited) neighbors.Add(cell - size.x);
        if (cell + size.x < board.Count && !board[cell + size.x].visited) neighbors.Add(cell + size.x);
        if ((cell + 1) % size.x != 0 && !board[cell + 1].visited) neighbors.Add(cell + 1);
        if (cell % size.x != 0 && !board[cell - 1].visited) neighbors.Add(cell - 1);

        return neighbors;
    }
}
