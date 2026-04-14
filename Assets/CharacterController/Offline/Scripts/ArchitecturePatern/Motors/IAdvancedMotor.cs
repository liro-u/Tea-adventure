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

    /// <summary>
    /// Raycasts straight down from the character's feet. If an <see cref="IPlatformVelocityProvider"/>
    /// is found, displaces the character by its velocity this tick and returns that velocity.
    /// Returns Vector3.zero when the character is not on a moving platform.
    /// </summary>
    public Vector3 InheritPlatformVelocity(float tickDelta);

}
