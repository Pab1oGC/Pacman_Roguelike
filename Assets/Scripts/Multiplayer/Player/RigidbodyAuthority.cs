using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyAuthority : NetworkBehaviour
{
    Rigidbody rb;
    void Awake() { rb = GetComponent<Rigidbody>(); }

    public override void OnStartClient()
    {
        if (rb) rb.isKinematic = !isServer; // remotos kinematic, host simula
    }

    public override void OnStartLocalPlayer()
    {
        if (rb) rb.isKinematic = false; // local puede simular (ajústalo a tu gusto)
    }
}
