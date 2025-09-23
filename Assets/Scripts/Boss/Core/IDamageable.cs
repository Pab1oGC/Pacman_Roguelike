public interface IDamageable
{
    void ApplyDamage(int amount);
    bool IsDead { get; }
}
