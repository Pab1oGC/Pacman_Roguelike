using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct Obb2D 
{
    public readonly Vector2 Center;
    public readonly Vector2 Ux; // eje mayor (unitario)
    public readonly Vector2 Vx; // eje menor (unitario)
    public readonly float LenU;
    public readonly float LenV;

    public Obb2D(Vector2 center, Vector2 ux, Vector2 vx, float lenU, float lenV)
    {
        Center = center; Ux = ux; Vx = vx; LenU = lenU; LenV = lenV;
    }
}
