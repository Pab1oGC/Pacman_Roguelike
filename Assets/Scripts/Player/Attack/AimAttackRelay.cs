using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimAttackRelay : NetworkBehaviour
{
    [SerializeField] private AttackController attackController; // si no lo asignas, se auto-busca
    [SerializeField] private Transform facing;                  // qué transform rota (por defecto, el del Player)
    [SerializeField] private bool snapFaceOnFire = true;        // girar al instante antes de atacar
    [SerializeField] private float rotateSpeed = 999f;          // por si prefieres Slerp
    [SerializeField] private bool replicateAnimViaRpc = true;   // si no usas NetworkAnimator, deja esto en true

    void Awake()
    {
        if (!attackController) attackController = GetComponentInChildren<AttackController>();
        if (!facing) facing = transform;
    }

    /// <summary>Llamado por el UI cuando sueltas el finger (desde el dueño local).</summary>
    public void Fire(Vector3 worldDir)
    {
        if (worldDir.sqrMagnitude < 1e-6f) return;
        if (!isLocalPlayer) return; // SOLO el jugador local puede ordenar atacar

        // 1) Feedback local inmediato (no bloquees al jugador)
        Face(worldDir);
        if (attackController != null) attackController.Attack();  // tu animación local

        // 2) Orden al servidor para que haga el spawn y (opcional) replique anim
        CmdFire(worldDir);
    }

    void Face(Vector3 worldDir)
    {
        Quaternion look = Quaternion.LookRotation(worldDir, Vector3.up);
        facing.rotation = snapFaceOnFire ? look : Quaternion.Slerp(facing.rotation, look, rotateSpeed * Time.deltaTime);
    }

    [Command]
    void CmdFire(Vector3 worldDir)
    {
        // En el servidor: spawnea la bala desde este Player
        if (attackController != null)
            attackController.AttackServer(worldDir);

        // Si NO usas NetworkAnimator, replicamos la anim con RPC
        if (replicateAnimViaRpc)
            RpcPlayAttackAnim();
    }

    [ClientRpc]
    void RpcPlayAttackAnim()
    {
        if (attackController != null) attackController.Attack();
    }
}
