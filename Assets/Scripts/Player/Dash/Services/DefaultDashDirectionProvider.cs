using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultDashDirectionProvider : IDashDirectionProvider
{
    private readonly Transform _owner;
    private readonly System.Func<Vector2> _moveInputGetter; // por ej. MovementController expone su input

    public DefaultDashDirectionProvider(Transform owner, System.Func<Vector2> moveInputGetter = null)
    {
        _owner = owner;
        _moveInputGetter = moveInputGetter;
    }

    public Vector3 ResolveDashDirection()
    {
        Vector2 in2 = _moveInputGetter != null ? _moveInputGetter() : Vector2.zero;
        Vector3 dir;
        if (in2.sqrMagnitude > 1e-6f)
        {
            // cámara-opcional: si tu MovementController ya da dir mundo, puedes inyectarla allí
            var tr = _owner;
            Vector3 fwd = tr.forward; fwd.y = 0f; fwd.Normalize();
            Vector3 right = tr.right; right.y = 0f; right.Normalize();
            dir = (right * in2.x + fwd * in2.y).normalized;
        }
        else
        {
            dir = _owner.forward; dir.y = 0f; dir.Normalize();
        }
        return dir == Vector3.zero ? Vector3.forward : dir;
    }
}
