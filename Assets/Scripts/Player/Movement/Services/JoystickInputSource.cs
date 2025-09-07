using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickInputSource : IMoveInputSource
{
    private readonly SimpleJoystick _joystick;
    public JoystickInputSource(SimpleJoystick joystick) => _joystick = joystick;

    public Vector2 GetMoveInput() => _joystick ? _joystick.Value : Vector2.zero;
}
