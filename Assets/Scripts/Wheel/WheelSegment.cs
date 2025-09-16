using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WheelSegment
{
    public string name = "Premio";
    [Range(1, 100)] public int weight = 1; // probabilidad relativa
    public RewardType reward = RewardType.Coins;
    public int amount = 10;                // monedas, heal, etc.
    public Color color = Color.white;      // usado si generas las cuñas en UI
    public Sprite icon;                    // opcional (no imprescindible)
}

public enum RewardType { Coins, Heal, Speed, Range, NoCoins, NoHeal, NoSpeed, NoRange }
