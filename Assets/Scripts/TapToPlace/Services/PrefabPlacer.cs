using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabPlacer : IPrefabPlacer
{
    private readonly float _extraClearance;

    public PrefabPlacer(float extraClearance) => _extraClearance = extraClearance;

    public GameObject Place(GameObject prefab, GameObject current, Transform anchor)
    {
        if (prefab == null || anchor == null) return current;

        if (current == null)
            current = Object.Instantiate(prefab, anchor);

        current.transform.localRotation = Quaternion.identity;
        current.transform.localPosition = Vector3.zero;

        float h = ComputeWorldHeight(current);
        current.transform.position = anchor.position + anchor.up * (h * 0.5f + _extraClearance);

        return current;
    }

    private static float ComputeWorldHeight(GameObject go)
    {
        Bounds? b = null;
        foreach (var c in go.GetComponentsInChildren<Collider>(true))
            b = b.HasValue ? Enc(b.Value, c.bounds) : c.bounds;
        if (!b.HasValue)
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                b = b.HasValue ? Enc(b.Value, r.bounds) : r.bounds;
        return b.HasValue ? b.Value.size.y : 0f;

        static Bounds Enc(Bounds a, Bounds b) { a.Encapsulate(b); return a; }
    }
}
