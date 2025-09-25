using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour
{
    [Header("Referencias (opcional)")]
    [SerializeField] private PlayerWallet playerWallet;  // si lo dejas vacío, se auto-bindea al localPlayer
    [SerializeField] private Text label;

    [Header("Formato")]
    [SerializeField] private string format = "{0}";

    void Awake()
    {
        if (!label) label = GetComponent<Text>();
    }

    void OnEnable()
    {
        // Si ya está asignado, hook directo
        if (playerWallet != null) Hook(playerWallet);
        else StartCoroutine(BindToLocalPlayerWallet());
    }

    void OnDisable()
    {
        if (playerWallet != null)
            playerWallet.OnCoinsChanged -= UpdateLabel;
    }

    IEnumerator BindToLocalPlayerWallet()
    {
        // Espera a que exista el localPlayer de este cliente
        while (NetworkClient.localPlayer == null)
            yield return null;

        var lp = NetworkClient.localPlayer;                 // NetworkIdentity del jugador local
        var wallet = lp.GetComponentInChildren<PlayerWallet>(true);
        while (wallet == null)
        {
            // si el componente aparece tarde (instanciado por code), seguimos esperando
            yield return null;
            wallet = lp.GetComponentInChildren<PlayerWallet>(true);
        }

        Hook(wallet);
    }

    void Hook(PlayerWallet wallet)
    {
        if (playerWallet != null)
            playerWallet.OnCoinsChanged -= UpdateLabel;

        playerWallet = wallet;
        playerWallet.OnCoinsChanged += UpdateLabel;

        // Estado inicial
        UpdateLabel(playerWallet.Coins);
    }

    void UpdateLabel(int coins)
    {
        if (label) label.text = string.Format(format, coins);
    }
}
