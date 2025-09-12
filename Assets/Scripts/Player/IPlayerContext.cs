using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerContext
{
    Rigidbody Rb { get; }
    Camera MainCamera { get; }
    SimpleJoystick Joystick { get; }
    Invulnerability Invulnerability { get; }
    ISpeedProvider SpeedProvider { get; }
}
