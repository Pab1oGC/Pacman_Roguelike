using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCollision : MonoBehaviour
{
    void Start()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, .2f);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Wall") && collider != this.GetComponent<Collider>())
            {
                // Encontramos el RoomBehaviour
                RoomBehaviour room = GetComponentInParent<RoomBehaviour>();
                if (room != null)
                {
                    // Buscamos el índice de este muro y lo marcamos como destruido
                    for (int i = 0; i < room.walls.Length; i++)
                    {
                        if (room.walls[i] == gameObject)
                        {
                            //room.destroyedWalls[i] = true;
                            break;
                        }
                    }
                }

                // Solo desactivamos este muro específico
                gameObject.SetActive(false);
                return;
            }
        }
    }


}
