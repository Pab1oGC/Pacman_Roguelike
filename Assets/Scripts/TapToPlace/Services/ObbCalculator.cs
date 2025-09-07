using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ObbCalculator : IObbCalculator
{
    public bool TryComputeObb(ARPlane plane, out Obb2D obb)
    {
        obb = default;

        NativeArray<Vector2> boundary = plane.boundary;
        if (!boundary.IsCreated || boundary.Length < 3) return false;

        // 1) media
        Vector2 mean = Vector2.zero;
        for (int i = 0; i < boundary.Length; i++) mean += boundary[i];
        mean /= boundary.Length;

        // 2) covarianza 2x2
        double sxx = 0, sxy = 0, syy = 0;
        for (int i = 0; i < boundary.Length; i++)
        {
            Vector2 d = boundary[i] - mean;
            sxx += d.x * d.x;
            sxy += d.x * d.y;
            syy += d.y * d.y;
        }

        // 3) PCA -> ángulo principal
        double angle = 0.5 * System.Math.Atan2(2.0 * sxy, sxx - syy);
        float c = (float)System.Math.Cos(angle);
        float s = (float)System.Math.Sin(angle);
        Vector2 ux = new Vector2(c, s);        // eje mayor
        Vector2 vx = new Vector2(-s, c);       // eje menor

        // 4) proyecciones min/max
        float minU = float.PositiveInfinity, maxU = float.NegativeInfinity;
        float minV = float.PositiveInfinity, maxV = float.NegativeInfinity;
        for (int i = 0; i < boundary.Length; i++)
        {
            Vector2 d = boundary[i] - mean;
            float pu = Vector2.Dot(d, ux);
            float pv = Vector2.Dot(d, vx);
            if (pu < minU) minU = pu; if (pu > maxU) maxU = pu;
            if (pv < minV) minV = pv; if (pv > maxV) maxV = pv;
        }

        float lenU = (maxU - minU);
        float lenV = (maxV - minV);
        Vector2 center = mean + ((minU + maxU) * 0.5f) * ux + ((minV + maxV) * 0.5f) * vx;

        obb = new Obb2D(center, ux.normalized, vx.normalized, lenU, lenV);
        return true;
    }
}
