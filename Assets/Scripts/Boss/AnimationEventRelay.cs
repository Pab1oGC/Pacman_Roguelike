using UnityEngine;

// Pon ESTE script en el MISMO GameObject que tiene el Animator (hijo "Model")
public class AnimationEventRelay : MonoBehaviour
{
    public void AE_DealDamage()
    {
        // Reenvía el evento al BossAttack del root
        var atk = GetComponentInParent<BossAttack>();
        if (atk) atk.AE_DealDamage();
        else Debug.LogWarning("[AnimationEventRelay] No encontré BossAttack en el padre.");
    }
}
