using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARSceneRefs : MonoBehaviour
{
    public static ARSceneRefs I { get; private set; }
    public Camera arCamera;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        if (!arCamera) arCamera = Camera.main;
    }
}
