using UnityEngine;
public class HitPlayerOnKey : MonoBehaviour
{
    public int damage = 999;
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.P)) return;

        var go = GameObject.FindGameObjectWithTag("Player");
        if (!go) { Debug.LogWarning("No hay Player con tag 'Player'"); return; }

        var health = go.GetComponentInParent<Health>();
        if (health != null)
        {
            var info = new DamageInfo((float)damage, this, go.transform.position);
            health.ApplyDamage(info);
            Debug.Log("Forcé daño al Player via Health");
        }
        else
        {
            var dmg = go.GetComponentInParent<IDamageable>();
            if (dmg != null) { dmg.ApplyDamage(damage); Debug.Log("Forcé daño via IDamageable"); }
            else Debug.LogWarning("El Player no tiene Health ni IDamageable");
        }
    }
}
