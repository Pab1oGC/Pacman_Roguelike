using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputSource
{
    bool TryGetTap(out Vector2 screenPos);
}
