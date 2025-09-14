using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAttackHandler : MonoBehaviour
{
    private AttackController attackController;

    void Start()
    {
        StartCoroutine(WaitForPlayer());
    }

    private IEnumerator WaitForPlayer()
    {
        GameObject player = null;

        // Espera hasta que el Player exista en la escena
        while (player == null)
        {
            player = GameObject.FindWithTag("Player"); // o FindObjectOfType<PlayerContext>()
            yield return null; // espera un frame
        }

        // Una vez encontrado, obtiene el AttackController del hijo
        attackController = player.GetComponentInChildren<AttackController>();
        if (attackController == null)
            Debug.LogError("No se encontró AttackController en el Player");
    }

    public void OnButtonPressed()
    {
        if (attackController != null)
            attackController.Attack(); // simula el Space
    }
}
