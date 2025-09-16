using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    [SerializeField] private int coins = 0;
    public event Action<int> OnCoinsChanged;

    public int Coins => coins;

    public void Add(int amount)
    {
        if (amount <= 0) return;
        coins += amount;
        OnCoinsChanged?.Invoke(coins);
    }

    public bool Spend(int amount)
    {
        if (amount <= 0 || coins < amount) return false;
        coins -= amount;
        OnCoinsChanged?.Invoke(coins);
        return true;
    }

}
