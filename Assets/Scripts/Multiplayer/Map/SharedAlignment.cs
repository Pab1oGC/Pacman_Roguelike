using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedAlignment : MonoBehaviour
{
    // Matriz que convierte puntos/rotaciones del HOST -> CLIENTE
    public static bool has;
    public static Matrix4x4 hostToClient = Matrix4x4.identity;

    public static Vector3 MapPos_HostToClient(Vector3 pHost)
    {
        return hostToClient.MultiplyPoint3x4(pHost);
    }

    public static Quaternion MapRot_HostToClient(Quaternion rHost)
    {
        var m = Matrix4x4.Rotate(rHost);
        var res = hostToClient * m;
        var f = res.GetColumn(2); // forward
        var u = res.GetColumn(1); // up
        return Quaternion.LookRotation(f, u);
    }
}
