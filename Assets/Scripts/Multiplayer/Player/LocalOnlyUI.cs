using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalOnlyUI : NetworkBehaviour
{
    [SerializeField] public GameObject root;  // arrastra el Canvas del joystick; si lo dejas vacío, usa el propio GO

    void Awake() { if (!root) root = gameObject; }

    public override void OnStartClient()
    {
        // solo el dueño ve/usa su joystick
        root.SetActive(isLocalPlayer);
    }

    // por si el orden de activación rara vez te gana
    void Start()
    {
        if (!isLocalPlayer) root.SetActive(false);
    }
}
