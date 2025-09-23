using UnityEngine;

public class EnemyContactDamageSOLID : MonoBehaviour
{
    [Header("Daño por contacto")]
    [SerializeField] private bool dealContactDamage = true;
    [SerializeField] private float contactDamage = 1f;
    [SerializeField] private float contactCooldown = 0.5f;
    [SerializeField] private bool selfDestructOnHit = false;
    [SerializeField] private string playerTag = "Player";
    private float _lastContactTime = -999f;

    [Header("Knockback al Player")]
    [SerializeField] private bool doKnockback = true;
    [SerializeField] private float knockbackForce = 1f;
    [SerializeField] private float knockbackUpForce = 1f;
    [SerializeField] private float movementLock = 0.15f;

    private EnemySOLID _enemy;

    private void Awake() => _enemy = GetComponent<EnemySOLID>();

    private void OnCollisionEnter(Collision other) => TryDealContactDamage(other.collider);
    private void OnTriggerEnter(Collider other) => TryDealContactDamage(other);

    private void TryDealContactDamage(Collider other)
    {
        if (!dealContactDamage || !enabled || !other.CompareTag(playerTag)) return;
        if (Time.time - _lastContactTime < contactCooldown) return;

        if (other.TryGetComponent<Health>(out var hp))
        {
            hp.ApplyDamage(contactDamage);
            _lastContactTime = Time.time;

            if (doKnockback)
                ApplyKnockback(other);

            if (selfDestructOnHit)
                _enemy.Die();
        }
    }

    private void ApplyKnockback(Collider other)
    {
        Vector3 dir = (other.transform.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
        dir.Normalize();

        if (other.TryGetComponent<PlayerKnockback>(out var kb))
            kb.ApplyKnockback(dir, knockbackForce, knockbackUpForce, movementLock);
        else if (other.attachedRigidbody != null)
            other.attachedRigidbody.AddForce(dir * knockbackForce + Vector3.up * knockbackUpForce, ForceMode.Impulse);
    }
}
