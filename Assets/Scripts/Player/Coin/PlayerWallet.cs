using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallet : NetworkBehaviour
{
    [SerializeField] private int startCoins = 0;

    // Monedero replicado (con hook para refrescar UI local)
    [SyncVar(hook = nameof(OnCoinsSync))]
    private int coins;

    public event Action<int> OnCoinsChanged;

    public int Coins => coins;

    public override void OnStartServer()
    {
        base.OnStartServer();
        coins = startCoins;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        OnCoinsChanged?.Invoke(coins);
    }

    void OnCoinsSync(int oldValue, int newValue)
    {
        OnCoinsChanged?.Invoke(newValue);
    }

    // ===== API de servidor =====
    [Server]
    public void AddCoinsServer(int amount)
    {
        if (amount <= 0) return;
        coins += amount;
    }

    /// <summary>Intenta gastar; devuelve true si se pudo.</summary>
    [Server]
    public bool SpendCoinsServer(int amount)
    {
        if (amount <= 0 || coins < amount) return false;
        coins -= amount;
        return true;
    }

    // ===== Opcional: peticiones del cliente =====
    // Útil si tu UI local tiene botones para comprar/gastar.
    // El servidor valida y actualiza el saldo.

    [Command(requiresAuthority = true)]
    public void CmdRequestAddCoins(int amount)
    {
        AddCoinsServer(amount);
    }

    [Command(requiresAuthority = true)]
    public void CmdRequestSpend(int amount)
    {
        SpendCoinsServer(amount);
        // si necesitas feedback de éxito/fracaso, usa un TargetRpc:
        // TargetSpendResult(connectionToClient, /*success*/ true/false);
    }

    /*[TargetRpc]
    void TargetSpendResult(NetworkConnection target, bool success) {
        // mostrar feedback en la UI del jugador que pidió gastar
    }*/

}
