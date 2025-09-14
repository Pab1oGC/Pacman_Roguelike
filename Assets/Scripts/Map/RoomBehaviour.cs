using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
    [SerializeField] public GameObject[] walls;
    [SerializeField] public GameObject[] doors;
    [SerializeField] Transform roomCenter;

    // Update is called once per frame
    public void UpdateRoom(bool[] status)
    {
        for (int i = 0; i < status.Length; i++)
        {
            doors[i].SetActive(status[i]);
            walls[i].SetActive(!status[i]);
        }
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
