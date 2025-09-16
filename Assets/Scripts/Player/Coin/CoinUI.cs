using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerWallet playerWallet; 
    [SerializeField] private Text label;                

    [Header("Formato")]
    [SerializeField] private string format = "{0}"; // usa "{0}" si quieres solo el número

    private void Awake()
    {
        if (!label) label = GetComponent<Text>();
    }

    private void OnEnable()
    {
        if (playerWallet != null) Hook(playerWallet);
        else StartCoroutine(WaitAndBindByTag());
    }

    private void OnDisable()
    {
        if (playerWallet != null)
            playerWallet.OnCoinsChanged -= UpdateLabel;
    }

    private IEnumerator WaitAndBindByTag()
    {
        // espera a que el Player (instancia) exista en escena
        while (playerWallet == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
            {
                // busca Wallet en root o en hijos (por si tu RB/Animator está abajo)
                var wallet = go.GetComponentInChildren<PlayerWallet>();
                if (wallet != null)
                {
                    Hook(wallet);
                    break;
                }
            }
            yield return null; // siguiente frame
        }
    }

    private void Hook(PlayerWallet wallet)
    {
        if (playerWallet != null)
            playerWallet.OnCoinsChanged -= UpdateLabel;

        playerWallet = wallet;
        playerWallet.OnCoinsChanged += UpdateLabel;

        // sincroniza valor inicial
        UpdateLabel(playerWallet.Coins);
    }

    private void UpdateLabel(int coins)
    {
        if (label) label.text = string.Format(format, coins);
    }
}
