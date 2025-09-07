using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRelativeProvider : IDirectionProvider
{
    private readonly Camera _cam;
    public CameraRelativeProvider(Camera cam) => _cam = cam;

    public Vector3 ResolveDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 1e-6f)
            return Vector3.zero;

        Vector3 fwd, right;
        if (_cam)
        {
            fwd = _cam.transform.forward; fwd.y = 0f; fwd.Normalize();
            right = _cam.transform.right; right.y = 0f; right.Normalize();
        }
        else
        {
            fwd = Vector3.forward; right = Vector3.right;
        }

        var dir = right * input.x + fwd * input.y;
        if (dir.sqrMagnitude < 1e-6f) return Vector3.zero;
        return dir.normalized;
    }
}
