using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackButtonUI : MonoBehaviour
{
    [Header("Referencia al player o hijo con AttackController")]
    [SerializeField] private GameObject playerRoot;

    private AttackController attackController;

    private void Awake()
    {
        if (playerRoot == null)
            playerRoot = GameObject.FindWithTag("Player"); // o arrastra manualmente

        if (playerRoot != null)
            attackController = playerRoot.GetComponentInChildren<AttackController>();
        else
            Debug.LogError("No se encontr� playerRoot para AttackButtonUI");
    }

    // Este m�todo lo asignas en el Button OnClick()

}
