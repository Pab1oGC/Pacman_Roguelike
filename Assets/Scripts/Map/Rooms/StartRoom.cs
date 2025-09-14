using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartRoom : MonoBehaviour
{
    [SerializeField] Transform spawnPlayer;
    [SerializeField] GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(player, spawnPlayer.position, Quaternion.Euler(0,0,0));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
