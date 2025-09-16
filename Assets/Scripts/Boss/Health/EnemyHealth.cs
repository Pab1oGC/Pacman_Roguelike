using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] int maxHealth = 120;
    public int Current { get; private set; }
    public bool IsDead { get; private set; }

    public UnityEvent<int> OnDamaged;
    public UnityEvent OnDied;

    void Awake() { Current = maxHealth; }

    public void ApplyDamage(int amount)
    {
        if (IsDead) return;
        amount = Mathf.Abs(amount);
        Current = Mathf.Max(0, Current - amount);
        OnDamaged?.Invoke(amount);
        if (Current == 0) { IsDead = true; OnDied?.Invoke(); }
    }
}
