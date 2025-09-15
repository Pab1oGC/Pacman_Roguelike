using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
    [SerializeField] public GameObject[] walls;
    [SerializeField] public GameObject[] doors;
    [SerializeField] Transform roomCenter;

    // Estado original de las puertas (solo puertas)
    private bool[] initialDoorStatus;
    private bool[] initialWallStatus;

    private void Awake()
    {
        initialDoorStatus = new bool[doors.Length];
        initialWallStatus = new bool[walls.Length];

        for (int i = 0; i < doors.Length; i++)
            initialDoorStatus[i] = doors[i].activeSelf;

        for (int i = 0; i < walls.Length; i++)
            initialWallStatus[i] = walls[i].activeSelf;
    }

    /// <summary>
    /// Se usa cuando se genera la dungeon
    /// </summary>
    public void UpdateRoom(bool[] status)
    {
        for (int i = 0; i < status.Length; i++)
        {
            doors[i].SetActive(status[i]);
            walls[i].SetActive(!status[i]);

            initialDoorStatus[i] = status[i];
            initialWallStatus[i] = !status[i];
        }
    }

    public void RestoreRoom()
    {
        for (int i = 0; i < doors.Length; i++)
            doors[i].SetActive(initialDoorStatus[i]);

        for (int i = 0; i < walls.Length; i++)
            walls[i].SetActive(initialWallStatus[i]);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CameraController.Instance.SetTargetRoom(roomCenter.position);
            Debug.Log("Entro");
        }
    }
}
