using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectifiedQuadFactory : IRectifiedQuadFactory
{
    private readonly Material _material;

    public RectifiedQuadFactory(Material material) => _material = material;

    public GameObject EnsureQuad(GameObject current, Transform parent)
    {
        if (current != null) return current;

        var go = new GameObject("RectifiedPlane");
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = _material;

        var mesh = new Mesh
        {
            vertices = new[]
            {
                    new Vector3(-0.5f, 0f, -0.5f),
                    new Vector3( 0.5f, 0f, -0.5f),
                    new Vector3( 0.5f, 0f,  0.5f),
                    new Vector3(-0.5f, 0f,  0.5f),
                },
            triangles = new[] { 0, 2, 1, 0, 3, 2 },
            normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up },
            uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up }
        };
        mesh.RecalculateBounds();
        mf.sharedMesh = mesh;

        go.transform.SetParent(parent, false);
        return go;
    }

    public void UpdateQuadTransform(Transform quad, Obb2D obb)
    {
        // u = X (right), v = Z (forward) en espacio local
        Vector3 u3 = new Vector3(obb.Ux.x, 0f, obb.Ux.y);
        Vector3 v3 = new Vector3(obb.Vx.x, 0f, obb.Vx.y);

        quad.localRotation = Quaternion.LookRotation(v3.normalized, Vector3.up);
        quad.localPosition = Vector3.zero;
        quad.localScale = new Vector3(Mathf.Max(obb.LenU, 0.01f), 1f, Mathf.Max(obb.LenV, 0.01f));
    }
}
