using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimAttackRelay : MonoBehaviour
{
    [SerializeField] private AttackController attackController; // si no lo asignas, se auto-busca
    [SerializeField] private Transform facing;                  // qué transform rota (por defecto, el del Player)
    [SerializeField] private bool snapFaceOnFire = true;        // girar al instante antes de atacar
    [SerializeField] private float rotateSpeed = 999f;          // por si prefieres Slerp en vez de snap

    private void Awake()
    {
        if (!attackController) attackController = GetComponentInChildren<AttackController>();
        if (!facing) facing = transform;
    }

    /// <summary>Llamado por el UI cuando sueltas el finger.</summary>
    public void Fire(Vector3 worldDir)
    {
        if (worldDir.sqrMagnitude < 1e-6f) return;

        // Alinea el forward del player con la dirección
        Quaternion look = Quaternion.LookRotation(worldDir, Vector3.up);
        if (snapFaceOnFire) facing.rotation = look;
        else facing.rotation = Quaternion.Slerp(facing.rotation, look, rotateSpeed * Time.deltaTime);

        // Dispara el ataque. Si tu AttackController requiere otro método, cámbialo aquí.
        if (attackController != null)
        {
            // Variante A: tu Attack() ya usa el forward del personaje
            attackController.Attack();

            // Variante B (si tuvieras Attack(Vector3 dir)): descomenta
            // attackController.Attack(worldDir);
        }
    }
}
