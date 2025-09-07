using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchMouseInput : IInputSource
{
    public bool TryGetTap(out Vector2 screenPos)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            screenPos = Input.mousePosition;
            return true;
        }
#else
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                screenPos = Input.GetTouch(0).position;
                return true;
            }
#endif
        screenPos = default;
        return false;
    }
}
