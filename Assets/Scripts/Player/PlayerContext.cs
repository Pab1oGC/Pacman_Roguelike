using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerContext : MonoBehaviour, IPlayerContext
{
    [Header("Refs")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private SimpleJoystick joystick;
    [SerializeField] private Invulnerability invulnerability;
    [SerializeField] private MonoBehaviour speedSource; // debe implementar ISpeedProvider

    public Rigidbody Rb => rb;
    public Camera MainCamera => mainCamera;
    public SimpleJoystick Joystick => joystick;
    public Invulnerability Invulnerability => invulnerability;
    public ISpeedProvider SpeedProvider => speedSource as ISpeedProvider;

    void OnValidate()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!invulnerability) invulnerability = GetComponent<Invulnerability>();
        if (!mainCamera) mainCamera = Camera.main;
        if (!joystick)
        {
#if UNITY_2022_2_OR_NEWER
            joystick = FindFirstObjectByType<SimpleJoystick>(FindObjectsInactive.Include);
#else
            joystick = FindObjectOfType<SimpleJoystick>(true);
#endif
        }
        if (!speedSource) speedSource = GetComponent<ISpeedProvider>() as MonoBehaviour;
    }
}
