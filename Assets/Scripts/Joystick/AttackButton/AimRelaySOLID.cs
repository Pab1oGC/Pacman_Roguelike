using UnityEngine;

public class AimRelaySOLID : IAimRelaySOLID
{
    private AimAttackRelay playerRelay;

    public AimRelaySOLID(AimAttackRelay playerRelay)
    {
        this.playerRelay = playerRelay;
    }

    public void Fire(Vector3 worldDir)
    {
        playerRelay?.Fire(worldDir);
    }
}
