using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRectifiedQuadFactory 
{
    GameObject EnsureQuad(GameObject current, Transform parent);
    void UpdateQuadTransform(Transform quad, Obb2D obb);
}
