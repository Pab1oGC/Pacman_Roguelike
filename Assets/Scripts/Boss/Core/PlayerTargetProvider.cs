using UnityEngine;

[DisallowMultipleComponent]
public class PlayerTargetProvider : MonoBehaviour
{
    [SerializeField] Transform explicitTarget;

    public Transform GetTarget()
    {
        if (explicitTarget) return explicitTarget;

        var pc = Object.FindObjectOfType<PlayerContext>(); // si existe en tu proyecto
        if (pc) return explicitTarget = pc.transform;

        var go = GameObject.FindGameObjectWithTag("Player");
        if (go) explicitTarget = go.transform;

        return explicitTarget;
    }
}
