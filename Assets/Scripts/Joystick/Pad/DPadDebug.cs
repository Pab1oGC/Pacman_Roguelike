using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class DPadDebug : MonoBehaviour
{
    void Update()
    {
        var dpad = GetComponent<DPadInputSource>();
        if (!dpad) return;
        var v = dpad.GetMoveInput();
        if (v != Vector2.zero) Debug.Log($"DPad dir: {v}");
    }
}
