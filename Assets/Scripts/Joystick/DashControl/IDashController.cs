using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDashController
{
    bool IsDashing { get; }
    bool IsOnCooldown { get; }
    float CooldownRemaining { get; }  // segundos restantes
    float CooldownDuration { get; }   // duración total del CD
    void TryDash();
}
