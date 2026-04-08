using UnityEngine;
using System;

public interface IAdvancedMotor : IMotor
{
    public Vector3 TargetDirection { get; }

    public event Action OnContactWithGround;
    public event Action OnContactWithGroundExited;

    public event Action OnFall;

    public void Move(Vector2 movementInput, float baseSpeed, float movementSpeedModifier, Quaternion cameraPivot);
    public void StickToGround();

}
