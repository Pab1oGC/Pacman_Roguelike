using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyTargetingUtil
{
    // Devuelve el Transform del jugador más cercano (server-only).
    public static Transform FindClosestPlayer(Vector3 fromPos)
    {
        if (!NetworkServer.active) return null;

        Transform best = null;
        float bestSqr = float.MaxValue;

        // Recorremos todas las conexiones activas del server
        foreach (var kv in NetworkServer.connections)
        {
            var conn = kv.Value;
            var id = conn?.identity;
            if (!id) continue;

            var go = id.gameObject;
            if (!go.activeInHierarchy) continue;
            if (!go.CompareTag("Player")) continue;

            float d2 = (go.transform.position - fromPos).sqrMagnitude;
            if (d2 < bestSqr)
            {
                bestSqr = d2;
                best = go.transform;
            }
        }

        // Fallback por si acaso (no debería ser necesario)
        if (!best)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var p in players)
            {
                float d2 = (p.transform.position - fromPos).sqrMagnitude;
                if (d2 < bestSqr) { bestSqr = d2; best = p.transform; }
            }
        }

        return best;
    }
}
