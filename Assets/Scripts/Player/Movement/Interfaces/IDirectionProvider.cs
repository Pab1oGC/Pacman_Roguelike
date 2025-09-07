using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDirectionProvider 
{
    Vector3 ResolveDirection(Vector2 input);
}
